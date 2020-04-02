using Agebull.Common.Ioc;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
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
        /// 计划生产者
        /// </summary>
        private static IPlanProducer PlanProducer;
        private static ZeroAppOption appOption;

        private static ZeroAppOption AppOption => appOption ??= IocHelper.Create<ZeroAppOption>();

        /// <summary>
        /// 发现传输对象
        /// </summary>
        /// <returns>传输对象构造器</returns>
        private static IPlanProducer PlanService()
        {
            return PlanProducer ??= IocHelper.Create<IPlanProducer>() ?? new PlanProducer();
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static ApiResult Post<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().Post(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static ApiResult Post(PlanOption option, string topic, string title, string content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().Post(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<ApiResult> PostAsync<TArg>(PlanOption option, string topic, string title, TArg content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().PostAsync(option, topic, title, content);
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="option">计划配置</param>
        /// <param name="topic">消息分类</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public static Task<ApiResult> PostAsync(PlanOption option, string topic, string title, string content)
        {
            if (AppOption.EnableGlobalContext)
            {
                option.request_id = GlobalContext.Current.Request.RequestId;
                option.caller = AppOption.AppName;
                option.service = AppOption.ServiceName;
            }
            return PlanService().PostAsync(option, topic, title, content);
        }

        #endregion
    }
}
