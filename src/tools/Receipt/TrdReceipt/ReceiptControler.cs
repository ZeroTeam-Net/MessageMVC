
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    [Service("TrdReceipt")]
    internal class ReceiptControler : IApiControler
    {
        [Route("v1/save")]
        public async Task<IApiResult> Save(InlineMessage message)
        {
            await RedisHelper.SetAsync($"receipt:{message.ID}", message);
            //var file = Path.Combine(ZeroAppOption.Instance.DataFolder, $"{message}.json");
            //await File.AppendAllTextAsync(file, JsonHelper.SerializeObject(message));
            return ApiResultHelper.Succees();
        }
        /// <summary>
        /// 载入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("v1/load")]
        public async Task<IApiResult<InlineMessage>> Load(string id)
        {
            //var file = Path.Combine(ZeroAppOption.Instance.DataFolder, $"{id}.json");
            //if (!File.Exists(file))
            //    return null;
            //return await File.ReadAllTextAsync(file);
            var message = await RedisHelper.GetAsync<InlineMessage>($"receipt:{id}");
            return message != null
                ? ApiResultHelper.Succees(message)
                : ApiResultHelper.State<InlineMessage>(OperatorStatusCode.ArgumentError);
        }

        [Route("v1/remove")]
        public async Task<IApiResult> Remove(string id)
        {
            //File.Delete(Path.Combine(ZeroAppOption.Instance.DataFolder, $"{id}.json"));
            var ab = await RedisHelper.DelAsync($"receipt:{id}");
            return ab == 1
                ? ApiResultHelper.Succees()
                : ApiResultHelper.State(OperatorStatusCode.ArgumentError);
        }
    }
}
