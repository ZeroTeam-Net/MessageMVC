#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

using Agebull.Common;
using Agebull.Common.Ioc;

using Agebull.EntityModel.Common;
using Agebull.EntityModel.EasyUI;
using ZeroTeam.MessageMVC.ZeroApis;
using Agebull.MicroZero.ZeroApis;



using ZeroTeam.MessageMVC.MessageTraceLink;
using ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
using ZeroTeam.MessageMVC.Context;

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
                return;
            var access = new MessageDataAccess();
            await access.InsertAsync(new MessageData
            {
                TraceId = trace.TraceId,
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

    /// <summary>
    /// 跟踪消息
    /// </summary>
    public class TraceLinkMessage
    {
        /// <summary>
        /// 跟踪
        /// </summary>
        public TraceInfo Trace { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public MessageItem Message { get; set; }

        /// <summary>
        /// 本地跟踪
        /// </summary>
        public TraceStep Root { get; set; }
    }
}