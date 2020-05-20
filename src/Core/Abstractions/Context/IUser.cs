using Newtonsoft.Json;
using System.ComponentModel;
using ZeroTeam.MessageMVC.Messages;

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
        long OrganizationId { get; set; }

        /// <summary>
        ///     用户组织名称
        /// </summary>
        string OrganizationName { get; set; }

        /// <summary>
        /// 通过Json来还原用户
        /// </summary>
        /// <param name="json"></param>
        void FormJson(string json);
    }

    /// <summary>
    ///  用户信息
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserInfo : IUser
    {
        /// <summary>
        /// 系统用户标识
        /// </summary>
        public const long SystemUserId = 0;
        /// <summary>
        /// 系统组织标识
        /// </summary>
        public const long SystemOrganizationId = 0;

        /// <summary>
        /// 未知用户标识
        /// </summary>
        public const long UnknownUserId = -1;
        /// <summary>
        /// 未知组织标识
        /// </summary>
        public const long UnknownOrganizationId = -1;
        /// <summary>
        ///     应用用户数字标识
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(UnknownUserId)]
        public long UserId { get; set; } = UnknownUserId;

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
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(UnknownOrganizationId)]
        public long OrganizationId { get; set; } = UnknownOrganizationId;

        /// <summary>
        ///     用户组织名称
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string OrganizationName { get; set; }


        /// <summary>
        /// 通过Json来还原用户
        /// </summary>
        /// <param name="json"></param>
        void IUser.FormJson(string json)
        {
            if (SmartSerializer.TryDeserialize(json, GetType(), out var dest) && dest is UserInfo user)
            {
                UserId = user.UserId;
                UserCode = user.UserCode;
                NickName = user.NickName;
                OrganizationId = user.OrganizationId;
                OrganizationName = user.OrganizationName;
            }
        }
    }
}