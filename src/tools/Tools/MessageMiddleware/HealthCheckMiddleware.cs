using Agebull.Common.Ioc;
using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 健康检查中间件
    /// </summary>
    public class HealthCheckMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.Front;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Prepare | MessageHandleScope.End;



        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        async Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            message.Trace ??= TraceInfo.New(message.ID);
            if (message.Title != "_HealthCheck_")
            {
                return true;
            }
            var col = Collection;

            Collection = new ApiCollection
            {
                Start = DateTime.Now,
                End = DateTime.Now
            };
            var checkers = DependencyHelper.GetServices<IHealthCheck>();
            var res = new NameValue<Dictionary<string, HealthInfo>>
            {
                Name = ZeroAppOption.Instance.ServiceName,
                Value = new Dictionary<string, HealthInfo>()
            };
            res.Value.Add("ApiCollection", new HealthInfo
            {
                Value = (col.End - col.Start).TotalSeconds,
                Details = col.ToJson()
            });
            foreach (var checker in checkers)
            {
                if (res.Value.ContainsKey(checker.Name))
                    continue;
                DateTime start = DateTime.Now;
                try
                {
                    var info = await checker.Check();
                    info.Value = (DateTime.Now - start).TotalMilliseconds;
                    if (info.Value < 10)
                        info.Level = 5;
                    else if (info.Value < 100)
                        info.Level = 4;
                    else if (info.Value < 500)
                        info.Level = 3;
                    else if (info.Value < 3000)
                        info.Level = 2;
                    else info.Level = 1;
                    res.Value.Add(checker.Name, info);
                }
                catch (Exception ex)
                {
                    res.Value.Add(checker.Name, new HealthInfo
                    {
                        Level = -1,
                        ItemName = checker.Name,
                        Details = ex.Message
                    });
                }
            }
            message.RealState = MessageState.Success;
            message.ResultData = ApiResultHelper.Succees(res);
            return false;
        }

        static ApiCollection Collection = new ApiCollection
        {
            Start = DateTime.Now,
            End = DateTime.Now
        };

        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            var col = Collection;

            col.End = DateTime.Now;

            Interlocked.Increment(ref col.Count);
            var time = (message.Trace.Start.Value - message.Trace.End.Value).TotalMilliseconds;
            int level = 0;
            if (time < 100)
                level = 5;
            else if (time < 500)
                level = 4;
            else if (time < 1000)
                level = 3;
            else if (time < 3000)
                level = 2;
            else
                level = 1;


            lock (col)
            {
                if (col.State.ContainsKey(message.State))
                {
                    col.State[message.State] += 1;
                }
                else
                {
                    col.State.Add(message.State, 1);
                }
                if (col.TimeLevel.TryGetValue(level, out var item))
                {
                    item.Count += 1;
                }
                else
                {
                    col.TimeLevel.Add(level, item = new CollectionItem
                    {
                        Count = 1
                    });
                }
                if (level < 3)
                {
                    var name = $"{message.ServiceName}/{message.ApiName}";
                    if (!item.Apis.TryGetValue(name, out var cnt))
                        item.Apis[name] += 1;
                    else
                        item.Apis.Add(name, 1);
                }
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// API统计
    /// </summary>
    public class ApiCollection
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// 请求总数
        /// </summary>
        public int Count;

        /// <summary>
        /// 状态统计
        /// </summary>
        public Dictionary<MessageState, int> State = new Dictionary<MessageState, int>();


        /// <summary>
        /// 状态统计
        /// </summary>
        public Dictionary<int, CollectionItem> TimeLevel = new Dictionary<int, CollectionItem>();

    }

    /// <summary>
    /// 统计节点
    /// </summary>
    public class CollectionItem
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 相关接口
        /// </summary>
        public Dictionary<string, int> Apis = new Dictionary<string, int>();
    }
}