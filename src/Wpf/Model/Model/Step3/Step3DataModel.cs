using Agebull.Common.Logging;
using MessageMVC.Wpf.Sample.Model;
using System.Threading.Tasks;
using System.Windows;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.ZeroApis;

namespace Agebull.EntityModel.Config
{
    public class Step3DataModel : FlowDataModel
    {
        public void Next()
        {
            IsBusy = true;
            Task.Factory.StartNew(async () =>
            {
                {
                    Logger.Information("正在请求弹出卡片...");
                    var state = await MessagePoster.PublishAsync("CardDevices", "v1/cmd/pull", new Argument
                    {
                        Value = "88888888"
                    });
                    Logger.Information($"调用状态为{state}");
                    Dispatcher.Invoke(() =>
                    {
                        if (state != ZeroTeam.MessageMVC.Messages.MessageState.Success)
                        {
                            MessageBox.Show("卡片未正常弹出,请联系工作人员(系统已通知工作人员)");
                        }
                        else
                        {
                            MessageBox.Show("卡片已弹出,请收好卡片");
                        }
                    });

                }
                {
                    Logger.Information("正在调用控制器(ViewFlow/v1/step3)...");
                    var (res, state) = await MessagePoster.CallAsync<IApiResult<string>>("ViewFlow", "v1/step3");
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
                    });
                    Logger.Information($"调用状态为{state}");
                }
                IsBusy = false;
            });
        }
    }
}