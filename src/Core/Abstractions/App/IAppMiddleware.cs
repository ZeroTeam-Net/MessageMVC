using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 表示一个应用中间件
    /// </summary>
    public interface IAppMiddleware
    {
        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        void CheckOption(ZeroAppConfigRuntime config) { }


        /// <summary>
        ///     初始化
        /// </summary>
        void Initialize() { }


        /// <summary>
        /// 开启
        /// </summary>
        Task Start()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void Close() { }


        /// <summary>
        /// 注销时调用
        /// </summary>
        void Dispose() { }

    }
}
