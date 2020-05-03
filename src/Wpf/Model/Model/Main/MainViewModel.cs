using Agebull.Common.Mvvm;
using Agebull.EntityModel.Config;

namespace MessageMVC.Wpf.Sample.Model
{
    public class MainViewModel : FlowViewModel<MainDataModel>
    {
        #region 返回主面

        /// <summary>
        /// 返回主面
        /// </summary>
        public DelegateCommand HomeCommmand => new DelegateCommand(() => BusinessFlow.FlowData.StepModel = "home");

        /// <summary>
        /// 返回主面
        /// </summary>
        public DelegateCommand Flow1Command => new DelegateCommand(() => BusinessFlow.FlowData.StepModel = "step1");

        #endregion
    }
}
