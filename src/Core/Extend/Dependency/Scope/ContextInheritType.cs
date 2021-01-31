namespace Agebull.Common.Ioc
{
    /// <summary>
    /// 上下文继承类型
    /// </summary>
    public enum ContextInheritType
    {
        /// <summary>
        /// 不继承：上下文是独立的
        /// </summary>
        None,
        /// <summary>
        /// 复制：初始内容与当前线程相同，但修改仅在范围内，上一层收不到
        /// </summary>
        Clone,
        /// <summary>
        /// 共享：与当前线程共享上下文
        /// </summary>
        Sharp
    }
}