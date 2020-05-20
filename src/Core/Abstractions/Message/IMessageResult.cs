using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 标准消息返回
    /// </summary>
    public interface IMessageResult
    {

        /// <summary>
        /// 标识
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 处理结果,对应状态的解释信息
        /// </summary>
        /// <remarks>
        /// 未消费:无内容
        /// 已接受:无内容
        /// 格式错误 : 无内容
        /// 无处理方法 : 无内容
        /// 处理异常 : 异常信息
        /// 处理失败 : 失败内容或原因
        /// 处理成功 : 结果信息或无
        /// </remarks>
        string Result { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        MessageState State { get; set; }

        /// <summary>
        ///     跟踪信息
        /// </summary>
        TraceInfo Trace { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ResultData { get; set; }

        /// <summary>
        /// 数据状态
        /// </summary>
        public MessageDataState DataState { get; set; }

    }
}
