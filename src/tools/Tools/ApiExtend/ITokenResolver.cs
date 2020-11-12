namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// 令牌解析器
    /// </summary>
    public interface ITokenResolver
    {
        /// <summary>
        /// 令牌解析为用户
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        IUser TokenToUser(string token);
    }
}