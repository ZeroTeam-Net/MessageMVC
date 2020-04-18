using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class ZeroArgument
    {
        #region Properties

        /// <summary>
        /// 当前消息
        /// </summary>
        internal IInlineMessage Message;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal StationConfig Config;

        #endregion


    }
}