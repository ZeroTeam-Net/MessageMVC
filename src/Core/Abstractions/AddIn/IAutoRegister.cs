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
        Task AutoRegist(IServiceCollection service);

    }
}