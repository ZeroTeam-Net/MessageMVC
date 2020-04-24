using System;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 表示一个流程中间件
    /// </summary>
    public interface IFlowMiddleware : IZeroMiddleware
    {
        /// <summary>
        ///     配置校验
        /// </summary>
        void CheckOption(ZeroAppOption config)
        {
            Console.WriteLine($"{GetType().GetTypeName()} >>> CheckOption");
        }


        /// <summary>
        ///     初始化
        /// </summary>
        void Initialize()
        {
            Console.WriteLine($"{GetType().GetTypeName()} >>> Initialize");
        }


        /// <summary>
        /// 开启
        /// </summary>
        void Start()
        {
            Console.WriteLine($"{GetType().GetTypeName()} >>> Start");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void Close()
        {
            Console.WriteLine($"{GetType().GetTypeName()} >>> End");
        }


        /// <summary>
        /// 注销时调用
        /// </summary>
        void End()
        {
            Console.WriteLine($"{GetType().GetTypeName()} >>> End");
        }

    }
}
