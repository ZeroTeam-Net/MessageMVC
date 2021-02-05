using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 回执服务中间件
    /// </summary>
    public class ReceiptMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.Last;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope =>
            ToolsOption.Instance.EnableReceipt ? MessageHandleScope.End : MessageHandleScope.None;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        async Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (!GlobalContext.IsOptionTrue("Receipt"))
            {
                return;
            }
            var receipt = MessageHelper.Simple(message.ID, ToolsOption.Instance.ReceiptService, ToolsOption.Instance.ReceiptApi, message.ToJson());
            await MessagePoster.Post(receipt);
        }
    }
}