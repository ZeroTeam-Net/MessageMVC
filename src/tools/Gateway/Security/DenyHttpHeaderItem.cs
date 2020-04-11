namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     阻止节点
    /// </summary>
    public class DenyHttpHeaderItem
    {
        /// <summary>
        ///     内容
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        public DenyType DenyType { get; set; }
    }
}