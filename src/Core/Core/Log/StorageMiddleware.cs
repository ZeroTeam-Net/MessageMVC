﻿using Agebull.Common.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 消息存储中间件
    /// </summary>
    public class StorageMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => -1;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        async Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            Save(message);
            var state = await next();
            State(message);
            return state;
        }

        void Save(IMessageItem message)
        {
            var file = Path.Combine(ZeroFlowControl.Config.DataFolder, "message", $"{message.ID}.msg");
            if (!File.Exists(file))
                File.WriteAllText(file, JsonHelper.SerializeObject(message));
            message.State = MessageState.Accept;
        }

        void State(IMessageItem message)
        {
            if (message.State != MessageState.Success)
                return;
            File.Delete(Path.Combine(ZeroFlowControl.Config.DataFolder,"message", $"{message.ID}.msg"));
        }
    }
}