﻿using Agebull.Common.Logging;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("test1")]
    public class KafkaControler : IApiControler
    {
        [Route("test")]
        public ApiResult Result()
        {
            LogRecorder.Trace(GetType().FullName);
            return ApiResult.Succees();
        }
    }
}
