using Microsoft.Extensions.DependencyInjection;

namespace ZeroTeam.MessageMVC.AddIn
{
    /// <summary>
    /// 生自注册对象
    /// </summary>
    public interface IAutoRegister
    {
        /// <summary>
        /// 执行自动注册
        /// </summary>
        void AutoRegist(IServiceCollection service) { }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize() { }


        /// <summary>
        /// 开始
        /// </summary>
        void Start() { }

        /// <summary>
        /// 结束
        /// </summary>
        void End() { }

    }
}