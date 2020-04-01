using Agebull.Common.Configuration;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    /// <summary>
    /// 计划任务配置
    /// </summary>
    public class PlanProducerOption
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 投递接口名称
        /// </summary>
        public string PostApiName { get; set; }
    }

    /// <summary>
    /// 计划投递对象
    /// </summary>
    public class PlanProducer : IPlanProducer
    {
        /// <summary>
        /// 构造
        /// </summary>
        public PlanProducer()
        {
            Option = ConfigurationManager.Get<PlanProducerOption>("PlanProducer");
            if (string.IsNullOrEmpty(Option.ServiceName))
            {
                Option.ServiceName = Option.ServiceName;
            }

            if (string.IsNullOrEmpty(Option.PostApiName))
            {
                Option.PostApiName = "post";
            }
        }

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; }

        private readonly PlanProducerOption Option = ConfigurationManager.Get<PlanProducerOption>("PlanProducer");

        #region 计划投送

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public ApiResult Plan<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            return MessageProducer.Producer<PlanCallInfo, ApiResult>(Option.ServiceName, Option.PostApiName, new PlanCallInfo
            {
                Option = option,
                Message = new MessageItem
                {
                    Topic = topic,
                    Title = title,
                    Content = JsonHelper.SerializeObject(content),
                    Context = JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy)
                }
            });
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public ApiResult Plan(PlanOption option, string topic, string title, string content)
        {
            return MessageProducer.Producer<PlanCallInfo, ApiResult>(Option.ServiceName, Option.PostApiName,
                new PlanCallInfo
                {
                    Option = option,
                    Message = new MessageItem
                    {
                        Topic = topic,
                        Title = title,
                        Content = content,
                        Context = JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy)
                    }
                });
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public Task<ApiResult> PlanAsync<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            return MessageProducer.ProducerAsync<PlanCallInfo, ApiResult>(Option.ServiceName, Option.PostApiName,
                new PlanCallInfo
                {
                    Option = option,
                    Message = new MessageItem
                    {
                        Topic = topic,
                        Title = title,
                        Content = JsonHelper.SerializeObject(content),
                        Context = JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy)
                    }
                });
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public Task<ApiResult> PlanAsync(PlanOption option, string topic, string title, string content)
        {
            return MessageProducer.ProducerAsync<PlanCallInfo, ApiResult>(Option.ServiceName, Option.PostApiName,
                new PlanCallInfo
                {
                    Option = option,
                    Message = new MessageItem
                    {
                        Topic = topic,
                        Title = title,
                        Content = content,
                        Context = JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy)
                    }
                });
        }

        #endregion
    }

}