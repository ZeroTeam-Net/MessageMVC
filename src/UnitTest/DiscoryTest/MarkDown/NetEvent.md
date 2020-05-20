# NetEvent 接口文档



---

## 注意：
1. 所有接口均可使用GET/POST方式访问
2. 如无参数，将不出现参数说明
3. 参数建议使用Json方式传递

## 返回值通用说明

### 格式
|名称|标题|类型|说明|
|-|-|-|-|-|
|success|成功标记|bool|操作成功返回true,否则根据code与msg判断结果|
|code|错误码|number|见通用说明与API具体说明|
|msg|消息|number|用户可见的文本消息，可用于用户提示|

### 通用错误码

|数值|名称|说明|HTTP状态码|
|-|-|-|-|
|1|Queue|正在排队|200|
|0|Success|成功|200|
|-1|ArgumentError|参数错误|200|
|-2|BusinessError|发生处理业务错误|200|
|-3|ArgumentError|发生未处理业务异常|200|
|-4|ArgumentError|发生未处理系统异常|200|
|-5|NetworkError|网络错误|200|
|-6|TimeOut|执行超时|200|
|-7|DenyAccess|拒绝访问|200|
|-8|TokenUnknow|未知的令牌|200|
|-9|TokenTimeOut|令牌过期|200|
|-10|NoReady|系统未就绪|200|
|-11|Ignore|异常中止|200|
|-12|ReTry|重试|200|
|-13|NoFind|方法不存在|200|
|-14|Unavailable|服务不可用|200|
|-15|Unknow|未知结果|200|

---

## 异步

**简要描述：** 


**请求URL：** 
` http://xx.com/NetEvent/v1/void`

**请求方式：**
- POST
- GET


**参数：** 

|参数名|必选|类型|示例|说明|
|:----    |:---|:----- |:----- |-----   |
|argument|是||true||

**参数示例**

```json
{
    "argument" :     true
{
```


 **返回参数说明** 

标准API返回格式，请参考[返回值通用说明]


 **data格式说明** 

|参数名|类型|说明|
|:-----  |:-----|-----                           |
|Value|string|文本内容|

**返回示例**

```json
{
    'success': true,
    'code': 0,
    'msg': '操作成功',
    'data':     {
    
        "Value" :     "示例文本"}
}
```

