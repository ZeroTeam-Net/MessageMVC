using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 消息投递
    /// </summary>
    public class MessagePoster : IFlowMiddleware
    {
        #region IFlowMiddleware

        string IZeroDependency.Name => nameof(MessagePoster);

        int IZeroMiddleware.Level => MiddlewareLevel.Last;

        /// <summary>
        ///     初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            logger ??= DependencyHelper.LoggerFactory.CreateLogger(nameof(MessagePoster));
            posters = new Dictionary<string, IMessagePoster>();
            foreach (var poster in DependencyHelper.RootProvider.GetServices<IMessagePoster>())
            {
                posters.TryAdd(poster.GetTypeName(), poster);
            }
            var sec = ConfigurationHelper.Get("MessageMVC:MessagePoster");
            if (sec == null)
            {
                if (posters.TryGetValue("HttpPoster", out Default))
                {
                    logger.Information("缺省发布器为HttpPoster.");
                }
                else
                {
                    logger.Information("无发布器,所有外部请求将失败.");
                }
                return Task.CompletedTask;
            }
            localTunnel = sec.GetBool("localTunnel", false);
            var def = sec.GetStr("default", "");
            if (posters.TryGetValue(def, out Default))
            {
                logger.Information(() => $"缺省发布器为{def}.");
            }

            foreach (var poster in posters)
            {
                poster.Value.Initialize();
                var cfgs = sec.GetStr(poster.Key, "");
                if (string.IsNullOrWhiteSpace(cfgs))
                {
                    continue;
                }
                logger.Information(() => $"服务[{cfgs}]配置为使用发布器{poster.Key}");
                var services = cfgs.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var service in services)
                {
                    AddPoster(poster.Value, service);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 开启
        /// </summary>
        Task ILifeFlow.Open()
        {
            foreach (var poster in posters.Values.Where(p => !p.IsLocalReceiver))
            {
                _ = poster.Open();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        async Task ILifeFlow.Close()
        {
            var tasks = new List<Task>();
            foreach (var poster in posters.Values.Where(p => !p.IsLocalReceiver))
            {
                tasks.Add(poster.Close());
            }
            foreach (var task in tasks)
            {
                await task;
            }
        }
        #endregion

        #region 消息生产者

        /// <summary>
        /// 启用本地隧道（即本地接收器存在的话，本地处理）
        /// </summary>
        private static bool localTunnel;

        /// <summary>
        /// 默认的生产者
        /// </summary>
        private static IMessagePoster Default;
        /// <summary>
        /// 服务查找表
        /// </summary>
        private static readonly Dictionary<string, List<IMessagePoster>> ServiceMap = new Dictionary<string, List<IMessagePoster>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 服务查找表
        /// </summary>
        private static Dictionary<string, IMessagePoster> posters = new Dictionary<string, IMessagePoster>();

        /// <summary>
        /// 日志对象
        /// </summary>
        private static ILogger logger;


        private static void AddPoster(IMessagePoster poster, string service)
        {
            if (!ServiceMap.TryGetValue(service, out _))
                _ = ServiceMap.TryAdd(service, new List<IMessagePoster>());
            var items = ServiceMap[service];
            bool hase = false;
            foreach (var item in items)
            {
                if (item.GetType() == poster.GetType())
                {
                    hase = true;
                    break;
                }
            }
            if (!hase)
                items.Add(poster);
        }

        /// <summary>
        ///     手动注销
        /// </summary>
        public static void UnRegistPoster(string service)
        {
            posters.Remove(service);
            ServiceMap.Remove(service);
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster<TMessagePoster>(params string[] services)
            where TMessagePoster : IMessagePoster, new()
        {
            var name = typeof(TMessagePoster).GetTypeName();
            if (!posters.TryGetValue(name, out var poster))
            {
                posters.Add(name, poster = new TMessagePoster());
                poster.Initialize();
            }
            foreach (var service in services)
            {
                AddPoster(poster, service);
            }
            logger ??= DependencyHelper.LoggerFactory.CreateLogger(nameof(MessagePoster));
            logger.Information(() => $"服务[{string.Join(',', services)}]注册为使用发布器{name}");
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster(IMessagePoster poster, params string[] services)
        {
            foreach (var service in services.Where(p => !string.IsNullOrEmpty(p)))
            {
                AddPoster(poster, service);
            }
            logger ??= DependencyHelper.LoggerFactory.CreateLogger(nameof(MessagePoster));
            logger.Information(() => $"服务[{string.Join(',', services)}]注册为使用发布器{poster.GetTypeName()}");
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <param name="def">是否使用默认投递器</param>
        /// <returns>传输对象构造器</returns>
        static IMessagePoster GetService(string name, bool def)
        {
            if (!ServiceMap.TryGetValue(name, out var items))
                return def ? Default : null;
            foreach (var item in items)
            {
                if (item.IsLocalReceiver && !localTunnel)
                    continue;
                return item;
            }
            return null;
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="defPoster">服务未注册时，是否使用缺省投递器</param>
        /// <param name="autoOffline">是否自动离线</param>
        /// <returns>返回值,如果未进行离线交换message返回为空,此时请检查state</returns>
        public static async Task<IInlineMessage> Post(IMessageItem message, bool autoOffline = true, bool defPoster = true)
        {
            if (message == null || string.IsNullOrEmpty(message.Topic) || string.IsNullOrEmpty(message.Title))
            {
                throw new NotSupportedException("参数[message]不能为空且[message.Topic]与[message.Title]必须为有效值");
            }

            var inline = CheckMessage(message);
            var producer = GetService(message.Topic, defPoster);
            if (producer == null)
            {
                FlowTracer.MonitorInfomation(() => $"服务{message.Topic}不存在");
                inline.State = MessageState.Unhandled;
                return inline;
            }
            try
            {
                var msg = await producer.Post(inline);
                if (msg != null)
                {
                    inline.CopyResult(msg);
                    FlowTracer.MonitorDetails(() => $"返回 => {SmartSerializer.ToInnerString(msg)}");
                }
                else
                {
                    FlowTracer.MonitorDetails(() => $"返回 => {SmartSerializer.ToInnerString(inline)}");
                }
                if (autoOffline)
                {
                    inline.OfflineResult();
                }
                return inline;
            }
            catch (MessagePostException ex)
            {
                logger.Exception(ex);
                inline.State = MessageState.NetworkError;
                return inline;
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                inline.State = MessageState.FrameworkError;
                return inline;
            }

        }

        static IInlineMessage CheckMessage(IMessageItem message)
        {
            if (message is IInlineMessage inline)
            {
                inline.Reset();
                inline.State = MessageState.Accept;
            }
            else
            {
                var dataState = MessageDataState.ArgumentOffline;
                if (!string.IsNullOrEmpty(message.Result))
                    dataState |= MessageDataState.ResultOffline;
                inline = new InlineMessage
                {
                    ID = message.ID,
                    State = MessageState.Accept,
                    Topic = message.Topic,
                    Title = message.Title,
                    Result = message.Result,
                    Content = message.Content,
                    Trace = message.Trace,
                    DataState = dataState
                };
            }
            if (!GlobalContext.EnableLinkTrace)
            {
                return inline;
            }
            var ctx = GlobalContext.CurrentNoLazy;
            if (ctx == null)
            {
                inline.Trace ??= TraceInfo.New(inline.ID);
                return inline;
            }
            if (ctx.Trace == null)
            {
                inline.Trace ??= TraceInfo.New(inline.ID);
                inline.Trace.Context = new StaticContext
                {
                    Option = ctx.Option,
                    UserJson = SmartSerializer.ToString(ctx.User)
                };
                return inline;
            }

            inline.Trace ??= new TraceInfo
            {
                TraceId = inline.ID,
                Start = DateTime.Now,
            };
            inline.Trace.TraceId = ctx.Trace.TraceId;
            //远程机器使用,所以Call是本机信息
            inline.Trace.CallId = ctx.Trace.LocalId;
            inline.Trace.CallApp = ctx.Trace.LocalApp;
            inline.Trace.CallMachine = ctx.Trace.LocalMachine;
            //层级
            inline.Trace.Level = ctx.Trace.Level + 1;
            //正常复制
            inline.Trace.TraceId = ctx.Trace.TraceId;
            inline.Trace.Token = ctx.Trace.Token;
            inline.Trace.Headers = ctx.Trace.Headers;
            return inline;
        }

        #endregion

        #region Publish

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static MessageState Publish<TArg>(string topic, string title, TArg content)
        {
            var msg = Post(MessageHelper.NewRemote(topic, title, content), false, true).Result;
            return msg.State;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static MessageState Publish(string topic, string title, string content)
        {
            var msg = Post(MessageHelper.NewRemote(topic, title, content), false, true).Result;
            return msg.State;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static async Task<MessageState> PublishAsync<TArg>(string topic, string title, TArg content)
        {
            var msg = await Post(MessageHelper.NewRemote(topic, title, content), false, true);
            return msg.State;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static async Task<MessageState> PublishAsync(string topic, string title, string content)
        {
            var msg = await Post(MessageHelper.NewRemote(topic, title, content), false, true);
            return msg.State;
        }

        #endregion

        #region Call

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (TRes res, MessageState state) Call<TArg, TRes>(string service, string api, TArg args)
            where TRes : class
        {
            var msg = Post(MessageHelper.NewRemote(service, api, args), true, true).Result;
            return (msg.GetResultData<TRes>(), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (string res, MessageState state) Call<TArg>(string service, string api, TArg args)
        {
            var msg = Post(MessageHelper.NewRemote(service, api), true, true).Result;
            return (msg.Result, msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static (TRes res, MessageState state) Call<TRes>(string service, string api)
              where TRes : class
        {
            var msg = Post(MessageHelper.NewRemote(service, api), true, true).Result;
            return (msg.GetResultData<TRes>(), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (string res, MessageState state) Call(string service, string api, string args)
        {
            var msg = Post(MessageHelper.NewRemote(service, api, args), true, true).Result;
            return (msg.Result, msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<(TRes res, MessageState state)> CallAsync<TArg, TRes>(string service, string api, TArg args)
                     where TRes : class
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), true, true);
            return (msg.GetResultData<TRes>(), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<(string res, MessageState state)> CallAsync<TArg>(string service, string api, TArg args)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), true, true);
            return (msg.Result, msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static async Task<(TRes res, MessageState state)> CallAsync<TRes>(string service, string api)
                     where TRes : class
        {
            var msg = await Post(MessageHelper.NewRemote(service, api), true, true);
            return (msg.GetResultData<TRes>(), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<(string res, MessageState state)> CallAsync(string service, string api, string args)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), true, true);
            return (msg.Result, msg.State);
        }

        #endregion

        #region CallApi

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (IApiResult res, MessageState state) CallApi<TArg>(string service, string api, TArg args)
        {
            var msg = Post(MessageHelper.NewRemote(service, api, args), false, true).Result;
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize(msg.Result), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<(IApiResult res, MessageState state)> CallApiAsync<TArg>(string service, string api, TArg args)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize(msg.Result), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<(IApiResult res, MessageState state)> CallApiAsync(string service, string api, string args)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize(msg.Result), msg.State);
        }
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static async Task<(IApiResult res, MessageState state)> CallApiAsync(string service, string api)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize(msg.Result), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (IApiResult<TRes> res, MessageState state) CallApi<TRes>(string service, string api, string args)
             where TRes : class
        {
            var msg = Post(MessageHelper.NewRemote(service, api, args), false, true).Result;
            if(msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State<TRes>(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), msg.State);
        }


        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (IApiResult<TRes> res, MessageState state) CallApi<TArg, TRes>(string service, string api, TArg args)
             where TRes : class
        {
            var msg = Post(MessageHelper.NewRemote(service, api, args), false, true).Result;
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State<TRes>(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), msg.State);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<(IApiResult<TRes> res, MessageState state)> CallApiAsync<TArg, TRes>(string service, string api, TArg args)
             where TRes : class
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (msg.Result == null && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State<TRes>(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), msg.State);
        }

        #endregion
    }
}
