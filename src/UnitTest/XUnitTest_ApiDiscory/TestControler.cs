using ZeroTeam.MessageMVC.ZeroApis;
using Microsoft.Extensions.Logging;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Messages;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using ZeroTeam.MessageMVC.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("UnitService")]
    public class TestControler : IApiControler
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        ILogger logger;

        /// <summary>
        /// 当前用户
        /// </summary>
        public ObjectFactory Factory { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        [FromConfig("MessageMVC:Option")]
        public ZeroAppConfig Option { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        [FromServices]
        public IEnumerable<IUser> User { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="log">日志对象,框架自动构造</param>
        public TestControler(ILogger log
            , IServiceCollection service
            , IXmlSerializeProxy xml 
            , [FromConfig("MessageMVC:Option")] ZeroAppConfig option)
        {
            logger = log;
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/argument")]
        public async Task<IApiResult> Argument(Argument argument)
        {
            logger.LogInformation($"Call {nameof(Argument)}({argument.Value})");
            await Task.Delay(100);
            return ApiResultHelper.Succees();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/argument22")]
        public async Task<IApiResult> Argument2(Argument argument
            , [ArgumentSerializeType(SerializeType.Json)]Argument arg2)
        {
            logger.LogInformation($"Call {nameof(Argument)}({argument.Value})");
            await Task.Delay(100);
            return ApiResultHelper.Succees();
        }

        [Route("v1/empty")]
        public void Empty()
        {
            logger.LogInformation($"Call {nameof(Empty)}");
        }

        [Route("v1/async")]
        public async Task<IApiResult> Async()
        {
            await Task.Yield();
            logger.LogInformation($"Call {nameof(Async)}");
            return ApiResultHelper.Succees(nameof(Async));
        }


        [Route("v1/task")]
        public Task<IApiResult<string>> TaskTest()
        {
            logger.LogInformation($"Call {nameof(TaskTest)}");
            var res = ApiResultHelper.Succees(nameof(TaskTest));
            return Task.FromResult(res);
        }

        [Route("v1/FromServices")]
        public IApiResult<string> FromServices([FromServices] ISerializeProxy a)
        {
            logger.LogInformation($"Call {nameof(FromServices)},Argument : {a.ToJson()}");
            return ApiResultHelper.Succees(a.ToJson());
        }

        [Route("v1/FromConfig")]
        public IApiResult<string> FromConfig([FromConfig("MessageMVC:Option")] ZeroAppConfig a)
        {
            logger.LogInformation($"Call {nameof(FromConfig)},Argument : {a.ToJson()}");
            return ApiResultHelper.Succees(a.ToJson());
        }

        [Route("v1/mulitArg")]
        public IApiResult<SerializeType> MulitArg(string a, int b, decimal c, SerializeType d)//
        {
            logger.LogInformation($"Call {nameof(MulitArg)},Argument : a {a}, b {b},c {c},d {d}");
            return ApiResultHelper.Succees(d);
        }
    }
}
