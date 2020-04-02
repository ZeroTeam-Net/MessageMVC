using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息投递
    /// </summary>
    public class MessagePoster : IFlowMiddleware
    {
        #region IFlowMiddleware

        /// <summary>
        /// 默认的生产者
        /// </summary>
        public static IMessagePoster Default { get; set; }

        string IZeroMiddleware.Name => "MessagePoster";

        int IZeroMiddleware.Level => int.MaxValue;

        #endregion

        #region 消息生产者

        private static readonly Dictionary<string, IMessagePoster> Producers = new Dictionary<string, IMessagePoster>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     初始化
        /// </summary>
        void IFlowMiddleware.Initialize()
        {
            var pros = IocHelper.RootProvider.GetServices<IMessagePoster>().ToArray();
            if (pros.Length == 1)
            {
                Default = pros[0];
                return;
            }
            var sec = ConfigurationManager.Get("MessagePoster");
            foreach (var pro in pros)
            {
                var strs = sec.GetStr(pro.GetTypeName(), "")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strs)
                {
                    Producers.TryAdd(str, pro);
                }
                pro.Initialize();
            }
        }

        /// <summary>
        ///     手动注册
        /// </summary>
        public static void Register(string name, IMessagePoster pro)
        {
            Producers.TryAdd(name, pro);
        }

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <param name="name">服务名称</param>
        /// <returns>传输对象构造器</returns>
        public static IMessagePoster GetService(string name)
        {
            if (Producers.TryGetValue(name, out var producer))
            {
                return producer;
            }
            return Default ??= IocHelper.Create<IMessagePoster>();
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns>返回值</returns>
        public static Task<(MessageState state, string result)> Post(IMessageItem item)
        {
            var producer = GetService(item.Topic);
            if (producer == null)
                return Task.FromResult((MessageState.NoSupper, default(string)));
            return producer.Post(item);
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
            var (state, _) = Post(MessageItem.NewMessage(topic, title, content)).Result;
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
            var (state, _) = Post(MessageItem.NewMessage(topic, title, content)).Result;
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
            var (state, _) = await Post(MessageItem.NewMessage(topic, title, content));
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
            var (state, _) = await Post(MessageItem.NewMessage(topic, title, content));
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
        public static TRes Call<TArg, TRes>(string service, string api, TArg args)
        {
            var (state, result) = Post(MessageItem.NewMessage(service, api, args)).Result;
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <param name="args">接口参数</param>
        /// <returns></returns>
        public static void Call<TArg>(string service, string api, TArg args)
        {
            Post(MessageItem.NewMessage(service, api, args)).Wait();
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static TRes Call<TRes>(string service, string api)
        {
            var (state, result) = Post(MessageItem.NewMessage(service, api)).Result;
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
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
            var (_, result) = Post(MessageItem.NewMessage(service, api, args)).Result;
            return result;
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
            var (state, result) = await Post(MessageItem.NewMessage(service, api, args));
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
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
            return Post(MessageItem.NewMessage(service, api, args));
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="service">服务名称</param>
        /// <param name="api">接口名称</param>
        /// <returns></returns>
        public static async Task<TRes> CallAsync<TRes>(string service, string api)
        {
            var (state, result) = await Post(MessageItem.NewMessage(service, api));
            return state != MessageState.Success
                ? default
                : JsonHelper.TryDeserializeObject<TRes>(result);
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
            var (_, result) = await Post(MessageItem.NewMessage(service, api, args));
            return result;
        }

        #endregion
    }
}
