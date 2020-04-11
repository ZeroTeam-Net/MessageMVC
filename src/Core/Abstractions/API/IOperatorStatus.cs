using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     操作状态
    /// </summary>
    public interface IOperatorStatus
    {
        /// <summary>
        ///  异常
        /// </summary>
        [JsonIgnore,IgnoreDataMember]
        Exception Exception { get; set; }

        /// <summary>
        ///     成功或失败标记
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        ///     错误码（系统定义）
        /// </summary>
        int Code { get; set; }

        /// <summary>
        ///  信息
        /// </summary>
        string Message { get; set; }

    }
}