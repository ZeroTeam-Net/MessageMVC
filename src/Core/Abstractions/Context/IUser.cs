using Newtonsoft.Json;
using System.ComponentModel;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///  用户信息
    /// </summary>
    public interface IUser
    {
        /// <summary>
        ///    用户数字标识
        /// </summary>
        long UserId { get; set; }

        /// <summary>
        ///     用户编码
        /// </summary>
        string UserCode { get; set; }

        /// <summary>
        ///     用户昵称
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        ///     用户组织数字标识
        /// </summary>
        long Organization { get; set; }

    }

    /// <summary>
    ///  用户信息
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserInfo : IUser
    {
        /// <summary>
        ///     应用用户数字标识
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate),DefaultValue(-1)]
        public long UserId { get; set; }

        /// <summary>
        ///     用户编码
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string UserCode { get; set; }

        /// <summary>
        ///     用户昵称
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue("Anymouse")]
        public string NickName { get; set; }

        /// <summary>
        ///     用户组织数字标识
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long Organization { get; set; }

    }
}