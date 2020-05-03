using Agebull.Common.Logging;
using MessageMVC.Wpf.Sample.Model;
using System.Threading.Tasks;
using System.Windows;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ZeroApis;

namespace Agebull.EntityModel.Config
{
    public class Step1DataModel : FlowDataModel
    {
        public void Next()
        {
            IsBusy = true;
            Task.Factory.StartNew(async () =>
            {
                Logger.Information("正在调用控制器(ViewFlow/v1/step1)...");
                var (res, state) = await MessagePoster.CallAsync<IApiResult<string>>(
                    "ViewFlow", "v1/step1");
                Logger.Information($"调用状态为{state}");
                Dispatcher.Invoke(() =>
                {
                    if (state != ZeroTeam.MessageMVC.Messages.MessageState.Success)
                    {
                        MessageBox.Show(state.ToString());
                    }
                    else
                    {
                        BusinessFlowControl.Instance.FlowData.StepModel = res.ResultData;
                    }

                    IsBusy = false;
                });
            });
        }
    }
}