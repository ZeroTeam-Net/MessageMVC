using Agebull.Common;
using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    [Service("ReceiptCenter")]
    [Route("receipt")]
    internal class ReceiptControler : IApiControler
    {
        [Route("v1/save")]
        public async Task<IApiResult> Save(MessageItem message)
        {
            await RedisHelper.SetAsync($"receipt:{message.ID}", message);
            //var file = Path.Combine(ZeroFlowControl.Config.DataFolder, $"{message}.json");
            //await File.AppendAllTextAsync(file, JsonHelper.SerializeObject(message));
            return ApiResultHelper.Succees();
        }

        [Route("v1/load")]
        public Task<string> Load(string id)
        {
            //var file = Path.Combine(ZeroFlowControl.Config.DataFolder, $"{id}.json");
            //if (!File.Exists(file))
            //    return null;
            //return await File.ReadAllTextAsync(file);

           return RedisHelper.GetAsync($"receipt:{id}");
        }

        [Route("v1/remove")]
        public void Remove(string id)
        {
            //File.Delete(Path.Combine(ZeroFlowControl.Config.DataFolder, $"{id}.json"));
            RedisHelper.DelAsync($"receipt:{id}");
        }
    }
}
