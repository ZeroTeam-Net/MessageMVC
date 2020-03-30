using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {

        public TestControler(ILogger<TestControler> logger,
            System.Collections.Generic.IEnumerable<IFlowMiddleware> mids,
            IApiResult res,
            IServiceCollection service,
            [FromConfig("Redis")]RedisOption option)
        {

        }
        [FromConfig("Redis")]
        public RedisOption Option { get; set; }

        [FromServices]
        public IApiResult Result { get; set; }

        [Route("v1/test")]
        public async Task<ApiResult> OnOrderNew( [FromConfig("Redis")]RedisOption option, ApiResult res)
        {
           await Task.Yield();
            return ApiResult.Succees();
        }
    }
    public class RedisOption
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <example>
        /// $"{Address}:{Port},password={PassWord},defaultDatabase={db},poolsize=50,ssl=false,writeBuffer=10240";
        /// </example>
        public string ConnectionString { get; set; }


        /// <summary>
        /// 异常守卫多久检查一次
        /// </summary>

        public int GuardCheckTime { get; set; }


        /// <summary>
        /// 消息处理过程锁定时长
        /// </summary>

        public int MessageLockTime { get; set; }


        /// <summary>
        /// 消息处理失败按发生异常处理
        /// </summary>

        public bool FailedIsError { get; set; }

        /// <summary>
        /// 无处理方法按发生异常处理
        /// </summary>

        public bool NoSupperIsError { get; set; }
    }
}
