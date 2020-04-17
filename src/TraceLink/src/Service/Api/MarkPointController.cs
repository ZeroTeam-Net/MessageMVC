#region
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
using ZeroTeam.MessageMVC.ZeroApis;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.WebApi
{
    /// <summary>
    ///  消息存储
    /// </summary>
    [Consumer("MarkPoint")]
    public partial class MarkPointController : IApiControler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("post")]
        public async Task OnPost(TraceLinkMessage linkMessage)
        {
            var trace = linkMessage.Trace;
            linkMessage.Message.Trace = null;
            if (trace == null)
            {
                return;
            }

            var access = new MessageDataAccess();
            await access.InsertAsync(new MessageData
            {
                TraceId = trace.TraceId,
                ApiName = $"{linkMessage.Message.Topic}/{linkMessage.Message.Title}",
                Start = trace.Start ?? DateTime.Now,
                End = trace.End ?? DateTime.Now,
                LocalId = trace.LocalId,
                LocalApp = trace.LocalApp,
                LocalMachine = trace.LocalMachine,
                CallId = trace.CallId,
                CallApp = trace.CallApp,
                CallMachine = trace.CallMachine,
                Context = trace.Context.ToJson(),
                Token = trace.Token,
                Level = trace.Level,
                Headers = trace.Headers.ToJson(),
                Message = linkMessage.Message.ToJson(),
                FlowStep = linkMessage.Root.ToJson()
            });

        }
    }
}