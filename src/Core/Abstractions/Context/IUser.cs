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
        string UserId { get; set; }

        /// <summary>
        ///     用户编码
        /// </summary>
        string OpenId { get; set; }

        /// <summary>
        ///     用户昵称
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        ///     用户组织数字标识
        /// </summary>
        string OrganizationId { get; set; }

        /// <summary>
        ///     用户组织名称
        /// </summary>
        string OrganizationName { get; set; }

        /// <summary>
        ///     取得扩展节点名称
        /// </summary>
        string GetClaim(string name);

        /// <summary>
        /// 通过Json来还原用户
        /// </summary>
        /// <param name="json"></param>
        void FormJson(string json);

        /// <summary>
        /// 序列化为JSON
        /// </summary>
        string ToJson();
    }
}