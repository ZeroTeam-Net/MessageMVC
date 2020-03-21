# MessageMVC实现了什么
我们要实现简单灵活地处理来自网络或任何来源的消息,就象实现一个AspnetCore的WebApi一样简单,就象这样子

```csharp
using Agebull.Common.Logging;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("test1")]
    public class TestControler : IApiControler
    {
        [Route("test/res")]
        public ApiResult Result()
        {
            LogRecorder.Trace("Result");
            return ApiResult.Succees();
        }

    }
}
```
其它所有的技术细节,不需要开发者过多的关注.

# 支持的消息类型
> 简单的通过特性自动构造消息传输对象

##  各种消息队列
1. Kafka(已实现)
2. RabbitMq
3. ActiveMQ
4. ZMQ

## 各种RPC
1. MicroZero(正在迁移)
2. gRPC

## 常见的平台消息处理
1. 微信公众号
2. 微信支付
3. 支付宝

## 测试支持
1. 测试调用
2. 控制台输入
3. 进程内通讯

# 灵活的中间件
通过NetCore强大的依赖注入功能,将每一个功能都通过中间件方式实现,最大程度的解除组件耦合
1. 配置分析
2. 日志中间件
3. 异常消息存储与重新消费中间件
4. 上下文件中间件
5. ApiExecuter中间件

