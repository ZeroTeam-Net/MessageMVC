using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Tools;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 计划投送对象
    /// </summary>
    public static class PlanPoster
    {
        #region 计划投送

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IOperatorStatus Post<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            return PostToService(option, topic, title, content).Result;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static IOperatorStatus Post(PlanOption option, string topic, string title, string content)
        {
            return PostToService(option, topic, title, content).Result;
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<IOperatorStatus> PostAsync<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            return PostToService(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<IOperatorStatus> PostAsync(PlanOption option, string topic, string title, string content)
        {
            return PostToService(option, topic, title, content);
        }

        #endregion


        #region 计划投送

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        static async Task<IOperatorStatus> PostToService<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            if (ToolsOption.Instance.EnableLinkTrace && GlobalContext.CurrentNoLazy != null)
            {
                option.trace = GlobalContext.CurrentNoLazy?.Trace;
            }
            var message = new InlineMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                ServiceName = PlanServiceOption.Instance.ServiceName,
                ApiName = PlanServiceOption.Instance.PostApiName,
                ArgumentData = new PlanCallInfo
                {
                    Option = option,
                    Message = MessageHelper.NewRemote(topic, title, content)
                }
            };
            option.plan_id = message.ID;
            var (msg, seri) = await MessagePoster.Post(message);
            if (msg.State == MessageState.Success)
            {
                return ApiResultHelper.Succees();
            }
            else
            {
                return msg.RuntimeStatus;
            }
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        static async Task<IOperatorStatus> PostToService(PlanOption option, string topic, string title, string content)
        {
            if (ToolsOption.Instance.EnableLinkTrace && GlobalContext.CurrentNoLazy != null)
            {
                option.trace = GlobalContext.CurrentNoLazy?.Trace;
            }
            var message = new InlineMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                ServiceName = PlanServiceOption.Instance.ServiceName,
                ApiName = PlanServiceOption.Instance.PostApiName,
                ArgumentData = new PlanCallInfo
                {
                    Option = option,
                    Message = MessageHelper.NewRemote(topic, title, content)
                }
            };
            option.plan_id = message.ID;
            var (msg, seri) = await MessagePoster.Post(message);
            if (msg.State == MessageState.Success)
            {
                return ApiResultHelper.Succees();
            }
            else
            {
                return msg.RuntimeStatus;
            }
        }
        #endregion
    }
}
