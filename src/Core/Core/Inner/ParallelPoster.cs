using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     并行生产者
    /// </summary>
    public class ParallelPoster : MessagePostBase, IMessagePoster
    {


        ILifeFlow IMessagePoster.GetLife() => null;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal ILogger logger;

        /// <summary>
        /// 名称
        /// </summary>
        string IZeroDependency.Name => nameof(ParallelPoster);



        /// <summary>
        ///     初始化
        /// </summary>
        public ParallelPoster()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger<ParallelPoster>();
            ConfigurationHelper.RegistOnChange("MessageMVC:ParallelService", ReloadOption, true);
        }

        readonly Dictionary<string, string[]> ServiceMap = new Dictionary<string, string[]>();

        void ReloadOption()
        {
            var old = ServiceMap.Keys.ToArray();
            ServiceMap.Clear();
            try
            {
                var section = ConfigurationHelper.Root.GetSection("MessageMVC:ParallelService");
                if (section != null && section.Exists())
                {
                    foreach (var ch in section.GetChildren())
                    {
                        ServiceMap.TryAdd(ch.Key, ch.Value.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Exception(ex);
            }
            foreach (var o in ServiceMap.Keys)
            {
                if (!old.Contains(o))
                {
                    MessagePoster.RegistPoster(this, o);
                }
            }
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            if (!ZeroAppOption.Instance.IsAlive)
            {
                message.State = MessageState.Cancel;
                return null;
            }
            if (!ServiceMap.TryGetValue(message.Service, out var map) || map == null || map.Length == 0)
            {
                return new MessageResult
                {
                    ID = message.ID,
                    Trace = message.TraceInfo,
                    State = MessageState.Cancel
                };
            }
            try
            {
                ZeroAppOption.Instance.BeginRequest();
                var tasks = new Task<IInlineMessage>[map.Length];
                for (int i = 0; i < map.Length; i++)
                {
                    string service = map[i];
                    var clone = message.Clone();
                    clone.Service = service;
                    tasks[i] = MessagePoster.Post(clone);
                }
                await Task.WhenAll(tasks);

                var results = new List<IMessageResult>();

                foreach (var task in tasks)
                {
                    if (task.Result.State == MessageState.Success)
                        results.Add(task.Result.ToMessageResult(true));
                    else
                        results.Add(new MessageResult
                        {
                            ID = message.ID,
                            Trace = message.TraceInfo,
                            State = task.Result.State
                        });
                }

                return new MessageResult
                {
                    ID = message.ID,
                    Trace = message.TraceInfo,
                    State = MessageState.Success,
                    ResultData = results
                };
            }
            finally
            {
                ZeroAppOption.Instance.EndRequest();
            }
        }

    }
}
