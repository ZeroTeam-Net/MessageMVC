using BeetleX.Buffers;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// 标准消息返回
    /// </summary>
    public class MessageResult : ResultBase
    {
        /// <summary>
        ///     返回内容
        /// </summary>
        /// <example>true</example>
        public IInlineMessage Message { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public override IHeaderItem ContentType => ContentTypes.JSON;

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="response"></param>
        public override void Write(PipeStream stream, HttpResponse response)
        {
            byte[] bytes;
            if (Message.DataState.HasFlag(MessageDataState.ResultInline))
            {
                if (Message.ResultData == null)
                    return;// bytes = "OK".ToUtf8Bytes();
                else
                    bytes = SmartSerializer.ToString(Message.ResultData).ToUtf8Bytes();
            }
            else if (Message.DataState.HasFlag(MessageDataState.ResultOffline))
            {
                if (Message.Result.IsBlank())
                    return;//  bytes = "OK".ToUtf8Bytes();
                else
                    bytes = Message.Result.ToUtf8Bytes();
            }
            else
            {
                return;//  bytes = "OK".ToUtf8Bytes();
            }
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
