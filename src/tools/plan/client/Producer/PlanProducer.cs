using Agebull.Common.Configuration;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Tools;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    /// <summary>
    /// 计划投递对象
    /// </summary>
    public class PlanProducer //: IPlanProducer
    {
        private readonly PlanProducerOption Option = ConfigurationManager.Get<PlanProducerOption>("PlanProducer");

        /// <summary>
        /// 构造
        /// </summary>
        public PlanProducer()
        {
            Option = ConfigurationManager.Get<PlanProducerOption>("MessageMVC:PlanProducer");
            if (string.IsNullOrEmpty(Option.ServiceName))
            {
                Option.ServiceName = Option.ServiceName;
            }

            if (string.IsNullOrEmpty(Option.PostApiName))
            {
                Option.PostApiName = "post";
            }
        }

        #region 计划投送

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public async Task<IOperatorStatus> Post<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            if (ToolsOption.Instance.EnableLinkTrace && GlobalContext.CurrentNoLazy != null)
            {
                option.trace = GlobalContext.CurrentNoLazy.Trace;
            }
            var message = new InlineMessage
            {
                ServiceName = Option.ServiceName,
                ApiName = Option.PostApiName,
                ArgumentData = new PlanCallInfo
                {
                    Option = option,
                    Message = MessageHelper.NewRemote(topic, title, content)
                }
            };
            var (msg, seri) = await MessagePoster.Post(message);
            if(msg.State == MessageState.Success)
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
        public async Task<IOperatorStatus> Post(PlanOption option, string topic, string title, string content)
        {
            if (ToolsOption.Instance.EnableLinkTrace && GlobalContext.CurrentNoLazy != null)
            {
                option.trace = GlobalContext.CurrentNoLazy.Trace;
            }
            var message = new InlineMessage
            {
                ServiceName = Option.ServiceName,
                ApiName = Option.PostApiName,
                ArgumentData = new PlanCallInfo
                {
                    Option = option,
                    Message = MessageHelper.NewRemote(topic, title, content)
                }
            };
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