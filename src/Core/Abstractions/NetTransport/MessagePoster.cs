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

        #endregion

        #region 消息生产者

        /// <summary>
        /// 默认的生产者
        /// </summary>
        private static IMessagePoster Default;

        private static readonly Dictionary<string, List<IMessagePoster>> ServiceMap = new Dictionary<string, List<IMessagePoster>>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, IMessagePoster> posters = new Dictionary<string, IMessagePoster>();
        private static ILogger logger;

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

        private static void AddPoster(IMessagePoster poster, string service)
        {
            if (!ServiceMap.TryGetValue(service, out var items))
                ServiceMap.TryAdd(service, items = new List<IMessagePoster>());
            items = ServiceMap[service];
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
                AddPoster(poster,service);
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
        /// <returns>传输对象构造器</returns>
        public static IMessagePoster GetService(string name)
        {
            if (name == null)
            {
                return null;
            }
            if (!ServiceMap.TryGetValue(name, out var items))
                return Default;
            foreach (var item in items)
                if (item.CanDo)
                    return item;

            return null;
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="autoOffline">是否自动离线</param>
        /// <returns>返回值,如果未进行离线交换message返回为空,此时请检查state</returns>
        public static async Task<(IInlineMessage message, MessageState state)> Post(IMessageItem message, bool autoOffline = true)
        {
            if (message == null || string.IsNullOrEmpty(message.Topic) || string.IsNullOrEmpty(message.Title))
            {
                throw new NotSupportedException("参数[message]不能为空且[message.Topic]与[message.Title]必须为有效值");
            }
            var producer = GetService(message.Topic);
            if (producer == null)
            {
                FlowTracer.MonitorInfomation(() => $"服务{message.Topic}不存在");
                return (null, MessageState.Unhandled);
            }
            var inline = CheckMessage(message);
            try
            {
                var msg = await producer.Post(inline);
                if (msg != null)
                    inline.CopyResult(msg);
                if (autoOffline)
                {
                    inline.OfflineResult();
                    FlowTracer.MonitorDetails(() => $"返回 => {SmartSerializer.ToInnerString(msg)}");
                }
                return (inline, inline.State);
            }
            catch (MessagePostException ex)
            {
                logger.Exception(ex);
                return (null, MessageState.NetworkError);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                return (null, MessageState.FrameworkError);
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
            if (inline.Trace != null && !GlobalContext.EnableLinkTrace)
            {
                return inline;
            }
            inline.Trace = new TraceInfo
            {
                TraceId = inline.ID,
                Start = DateTime.Now,
            };
            var ctx = GlobalContext.CurrentNoLazy;
            if (ctx == null)
            {
                return inline;
            }
            inline.Trace.Context = new StaticContext
            {
                Option = ctx.Option,
                UserJson = SmartSerializer.ToString(ctx.User)
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

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static MessageState Publish<TArg>(string topic, string title, TArg content)
        {
            var (_, state) = Post(MessageHelper.NewRemote(topic, title, content), false).Result;
            return state;
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
            var (_, state) = Post(MessageHelper.NewRemote(topic, title, content), false).Result;
            return state;
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
            var (_, state) = await Post(MessageHelper.NewRemote(topic, title, content), false);
            return state;
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
            var (_, state) = await Post(MessageHelper.NewRemote(topic, title, content), false);
            return state;
        }

        #endregion

        #region 远程调用

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
            var (msg, state) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            return (msg?.GetResultData<TRes>(), state);
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
            var (msg, state) = Post(MessageHelper.NewRemote(service, api)).Result;
            if((msg.DataState & MessageDataState.ResultOffline) == MessageDataState.ResultOffline)
                return (msg?.Result, state);

            msg.OfflineResult();
            return (msg?.Result, state);
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
            var (msg, state) = Post(MessageHelper.NewRemote(service, api)).Result;
            return (msg?.GetResultData<TRes>(), state);
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
            var (msg, state) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            msg?.OfflineResult();
            return (msg.Result, state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api, args));
            return (msg?.GetResultData<TRes>(), state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api, args));

            if ((msg.DataState & MessageDataState.ResultOffline) == MessageDataState.ResultOffline)
                return (msg?.Result, state);

            msg.OfflineResult();
            return (msg?.Result, state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api));
            return (msg?.GetResultData<TRes>(), state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api, args));
            msg.OfflineResult();
            msg?.OfflineResult();
            return (msg.Result, state);
        }

        #endregion

        #region ApiResult

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static (IApiResult res, MessageState state) CallApi<TArg>(string service, string api, TArg args)
        {
            var (msg, state) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize(msg.Result), state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api, args));
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize(msg.Result), state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api, args));
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize(msg.Result), state);
        }
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static async Task<(IApiResult res, MessageState state)> CallApiAsync(string service, string api)
        {
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api));
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize(msg.Result), state);
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
            var (msg, state) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult<TRes> res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), state);
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
            var (msg, state) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult<TRes> res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), state);
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
            var (msg, state) = await Post(MessageHelper.NewRemote(service, api, args));
            if (!state.IsSuccess() || msg == null)
            {
                return (null, state);
            }
            return msg.ResultData is IApiResult<TRes> res
                ? (res, state)
                : (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), state);
        }

        #endregion
    }
}
