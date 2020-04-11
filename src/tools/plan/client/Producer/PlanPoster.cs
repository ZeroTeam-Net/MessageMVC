using System.Threading.Tasks;
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
        private static readonly PlanProducer PlanProducer = new PlanProducer();

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
            return PlanProducer.Post(option, topic, title, content).Result;
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
            return PlanProducer.Post(option, topic, title, content).Result;
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
            return PlanProducer.Post(option, topic, title, content);
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
            return PlanProducer.Post(option, topic, title, content);
        }

        #endregion
    }
}
