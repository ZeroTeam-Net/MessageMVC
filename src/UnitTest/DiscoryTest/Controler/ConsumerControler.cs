﻿using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("Consumer")]
    public class ConsumerControler : IApiController
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

        public IZeroContext Context { get; set; }

        [Route("v1/json")]
        public string Json(Argument argument)
        {
            return DependencyHelper.GetService<IJsonSerializeProxy>().ToString(argument);
        }

        [Route("v1/xml")]
        [ResultSerializeType(SerializeType.Xml)]
        public ApiResult Xml()
        {
            return new ApiResult();
        }

        [Route("v1/void"), ArgumentScope(ArgumentScope.Dictionary)]
        public void Void(string argument)
        {
            Logger.LogInformation("Call Void");
        }
    }
}
