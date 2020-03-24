using System.Threading.Tasks;
using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    ///     ZMQ生产者
    /// </summary>
    internal class ZmqProducer : IMessageProducer
    {
        #region Properties

        /// <summary>
        /// 实例
        /// </summary>
        public static ZmqProducer Instance = new ZmqProducer();

        /// <summary>
        /// 构造
        /// </summary>
        public ZmqProducer()
        {
            Instance = this;
        }

        /// <summary>
        ///     调用器
        /// </summary>
        private readonly ZmqCaller _core = new ZmqCaller();

        /// <summary>
        ///     返回值
        /// </summary>
        public byte ResultType => _core.ResultType;

        /// <summary>
        ///     返回值
        /// </summary>
        public byte[] Binary => _core.Binary;


        /// <summary>
        ///     返回值
        /// </summary>
        public string Result => _core.Result;

        /// <summary>
        ///     请求站点
        /// </summary>
        public string Station { get => _core.Station; set => _core.Station = value; }

        /// <summary>
        ///     上下文内容（透传方式）
        /// </summary>
        public string ContextJson { get => _core.ContextJson; set => _core.ContextJson = value; }

        /// <summary>
        ///     标题
        /// </summary>
        public string Title { get => _core.Title; set => _core.Title = value; }

        /// <summary>
        ///     调用命令
        /// </summary>
        public string Commmand { get => _core.Commmand; set => _core.Commmand = value; }

        /// <summary>
        ///     参数
        /// </summary>
        public string Argument { get => _core.Argument; set => _core.Argument = value; }

        /// <summary>
        ///     扩展参数
        /// </summary>
        public string ExtendArgument { get => _core.ExtendArgument; set => _core.ExtendArgument = value; }

        /// <summary>
        ///     结果状态
        /// </summary>
        public ZeroOperatorStateType State => _core.State;

        /// <summary>
        /// 最后一个返回值
        /// </summary>
        public ZeroResult LastResult => _core.LastResult;

        /// <summary>
        /// 简单调用
        /// </summary>
        /// <remarks>
        /// 1 不获取全局标识(过时）
        /// 2 无远程定向路由,
        /// 3 无上下文信息
        /// </remarks>
        public bool Simple { set; get; }

        #endregion

        #region 流程


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        public void CallCommand()
        {
            _core.Call();
        }


        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        public void CheckStateResult()
        {
            _core.CheckStateResult();
        }

        #endregion

        #region async


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        public Task CallCommandAsync()
        {
            return _core.CallAsync();
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static async Task<string> CallAsync(string station, string commmand, string argument)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = commmand,
                Argument = argument
            };
            await client.CallCommandAsync();
            client._core.CheckStateResult();
            return client.Result;
        }


        #endregion

        #region 快捷方法

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static IApiResult<TResult> CallApi<TArgument, TResult>(string station, string api, TArgument arg)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static IApiResult CallApi<TArgument>(string station, string api, TArgument arg)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult<TResult> CallApi<TResult>(string station, string api)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult CallApi(string station, string api)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static TResult Call<TArgument, TResult>(string station, string api, TArgument arg)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static TResult Call<TResult>(string station, string api)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        #endregion

        #region 快捷方法Async

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static async Task<IApiResult<TResult>> CallApiAsync<TArgument, TResult>(string station, string api, TArgument arg)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            await client.CallCommandAsync();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static async Task<IApiResult> CallApiAsync<TArgument>(string station, string api, TArgument arg)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            await client.CallCommandAsync();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static async Task<IApiResult<TResult>> CallApiAsync<TResult>(string station, string api)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api
            };
            await client.CallCommandAsync();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static async Task<IApiResult> CallApiAsync(string station, string api)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api
            };
            await client.CallCommandAsync();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static async Task<TResult> CallAsync<TArgument, TResult>(string station, string api, TArgument arg)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            await client.CallCommandAsync();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static async Task<TResult> CallAsync<TResult>(string station, string api)
        {
            var client = new ZmqProducer
            {
                Station = station,
                Commmand = api
            };
            await client.CallCommandAsync();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }


        #endregion

        #region IMessageProducer

        IApiResult IMessageProducer.Producer(string topic, string title, string content)
        {
            using (MonitorScope.CreateScope($"Call: {topic}/{title}"))
            {
                var client = new ZmqProducer
                {
                    Station = topic,
                    Commmand = title,
                    Argument = content
                };
                client.CallCommand();
                if (client.State != ZeroOperatorStateType.Ok)
                {
                    client.CheckStateResult();
                }
                return string.IsNullOrEmpty(client.Result)
                    ? ApiResultIoc.Ioc.Succees()
                    : ApiResultIoc.Ioc.DeserializeObject<ApiResult>(client.Result);
            }
        }

        IApiResult IMessageProducer.Producer<T>(string topic, string title, T content)
        {
            using (MonitorScope.CreateScope($"Call: {topic}/{title}"))
            {
                var client = new ZmqProducer
                {
                    Station = topic,
                    Commmand = title,
                    Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
                };
                client.CallCommand();
                if (client.State != ZeroOperatorStateType.Ok)
                {
                    client.CheckStateResult();
                }
                return string.IsNullOrEmpty(client.Result)
                    ? ApiResultIoc.Ioc.Succees()
                    : ApiResultIoc.Ioc.DeserializeObject<ApiResult>(client.Result);
            }
        }

        async Task<IApiResult> IMessageProducer.ProducerAsync(string topic, string title, string content)
        {
            using (MonitorScope.CreateScope($"Call: {topic}/{title}"))
            {
                var client = new ZmqProducer
                {
                    Station = topic,
                    Commmand = title,
                    Argument = content
                };
                await client.CallCommandAsync();
                if (client.State != ZeroOperatorStateType.Ok)
                {
                    client.CheckStateResult();
                }
                return string.IsNullOrEmpty(client.Result)
                    ? ApiResultIoc.Ioc.Succees()
                    : JsonHelper.DeserializeObject<ApiResult>(client.Result);
            }
        }

        async Task<IApiResult> IMessageProducer.ProducerAsync<T>(string topic, string title, T content)
        {
            using (MonitorScope.CreateScope($"Call: {topic}/{title}"))
            {
                var client = new ZmqProducer
                {
                    Station = topic,
                    Commmand = title,
                    Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
                };
                await client.CallCommandAsync();
                if (client.State != ZeroOperatorStateType.Ok)
                {
                    client.CheckStateResult();
                }
                return string.IsNullOrEmpty(client.Result)
                    ? ApiResultIoc.Ioc.Succees()
                    : ApiResultIoc.Ioc.DeserializeObject<ApiResult>(client.Result);
            }
        }
        #endregion
    }
}