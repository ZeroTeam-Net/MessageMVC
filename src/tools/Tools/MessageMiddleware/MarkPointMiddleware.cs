using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 埋点发出中间件
    /// </summary>
    public class MarkPointMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => ToolsOption.Instance.EnableMarkPoint
                ? MessageHandleScope.End
                : MessageHandleScope.None;

        /// <summary>
        /// 结果处理
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            return MessagePoster.PublishAsync(ToolsOption.Instance.MarkPointName, message.Topic, message);
        }
    }
}