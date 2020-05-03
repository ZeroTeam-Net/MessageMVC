using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel;
using Microsoft.Extensions.Logging;

namespace MessageMVC.Wpf.Sample.Model
{
    /// <summary>
    /// 应用数据
    /// </summary>
    public class BusinessFlowData : NotificationObject
    {
        protected ILogger logger;

        protected ILogger Logger => logger ??= DependencyHelper.LoggerFactory.CreateLogger(nameof(BusinessFlowData));

        private string title = "MessageMVC WPF 示例";

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                if (title == value)
                {
                    return;
                }

                title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }

        #region 数据模板驱动

        private string stepModel = "home";

        public string StepModel
        {
            get => stepModel;
            set
            {
                if (stepModel == value)
                {
                    return;
                }

                stepModel = value;
                RaisePropertyChanged(nameof(StepModel));
                Logger.Information($"流程步骤调整为 {value}");
            }
        }
        #endregion

        #region 卡片模拟

        private string cardInfo;
        public string CardInfo
        {
            get => cardInfo;
            set
            {
                if (cardInfo == value)
                {
                    return;
                }

                cardInfo = value;
                RaisePropertyChanged(nameof(CardInfo));
            }
        }


        internal void CardPush(string id)
        {
            CardInfo = $"卡片{id}已插入";
            StepModel = "push";
        }


        internal void CardPull(string id)
        {
            CardInfo = $"卡片{id}已拨出";
            StepModel = "pull";
        }
        #endregion

    }
}
