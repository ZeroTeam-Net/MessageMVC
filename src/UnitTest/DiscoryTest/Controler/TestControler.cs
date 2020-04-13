using ZeroTeam.MessageMVC.ZeroApis;
using Microsoft.Extensions.Logging;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Messages;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using ZeroTeam.MessageMVC.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("UnitService")]
    public class TestControler : IApiControler
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        readonly ILogger logger;

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
            , [FromServices]IXmlSerializeProxy xml
            , [FromConfig("MessageMVC:Option")] ZeroAppConfig option)
        {
            logger = log;
            Option = option;
            if (service == DependencyHelper.ServiceCollection)
                Console.WriteLine(Option.AppName);

        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/argument")]
        public IApiResult Argument(Argument argument)
        {
            logger.LogInformation($"Call {nameof(Argument)}({argument.Value})");
            return ApiResultHelper.Succees();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/validate")]
        public IApiResult Validate(TestArgument argument)
        {
            return ApiResultHelper.Succees(argument.Value);
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <param name="argument">参数</param>
        /// <returns>ApiResult格式的成功或失败</returns>
        [Route("v1/customSerialize")]
        [SerializeType(SerializeType.Xml)]
        public async Task<IApiResult> CustomSerialize(TestArgument argument)
        {
            logger.LogInformation($"Call {nameof(Argument)}({argument.Value})");
            await Task.Delay(100);
            return ApiResultHelper.Succees(argument.Value);
        }

        [Route("v1/empty")]
        public void Empty()
        {
            logger.LogInformation($"Call {nameof(Empty)}");
        }

        [Route("v1/async")]
        public async Task Async()
        {
            await Task.Yield();
        }


        [Route("v1/context")]
        public IZeroContext ZeroContext(IUser user, IZeroContext context)
        {
            logger.LogInformation($"Call {nameof(ZeroContext)} user:{user.NickName} context:{context?.Trace.CallApp}");
            return context;
        }

        [Route("v1/exception")]
        public async Task<IApiResult> Exception()
        {
            await Task.Yield();
            logger.LogInformation($"Call {nameof(Async)}");
            throw new Exception("异常测试");
        }

        [Route("v1/json")]
        public string Json(Argument argument)
        {
            return DependencyHelper.Create<IJsonSerializeProxy>().ToString(argument);
        }

        [Route("v1/xml")]
        [ResultSerializeType(SerializeType.Xml)]
        [ArgumentSerializeType(SerializeType.Json)]
        public IApiResult<string> Xml(Argument argument)
        {
            return ApiResultHelper.Succees(argument.Value);
        }


        [Route("v1/err")]
        public Task<IApiResult<string>> Error()
        {
            logger.LogInformation($"Call {nameof(Error)}");
            var res = ApiResultHelper.Error<string>(1);
            res = ApiResultHelper.Error<string>(1, "测试失败");
            res = ApiResultHelper.Error<string>(1, "测试失败", "内部信息");
            res = ApiResultHelper.Error<string>(1, "测试失败", "内部信息", "这是测试", "没有解释");
            return Task.FromResult(res);
        }

        [Route("v1/task")]
        public Task<IApiResult<string>> TaskTest()
        {
            logger.LogInformation($"Call {nameof(TaskTest)}");
            return Task.FromResult(ApiResultHelper.Succees(nameof(TaskTest)));
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
