using Agebull.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Services;

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
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => -1;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Handle;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task> next)
        {
            Save(message);
            await next();
            State(message);
        }

        private void Save(IMessageItem message)
        {
            try
            {
                var file = Path.Combine(ZeroAppOption.Instance.DataFolder, "message", $"{message.ID}.msg");
                if (!File.Exists(file))
                {
                    File.WriteAllText(file, JsonHelper.SerializeObject(message));
                }

                message.State = MessageState.Accept;
            }
            catch
            {
            }
        }

        private void State(IMessageItem message)
        {
            if (message.State != MessageState.Success)
            {
                return;
            }
            try
            {
                File.Delete(Path.Combine(ZeroAppOption.Instance.DataFolder, "message", $"{message.ID}.msg"));
            }
            catch
            {
            }
        }
    }
}