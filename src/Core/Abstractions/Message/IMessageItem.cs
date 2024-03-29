﻿using System.Collections.Generic;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 标准消息交互格式
    /// </summary>
    public interface IMessageItem
    {

        /// <summary>
        /// 标识
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 处理结果,对应状态的解释信息
        /// </summary>
        /// <remarks>
        /// 未消费:无内容
        /// 已接受:无内容
        /// 格式错误 : 无内容
        /// 无处理方法 : 无内容
        /// 处理异常 : 异常信息
        /// 处理失败 : 失败内容或原因
        /// 处理成功 : 结果信息或无
        /// </remarks>
        string Result { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        MessageState State { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        TraceInfo TraceInfo { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        string Service { get; set; }

        /// <summary>
        /// 方法
        /// </summary>

        string Method { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        string Argument { get; set; }

        /// <summary>
        /// 扩展信息（固定为字典）
        /// </summary>
        string Extension { get; set; }

        /// <summary>
        /// 上下文信息
        /// </summary>
        Dictionary<string, string> Context { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        Dictionary<string, string> User { get; set; }

        /*// <summary>
        /// 生产者信息
        /// </summary>
        string ProducerInfo { get; set; }

        /// <summary>
        ///     文件内容二进制数据
        /// </summary>
        byte[] Bytes { get; set; }*/
    }
}
