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
		NoSupper = 3,

		/// <summary>
		/// 取消处理
		/// </summary>
		Cancel = 4,

		/// <summary>
		/// 处理异常
		/// </summary>
		Exception = 5,

		/// <summary>
		/// 处理失败
		/// </summary>
		Failed = 6,

		/// <summary>
		/// 处理成功
		/// </summary>
		Success =	7
	}
}
