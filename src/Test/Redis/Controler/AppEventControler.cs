using Agebull.Common.Logging;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.RedisMQ.Sample.Controler;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [NetEvent("AppEvent")]
    public class AppEventControler : IApiControler
    {
        [Route("v1/error")]
        public async Task OnAppError(AppErrorInfo argument)
        {
            var res = await new AliyunSmsSender().Send(new SmsObject
            {
                Mobile = "18812345678",
                Signature = "我的签名",
                TempletKey = "模板ID",
                Info = argument,
                OutId = "OutId"
            });

            LogRecorder.Trace(() => $"State：{res.success} Response：{res.response}");
        }
    }

}
