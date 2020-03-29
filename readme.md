# MessageMVC实现了什么

# 我们的目的
我们期望MessageMVC以简单优雅的方式对消息进行处理,简单灵活地处理来自网络或任何来源的消息,就象实现一个AspnetCore的WebApi一样简单,就象这样子

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
```。

我们的处理方式主推与MVC高度兼容的仿WebApi,通过书写您习惯的Controler即可完成工作任务。但我们也提供其它扩展方式,就象Aspnet.core中间件一样灵活，同时我们会根据您的应用场景，努力的开发出更多的消息处理中间件，比如说对WebSocket进行桥接，将您的消息推向更广阔的领域。


###  各种消息队列
我们会通过一种灵活而独立的方式，支持各种常见的消息传输方式，从而实现对常见消息队列、RPC远程调用、分布式事件的透明支持。
1. Kafka(已实现)
2. RabbitMq
3. ActiveMQ
4. ZMQ

### 各种RPC
1. MicroZero(正在迁移)
2. gRPC

### 常见的平台消息处理
1. 微信公众号
2. 微信支付
3. 支付宝

### 测试支持
1. 测试调用
2. 控制台输入
3. 进程内通讯

### 灵活的中间件
通过NetCore强大的依赖注入功能,将每一个功能都通过中间件方式实现,最大程度的解除组件耦合
1. 配置分析
2. 日志中间件
3. 异常消息存储与重新消费中间件
4. 上下文件中间件
5. ApiExecuter中间件


# 设计思想
我们通过对依赖注入、控制反转知识的深入研究，结合AOP思想。通过对Aspnet.core的学习,通过中间件的形式对系统进行高度抽象,通过控制反转思维,独立各种能力,最大程度的解耦,最大程度的扩展支持.


系统不同组件，尤其是基于**依赖注入**产生的组件，他们要合理有序的运行，拥有一个统一的流程和一个维护这个流程的控制器是一个靠谱的思路。

在这里，我们定义了所有服务的基本执行流程,根据这个流程,进行服务的生命周期管理，即**配置检查**—**初始化**—**启动**—++服务运行++—**关闭通知**-++服务中止++—**资源清理**。

### 启动

其中,启动动作会打开一个独立线程执行IService的Run方法,以支持多服务并行。一般来说，服务运行是驱动网络传输对象进入消息轮询状态。如果需要更合理的控制服务，网络传输对象可决定服务关闭或重启。为保证双向控制合理有效，ZeroService对象，内部通过状态机对象合理的响应控制。
> ZeroRPC服务的状态,接受服务中心的统一调度,通过分布式事件广播通知到服务.为达到合理控制的目的,ZeroRpcTransfer对象内部通过事件处理,可同步关闭或开启服务.

### 关闭通知
关闭首先由控制发出关闭指令（调用IService.Close),但我们建议的是,Service不直接关闭,而是通过ConcelToken\信号量等方式优雅地通知或触发Run执行中止,保证处理中的消息处理是可控的.

### IFlowMiddleware与IService

在这个基础流程的基础上，定义了两种接口，分别负责子环境流程控制与消息处理服务。在每一个流程节点中，主控制器（ZerFlowControl）调用接口对象（Middlewares与Services）的对应方法，成功实现控制反转，从而驱动未知组件。
- 在CheckOption后,可通过IocHelper.AddXXX方法注册子流程控制器IFlowMiddleware.
- Initiation之前,可通过ZerFlowControl.RegistService注册IService服务.也可以通过IocHelper.AddXXX方法注册IService服务.
- 系统启动时，通过依赖注入工具类，构建出所有子控制器；
- 在接口发现阶段（CheckOption后Initiation前）通过分析IApiControl，自动构造与添加服务。

### Option配置化
基于netcore强大的配置能力，实现各种高级的配置化能力，从而实现动态运行时微调

#  消息格式
现实世界，不同的通信协议，不同的网络数据流，各式各样，我们必须接受这种现实，与此矛盾的是我们的需求是 **单一形式的消息出\入口**。

网络世界最成功的案例，非网关莫属，每一个不同的网络协议，互相妥协，实现一个相同的数据交换标准，从而大获成功。同理可证，需要简单合理的处理消息，我们必须指定一个交换标准，这就是**IMessageItem**。

通过对各种消息的分析总结，我们发现，**主题-标题-内容**，可以基本涵盖绝大多数需求，每一个网络传输对象，自行实现接收的数据到标准数据模型的转换，也实现标准数据模型到内部传输的还原。

# 基础组件
###  ZeroFlowControl流程控制

通过对流程的高度抽象与统一，不同服务，在统一流程控制下，完成不同阶段的任务，进入和退出处于异步状态内部轮训均在可控范围。每一个节点亦可根据实际情况空转，并不拘泥。

###  MessageProcess消息处理
本项目最重要的目的，就是对消息进行处理，得出正确的结果。消息处理，必然源于现实世界的业务需求，即使60%以上的可以通过API接口处理，也就是ApiExecuter。但作为一个灵活组合的开放系统，我们不应该锁定一个用途，把其他需求拒之门外或难以实现。

我们参考了netcore的中间件的实现，通过链式控制权传播（先进后出的倒立堆栈形式），前一个组件决定是继续调用下一个组件或者原路返回。

在调用链中，每一个中间件可以选择感兴趣的消息进行普通或独占处理。从而灵活的满足不同的消息处理需求。

###  IService服务中
每一个topic，都需要一个服务进行支持，完成对网络传输工具的控制，从而实现数据接受/处理/返回的正确处理。
- INetTransfer网络传输组件
- ImessageProducer消息发布组件
- IMessageMiddleware消息处理中间件
> 实现不同的消息处理能力，从而支持自由组合拳

### 消息收发

IO处理，是计算机最重要的能力。接受输入，输出结果是计算机的使命。

####  MessageProducer

基于IMessageProducer的接口实现数据发送（Input）的各种细节，从而降低使用者门槛，无需精通网络传输技能便可做出高效稳定的项目。

####  INetTransfer

网络数据接受，是项目的另一个重点，支持更多的网络传输能力和无缝切换，是决定是否被技术选型挑中的关键。

根据消息的特点，我们继承了三种常见的应用场景，IRPCTransfer对应RPC服务，IMessageConsumer对应消息队列，INetEvent对应分布式消息。
> 长江后浪推前浪是技术领域的常态，企业不断发展，工具必然不断迭代

# 流程处理中间件
> IFlowMiddleware

常用于支持服务的正常初始化或细节控制，比如说网络环境的检测等功能，其中最典型的是为zeroRPC与微服务调度中心的交互而实现的ZeroFlow流程控制器。

# 消息处理中间件
> ImesssageMiddleware

# MVC支持工具类
- ApiExecuter接口执行器
- ApiDiscovery接口发现工具
- IApicontroler接口实现标记

# 已实现
- KafkaMVC卡夫卡发布订阅
- ZeroMQInproc进程内通讯
- ZeroRPC通过zeroMQ实现的一种RPC通讯
- gRPC大名鼎鼎的谷歌RPC
- RabbitMQ用户量极大的消息队列
- WebsocketBridge消息转发到客户端


# 其它辅助

- GlobalContext全局上下文，通用参数传递者
> 上下文设计模式，可以极度简化状态数据存取，简化方法调用的参数设计。使我们的系统更加的践行AOP模式。
- IApiResult返回值规范
- IApiArgument参数规范
- IOChelper简单使用依赖注入
- ConfigurationManager简单的配置读写工具
- JsonHelper解决序列化过程的坑
- LogRecorder简化日志的记录



