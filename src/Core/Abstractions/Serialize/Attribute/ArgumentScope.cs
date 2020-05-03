namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 序列化类型
    /// </summary>
    public enum ArgumentScope
    {
        /// <summary>
        /// 内容
        /// </summary>
        Content,
        /// <summary>
        /// HTTP的URL的参数
        /// </summary>
        HttpArgument,
        /// <summary>
        /// HTTP的Form
        /// </summary>
        HttpForm,
        /// <summary>
        /// 取字典内容
        /// </summary>
        Dictionary = HttpForm
    }
}