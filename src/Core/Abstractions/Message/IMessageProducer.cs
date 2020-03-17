using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息生产对象
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        IApiResult Producer(string topic,string title,string content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        IApiResult Producer<T>(string topic, string title, T content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        Task<IApiResult> ProducerAsync(string topic, string title, string content);

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        Task<IApiResult> ProducerAsync<T>(string topic, string title, T content);
    }
}
