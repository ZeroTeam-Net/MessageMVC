namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     阻止类型
    /// </summary>
    public enum DenyType
    {
        /// <summary>
        ///     不阻止
        /// </summary>
        None,

        /// <summary>
        ///     有此内容
        /// </summary>
        Hase,

        /// <summary>
        ///     没有此内容
        /// </summary>
        NonHase,

        /// <summary>
        ///     达到数组数量
        /// </summary>
        Count,

        /// <summary>
        ///     内容等于
        /// </summary>
        Equals,

        /// <summary>
        ///     内容包含
        /// </summary>
        Like,

        /// <summary>
        ///     正则匹配
        /// </summary>
        Regex
    }
}