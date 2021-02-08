using BeetleX.Buffers;
using System;
using ZeroTeam.MessageMVC.Messages;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// Http的返回值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectResult : ResultBase
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        public override IHeaderItem ContentType => ContentTypes.JSON;

        /// <summary>
        ///     返回内容
        /// </summary>
        /// <example>true</example>
        public object Message { get; set; }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="response"></param>
        public override void Write(PipeStream stream, HttpResponse response)
        {
            if (Message == null)
                return;
            var bytes =  SmartSerializer.ToString(Message).ToUtf8Bytes();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
