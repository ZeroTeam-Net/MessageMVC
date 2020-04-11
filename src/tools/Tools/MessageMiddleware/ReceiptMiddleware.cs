using Agebull.Common;
using Agebull.Common.Logging;
using System;
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
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => 0;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.End;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            string re = null;
            var vl = GlobalContext.CurrentNoLazy?.Option?.TryGetValue("Receipt", out re) ?? false;
            if (!vl || string.Equals(re, "true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var json = JsonHelper.SerializeObject(message);
            var rep = MessagePoster.GetService(ToolsOption.Instance.ReceiptService);
            if (rep == null)
            {
                LogRecorder.Debug($"回执服务未注册,无法处理异常发送结果\r\n{json}");
                return;
            }
            message.OfflineResult(null);
            var receipt = MessageHelper.Simple(message.ID, ToolsOption.Instance.ReceiptService, ToolsOption.Instance.ReceiptApi, json);
            await rep.Post(receipt);
        }
    }
}