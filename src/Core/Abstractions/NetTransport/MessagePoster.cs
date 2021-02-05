using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 日志对象
        /// </summary>
        private static ILogger logger;

        string IZeroDependency.Name => nameof(MessagePoster);

        int IZeroMiddleware.Level => MiddlewareLevel.Last;

        Task IZeroDiscover.Discovery()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(MessagePoster));
            return WaitAll<IZeroDiscover>(p => p.Discovery());
        }
        /// <summary>
        ///     初始化
        /// </summary>
        Task ILifeFlow.Initialize()
        {
            return WaitAll(p => p.Initialize());
        }

        /// <summary>
        /// 开启
        /// </summary>
        Task ILifeFlow.Open()
        {
            return WaitAll(p => p.Open());
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Closing()
        {
            return WaitAll(p => p.Closing());
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Close()
        {
            return WaitAll(p => p.Close());
        }
        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Destroy()
        {
            return WaitAll(p => p.Destroy());
        }
        async Task WaitAll<TFlow>(Func<TFlow, Task> func) where TFlow : class
        {
            var tasks = new List<Task>();
            foreach (var poster in MessagePostOption.Instance.posters.Values)
            {
                if (poster is TFlow flow)
                    tasks.Add(func(flow));
            }

            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    logger.Exception(ex);
                }
            }
        }
        async Task WaitAll(Func<ILifeFlow, Task> func)
        {
            var tasks = new List<Task>();
            foreach (var poster in MessagePostOption.Instance.posters.Values)
            {
                var life = poster.GetLife();
                if (life != null)
                    tasks.Add(func(life));
            }

            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    logger.Exception(ex);
                }
            }
        }
        #endregion

        #region 配置

        /// <summary>
        /// 启用本地隧道（即本地接收器存在的话，本地处理）
        /// </summary>
        public static bool LocalTunnel => MessagePostOption.Instance.LocalTunnel;

        /// <summary>
        /// 默认的生产者
        /// </summary>
        public static string DefaultPosterName => MessagePostOption.Instance.DefaultPosterName;


        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster(IMessagePoster poster, params string[] services)
        {
            MessagePostOption.Instance.RegistPoster(poster, services);
        }
        /// <summary>
        ///     手动注销
        /// </summary>
        public static void UnRegistPoster(string poster)
        {
            MessagePostOption.Instance.UnRegistPoster(poster);
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster<TMessagePoster>(params string[] services)
            where TMessagePoster : IMessagePoster, new()
        {
            MessagePostOption.Instance.RegistPoster<TMessagePoster>(services);
        }

        #endregion

        #region 消息生产者


        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="defPoster">服务未注册时，是否使用缺省投递器</param>
        /// <param name="autoOffline">是否自动离线</param>
        /// <returns>返回值,如果未进行离线交换message返回为空,此时请检查state</returns>
        public static async Task<IInlineMessage> Post(IMessageItem message, bool autoOffline = true, bool defPoster = true)
        {
            if (message == null || string.IsNullOrEmpty(message.Service) || string.IsNullOrEmpty(message.Method))
            {
                FlowTracer.MonitorError($"参数错误：Service:{message?.Service}  Method:{message?.Method}");
                throw new ArgumentException("参数[message]不能为空且[message.Topic]与[message.Title]必须为有效值");
            }

            using var scope = FlowTracer.DebugStepScope(() => $"[MessagePoster] {message.Service}/{message.Method}");

            if (!ZeroAppOption.Instance.CanRun)
            {
                FlowTracer.MonitorError($"系统未运行,当前状态为：{ZeroAppOption.Instance.ApplicationState}");

                if (message is IInlineMessage inlineMessage)
                {
                    message.State = MessageState.Cancel;
                    return inlineMessage;
                }
                else
                {
                    return new InlineMessage
                    {
                        ID = message.ID,
                        State = MessageState.Cancel,
                        Service = message.Service,
                        Method = message.Method,
                        Result = message.Result,
                        Argument = message.Argument,
                        TraceInfo = message.TraceInfo
                    };
                }
            }

            var producer = MessagePostOption.Instance.GetService(message.Service, defPoster);
            if (producer == null)
            {
                FlowTracer.MonitorError($"服务({message.Service})不存在");
                if (message is IInlineMessage inlineMessage)
                {
                    message.State = MessageState.Unhandled;
                    return inlineMessage;
                }
                else
                {
                    return new InlineMessage
                    {
                        ID = message.ID,
                        State = MessageState.Unhandled,
                        Service = message.Service,
                        Method = message.Method,
                        Result = message.Result,
                        Argument = message.Argument,
                        TraceInfo = message.TraceInfo
                    };
                }
            }
            FlowTracer.MonitorDetails(() => $"[Poster] {producer.GetTypeName()}");


            var inline = CheckMessage(producer, message);

            try
            {
                FlowTracer.MonitorTrace(() => $"[Context]    {message.Context?.ToInnerString()}");
                FlowTracer.MonitorTrace(() => $"[User]       {message.User?.ToInnerString()}");
                FlowTracer.MonitorTrace(() => $"[TraceInfo]  {message.TraceInfo?.ToInnerString()}");
                FlowTracer.MonitorDetails(() => $"[Argument]   {inline.Argument ?? inline.ArgumentData?.ToInnerString()}");
                var msg = await producer.Post(inline);
                if (msg != null)
                {
                    inline.CopyResult(msg);
                }
                if (autoOffline)
                {
                    inline.OfflineResult();
                }
                FlowTracer.MonitorDetails(() => $"[State] {inline.State} [Result] {inline.Result ?? inline.ResultData?.ToJson() ?? "无返回值"}");
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
            finally
            {
            }
        }

        static IInlineMessage CheckMessage(IMessagePoster poster, IMessageItem message)
        {
            var re = poster.CheckMessage(message);
            if (re != null)
                return re;

            var ctx = GlobalContext.CurrentNoLazy;
            if (message is IInlineMessage inline)
            {
                if (ctx != null && ctx.Message == message)
                {
                    inline = inline.CopyToRequest();
                }
                else
                {
                    inline.ResetToRequest();
                }
            }
            else
            {
                inline = new InlineMessage
                {
                    ID = message.ID,
                    Service = message.Service,
                    Method = message.Method,
                    Result = message.Result,
                    Argument = message.Argument,
                    DataState = MessageDataState.ArgumentOffline
                };
            }
            inline.CheckPostTraceInfo();

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
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        public static async Task<(IApiResult<TRes> res, MessageState state)> CallApiAsync<TRes>(string service, string api)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        public static async Task<(IApiResult res, MessageState state)> CallApiAsync<TArg>(string service, string api, TArg args)
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        public static async Task<(IApiResult<TRes> res, MessageState state)> CallApiAsync<TRes>(string service, string api, Dictionary<string, string> args)
             where TRes : class
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        public static async Task<(IApiResult<TRes> res, MessageState state)> CallApiAsync<TRes>(string service, string api, params (string name, object value)[] args)
             where TRes : class
        {
            var dir = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                dir.TryAdd(arg.name, arg.value?.ToString());
            }
            var msg = await Post(MessageHelper.NewRemote(service, api, dir), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        public static async Task<(IApiResult<TRes> res, MessageState state)> CallApiAsync<TRes>(string service, string api, string args)
             where TRes : class
        {
            var msg = await Post(MessageHelper.NewRemote(service, api, args), false, true);
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        public static (IApiResult<TRes> res, MessageState state) CallApi<TRes>(string service, string api, string args)
             where TRes : class
        {
            var msg = Post(MessageHelper.NewRemote(service, api, args), false, true).Result;
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
        /// <returns></returns>
        public static (IApiResult<TRes> res, MessageState state) CallApi<TRes>(string service, string api)
             where TRes : class
        {
            var msg = Post(MessageHelper.NewRemote(service, api), false, true).Result;
            if (msg.ResultData != null)
            {
                return (msg.ResultData as IApiResult<TRes>, msg.State);
            }
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
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
            if (string.IsNullOrEmpty(msg.Result) && msg.ResultData == null)
            {
                return (ApiResultHelper.Helper.State<TRes>(msg.State.ToErrorCode()), msg.State);
            }
            return (ApiResultHelper.Helper.Deserialize<TRes>(msg.Result), msg.State);
        }

        #endregion
    }
}
