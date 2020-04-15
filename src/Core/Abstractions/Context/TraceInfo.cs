using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ZeroTeam.MessageMVC.Context
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
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TraceId { get; set; }

        /// <summary>
        ///     开始时间
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Start { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? End { get; set; }

        /// <summary>
        ///     调用层级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Level { get; set; }


        /// <summary>
        /// 本地的全局标识
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocalId { get; set; }

        /// <summary>
        /// 本地的应用
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocalApp { get; set; }

        /// <summary>
        /// 本地的机器
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocalMachine { get; set; }

        /// <summary>
        /// 请求的全局标识(传递)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CallId { get; set; }

        /// <summary>
        /// 请求应用
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CallApp { get; set; }

        /// <summary>
        /// 请求机器
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CallMachine { get; set; }

        /// <summary>
        /// 上下文信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IZeroContext Context { get; set; }

        /// <summary>
        /// 身份令牌
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Token { get; set; }


        /// <summary>
        ///     请求头信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, List<string>> Headers { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public TraceInfo()
        {
            Context = DependencyHelper.Create<IZeroContext>();//防止反序列化失败
        }

        /// <summary>
        /// 构造
        /// </summary>
        public static TraceInfo New(string id)
        {
            return new TraceInfo
            {
                TraceId = id,
                Start = DateTime.Now,
                LocalId = id,
                LocalApp = $"{ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})",
                LocalMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})"
            };
        }

        /// <summary>
        /// 复制上下文信息
        /// </summary>
        public void CopyFromContext()
        {
            var ctx = GlobalContext.CurrentNoLazy;
            if (ctx != null)
            {
                //远程机器使用,所以Call是本机信息
                CallId = ctx.Trace.LocalId;
                CallApp = ctx.Trace.LocalApp;
                CallMachine = ctx.Trace.LocalMachine;
                //层级
                Level = ctx.Trace.Level + 1;
                //正常复制
                TraceId = ctx.Trace.TraceId;
                Token = ctx.Trace.Token;
                Headers = ctx.Trace.Headers;
            }
            Context = ctx;
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
