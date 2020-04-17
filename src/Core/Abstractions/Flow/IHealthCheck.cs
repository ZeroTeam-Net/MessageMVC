using ZeroTeam.MessageMVC.ZeroApis;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 表示一个应用中间件
    /// </summary>
    public interface IHealthCheck : IZeroDependency
    {
        /// <summary>
        /// 执行健康检查
        /// </summary>
        Task<HealthInfo> Check();
    }
}
