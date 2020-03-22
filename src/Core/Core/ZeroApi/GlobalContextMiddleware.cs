using System;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 上下文对象
    /// </summary>
    public class GlobalContextMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => -1;

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        Task<MessageState> IMessageMiddleware.Handle(IService service, IMessageItem message, object tag, Func<Task<MessageState>> next)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(message.Context))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<GlobalContext>(message.Context));
                }
                else
                {
                    GlobalContext.SetEmpty();
                }
            }
            catch (Exception e)
            {
                //LogRecorder.Trace(()=> "Restory context exception:{e.Message}");
                //ZeroTrace.WriteException(service.ServiceName, e, message.Title, "restory context", message.Context);
                //message.Result = ApiResultIoc.ArgumentErrorJson;
                //message.State = MessageState.FormalError;
                //return Task.FromResult(MessageState.FormalError);

                GlobalContext.SetEmpty();
            }
            return next();
        }
    }
}
