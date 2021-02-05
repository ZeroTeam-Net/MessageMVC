using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    ///     全局上下文
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ZeroContextEx : ZeroContext
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ZeroContextEx()
        {
            User = DependencyHelper.GetService<IUser>();//防止反序列化失败
        }

        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IUser User { get; set; }

        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        public override Dictionary<string, string> ToTransfer() => Option;
    }
}