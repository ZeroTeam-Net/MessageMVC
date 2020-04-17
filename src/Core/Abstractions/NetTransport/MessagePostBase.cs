using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息投递器基类
    /// </summary>
    public class MessagePostBase : ISerializeProxy
    {
        /// <summary>
        /// 日志器
        /// </summary>
        protected ILogger logger { get;private  set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger(GetType().GetTypeName());
            serializer = DependencyHelper.Create<ISerializeProxy>();
        }

        #region 序列化
        /// <summary>
        /// 序列化器
        /// </summary>
        protected ISerializeProxy serializer { get; private set; }

        ///<inheritdoc/>
        object ISerializeProxy.Serialize(object soruce)
        {
            return serializer.Serialize(soruce);
        }

        ///<inheritdoc/>
        object ISerializeProxy.Deserialize(object soruce, Type type)
        {
            return serializer.Deserialize(soruce, type);
        }


        ///<inheritdoc/>
        T ISerializeProxy.ToObject<T>(string soruce)
        {
            return serializer.ToObject<T>(soruce);
        }


        ///<inheritdoc/>
        object ISerializeProxy.ToObject(string soruce, Type type)
        {
            return serializer.ToObject(soruce, type);
        }

        ///<inheritdoc/>
        string ISerializeProxy.ToString(object obj, bool indented)
        {
            return serializer.ToString(obj, indented);
        }

        #endregion
    }
}

/*
 
        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        TRes Producer<TArg, TRes>(string topic, string title, TArg content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        void Producer<TArg>(string topic, string title, TArg content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        TRes Producer<TRes>(string topic, string title);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        string Producer(string topic, string title, string content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        Task<TRes> ProducerAsync<TArg, TRes>(string topic, string title, TArg content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        Task ProducerAsync<TArg>(string topic, string title, TArg content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        Task<TRes> ProducerAsync<TRes>(string topic, string title);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        Task<string> ProducerAsync(string topic, string title, string content);

     */
