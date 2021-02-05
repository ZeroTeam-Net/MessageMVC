using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("test3")]
    public class HttpTest3Controler : IApiController
    {
        public ILogger Logger { get; set; }
        async Task Debug(int step, object a, object b)
        {
            await MessagePoster.CallApiAsync("test3", "Hello2");

            var builder = new StringBuilder();
            builder.Append(step);
            builder.Append('.');
            builder.Append(a == b ? "链接" : "断开");
            builder.Append($"｛ Task: {Task.CurrentId}");
            builder.Append($" Thread: {Thread.CurrentThread.ManagedThreadId}");
            builder.Append($" HashCode: {a?.GetHashCode()}|{b?.GetHashCode()}｝");
            Logger.Information(builder.ToString());
        }
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("Hello2")]
        public IApiResult<string> Test2()
        {
            return ApiResultHelper.Succees("Hello2");
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("arg")]
        public async Task<IApiResult<string>> Hello(string name, int count, Guid guid, byte[] file, long id = -1, Em em = Em.Test)
        {
            var test3 = DependencyHelper.GetService<TestObject>();
            var test2 = DependencyHelper.GetService<ITest>(nameof(TestObject));

            var ctx = context = GlobalContext.Current;
          await  Debug(1, context, context);
            ScopeRuner.RunScope("Hello2", Hello2);
            await Debug(1, ctx, GlobalContext.Current);
            if (DateTime.Now.Ticks % 13 == 11)
                Logger.Exception(new Exception("故意的"));
            if (DateTime.Now.Ticks % 23 == 19)
                Logger.Error("故意的");
            if (DateTime.Now.Ticks % 67 == 57)
                Logger.Log(LogLevel.Critical, "故意的");
            return ApiResultHelper.Helper.Succees($"name:{name} count:{count} guid:{guid} file:{file} id:{id} em:{em}");
        }
        object context;
        async Task Hello2()
        {
            await Debug(2, context, GlobalContext.Current);
            context = GlobalContext.Current;
            await Task.Yield();
            await Task.Delay(100);
            await Debug(2, context, GlobalContext.Current);
            await Hello3();
            await Debug(2, context, GlobalContext.Current);
            ScopeRuner.DisposeLocal();
        }
        async Task Hello3()
        {
            await Debug(3, context, GlobalContext.Current);
            await Task.Yield();
            await Debug(3, context, GlobalContext.Current);
            await Task.Delay(100);
            await Debug(3, context, GlobalContext.Current);
        }
    }
    public enum Em
    {
        None,
        Test
    }
}
