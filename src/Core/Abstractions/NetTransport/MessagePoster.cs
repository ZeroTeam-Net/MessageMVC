using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 消息投递
    /// </summary>
    public class MessagePoster : IFlowMiddleware
    {
        #region IFlowMiddleware

        string IZeroMiddleware.Name => "MessagePoster";

        int IZeroMiddleware.Level => int.MaxValue;

        #endregion

        #region 消息生产者

        /// <summary>
        /// 默认的生产者
        /// </summary>
        private static IMessagePoster Default;

        private static readonly Dictionary<string, IMessagePoster> ServiceMap = new Dictionary<string, IMessagePoster>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, IMessagePoster> posters = new Dictionary<string, IMessagePoster>();
        private static ILogger logger;

        /// <summary>
        ///     初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            logger = IocHelper.LoggerFactory.CreateLogger(nameof(MessagePoster));
            posters = new Dictionary<string, IMessagePoster>();
            foreach (var poster in IocHelper.RootProvider.GetServices<IMessagePoster>())
            {
                posters.TryAdd(poster.GetTypeName(), poster);
            }
            var sec = ConfigurationManager.Get("MessageMVC:MessagePoster");
            var def = sec.GetStr("default", "");
            if (posters.TryGetValue(def, out Default))
            {
                logger.Information(() => $"Poster {def} is config for default.");
            }

            foreach (var poster in posters)
            {
                poster.Value.Initialize();
                var cfgs = sec.GetStr(poster.Key, "");
                if (string.IsNullOrWhiteSpace(cfgs))
                {
                    continue;
                }

                var services = cfgs.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var service in services)
                {

                    ServiceMap[service] = poster.Value;
                }
                logger.Information(() => $"Poster {poster.Key} is config for {cfgs}");
            }
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
                ServiceMap[service] = poster;
            }
            logger.Information(() => $"Poster {name} is regist for {string.Join(',', services)}");
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void RegistPoster(object poster, params string[] services)
        {
            foreach (var service in services)
            {
                ServiceMap[service] = poster as IMessagePoster;
            }

            logger.Information(() => $"Poster {poster.GetTypeName()} is regist for services [{string.Join(' ', services)}]");
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

            return ServiceMap.TryGetValue(name, out var producer)
                ? producer
                : Default;
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns>返回值</returns>
        public static async Task<(IInlineMessage message, ISerializeProxy serialize)> Post(IMessageItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.Topic) || string.IsNullOrEmpty(item.Title))
            {
                throw new NotSupportedException("参数[item]不能为空且[Topic]与[Title]必须为有效值");
            }
            var inline = item.ToInline();
            var producer = GetService(item.Topic);
            if (producer == null)
            {
                inline.State = MessageState.NoSupper;
                logger.Warning(() => $"No find [{item.Topic}] poster");
                return (inline, null);
            }
            var msg = await producer.Post(inline);
            logger.Trace(() => inline.ToJson(true));
            return (msg, producer);
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
            var res = Post(MessageHelper.NewRemote(topic, title, content)).Result;
            return res.message.State;
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
            var res = Post(MessageHelper.NewRemote(topic, title, content)).Result;
            return res.message.State;
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
            var res = await Post(MessageHelper.NewRemote(topic, title, content));
            return res.message.State;
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
            var res = await Post(MessageHelper.NewRemote(topic, title, content));
            return res.message.State;
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
        public static TRes Call<TArg, TRes>(string service, string api, TArg args)
        {
            var (msg, seri) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            return msg.GetResultData<TRes>(seri);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static MessageState Call<TArg>(string service, string api, TArg args)
        {
            return Post(MessageHelper.NewRemote(service, api, args)).Result.message.State;
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static TRes Call<TRes>(string service, string api)
        {
            var (msg, seri) = Post(MessageHelper.NewRemote(service, api)).Result;
            return msg.GetResultData<TRes>(seri);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static string Call(string service, string api, string args)
        {
            var (msg, seri) = Post(MessageHelper.NewRemote(service, api, args)).Result;
            return msg.GetResult(seri);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<TRes> CallAsync<TArg, TRes>(string service, string api, TArg args)
        {
            var (msg, seri) = await Post(MessageHelper.NewRemote(service, api, args));
            return msg.GetResultData<TRes>(seri);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static Task CallAsync<TArg>(string service, string api, TArg args)
        {
            return Post(MessageHelper.NewRemote(service, api, args));
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static async Task<TRes> CallAsync<TRes>(string service, string api)
        {
            var (msg, seri) = await Post(MessageHelper.NewRemote(service, api));
            return msg.GetResultData<TRes>(seri);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static async Task<string> CallAsync(string service, string api, string args)
        {
            var (msg, seri) = await Post(MessageHelper.NewRemote(service, api, args));
            return msg.GetResult(seri);
        }

        #endregion

    }
}
