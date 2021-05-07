namespace BeetleX.Zeroteam.WebSocketBus
{
    /// <summary>
    /// 消息组
    /// </summary>
    public class MessageRoomOption
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分组字段
        /// </summary>
        public string[] Group { get; set; }

        /// <summary>
        /// 过滤组织边界字段
        /// </summary>
        public string BoundaryCode { get; set; }
    }
}
