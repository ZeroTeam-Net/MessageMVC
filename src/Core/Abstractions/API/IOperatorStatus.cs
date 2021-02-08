using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     操作状态
    /// </summary>
    public interface IOperatorStatus
    {
        /// <summary>
        /// 请求ID
        /// </summary>
        string RequestId { get; set; }

        /// <summary>
        ///     成功或失败标记
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        ///     状态码（系统定义）
        /// </summary>
        int Code { get; set; }

        /// <summary>
        ///  信息
        /// </summary>
        string Message { get; set; }

        /// <summary>
        ///  异常
        /// </summary>
        Exception Exception { get; set; }

    }
}