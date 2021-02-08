using BeetleX.Buffers;
using System;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// 纯文本返回内容
    /// </summary>
    public class StringResult : ResultBase
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        public override IHeaderItem ContentType => ContentTypes.JSON;

        /// <summary>
        ///     返回内容
        /// </summary>
        /// <example>true</example>
        public string Message { get; set; }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="response"></param>
        public override void Write(PipeStream stream, HttpResponse response)
        {
            if (Message == null)
                return;
            var bytes = Message.ToUtf8Bytes();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
