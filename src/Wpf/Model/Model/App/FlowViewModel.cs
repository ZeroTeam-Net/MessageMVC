using Agebull.Common.Mvvm;
using Agebull.EntityModel;
using Agebull.EntityModel.Config;
using System.Threading.Tasks;
using System.Windows;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ZeroApis;

namespace MessageMVC.Wpf.Sample.Model
{
    public class FlowViewModel<TModel> : ViewModelBase<TModel> where TModel : FlowDataModel, new()
    {
        /// <summary>
        /// 流程控制器
        /// </summary>
        public BusinessFlowControl BusinessFlow => BusinessFlowControl.Instance;

        #region 直接命令调用

        /// <summary>
        /// 发送消息命令
        /// </summary>
        public DelegateCommand<string> PostMessage => new DelegateCommand<string>(DoPost);

        /// <summary>
        /// 直接命令调用
        /// </summary>
        /// <param name="api"></param>
        internal void DoPost(string api)
        {
            Model.IsBusy = true;
            Task.Factory.StartNew(async () =>
            {
                var strs = api.Split('/', 2);
                var (res, state) = await MessagePoster.CallAsync<IApiResult>(strs[0], strs[1]);
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(state.ToString());
                    Model.IsBusy = false;
                });
            });
        }
        #endregion


    }
}
