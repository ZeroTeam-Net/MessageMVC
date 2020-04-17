#region
using System;
using ZeroTeam.MessageMVC.Context;
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
        public void OnPost(TraceLinkMessage linkMessage)
        {
            Console.WriteLine(linkMessage.Trace.TraceId);
        }
    }

}