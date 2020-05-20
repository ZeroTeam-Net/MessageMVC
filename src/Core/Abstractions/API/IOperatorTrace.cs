namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API接口跟踪
    /// </summary>
    public interface IOperatorTrace
    {
        /// <summary>
        ///     API请求标识
        /// </summary>
        /// <example>AxV6389FC</example>
        string RequestId { get; set; }

        /// <summary>
        ///     错误点
        /// </summary>
        /// <remarks>
        /// 系统在哪一个节点发生错误的标识
        /// </remarks>
        /// <example>http-gateway</example>
        string Point { get; set; }

        /// <summary>
        ///     指导码
        /// </summary>
        /// <remarks>
        /// 内部使用:指示下一步应如何处理的代码
        /// </remarks>
        /// <example>retry</example>
        string Guide { get; set; }

        /// <summary>
        ///     错误说明
        /// </summary>
        /// <remarks>
        /// 内部使用:详细说明错误内容
        /// </remarks>
        /// <example>系统未就绪</example>
        string Describe { get; set; }
    }
}