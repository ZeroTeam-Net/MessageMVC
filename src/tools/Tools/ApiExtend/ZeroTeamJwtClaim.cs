namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// JWT兼容定义
    /// </summary>
    public static class ZeroTeamJwtClaim
    {
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-5
        /// </summary>
        public const string Actort = "actort";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-5
        /// </summary>
        public const string Typ = "typ";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Sub = "sub";
        /// <summary>
        /// http://openid.net/specs/openid-connect-frontchannel-1_0.html#OPLogout
        /// </summary>
        public const string Sid = "sid";
        /// <summary>
        /// http://openid.net/specs/openid-connect-frontchannel-1_0.html#OPLogout
        /// </summary>
        public const string Prn = "prn";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Nbf = "nbf";
        /// <summary>
        ///  https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
        /// </summary>
        public const string Nonce = "nonce";
        /// <summary>
        ///  https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
        /// </summary>
        public const string NameId = "nameid";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Jti = "jti";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Iss = "iss";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Iat = "iat";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        /// </summary>
        public const string GivenName = "given_name";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        /// </summary>
        public const string FamilyName = "family_name";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        /// </summary>
        public const string Gender = "gender";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Exp = "exp";
        /// <summary>
        /// https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        /// </summary>
        public const string Email = "email";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#CodeIDToken
        /// </summary>
        public const string AtHash = "at_hash";
        /// <summary>
        /// https://openid.net/specs/openid-connect-core-1_0.html#HybridIDToken
        /// </summary>
        public const string CHash = "c_hash";
        /// <summary>
        /// https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        /// </summary>
        public const string Birthdate = "birthdate";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#IDToken
        /// </summary>
        public const string Azp = "azp";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#IDToken
        /// </summary>
        public const string AuthTime = "auth_time";
        /// <summary>
        /// http://tools.ietf.org/html/rfc7519#section-4
        /// </summary>
        public const string Aud = "aud";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#IDToken
        /// </summary>
        public const string Amr = "amr";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#IDToken
        /// </summary>
        public const string Acr = "acr";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#IDToken
        /// </summary>
        public const string UniqueName = "unique_name";
        /// <summary>
        /// http://openid.net/specs/openid-connect-core-1_0.html#IDToken
        /// </summary>
        public const string Website = "website";

        /// <summary>
        /// 设备类型
        /// </summary>
        public const string TokenType = "z_type";
        /// <summary>
        /// 登录类型
        /// </summary>
        public const string LoginType = "z_type";
        /// <summary>
        /// 登录令牌
        /// </summary>
        public const string LoginToken = "z_ltoken";
        /// <summary>
        /// 全局用户ID
        /// </summary>
        public const string GlobalUserId = "z_guid";
        /// <summary>
        /// 当前应用用户ID
        /// </summary>
        public const string AppUserId = "z_auid";
        /// <summary>
        /// OpenId
        /// </summary>
        public const string OpenId = "z_opid";
        /// <summary>
        /// 头像
        /// </summary>
        public const string AvatarUrl = "z_ava";
        /// <summary>
        /// 登录使用的账号
        /// </summary>
        public const string Account = "z_acc";
        /// <summary>
        /// 手机号
        /// </summary>
        public const string MobilePhone = "z_phone";
        /// <summary>
        /// 设备标识
        /// </summary>
        public const string DeviceId = "z_did";
        /// <summary>
        /// 组织名称
        /// </summary>
        public const string Organization = "z_org";
        /// <summary>
        /// 组织ID
        /// </summary>
        public const string OrganizationId = "z_oids";

        /// <summary>
        /// 当前权限
        /// </summary>
        public const string CurrentPermission = "z_cur_per";
        /// <summary>
        /// 权限信息
        /// </summary>
        public const string Permissions = "z_all_pers";
        /// <summary>
        /// 追踪码
        /// </summary>
        public const string TraceMark = "z_mark";
        /// <summary>
        /// 设备信息
        /// </summary>
        public const string DeviceInfo = "z_dev";

        /// <summary>
        /// 系统用户标识
        /// </summary>
        public const string SystemUserId = "0";
        /// <summary>
        /// 系统组织标识
        /// </summary>
        public const string SystemOrganizationId = "0";

        /// <summary>
        /// 未知用户标识
        /// </summary>
        public const string UnknownUserId = "-1";
        /// <summary>
        /// 未知组织标识
        /// </summary>
        public const string UnknownOrganizationId = "-1";
    }
}