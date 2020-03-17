namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示消息状态
    /// </summary>
    public enum MessageState
	{
		/// <summary>
		/// 未消费
		/// </summary>
		None = 0,
		/// <summary>
		/// 已接受
		/// </summary>
		Accept = 	1,
		/// <summary>
		/// 格式错误
		/// </summary>
		FormalError = 2,
		/// <summary>
		/// 无处理方法
		/// </summary>
		NoAction = 3,

		/// <summary>
		/// 处理异常
		/// </summary>
		Exception = 4,
		/// <summary>
		/// 处理失败
		/// </summary>
		Failed = 5,

		/// <summary>
		/// 处理成功
		/// </summary>
		Success =	7
	}
}
