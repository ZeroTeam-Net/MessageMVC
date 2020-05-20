using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.AddIn
{
    /// <summary>
    /// 生自注册对象
    /// </summary>
    public interface IAutoRegister : ILifeFlow
    {
        /// <summary>
        /// 执行自动注册
        /// </summary>
        /// <returns>返回false表示后续无操作</returns>
        Task<bool> AutoRegist(IServiceCollection service) { return Task.FromResult(false); }

    }
}