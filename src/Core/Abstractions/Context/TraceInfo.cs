using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 跟踪信息
    /// </summary>
    [DataContract, Category("跟踪信息"), JsonObject(MemberSerialization.OptIn)]
    public class TraceInfo
    {
        /// <summary>
        /// 全局请求标识（源头为用户请求）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string TraceId { get; set; }

        /// <summary>
        /// 生产时间戳,UNIX时间戳,自1970起秒数
        /// </summary>
        public int LocalTimestamp { get; set; }

        /// <summary>
        /// 本地的全局标识
        /// </summary>
        public string LocalId { get; set; }

        /// <summary>
        /// 本地的应用
        /// </summary>
        public string LocalApp { get; set; }

        /// <summary>
        /// 本地的机器
        /// </summary>
        public string LocalMachine { get; set; }

        /// <summary>
        /// 生产时间戳,UNIX时间戳,自1970起秒数
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int CallTimestamp { get; set; }

        /// <summary>
        /// 请求的全局标识(传递)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string CallId { get; set; }

        /// <summary>
        /// 请求应用
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string CallApp { get; set; }

        /// <summary>
        /// 请求机器
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string CallMachine { get; set; }

        /// <summary>
        /// 上下文信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string ContextJson { get; set; }

        /// <summary>
        /// 身份令牌
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }


        /// <summary>
        ///     请求头信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, List<string>> Headers { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        /// <summary>
        /// 请求端口号
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int Port { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public static TraceInfo New(string id)
        {
            return new TraceInfo
            {
                TraceId = id,
                LocalId = id,
                LocalTimestamp = DateTime.Now.ToTimestamp(),
                LocalApp = $"{ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})",
                LocalMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})"
            };
        }
    }
}

/*
 * 
        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("argumentType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ArgumentType ArgumentType { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("requestType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        [JsonProperty("ip", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        /// <summary>
        /// 请求端口号
        /// </summary>
        [JsonProperty("port", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Port { get; set; }
        /// <summary>
        ///     当前用户登录到哪个系统（预先定义的系统标识）
        /// </summary>
        [JsonProperty("app", NullValueHandling = NullValueHandling.Ignore)]
        public string App { get; set; }

        /// <summary>
        /// HTTP的UserAgent
        /// </summary>
        [JsonProperty("userAgent", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string UserAgent { get; set; }


        /// <summary>
        ///     登录设备的操作系统
        /// </summary>
        [JsonProperty("os", NullValueHandling = NullValueHandling.Ignore)]
        public string Os { get; set; }
 
*/
