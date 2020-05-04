using Agebull.EntityModel.Common;
using CSRedis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    [Service("ConfigEdit")]
    internal class ConfigEditControler : IApiController
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

        const string clientKey = "MessageMVC:HttpClient";
        const string posterKey = "MessageMVC:MessagePoster";

        /// <summary>
        /// 服务注册
        /// </summary>
        /// <returns>所有配置项</returns>
        [Route("v1/regist")]
        public async Task<IApiResult> ServiceRegist(List<NameValue> services)
        {
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            var posters = await redis.HGetAsync<Dictionary<string, string>>(ConfigChangOption.ConfigRedisKey, posterKey)
                ?? new Dictionary<string, string>();
            foreach (var service in services.Where(p => !string.IsNullOrEmpty(p.Value)))
            {
                if (posters.TryGetValue(service.Name, out var s) && !string.IsNullOrEmpty(s))
                {
                    var ses = s.Split(',').ToList();
                    ses.AddRange(service.Value.Split(','));

                    posters[service.Name] = string.Join(",", ses.Distinct());
                }
                else
                {
                    posters[service.Name] = service.Value;
                }
            }
            var json = posters.ToJson(true);
            await redis.HSetAsync(ConfigChangOption.ConfigRedisKey, posterKey, json);

            await MessagePoster.PublishAsync("ConfigSync", "v1/changed", new ConfigChangedArgument
            {
                Key = posterKey,
                Type = "section",
                Section = posterKey,
                Value = json
            });
            return ApiResultHelper.Succees();
        }
        /// <summary>
        /// 服务注册
        /// </summary>
        /// <returns>所有配置项</returns>
        [Route("v1/http")]
        public async Task<IApiResult> HttpRegist(List<HttpClientItem> services)
        {
            using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
            var clientItems = await redis.HGetAsync<List<HttpClientItem>>(ConfigChangOption.ConfigRedisKey, clientKey);
            if (clientItems == null || clientItems.Count == 0)
                clientItems = services;
            else
                foreach (var service in services)
                {
                    var old = clientItems.FirstOrDefault(p => p.Services != null && p.Services.Contains(service.Services));
                    if (old != null)
                    {
                        old.Url = service.Url;
                    }
                    else
                    {
                        clientItems.Add(service);
                    }
                }

            var json = clientItems.ToJson(true);
            await redis.HSetAsync(ConfigChangOption.ConfigRedisKey, clientKey, json);

            await MessagePoster.PublishAsync("ConfigSync", "v1/changed", new ConfigChangedArgument
            {
                Key = clientKey,
                Type = "section",
                Section = clientKey,
                Value = json
            });
            return ApiResultHelper.Succees();
        }
    }
}
