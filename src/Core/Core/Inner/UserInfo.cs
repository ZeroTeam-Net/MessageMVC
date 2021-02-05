using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///  用户信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserInfo : IUser
    {
        /// <summary>
        /// 是否一个有效用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsLoginUser(IUser user)
        {
            return !string.IsNullOrEmpty(user?.UserId)
                && user.UserId != ZeroTeamJwtClaim.UnknownUserId
                && user.UserId != ZeroTeamJwtClaim.SystemUserId;
        }

        /// <summary>
        /// 是否一个有效用户
        /// </summary>
        /// <returns></returns>
        public bool IsLogin()
        {
            return !string.IsNullOrEmpty(UserId)
                && UserId != ZeroTeamJwtClaim.UnknownUserId
                && UserId != ZeroTeamJwtClaim.SystemUserId;
        }

        /// <summary>
        ///     应用用户数字标识
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string UserId
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.AppUserId, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.AppUserId] = value;
        }

        /// <summary>
        ///     用户昵称
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string NickName
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.GivenName, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.GivenName] = value;
        }

        /// <summary>
        ///     用户组织数字标识
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string OrganizationId
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.OrganizationId, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.OrganizationId] = value;
        }

        /// <summary>
        ///     确定后的当前权限信息
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string CurrentPermission
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.CurrentPermission, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.CurrentPermission] = value;
        }

        /// <summary>
        ///     所有权限信息
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string Permissions
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.Permissions, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.Permissions] = value;
        }

        /// <summary>
        /// JWT信息字典
        /// </summary>
        protected Dictionary<string, string> claims = new Dictionary<string, string>();

        /// <summary>
        /// 快捷读写字典
        /// </summary>
        [JsonProperty("claims"), JsonPropertyName("claims")]
        public Dictionary<string, string> Claims
        {
            get
            {
                return claims;
            }
            set
            {
                if (value == null || value.Count == 0)
                    claims.Clear();
                foreach (var kv in value)
                    claims[kv.Key] = kv.Value;
            }
        }

        /// <summary>
        /// 快捷读写字典
        /// </summary>
        /// <param name="type"></param>
        public string this[string type]
        {
            get
            {
                return claims.TryGetValue(type, out var value) ? value : null;
            }
            set
            {
                if (!string.IsNullOrEmpty(type))
                    claims[type] = value;
            }
        }

        /// <summary>
        /// 设置节点名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void SetClaim(string type, string value)
        {
            if (!string.IsNullOrEmpty(type))
                claims[type] = value;
        }

        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        Dictionary<string, string> IDictionaryTransfer.ToDictionary()
        {
            return claims.Count == 0 ? null : claims;
        }
        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        void IDictionaryTransfer.Reset(Dictionary<string, string> dict)
        {
            claims = dict ?? new Dictionary<string, string>();
        }
    }
}