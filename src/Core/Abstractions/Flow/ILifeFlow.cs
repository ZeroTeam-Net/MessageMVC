using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 表示应用检查器
    /// </summary>
    public interface IAppChecker
    {
        /// <summary>
        ///     预检
        /// </summary>
        Task Check(ZeroAppOption config)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 表示一个起始流程
    /// </summary>
    public interface IZeroDiscover
    {
        /// <summary>
        ///  发现
        /// </summary>
        Task Discovery()
        {
            return Task.CompletedTask;
        }
    }
    /// <summary>
    /// 表示一个生命周期流程
    /// </summary>
    public interface ILifeFlow
    {
        /// <summary>
        ///     初始化
        /// </summary>
        Task Initialize()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 启动
        /// </summary>
        Task Open()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task Closing()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task Close()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 注销
        /// </summary>
        Task Destroy()
        {
            return Task.CompletedTask;
        }
    }
}
