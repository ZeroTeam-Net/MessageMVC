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
        void CheckOption(ZeroAppOption config) { }


        /// <summary>
        ///     初始化
        /// </summary>
        void Initialize() { }


        /// <summary>
        /// 开启
        /// </summary>
        void Start()
        {
        }

        /// <summary>
        /// 关闭
        /// </summary>
        void Close() { }


        /// <summary>
        /// 注销时调用
        /// </summary>
        void End() { }

    }
}
