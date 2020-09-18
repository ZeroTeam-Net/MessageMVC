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
    public class ParallelPoster : MessagePostBase, IMessagePoster, IFlowMiddleware
    {

        int IZeroMiddleware.Level => MiddlewareLevel.General;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal ILogger logger;


        /// <summary>
        ///     初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger<ParallelPoster>();
            ConfigurationHelper.RegistOnChange("MessageMVC:ParallelService", ReloadOption, true);
            return Task.CompletedTask;
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
        /// 名称
        /// </summary>
        string IZeroDependency.Name => nameof(ParallelPoster);


        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            if (!ServiceMap.TryGetValue(message.ServiceName, out var map) || map == null || map.Length == 0)
            {
                return new MessageResult
                {
                    ID = message.ID,
                    Trace = message.Trace,
                    State = MessageState.Cancel
                };
            }
            var tasks = new Task<IInlineMessage>[map.Length];
            for (int i = 0; i < map.Length; i++)
            {
                string service = map[i];
                var clone = message.Clone();
                clone.Topic = service;
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
                        Trace = message.Trace,
                        State = task.Result.State
                    });
            }

            return new MessageResult
            {
                ID = message.ID,
                Trace = message.Trace,
                State = MessageState.Success,
                ResultData = results
            };
        }

    }
}
