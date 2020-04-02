# 写在前面
人的认知，总在不断的蜕变，昨日的善，今日成恶…恶中有善，善中藏恶…并不是我们错了，而是时间让我们学会了更多维度地看待这个世界。

一揽子解决方案，简单高效且实用，可殊不知，一千个哈利波特有无穷无尽的飞行模式，执行者亦然如此，带来的结果是更大的失败率，非铁腕不可完成。

技术领域，没有铁腕，只有规则。在相同的规则下，各组件有机组合，或丰盛，或简约，或个性，或大众，被接受将是极其简单的事。

构建规则，组件剥离，能力下沉，这就是全新的MessageMVC。

# 我们的目的
我们期望MessageMVC以简单优雅的方式对消息进行处理。

我们的处理方式主推与MVC高度兼容，完全兼容WebApi方式,通过书写您习惯的Controler即可完成工作任务。
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
这是多么熟悉的味道,无需求更多的学习成本,便可快速上手，内部的技术细节，我们负责帮你搞定。

我们会根据您的应用场景，努力的开发出更多的消息处理中间件，比如说对WebSocket进行桥接，消息的可靠性处理，本地异常消息重放，将您的消息推向更广阔的领域和更高的可靠性。

在系统内部，我们通过一种灵活而独立的方式，支持各种常见的消息传输方式，从而实现对常见消息队列、RPC远程调用、分布式事件的透明支持。我们也提供无环境下的简单通讯模拟，让你的单元测试更加简单高效。这里的一切，您无需关心内部的细节,开箱即用。

# 设计思想

我们依托aop思想，结合流程控制、依赖注入、控制反转等方式进行底层设计。Aspnet.core的世界，中间件正在大行其道，在这种优秀设计模式的指引下,我们以中间件的形式对系统进行高度抽象,通过控制反转的模式，独立各种能力,最大程度的解耦,实现最简单的扩展支持。

我们建议使用配置化,理由是基于netcore强大的配置能力，可以简单地实现各种高级的配置化能力，从而实现动态运行时微调.

我们建议采用上下文设计模式，因为它可以极度简化状态数据存取，简化方法调用的参数设计。使我们的系统更加的践行AOP思想。

系统中的组件，尤其是**依赖注入**构建的组件，他们要合理有序的运行，拥有一个统一的流程和一个维护这个流程的控制器是一个靠谱的思路。在这里，我们定义了所有服务的基本执行流程,根据这个流程,进行服务的生命周期管理，即**配置检查**—**初始化**—**启动**—++服务运行++—**关闭通知**-++服务中止++—**资源清理**。

#  消息格式

现实世界，不同的通信协议，不同的网络数据流，各式各样，我们必须接受这种现实，与此矛盾的是我们的需求是 **单一形式的消息出\入口**。

网络世界最成功的案例，非网关莫属，每一个不同的网络协议，互相妥协，实现一个相同的数据交换标准，从而大获成功。同理可证，需要简单合理的处理消息，我们必须指定一个交换标准，这就是**IMessageItem**。

通过对各种消息的分析总结，我们发现，**主题-标题-内容**，可以基本涵盖绝大多数需求，每一个网络传输对象，自行实现接收的数据到标准数据模型的转换，也实现标准数据模型到内部传输的还原。

# 基础能力

## ZeroFlowControl流程控制

通过对流程的高度抽象与统一，不同服务，在统一流程控制下，完成不同阶段的任务，进入和退出处于异步状态内部轮训均在可控范围。每一个节点亦可根据实际情况空转，并不拘泥。

###  IFlowMiddleware 子流程控制反转
在系统主流程的节点中,我们需要处理具体的环境的配置,初始化,清理等细节,所以我们通过设计IFlowMiddleware来接受这些操作.


###  IService 服务驱动
每一个topic，都需要一个服务进行支持，完成对网络传输工具的控制，从而实现数据接受/处理/返回的正确处理。

## 消息收发

IO处理，是计算机最重要的能力，支持更多的网络传输能力和无缝切换，是决定是否被技术选型挑中的关键。所以本项目最重要的目的，就是对消息进行处理并得出正确的结果。
我们通过MessagePoster与MessageProcessor对象作为消息两端的输入\输出,它们之间的交互与细节对使用者透明.

###  MessageProcessor消息处理

