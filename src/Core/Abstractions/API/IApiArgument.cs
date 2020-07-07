namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     表示API请求参数
    /// </summary>
    public interface IApiArgument
    {
        /// <summary>
        ///     数据校验
        /// </summary>
        /// <param name="status">返回的状态</param>
        /// <returns>成功则返回真</returns>
        bool Validate(out IOperatorStatus status);
    }
}