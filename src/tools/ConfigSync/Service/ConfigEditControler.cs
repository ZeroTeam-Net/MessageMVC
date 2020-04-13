using CSRedis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
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
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            var list = await redis.HKeysAsync(ConfigChangOption.ConfigRedisKey);
            return ApiResultHelper.Succees(list.OrderBy(p => p).ToArray());
        }

        /// <summary>
        /// 读配置节点详情
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/add")]
        public async Task<IApiResult<string[]>> Add(string section)
        {
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            await redis.HSetAsync(ConfigChangOption.ConfigRedisKey, section, "{}");
            var list = await redis.HKeysAsync(ConfigChangOption.ConfigRedisKey);
            return ApiResultHelper.Succees(list.OrderBy(p => p).ToArray());
        }

        /// <summary>
        /// 读配置节点详情
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/details")]
        public async Task<IApiResult<string>> Details(string section)
        {
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            var json = await redis.HGetAsync(ConfigChangOption.ConfigRedisKey, section);
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
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            await redis.HDelAsync(ConfigChangOption.ConfigRedisKey, section);
            var list = await redis.HKeysAsync(ConfigChangOption.ConfigRedisKey);
            return ApiResultHelper.Succees(list.OrderBy(p => p).ToArray());
        }

        /// <summary>
        /// 更新配置节点
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>配置内容</returns>
        [Route("v1/update")]
        public async Task<IApiResult> Update(ConfigChangedArgument argument)
        {
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            if (argument.Type == "section")
            {
                await redis.HSetAsync(ConfigChangOption.ConfigRedisKey, argument.Section, argument.Value);
            }
            else
            {
                var json = await redis.HGetAsync(ConfigChangOption.ConfigRedisKey, argument.Section) ?? "{}";
                var obj = (JObject)JsonConvert.DeserializeObject(json);
                obj[argument.Key] = argument.Value;
                await redis.HSetAsync(ConfigChangOption.ConfigRedisKey, argument.Section, JsonConvert.SerializeObject(obj));
            }
            await MessagePoster.PublishAsync("ConfigSync", "v1/changed", argument);

            return ApiResultHelper.Succees();
        }
    }
}
