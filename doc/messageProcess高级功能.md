# 提前返回
通过返回一个TaskComplateSource控制的task给netTransfer等待。

## 提供给IMessageMiddleware

当IMesMiddleware发现可以返回到网络环境时，通过setResult等方式，结束调用等待，但并不终止后续处理。从而提高响应速度。

## mp应该提供结束调用的方法
避免直接暴露，调用应该记录，二次调用应该空操作，同时也保证mp处理结束后可以正常调用而不出错。

中间件调用参数增加mp对象。

## 