消息处理，必然源于现实世界的业务需求，即使60%以上的可以通过API接口处理，也就是ApiExecuter。但作为一个灵活组合的开放系统，我们不应该锁定一个用途，把其他需求拒之门外或难以实现。

我们参考了netcore的中间件的实现，通过链式控制权传播（先进后出的倒立堆栈形式），前一个组件决定是继续调用下一个组件或者原路返回。


在调用链中，每一个中间件可以选择感兴趣的消息进行普通或独占处理。从而灵活的满足不同的消息处理需求。

###  MessagePoster 消息投递

通过驱动各种网络传输工具(IMessagePoster),实现消息的广播 与 远程调用.

###  INetTransfer

网络数据接受，是项目的另一个重点

根据消息的特点，我们继承了三种常见的应用场景，IRPCTransfer对应RPC服务，IMessageConsumer对应消息队列，INetEvent对应分布式消息。
> 长江后浪推前浪是技术领域的常态，企业不断发展，工具必然不断迭代

####  IMessagePoster

IMessagePoster的接口实现数据发送的各种细节，无需精通网络传输技能便可做出高效稳定的项目。



##  其它可选择支持

- GlobalContext全局上下文，通用参数传递者
- IApiResult返回值规范
- IApiArgument参数规范
- IOChelper简单使用依赖注入
- ConfigurationManager简单的配置读写工具
- JsonHelper解决序列化过程的坑
- LogRecorder简化日志的记录

# 流程控制细节

### 配置检查
1. 构建子流程中间件
2. 驱动中间件的CheckOption方法

### 初始化
驱动中间件的Initiation方法

### 服务发现
可选,通过调用Discovery方法分析并发现服务接口.

### 启动
1. 通过依赖注入工具类，构建出所有子控制器
2. 调用所有IFlowMiddleware对象的Start方法
3. 执行IService的Run方法,IService内部会打开一个独立线程以支持多服务并行。一般来说，服务运行是驱动网络传输对象进入消息轮询状态。如果需要更合理的控制服务，网络传输对象可决定服务关闭或重启。为保证双向控制合理有效，ZeroService对象，内部通过状态机对象合理的响应控制。
> ZeroRPC服务的状态,接受服务中心的统一调度,通过分布式事件广播通知到服务.为达到合理控制的目的,ZeroRpcTransfer对象内部通过事件处理,可同步关闭或开启服务.

### 关闭通知
关闭首先由控制发出关闭指令（调用IService.Close),但我们建议的是,Service不直接关闭,而是通过ConcelToken\信号量等方式优雅地通知或触发Run执行中止,保证处理中的消息处理是可控的.



# 中间件

在基础流程的基础上，定义了两种中间件接口IFlowMiddleware与IMessageMiddleware，分别负责子环境流程控制与消息处理服务。在每一个流程节点中，主控制器（ZerFlowControl）调用接口对象（Middlewares与Services）的对应方法，成功实现控制反转，从而驱动未知组件。

### 子流程处理中间件 IFlowMiddleware


常用于支持服务的正常初始化或细节控制，比如说网络环境的检测等功能，其中最典型的是为zeroRPC与微服务调度中心的交互而实现的ZeroFlow流程控制器。
> 在CheckOption前,可通过IocHelper.AddXXX方法注册IFlowMiddleware实现.

### 消息处理中间件 IMesssageMiddleware

> Initiation之前,可通过ZerFlowControl.RegistService注册IService服务.也可以通过IocHelper.AddXXX方法注册IService服务


# MVC模式支持工具类


### 接口实现标记IApicontroler
ApiDiscovery通过发现实现此接口的类来绑定消息与Api方法.

### 接口发现工具 ApiDiscovery
在接口发现阶段（CheckOption后Initiation前）通过分析IApiControl，自动构造与添加服务,同时在服务内部构建ApiName与Action的映射表。
为更自由的实现INetTransfer对象与IService的绑定，可通过实现 ITransportDiscory 接口并注入，可实现自定义的绑定

### 接口执行器ApiExecuter

通过服务Actions映射表,实现从消息到Api方法的处理细节


# 生产目标
- KafkaMVC卡夫卡发布订阅
- ZeroMQInproc进程内通讯
- ZeroRPC通过zeroMQ实现的一种RPC通讯
- gRPC大名鼎鼎的谷歌RPC
- RabbitMQ用户量极大的消息队列
- WebsocketBridge消息转发到客户端

