using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    [Service("ConfigEdit")]
    internal class ConfigEditControler : IApiControler
    {
        /// <summary>
        /// 读配置节点列表
        /// </summary>
        /// <returns>所有配置项</returns>
        [Route("v1/list")]
        public async Task<IApiResult<string[]>> Sections()
        {
            var list = await RedisHelper.HKeysAsync(ConfigChangOption.ConfigRedisKey );
            return ApiResultHelper.Succees(list);
        }

        /// <summary>
        /// 读配置节点详情
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/add")]
        public async Task<IApiResult<string[]>> Add(string section)
        {
            await RedisHelper.HSetAsync(ConfigChangOption.ConfigRedisKey , section, "{}");
            var list = await RedisHelper.HKeysAsync(ConfigChangOption.ConfigRedisKey );
            return ApiResultHelper.Succees(list);
        }

        /// <summary>
        /// 读配置节点详情
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/details")]
        public async Task<IApiResult<string>> Details(string section)
        {
            var json = await RedisHelper.HGetAsync(ConfigChangOption.ConfigRedisKey , section);
            return ApiResultHelper.Succees(json ?? "{}");
        }

        /// <summary>
        /// 删除配置节点
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/del")]
        public async Task<IApiResult<string[]>> Delete(string section)
        {
            await RedisHelper.HDelAsync(ConfigChangOption.ConfigRedisKey , section);
            var list = await RedisHelper.HKeysAsync(ConfigChangOption.ConfigRedisKey );
            return ApiResultHelper.Succees(list);
        }

        /// <summary>
        /// 更新配置节点
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/update")]
        public async Task<IApiResult> Update(ConfigChangedArgument argument)
        {
            if (argument.Type == "section")
            {
                await RedisHelper.HSetAsync(ConfigChangOption.ConfigRedisKey , argument.Section, argument.Value);
            }
            else
            {
                var json = await RedisHelper.HGetAsync(ConfigChangOption.ConfigRedisKey , argument.Section) ?? "{}";
                var obj = (JObject)JsonConvert.DeserializeObject(json);
                obj[argument.Key] = argument.Value;
                await RedisHelper.HSetAsync(ConfigChangOption.ConfigRedisKey , argument.Section, JsonConvert.SerializeObject(obj));
            }
            await MessagePoster.PublishAsync("ConfigSync", "v1/changed", argument);

            return ApiResultHelper.Succees();
        }
    }
}
