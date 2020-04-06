﻿using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.Services;

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
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.End;

        /// <summary>
        /// 结果处理
        /// </summary>
        /// <param name="message">当前消息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnEnd(IMessageItem message)
        {
            return MessagePoster.PublishAsync(ToolsOption.Instance.MarkPointName, message.Topic, message);
        }
    }
}