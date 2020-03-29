namespace ZeroTeam.MessageMVC.Sample
{
    /// <summary>
    /// 订单实时状态
    /// </summary>
    public enum OrderRealState
    {
        /// <summary>
        /// 未确定
        /// </summary>
        None,

        /// <summary>
        /// 正确
        /// </summary>
        Ok,

        /// <summary>
        /// 不适用
        /// </summary>
        NotApply,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 因不能叠加等原因被禁用
        /// </summary>
        Disable
    }
}