namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    ///  用户信息
    /// </summary>
    public interface IUser
    {
        /// <summary>
        ///     应用用户数字标识
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

    }

    /// <summary>
    ///  用户信息
    /// </summary>
    public class UserInfo : IUser
    {
        /// <summary>
        ///     应用用户数字标识
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///     用户编码
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        ///     用户昵称
        /// </summary>
        public string NickName { get; set; }

    }
}