using Agebull.Common.Logging;
using BeetleX;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Tcp
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    internal sealed class TcpWriter : IMessageWriter
    {
        internal ISession Session { get; set; }

        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageWriter.OnResult(IInlineMessage message, object _)
        {
            try
            {
                if(Session.IsDisposed)
                    return Task.FromResult(false);
                var pipeStream = Session.Stream.ToPipeStream();
                var json = SmartSerializer.ToInnerString(message.ToMessageResult(true));
                var len = pipeStream.WriteLine(json);
                Session.Stream.Flush();
            }
            catch (Exception ex)
            {
                FlowTracer.MonitorError(() => ex.ToString());

            }
            return Task.FromResult(true);
        }
    }
}