namespace ZeroTeam.MessageMVC.PubSub
{
    /// <summary>
    /// 发布节点
    /// </summary>
    public class PublishItem
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 命令
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string Argument { get; set; }
    }
}
