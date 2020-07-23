using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// 跟踪信息
    /// </summary>
    [Category("跟踪信息"), JsonObject(MemberSerialization.OptIn)]
    public class TraceInfo
    {
        /// <summary>
        /// 跟踪信息内容
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TraceInfoType ContentInfo { get; set; }

        /// <summary>
        /// 全局请求标识（源头为用户请求）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TraceId { get; set; }

        /// <summary>
        ///     调用层级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Level { get; set; }

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
        /// 请求的页面(传递)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CallPage { get; set; }

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
        public StaticContext Context { get; set; }

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
        public static TraceInfo New(string id)
        {
            if(ZeroAppOption.Instance.TraceInfo.HasFlag(TraceInfoType.LinkTrace))
                return new TraceInfo
                {
                    TraceId = id,
                    ContentInfo = ZeroAppOption.Instance.TraceInfo,
                    Start = DateTime.Now,
                    LocalId = id,
                    LocalApp = $"{ZeroAppOption.Instance.ShortName ?? ZeroAppOption.Instance.AppName}({ZeroAppOption.Instance.AppVersion})",
                    LocalMachine = $"{ZeroAppOption.Instance.ServiceName}({ZeroAppOption.Instance.LocalIpAddress})"
                };
            return new TraceInfo
            {
                TraceId = id,
                ContentInfo = ZeroAppOption.Instance.TraceInfo,
                Start = DateTime.Now
            };
        }

        /// <summary>
        /// 构造
        /// </summary>
        public TraceInfo Copy()
        {
            return new TraceInfo
            {
                TraceId = TraceId,
                Start = Start,
                CallPage = CallPage,
                LocalId = LocalId,
                LocalApp = LocalApp,
                LocalMachine = LocalMachine,
                CallId = CallId,
                CallApp = CallApp,
                CallMachine = CallMachine,
                Headers = Headers,
                Token = Token,
                Context = Context,
                Level = Level
            };
        }
    }


    /// <summary>
    ///     跟踪上下文(用于序列化)
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StaticContext
    {
        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserJson { get; set; }

        /// <summary>
        /// 上下文配置
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Option { get; set; }

    }
}

