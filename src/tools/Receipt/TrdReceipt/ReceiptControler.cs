using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 回执服务
    /// </summary>
    [Service("TrdReceipt")]
    internal class ReceiptControler : IApiController
    {
        /// <summary>
        /// 保存回执
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Route("v1/save")]
        public async Task<IApiResult> Save(InlineMessage message)
        {
            await RedisHelper.SetAsync($"receipt:{message.ID}",GlobalContext.Current.Message.Argument);
            //var file = Path.Combine(ZeroAppOption.Instance.DataFolder, $"{message}.json");
            //await File.AppendAllTextAsync(file, JsonHelper.SerializeObject(message));
            return ApiResultHelper.Succees();
        }
        /// <summary>
        /// 载入回执
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

        /// <summary>
        /// 删除回执
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
