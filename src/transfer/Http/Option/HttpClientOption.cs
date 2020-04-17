namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// HttpClient预定义服务映射配置
    /// </summary>
    internal class HttpClientOption
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 基础地址,包含http://
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 绑定的服务列表,组合结果为 [Url]/[Service]/[ApiName]
        /// </summary>
        public string Services { get; set; }
    }
}
/*
 
        private (MessageState success, string result) Post(string service, string title, string content)
        {
            if (!ServiceMap.TryGetValue(service, out var name))
            {
                name = defName;
            }
            var url = $"{name}/{service}/{title}";
            using (MonitorStepScope.CreateScope("[HttpPoster] {0}", url))
            {
                try
                {
                    var client = httpClientFactory.CreateClient(name);
                    var response = client.PostAsync(url, new StringContent(content ?? "")).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        LogRecorder.MonitorTrace("Error:{0}", response.StatusCode);
                        return (MessageState.NetworkError, null);
                    }
                    var result = response.Content.ReadAsStringAsync().Result;
                    LogRecorder.MonitorTrace(result);
                    return (MessageState.Success, result);
                }
                catch (HttpRequestException ex)
                {
                    LogRecorder.MonitorTrace("Error : {0}", ex.Message);
                    throw new NetTransferException(ex.Message,ex);
                }
            }
        }
        /// <inheritdoc/>
        public string Producer(string service, string title, string content)
        {
            return Post(service, title, content).result;
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string service, string title, TArg content)
        {
            var (success, result) = Post(service, title, JsonHelper.SerializeObject(content));
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }

        /// <inheritdoc/>
        public void Producer<TArg>(string service, string title, TArg content)
        {
            Post(service, title, JsonHelper.SerializeObject(content));
        }
        TRes IMessagePoster.Producer<TRes>(string service, string title)
        {
            var (success, result) = Post(service, title, null);
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }


        async Task<string> IMessagePoster.ProducerAsync(string service, string title, string content)
        {
            var (_, result) = await PostAsync(service, title, content);
            return result;
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string service, string title, TArg content)
        {
            var (success, result) = await PostAsync(service, title, JsonHelper.SerializeObject(content));
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }
        Task IMessagePoster.ProducerAsync<TArg>(string service, string title, TArg content)
        {
            return PostAsync(service, title, string.Empty);
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TRes>(string service, string title)
        {
            var (success, result) = await PostAsync(service, title, string.Empty);
            return !success ? default : JsonHelper.DeserializeObject<TRes>(result);
        }
*/
