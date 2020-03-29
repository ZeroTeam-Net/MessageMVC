using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息生产对象
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType State { get; }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        TRes Producer<TArg,TRes>(string topic, string title, TArg content);

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
<<<<<<< HEAD
        string Producer(string topic, string title, string content);
=======
        TRes Producer<TArg,TRes>(string topic, string title, TArg content);
>>>>>>> c9fa0596fe7d47bbd0bf81699d533fa3d886c8e2

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
<<<<<<< HEAD
        Task<TRes> ProducerAsync<TArg, TRes>(string topic, string title, TArg content);
=======
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
>>>>>>> c9fa0596fe7d47bbd0bf81699d533fa3d886c8e2

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
<<<<<<< HEAD
        Task ProducerAsync<TArg>(string topic, string title, TArg content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <returns></returns>
        Task<TRes> ProducerAsync<TRes>(string topic, string title);
=======
        Task<TRes> ProducerAsync<TArg, TRes>(string topic, string title, TArg content);
>>>>>>> c9fa0596fe7d47bbd0bf81699d533fa3d886c8e2

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
<<<<<<< HEAD
=======
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
>>>>>>> c9fa0596fe7d47bbd0bf81699d533fa3d886c8e2
        Task<string> ProducerAsync(string topic, string title,string content);

    }
}
