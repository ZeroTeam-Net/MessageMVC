using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息投递对象
    /// </summary>
    public interface IMessagePoster
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType State { get; }

        /// <summary>
        ///     初始化
        /// </summary>
        void Initialize()
        {

        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<(MessageState state, string result)> Post(IMessageItem message);

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
