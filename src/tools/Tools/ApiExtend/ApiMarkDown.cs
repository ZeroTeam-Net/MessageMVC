using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Documents;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// API扩展功能
    /// </summary>
    public class ApiMarkDown
    {
        /// <summary>
        /// 站点文档信息
        /// </summary>
        public Dictionary<string, ServiceInfo> ServiceInfos { get; set; }

        /// <summary>
        /// 生成单元测试代码
        /// </summary>
        /// <param name="path"></param>
        public void MarkDown(string path)
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                var file = Path.Combine(path, $"{serviceInfo.Name}.md");
                var code = new StringBuilder();
                HeadDoc(serviceInfo, code);
                foreach (var api in serviceInfo.Aips.Values.Cast<ApiActionInfo>())
                {
                    ApiDoc(serviceInfo, code, api);
                }
                File.WriteAllText(file, code.ToString());
            }
        }

        private static void HeadDoc(ServiceInfo serviceInfo, StringBuilder code)
        {
            code.AppendLine($@"# {(serviceInfo.Caption ?? serviceInfo.Name)} 接口文档

{serviceInfo.Description}

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

---");
        }

        private void ApiDoc(ServiceInfo serviceInfo, StringBuilder code, ApiActionInfo api)
        {
            code.AppendLine($@"
## {(api.Caption ?? api.Name)}

**简要描述：** 
{(api.Description ?? api.Caption)}

**请求URL：** 
` http://xx.com/{serviceInfo.Name}/{api.Route}`

**请求方式：**
- POST
- GET
");
            MarkdownArgument(code, api);
            MarkdownResult(code, api);
        }

        private void MarkdownArgument(StringBuilder code, ApiActionInfo api)
        {
            if (!api.HaseArgument)
            {
                return;
            }
            code.AppendLine($@"
**参数：** 

|参数名|必选|类型|示例|说明|
|:----    |:---|:----- |:----- |-----   |");

            if (!api.Arguments.Values.First().IsBaseType)
            {
                MarkdownArgument(code, new HashSet<TypeDocument>(), api.Arguments.Values.First(), "");
            }
            else
            {
                foreach (var field in api.Arguments.Values)
                {
                    code.AppendLine($"|{field.DocName}|{(field.CanNull ? "否" : "是")}|{field.TypeName}|{(field.DocExample)}|{field.DocDesc}|");
                }
            }
            code.AppendLine($@"
**参数示例**

```json
{api.ArgumentJson()}
```
");
        }

        private void MarkdownArgument(StringBuilder code, HashSet<TypeDocument> hashSet, TypeDocument type, string space)
        {
            if (hashSet.Contains(type))
                return;
            hashSet.Add(type);
            if (type.Fields == null || type.IsEnum)
            {
                return;
            }
            foreach (var field in type.Fields.Values)
            {
                if (field.DocName.Contains('.'))
                    continue;
                code.AppendLine($"|{space}{field.DocName}|{(field.CanNull ? "否" : "是")}|{(field.IsEnum ? "int(枚举)" : field.TypeName)}|{(field.DocExample)}|{field.DocDesc}|");
                MarkdownArgument(code, hashSet, field, space + "-");
            }
        }

        private void MarkdownResult(StringBuilder code, ApiActionInfo api)
        {

            if (api.ResultInfo == null || api.ResultType == typeof(Task) || api.ResultType == typeof(void))
            {
                code.AppendLine($@"
无返回");
                return;
            }
            var reType = api.ResultType;
            if (reType.IsGenericType && reType.GetGenericTypeDefinition().IsSupperInterface(typeof(Task<>)))
            {
                reType = reType.GenericTypeArguments[0];
            }

            string json;

            if (reType.IsBaseType())
            {
                code.AppendLine($@"

 **返回参数说明** 

|类型|说明|
|-|-|
|{reType.GetTypeName()}|{api.ResultInfo.Description}|

 **返回示例**

```
{api.ResultInfo.DocExample}
```
");
                return;
            }
            code.AppendLine($@"
 **返回参数说明** ");
            var doc = api.ResultInfo;
            if (reType.IsSupperInterface(typeof(IApiResult)))
            {
                if (!reType.IsGenericType)
                {
                    code.AppendLine(@"
标准API返回格式，请参考[返回值通用说明]

**返回示例**

```json
{
    'success': true,
    'code': 0,
    'msg': '操作成功'
}
```
");
                    return;
                }
                var gbase = reType.GetGenericTypeDefinition();
                if (!gbase.IsSupperInterface(typeof(IApiResult<>)))
                {
                    code.AppendLine(@"
标准API返回格式，请参考[返回值通用说明]

**返回示例**

```json
{
    'success': true,
    'code': 0,
    'msg': '操作成功'
}
```
");
                    return;
                }
                code.AppendLine($@"
标准API返回格式，请参考[返回值通用说明]");
                reType = reType.GenericTypeArguments[0];
                if (reType.IsBaseType())
                {
                    code.AppendLine($@"
 **data格式说明** 

- 类型：{reType.GetTypeName()}
- 说明： {api.ResultInfo.Description}

**返回示例**

```json
{{
    'success': true,
    'code': 0,
    'msg': '操作成功',
    'data': '{api.ResultInfo.DocExample}'
}}
```
");
                    return;
                }

                doc = XmlDocumentDiscover.ReadEntity(reType, "result");
                if (doc == null)
                {
                    code.AppendLine(@"
 **data格式说明** 
[未找到对应类型说明，请手工写入]

**返回示例**

```json
{
    'success': true,
    'code': 0,
    'msg': '操作成功',
    'data': {}
}
```
");
                    return;
                }
                json = $@"{{
    'success': true,
    'code': 0,
    'msg': '操作成功',
    'data': {doc.JsonExample(false).Trim()}
}}";
                code.AppendLine($@"

 **data格式说明** ");
            }
            else
            {
                json = doc.JsonExample();
            }

            code.AppendLine($@"
|参数名|类型|说明|
|:-----  |:-----|-----                           |");

            MarkdownResult(code, new HashSet<TypeDocument>(), doc, "");

            code.AppendLine($@"
**返回示例**

```json
{json}
```
");
        }

        private void MarkdownResult(StringBuilder code, HashSet<TypeDocument> hashSet, TypeDocument type, string space)
        {
            if (hashSet.Contains(type))
                return;
            hashSet.Add(type);
            if (type.Fields == null || type.IsEnum)
            {
                return;
            }
            foreach (var field in type.Fields.Values)
            {
                if (field.DocName.Contains('.'))
                    continue;
                code.AppendLine($"|{space}{field.DocName}|{(field.IsEnum ? "int(枚举)" : field.TypeName)}|{field.DocDesc}|");
                MarkdownResult(code, hashSet, field, space + '-');
            }
        }
    }
}