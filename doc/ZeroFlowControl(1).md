# ZeroFlowControl


应用流程控制器


[Flow](https://github.com/ZeroTeam-Net/MessageMVC/blob/master/doc/Flow.png?raw=true)


# 主流程


## CheckOption
> 配置校验


1.日志对象初始化
>  LogRecorder.Initialize

2. 生成配置对象
> Config : ZeroAppConfigRuntime

3. 获取注入的所有IFlowMiddleware中间件
> IService正常不通过依赖注入,而是在ApiDiscover发现完成后,通过RegistZeroObject注册到Services属性中,控制器会对IService对象进行更深度的管理与控制

4. 对Middlewares按Level排序,以保证正确的处理顺序

|Middleware|Level|Description|
|:-|:-:|:-|
|ConfigMiddleware|int.MinValue|作为配置解析,依赖对象,配置对象的初始化中间件,它必须排在最先执行|
|AddInImporter|short.MinValue|作为插件发现工具,应该排行尽可能前|
|ReConsumerMiddleware|0|异常消息重新浪费|
|KafkaProducer|0|Kafka消息发布,目前仅实现初始化,后续应实现里程内通讯,以保证可靠高效的投递|

5. 所有中间件执行CheckOption
> 并未进行异常处理,仅记录日志,所有执行异常将导致应用程序退出

6. IocHelper对象刷新

## Discove
> 调用进行Controler对象的发现,可选流程

## Initialize

1. 初始化Middlewares对象
2. 初始化Services对象
3. IocHelper对象刷新
> 在此之后,不建议再进行依赖对象注册.
4. 控制器状态设置为 StationState.Initialized

## Start
> 根据用途的不同,应该调用Run / RunAsync / RunAwaite / RunAwaiteAsync其中之一

1. 状态设置为StationState.BeginRun
2. 以Task方式调用Middleware.Start
3. 以Task方式调用Services.Start
4. 等待Services.Start全部执行完成
> 通过信号量完成,所有服务启动成功应调用OnObjectActive,启动失败应调用OnObjectFailed,调用后,会检查登记数量是否与注册的服务数量相同,达到时会释放信号量

5. 设置状态为StationState.Run

## Shutdown
> RunAwaite/RunAwaiteAsync会注册系统中断信号(SIGINT,即Control+C)发出时调用

1. 状态设置为StationState.Closing
2. 调用所有中间件与服务的Close方法
> 注意Close方法应实现为通知形式来合理的中断服务,尤其是处于轮询状态的网络对象,建议采用CancellationToken的Cancel方法.

3. 等待所有对象正常退出
> 服务在正常中止后,应调用OnObjectClose,以保证信号量可以调用Release方法中止等待

4. 状态设置为StationState.Closed
5. 调用所有中间件与服务的End方法
6. 状态设置为StationState.Destroy
7. 如果有RunAwaite,则释放Task
> TaskCompletionSource是一个轻量级的Task状态机,强烈建议使用.


## 示例

```csharp
        /// <summary>
        /// 使用KafkaMVC
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="waitEnd"></param>
        public static Task UseKafka(Assembly assembly, bool waitEnd)
        {
            Console.WriteLine("Weconme ZeroTeam KafkaMVC");

            IocHelper.AddTransient<IFlowMiddleware, ConfigMiddleware>();//配置\依赖对象初始化,系统配置获取
            IocHelper.AddTransient<IFlowMiddleware, AddInImporter>();//插件载入
            IocHelper.AddTransient<IMessageConsumer, KafkaConsumer>();//采用Kafka消费客户端
            IocHelper.AddTransient<IMessageMiddleware, LoggerMiddleware>();//启用日志
            IocHelper.AddTransient<IMessageMiddleware, GlobalContextMiddleware>();//启用全局上下文
            IocHelper.AddTransient<IMessageMiddleware, ApiExecuter>();//API路由与执行

            //消息存储与异常消息重新消费
            IocHelper.AddTransient<IMessageMiddleware, StorageMiddleware>();
            IocHelper.AddTransient<IFlowMiddleware, ReConsumerMiddleware>();
            //主流程
            ZeroFlowControl.CheckOption();
            KafkaProducer.Initialize();
            ZeroFlowControl.Discove(assembly);
            ZeroFlowControl.Initialize();
            if (waitEnd)
               return ZeroFlowControl.RunAwaiteAsync();
            else
                return ZeroFlowControl.RunAsync();
        }
```







