using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    /// <summary>
    /// 测试服务
    /// </summary>
    [Service("UnitService")]
    public class TestControler : IApiController
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
            , IUser user
            , IZeroContext ctx
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
        /// <summary>
        /// 空参数
        /// </summary>
        [Route("v1/empty")]
        public void Empty()
        {
            logger.LogInformation($"Call {nameof(Empty)}");
        }
        /// <summary>
        /// 异步
        /// </summary>
        /// <returns></returns>
        [Route("v1/async")]
        public async Task Async()
        {
            await Task.Yield();
        }

        /// <summary>
        /// 上下文
        /// </summary>
        /// <param name="user"></param>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="msg2"></param>
        /// <returns></returns>
        [Route("v1/context"), ApiOption(ApiOption.ArgumentCanNil)]
        public IZeroContext ZeroContext(IUser user, IZeroContext context, IInlineMessage msg, IInlineMessage msg2)
        {
            logger.LogInformation($"Call {nameof(ZeroContext)} user:{user.NickName} context:{context?.Trace.CallApp}");
            return context;
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <returns></returns>
        [Route("v1/exception")]
        public async Task<IApiResult> Exception()
        {
            await Task.Yield();
            logger.LogInformation($"Call {nameof(Async)}");
            throw new Exception("异常测试");
        }
        /// <summary>
        /// 错误
        /// </summary>
        /// <returns></returns>
        [Route("v1/err")]
        public Task<IApiResult<string>> Error()
        {
            logger.LogInformation($"Call {nameof(Error)}");
            var res = ApiResultHelper.State<string>(1);
            res = ApiResultHelper.State<string>(1, "测试失败");
            res = ApiResultHelper.State<string>(1, "测试失败", "内部信息");
            res = ApiResultHelper.State<string>(1, "测试失败", "内部信息", "这是测试", "没有解释");
            return Task.FromResult(res);
        }
        /// <summary>
        /// 异步任务
        /// </summary>
        /// <returns></returns>
        [Route("v1/task")]
        public Task<IApiResult<string>> TaskTest()
        {
            logger.LogInformation($"Call {nameof(TaskTest)}");
            return Task.FromResult(ApiResultHelper.Succees(nameof(TaskTest)));
        }
        /// <summary>
        /// 依赖构造
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [Route("v1/FromServices")]
        public IApiResult<string> FromServices([FromServices] ISerializeProxy a)
        {
            logger.LogInformation($"Call {nameof(FromServices)},Argument : {a.ToJson()}");
            return ApiResultHelper.Succees(a.ToJson());
        }
        /// <summary>
        /// 配置构造
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [Route("v1/FromConfig")]
        public IApiResult<string> FromConfig([FromConfig("MessageMVC:Option")] ZeroAppConfig a)
        {
            logger.LogInformation($"Call {nameof(FromConfig)},Argument : {a.ToJson()}");
            return ApiResultHelper.Succees(a.ToJson());
        }
        /// <summary>
        /// 多参数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        [Route("v1/mulitArg")]
        public IApiResult<SerializeType> MulitArg(string a, int b, decimal c, SerializeType d)//
        {
            logger.LogInformation($"Call {nameof(MulitArg)},Argument : a {a}, b {b},c {c},d {d}");
            return ApiResultHelper.Succees(d);
        }
    }
}
