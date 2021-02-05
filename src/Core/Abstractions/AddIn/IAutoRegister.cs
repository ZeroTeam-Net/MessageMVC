using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ZeroTeam.MessageMVC.AddIn
{
    /// <summary>
    /// 生自注册对象
    /// </summary>
    public interface IAutoRegister
    {
        /// <summary>
        /// 执行自动注册(配置载入前)
        /// </summary>
        /// <returns>返回false表示后续无操作</returns>
        void AutoRegist(IServiceCollection service, ILogger logger) { }

        /// <summary>
        /// 执行自动注册(配置载入后)
        /// </summary>
        void LateConfigRegist(IServiceCollection services, ILogger logger) { }
    }
}