using Agebull.Common.Ioc;
using Agebull.Common.Logging;
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
        ///     确定后的当前权限信息
        /// </summary>
        public string CurrentPermission
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.CurrentPermission, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.CurrentPermission] = value;
        }

        /// <summary>
        ///     所有权限信息
        /// </summary>
        [JsonIgnore]
        public string Permissions
        {
            get => claims.TryGetValue(ZeroTeamJwtClaim.Permissions, out var val) ? val : null;
            set => claims[ZeroTeamJwtClaim.Permissions] = value;
        }

        /// <summary>
        /// JWT信息字典
        /// </summary>
        [JsonIgnore]
        protected readonly Dictionary<string, string> claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
        /// 通过Json来还原用户
        /// </summary>
        /// <param name="json"></param>
        public void FormJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            claims.Clear();
            try
            {
                var dest = SmartSerializer.FromInnerString<Dictionary<string, string>>(json);
                if (dest != null)
                {
                    foreach (var claim in dest)
                    {
                        claims.TryAdd(claim.Key, claim.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                DependencyRun.Logger.Exception(ex, "UserInfo.FormJson\n{ex}", ex);
            }
        }

        /// <summary>
        /// 序列化为JSON
        /// </summary>
        public string ToJson()
        {
            return SmartSerializer.ToInnerString(claims);
        }
    }
}