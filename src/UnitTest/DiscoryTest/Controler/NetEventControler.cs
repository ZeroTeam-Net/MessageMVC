using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    /// <summary>
    /// 事件控制器（测试）
    /// </summary>
    [NetEvent("NetEvent")]
    public class NetEventControler : IApiController
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public IUser User { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public string Why { get; set; }

        /// <summary>
        /// 上下文（依赖构造）
        /// </summary>
        public IZeroContext Context { get; set; }

        //[Route("v1/json")]
        //public string Json(Argument argument)
        //{
        //    return DependencyHelper.GetService<IJsonSerializeProxy>().ToString(argument);
        //}

        //[Route("v1/xml")]
        //[ResultSerializeType(SerializeType.Xml)]
        //public ApiResult Xml()
        //{
        //    return new ApiResult();
        //}

        /// <summary>
        /// 空参数，空返回
        /// </summary>
        /// <returns></returns>
        [Route("v1/void"), ArgumentScope(ArgumentScope.Dictionary)]
        public void Task()
        {
            Logger.LogInformation("Call Void");
        }
    }
}
