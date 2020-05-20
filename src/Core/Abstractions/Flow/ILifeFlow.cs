using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 表示一个生命周期流程
    /// </summary>
    public interface ILifeFlow
    {
        /// <summary>
        ///     预检
        /// </summary>
        Task Check(ZeroAppOption config)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///  发现
        /// </summary>
        Task Discover()
        {
            return Task.CompletedTask;
        }

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
        Task Close()
        {
            return Task.CompletedTask;
        }


        /// <summary>
        /// 注销
        /// </summary>
        Task Destory()
        {
            return Task.CompletedTask;
        }
    }
}
