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
        ///     用户昵称
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        ///     用户组织数字标识
        /// </summary>
        string OrganizationId { get; set; }

        /// <summary>
        ///     确定后的当前权限信息
        /// </summary>
        string CurrentPermission { get; set; }

        /// <summary>
        ///     角色集合
        /// </summary>
        string Permissions { get; set; }
        
        /// <summary>
        /// 通过Json来还原用户
        /// </summary>
        /// <param name="json"></param>
        void FormJson(string json);

        /// <summary>
        /// 序列化为JSON
        /// </summary>
        string ToJson();

        /// <summary>
        /// 快捷读写字典
        /// </summary>
        /// <param name="type"></param>
        string this[string type] { get;set; }
    }
}