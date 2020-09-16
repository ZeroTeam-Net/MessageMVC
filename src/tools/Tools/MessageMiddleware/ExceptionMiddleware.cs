using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// 反向代理中间件
    /// </summary>
    public class ExceptionMiddleware : IMessageMiddleware
    {
        /// <summary>
        /// 代表资源相关的异常
        /// </summary>
        public static Dictionary<Type, NameValue<MessageState>> ResourceExceptions = new Dictionary<Type, NameValue<MessageState>>
        {
            { typeof(DbException),new NameValue<MessageState>("数据异常",MessageState.FrameworkError)},
            { typeof(IOException),new NameValue<MessageState>("文件异常",MessageState.FrameworkError)},
            { typeof(OperationCanceledException),new NameValue<MessageState>("操作取消",MessageState.Cancel)},
            { typeof(ThreadInterruptedException),new NameValue<MessageState>("操作取消",MessageState.Cancel)},
            { typeof(ArgumentException),new NameValue<MessageState>("过程参数错误",MessageState.FormalError)},
            { typeof(MessageBusinessException),new NameValue<MessageState>("业务异常",MessageState.BusinessError)},
            { typeof(MessagePostException),new NameValue<MessageState>("网络异常",MessageState.NetworkError)},
            { typeof(SystemException),new NameValue<MessageState>("系统异常",MessageState.FrameworkError)},
        };

        /// <summary>
        /// 注册代表资源相关的异常
        /// </summary>
        public static void RegistResourceException<TException>(string resource, MessageState state) where TException : Exception
        {
            ResourceExceptions.TryAdd(typeof(TException), new NameValue<MessageState>(resource, state));
        }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => MiddlewareLevel.General;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Exception;

        /// <summary>
        /// 全局异常发生时
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="exception">异常信息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        Task IMessageMiddleware.OnGlobalException(IService service, IInlineMessage message, Exception exception, object tag)
        {
            message.DataState |= MessageDataState.ResultOffline;
            var ex = ResourceExceptions.FirstOrDefault(p => exception.GetType().IsSubclassOf(p.Key));
            if (ex.Value != null)
            {
                message.Result = ex.Value.Name;
                message.RealState = ex.Value.Value;
            }
            else
            {
                message.Result = "*内部错误*";
                
                message.RealState = MessageState.FrameworkError;
            }
            return Task.CompletedTask;
        }
    }
}