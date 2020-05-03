namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 序列化类型
    /// </summary>
    public enum SerializeType
    {
        /// <summary>
        /// 不需要
        /// </summary>
        None,
        /// <summary>
        /// JSON
        /// </summary>
        Json,
        /// <summary>
        /// Newtonsoft的JSON
        /// </summary>
        NewtonJson,
        /// <summary>
        /// XML
        /// </summary>
        Xml,
        /// <summary>
        /// gRPC的BSON
        /// </summary>
        Bson,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }
}