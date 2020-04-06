using Agebull.Common;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTransfers;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.MessageMVC.ZeroMQ.Inporc
{
    /// <summary>
    ///     ZMQ生产者
    /// </summary>
    public class InprocPoster : IMessagePoster
    {
        #region Properties

        /// <summary>
        /// 运行状态
        /// </summary>
        public StationStateType State { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static InprocPoster Instance = new InprocPoster();

        /// <summary>
        /// 构造
        /// </summary>
        public InprocPoster()
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
        public ZeroOperatorStateType OperatorState => _core.State;

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
        public void CallCommand() => _core.Call();

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        public void CheckStateResult() => _core.CheckStateResult();

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        public MessageState MessageState => _core.MessageState;

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
            var client = new InprocPoster
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
            var client = new InprocPoster
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.OperatorState != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultHelper.Ioc.DeserializeObject<TResult>(client.Result);
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
            var client = new InprocPoster
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.OperatorState != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultHelper.Ioc.DeserializeObject(client.Result);
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
            var client = new InprocPoster
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.OperatorState != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultHelper.Ioc.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult CallApi(string station, string api)
        {
            var client = new InprocPoster
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.OperatorState != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultHelper.Ioc.DeserializeObject(client.Result);
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
            var client = new InprocPoster
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.OperatorState != ZeroOperatorStateType.Ok)
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
            var client = new InprocPoster
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.OperatorState != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        #endregion

        #region IMessagePoster

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task<(MessageState state, string result)> Post(IMessageItem message)
        {
            var client = new InprocPoster
            {
                Station = message.Topic,
                Commmand = message.Title
            };
            await client.CallCommandAsync();
            client.CheckStateResult();
            return (client.MessageState, client.Result);
        }
        #endregion
    }
}
/*

        string IMessagePoster.Producer(string topic, string title, string content)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
            return client.Result;
        }

        TRes IMessagePoster.Producer<TArg, TRes>(string topic, string title, TArg content)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
        void IMessagePoster.Producer<TArg>(string topic, string title, TArg content)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            client.CallCommand();
        }
        TRes IMessagePoster.Producer<TRes>(string topic, string title)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title
            };
            client.CallCommand();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }


        async Task<string> IMessagePoster.ProducerAsync(string topic, string title, string content)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            await client.CallCommandAsync();
            return client.Result;
        }

        async Task<TRes> IMessagePoster.ProducerAsync<TArg, TRes>(string topic, string title, TArg content)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            await client.CallCommandAsync();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }
        Task IMessagePoster.ProducerAsync<TArg>(string topic, string title, TArg content)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title,
                Argument = Equals(content, default) ? null : JsonHelper.SerializeObject(content)
            };
            return client.CallCommandAsync();
        }
        async Task<TRes> IMessagePoster.ProducerAsync<TRes>(string topic, string title)
        {
            var client = new InporcProducer
            {
                Station = topic,
                Commmand = title
            };
            await client.CallCommandAsync();
            return string.IsNullOrEmpty(client.Result)
                ? default
                : JsonHelper.DeserializeObject<TRes>(client.Result);
        }

*/
