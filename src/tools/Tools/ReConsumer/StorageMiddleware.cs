using Agebull.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.Tools;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息存储中间件
    /// </summary>
    public class StorageMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 当前处理器
        /// </summary>
        MessageProcessor IMessageMiddleware.Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => 0xFFFFFF;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope =>
            ToolsOption.Instance.EnableMessageReConsumer
            ? MessageHandleScope.Prepare | MessageHandleScope.End
            : MessageHandleScope.None;

       static readonly string path = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "message");

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        async Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            try
            {
                var file = Path.Combine(path, $"{message.ID}.msg");
                if (!File.Exists(file))
                {
                    await File.WriteAllTextAsync(file, JsonHelper.SerializeObject(message));
                }
            }
            catch
            {
            }
            return true;
        }

        /// <summary>
        /// 处理结束时(结果交付Service后)
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IInlineMessage message)
        {
            if (message.State != MessageState.Success)
            {
                return Task.CompletedTask;
            }
            try
            {
                File.Delete(Path.Combine(path, $"{message.ID}.msg"));
            }
            catch
            {
            }
            return Task.CompletedTask;
        }
    }
}