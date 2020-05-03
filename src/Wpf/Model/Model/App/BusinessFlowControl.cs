using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel;
using Microsoft.Extensions.Logging;

namespace MessageMVC.Wpf.Sample.Model
{
    /// <summary>
    /// 业务流程控制器
    /// </summary>
    public class BusinessFlowControl : NotificationObject
    {
        protected ILogger logger;

        protected ILogger Logger => logger ??= DependencyHelper.LoggerFactory.CreateLogger(nameof(BusinessFlowControl));

        /// <summary>
        /// 实例
        /// </summary>
        public static BusinessFlowControl Instance { get; } = new BusinessFlowControl();

        internal void CardPush(string id)
        {
            FlowData.CardInfo = $"卡片{id}已插入";
            Logger.Information("卡片状态已更新");
            // FlowData.StepModel = "push";
        }

        internal void CardPull(string id)
        {
            FlowData.CardInfo = $"卡片{id}已拨出";
            Logger.Information("卡片状态已更新");
            //FlowData.StepModel = "pull";
        }

        public MainViewModel ViewModel;

        public BusinessFlowData FlowData { get; } = new BusinessFlowData();

    }
}
