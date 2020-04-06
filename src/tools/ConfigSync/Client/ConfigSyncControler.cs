using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    [NetEvent("ConfigSync")]
    public class ConfigSyncControler : IApiControler
    {
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="argument">参数</param>
        [Route("v1/changed")]
        public async Task OnChanged(ConfigChangedArgument argument)
        {
            var file = Path.Combine(ZeroAppOption.Instance.ConfigFolder,"sync", $"{argument.Section}.json");
            if (!File.Exists(file))//本地不需要
                return;
            //string json;
            //if (argument.Type == "section")
            //{
            //    json = await RedisHelper.HGetAsync(ConfigChangOption.ConfigRedisKey , argument.Section);
            //}
            //else
            //{
            //    json = await File.ReadAllTextAsync(file) ?? "{}";
            //    var obj = (JObject)JsonConvert.DeserializeObject(json);
            //    if (!obj.ContainsKey(argument.Key))//本地不需要
            //        return;
            //    obj[argument.Key] = argument.Value;
            //    json = JsonConvert.SerializeObject(obj);
            //}

            var json = await RedisHelper.HGetAsync(ConfigChangOption.ConfigRedisKey, argument.Section);
            //写入文件,更新留给Core自己处理
            File.WriteAllText(file, json ?? "{}", Encoding.UTF8);
        }
    }
}
