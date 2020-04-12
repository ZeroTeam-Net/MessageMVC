using Agebull.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     Api调用器
    /// </summary>
    public class ApiExecuter : IMessageMiddleware
    {
        #region 对象

        /// <summary>
        /// 当前处理器
        /// </summary>
        public MessageProcessor Processor { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => short.MaxValue;

        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Handle;

        /// <summary>
        /// 当前站点
        /// </summary>
        internal IService Service;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal IInlineMessage Message;

        /// <summary>
        /// 调用的内容
        /// </summary>
        internal object Tag;

        #endregion

        #region IMessageMiddleware

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag"></param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        public async Task Handle(IService service, IInlineMessage message, object tag, Func<Task> next)
        {
            Service = service;
            Message = message;
            Tag = tag;
            var action = Service.GetApiAction(Message.Title);
            //1 查找调用方法
            if (action == null)
            {
                LogRecorder.Trace("错误: 接口({0})不存在", Message.Title);
                Message.RuntimeStatus = ApiResultHelper.Error(DefaultErrorCode.NoFind);
                Message.State = MessageState.NoSupper;
                if (next != null)
                {
                    await next();
                }
                return;
            }
            //2 确定调用方法及对应权限
            if (!ZeroAppOption.Instance.IsOpenAccess &&
                (!action.Access.AnyFlags(ApiAccessOption.Anymouse) || action.Access.AnyFlags(ApiAccessOption.Authority))
                && (GlobalContext.User == null || GlobalContext.User.UserId <= UserInfo.SystemOrganizationId))
            {
                LogRecorder.Trace("错误: 需要用户登录信息");
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.DenyAccess, "错误: 需要用户令牌");
                }
                Message.State = MessageState.NoSupper;
                if (next != null)
                {
                    await next();
                }
                return;
            }
            //参数处理
            if (!ArgumentPrepare(action))
            {
                if (next != null)
                {
                    await next();
                }
                return;
            }
            try
            {
                //方法执行
                var (state, result) = await action.Execute(Message, Service.Serialize);
                Message.State = state;
                Message.ResultData = result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (MessageBusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MessageBusinessException($"接口方法({message.ServiceName}/{message.ApiName}) 执行出错.", ex);
            }

            if (next != null)
            {
                await next();
            }
        }

        #endregion

        #region CommandPrepare

        /// <summary>
        ///    参数校验
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool ArgumentPrepare(IApiAction action)
        {
            //还原参数
            Message.Inline(action.ArgumentSerializer ?? Service.Serialize,
                action.Access.HasFlag(ApiAccessOption.DictionaryArgument) ? null : action.ArgumentType,
                action.ResultSerializer,
                ApiResultHelper.Error);


            //3 参数校验
            if (action.Access.HasFlag(ApiAccessOption.DictionaryArgument))
            {
                return true;
            }

            try
            {
                if (!action.RestoreArgument(Message))
                {
                    LogRecorder.Trace("错误 : 无法还原参数");
                    if (action.IsApiContract)
                    {
                        Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, "错误 : 无法还原参数");
                    }
                    Message.State = MessageState.FormalError;
                    return false;
                }
            }
            catch (Exception ex)
            {
                var msg = $"错误 : 还原参数异常{ex.Message}";
                LogRecorder.Trace(msg);
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, msg);
                }
                Message.State = MessageState.FormalError;
                return false;
            }

            try
            {
                if (action.ValidateArgument(Message, out string info))
                {
                    return true;
                }
                LogRecorder.Trace("参数校验失败 : {0}", info);
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, info);
                }

                Message.State = MessageState.FormalError;
                return false;
            }
            catch (Exception ex)
            {
                var msg = $"错误 : 参数校验异常{ex.Message}";
                LogRecorder.Trace(msg);
                if (action.IsApiContract)
                {
                    Message.ResultData = ApiResultHelper.Error(DefaultErrorCode.ArgumentError, msg);
                }
                Message.State = MessageState.FormalError;
                return false;
            }
        }

        #endregion
    }
}