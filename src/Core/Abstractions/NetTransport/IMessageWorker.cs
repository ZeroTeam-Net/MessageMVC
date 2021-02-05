namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息处理对象
    /// </summary>
    public interface IMessageWorker : IZeroDependency
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        StationStateType State { get; set; }
    }
}
