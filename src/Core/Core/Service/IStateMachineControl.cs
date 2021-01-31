using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 受状态机控制的对象
    /// </summary>
    public interface IStateMachineControl
    {
        /// <summary>
        /// 系统启动时调用
        /// </summary>
        Task<bool> DoStart();

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        Task<bool> DoClosing();

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        Task<bool> DoClose();

        /// <summary>
        /// 注销时调用
        /// </summary>
        Task<bool> DoEnd();
    }
}