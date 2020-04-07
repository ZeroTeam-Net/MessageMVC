﻿﻿# MessageProcessor
消息处理器

# 消息格式
> 通过IMessageItem接口进行定义,重点为三要素一状态

- Topic : 消息分组
> 按不同的环境可解释为

|应用场景|解释|Controler|
|:-:|:-|:-|
|MQ|消息主题|Consumer|
|RPC|服务名称|Service|
|Event|事件分组|NetEvent|

- Title : 标题

|应用场景|解释|Controler|
|:-:|:-|:-|
|MQ|消息标题|Route-API|
|RPC|接口名称|Route-API|
|Event|事件名称|Route-API|

- Content : 内容

|应用场景|解释|Controler|
|:-:|:-|:-|
|MQ|消息内容|Argument|
|RPC|接口参数|Argument|
|Event|事件参数|Argument|

-State : 处理状态

# 消息处理中间件
> IMessageMiddleware


1. 依赖构造所有中间件
2. 通过Level进行优先级排序
3. 通过链式进行中间件处理
> 参考AspnetCore的实现,第一个中间件处理最先和最后的流程,下一个中间件,通过调用next进行处理,也可直接跳过next.

## ApiExecuter
> 作为核心消息与Controler交互的桥梁.

1. 查找方法表,找到对应的Controler及方法调用的Action
2. 还原参数(如果有)
3. 调用对应的Action
4. 处理返回值
5. 与IMessageReceiver交互,调用OnResult/OnError之一(只调用其中一个方法一次)以传送结果.


