namespace ZeroTeam.MessageMVC
{

    /// <summary>
    /// 表示一个应用中间件
    /// </summary>
    public interface IZeroMiddleware : IZeroDependency
    {
        /// <summary>
        /// 等级,用于确定中间件优先级
        /// </summary>
        int Level { get; }

    }
}
