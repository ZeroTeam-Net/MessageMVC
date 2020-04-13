using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 配置同步控制器
    /// </summary>
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
            if (argument.Type == "section")
            {
                await ConfigHelper.SaveToFile(argument.Section);
            }
            //else
            //{
            //    var sections = argument.Section.Split(':', '.');
            //    var key = sections[^1];


            //    var file = Path.Combine(ZeroAppOption.Instance.ConfigFolder, "sync", $"{string.Join('.', sections)}.json");
            //    if (!File.Exists(file))//本地不需要
            //        return;
            //    var json = await File.ReadAllTextAsync(file) ?? "{}";
            //    var obj = (JObject)JsonConvert.DeserializeObject(json);
            //    if (!obj.ContainsKey(argument.Key))//本地不需要
            //        return;
            //    obj[argument.Key] = argument.Value;
            //    json = JsonConvert.SerializeObject(obj);
            //}

        }
    }
}
