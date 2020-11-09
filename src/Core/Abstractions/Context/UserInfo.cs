using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///  用户信息
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
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
        ///     应用用户数字标识
        /// </summary>
        [JsonIgnore]
        public string UserId
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.AppUserId, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.AppUserId] = value;
        }

        /// <summary>
        ///     用户编码
        /// </summary>
        [JsonIgnore]
        public string OpenId
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.AppUserId, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.AppUserId] = value;
        }

        /// <summary>
        ///     用户昵称
        /// </summary>
        [JsonIgnore]
        public string NickName
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.GivenName, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.GivenName] = value;
        }

        /// <summary>
        ///     用户组织数字标识
        /// </summary>
        [JsonIgnore]
        public string OrganizationId
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.OrganizationId, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.OrganizationId] = value;
        }

        /// <summary>
        ///     用户组织名称
        /// </summary>
        [JsonIgnore]
        public string OrganizationName
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.Organization, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.Organization] = value;
        }

        [JsonIgnore]
        readonly Dictionary<string, string> claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 设置信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetClaim(string key, string value)
        {
            claims.TryAdd(key, value);
        }

        /// <summary>
        /// 通过Json来还原用户
        /// </summary>
        /// <param name="json"></param>
        public void FormJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            claims.Clear();
            var dest = SmartSerializer.FromInnerString<Dictionary<string, string>>(json);
            if (dest != null)
            {
                foreach (var claim in dest)
                {
                    claims.TryAdd(claim.Key, claim.Value);
                }
            }
        }

        /// <summary>
        ///     取得扩展节点名称
        /// </summary>
        string IUser.GetClaim(string name) => claims.TryGetValue(name, out var val) ? val : null;

        /// <summary>
        /// 序列化为JSON
        /// </summary>
        public string ToJson()
        {
            return SmartSerializer.ToInnerString(claims);
        }
    }
}