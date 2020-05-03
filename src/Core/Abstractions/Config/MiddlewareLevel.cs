namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 中间件优先级预定义常量
    /// </summary>
    public static class MiddlewareLevel
    {
        /// <summary>
        /// 基础级别(1)
        /// </summary>
        public const int Basic = int.MinValue;

        /// <summary>
        /// 框架级别(2)
        /// </summary>
        public const int Framework = -0xFFFFFF; 
            
        /// <summary>
        /// 高级别(3)
        /// </summary>
        public const int Front = -0xFFFF;


        /// <summary>
        /// 普通级别(4)
        /// </summary>
        public const int General = 0;


        /// <summary>
        /// 低级别(5)
        /// </summary>
        public const int Back = 0xFFFF;

        /// <summary>
        /// 收尾级别(6)
        /// </summary>
        public const int Last = 0xFFFFFF;

        /// <summary>
        /// 最后级别(7)
        /// </summary>
        public const int End = int.MaxValue;
    }
}