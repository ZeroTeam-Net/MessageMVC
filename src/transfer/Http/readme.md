# 基本设计

在设计规则中，我们需要的是向上保证一致性，消息传输对象,应保证MessageProcessor正确进行消息处理，
所以我们设计了HttpRoute类,通过标准的IApplicationBuilder.Run注册Http的处理入口方法HttpRoute.Call,
HttpRoute.Call方法代替HttpTransfer调用MessageProcessor,从而保证了向上一致性。


## HttpTransfer

由于我们使用了AspnetCore的基础框架,所以并无法象其它消息传输对象一样,在Loop中通过轮询实现。
HttpTransfer的存在，仅是为了满足设计规范，保证ApiDiscover时可生成正确的服务对象,
ZeroFlowControl可以正确运行，MessageProcessor处理时可获得正确的IService对象进行回调。

作为为规则存在而存在的对象，如果消耗算力，那就罪大恶极了，所以我们在Loop方法中，使用了TaskCompletionSource,
在运行时,会在Task的调度器中休眠,只有关闭指令发出时,Close方法SetResult释放,基本无消耗。
 
OnResult 与 OnError 也无用,HttpRoute.Call方法是通过MessageProcessor的调用返回结束调用的.

## HttpMessage

由于Http协议的特点,MessageItem并不能满足需求,所以我们设计了HttpMessage,正确记录了Request与Response关联的信息.

## HttpProtocol

为简单支持跨域而设计

## HttpRoute

1. 解析参数
2. 取得Service,
3. 满足条件后调用MessageProcessor
4. 回写Response数据

同时,为简化操作,Initialize方法提供简单启动ZeroFlowControl的能力.

# 系统配置

```json
{
  "HttpRoute": {
    /// 启用文件上传
    "EnableFormFile": false,
    /// 启用身份令牌
    "EnableAuthToken": false,
    /// 启用Header跟踪(如HTTP请求头)
    "EnableHeader": false,
    /// 特殊URL取第几个路径作为服务名称的映射表
    "HostPaths": false,
    ///启用快速调用,即直接使用ApiExecuter
    "FastCall": false
  },
}

```

# 使用说明

## Program.cs

```csharp
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using System;
using Microsoft.Extensions.Hosting;

namespace ZeroTeam.MessageMVC.Http
{
    public class Program
    {

        public static void Main(string[] args)
        {
            LogRecorder.LogPath = Path.Combine(Environment.CurrentDirectory, "logs", ConfigurationManager.Root["AppName"]);

            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseConfiguration(ConfigurationManager.Root)
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        var option = ConfigurationManager.Get("Logging");
                        builder.AddConfiguration(ConfigurationManager.Root.GetSection("Logging"));
                        if (option.GetBool("console", true))
                            builder.AddConsole();
                        if (option.GetBool("innerLogger", false))
                        {
                            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());
                            LoggerProviderOptions.RegisterProviderOptions<TextLoggerOption, TextLoggerProvider>(builder.Services);
                        }
                    })
                    .UseUrls(ConfigurationManager.Root.GetSection("Kestrel:Endpoints:Http:Url").Value)
                    .UseKestrel((ctx, opt) =>
                    {
                        opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                    })
                    .UseStartup<Startup>();
                });
    }
}
```

1. 设置LogRecorder.LogPath,保存路径相同,如果不启用LogRecorder的文本记录器,可跳过
2. UseConfiguration(ConfigurationManager.Root),保证ConfigurationManager用途的一致性.
3. 日志配置ConfigureLogging ,此处响应Logging的扩展配置,用于更合理的使用LogRecorder
4. UseUrls,是为了不跳出默认使用5000及5001端口的讨厌Warning
5. UseKestrel,使用Core的标准配置,可参考MSDN了解更多配置的详情.

## Startup.cs

```csharp
using Agebull.Common.Ioc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// 启动类
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            IocHelper.SetServiceCollection(services);
            HttpRoute.Initialize(services);
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.RunMessageMVC(); 
        }
    }
}
```
1. IocHelper.SetServiceCollection(services) 保证依赖对象一致性
2. HttpRoute.Initialize(services) 初始化并启动ZeroFlow对象，简化操作
3. app.Run(HttpRoute.Call) 注册Http处理，以挂接到流程中。

## Controler.cs
```csharp
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {
        [Route("v1/test")]
        public ApiResult OnOrderNew(Argument argument)
        {
            return ApiResultHelper.Succees(argument?.Value);
        }
    }
}
```
1. Service标签，保证可以正确对应到服务（HttpRoute.Initialize内部已注册了IRpcTransfer的实现HttpTransfer）
2. 继承IApiControler接口，保证可以被正确发现
3. Route特性，说明这是一个Api。

# 性能

阿里云4核8G机器，最高达18000+QPS,与用默认WebApi快了10%