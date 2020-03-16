namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// Api处理器
    /// </summary>
    public interface IApiHandler
    {
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="station"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        void Prepare(ZeroService station, ApiCallItem item);

        /// <summary>
        /// 结束处理
        /// </summary>
        /// <param name="station"></param>
        /// <param name="item"></param>
        void End(ZeroService station, ApiCallItem item);
    }
}