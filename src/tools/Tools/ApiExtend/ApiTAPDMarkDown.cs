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
    public class ApiTAPDMarkDown
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
||名称||标题||类型||说明||
||isSuccess||成功标记||bool||操作成功返回true,否则根据code与msg判断结果||
||businessCode||错误码||number||见通用说明与API具体说明||
||businessMessage||消息||number||用户可见的文本消息，可用于用户提示||

### 通用错误码

||错误码||错误信息||备注||
||0 ||正确||||
||1001||用户名密码错误||||
||1002||验证码错误||图片 or 滑动验证 or 短信邮件验证码错误 ||
||1003||用户被锁定||错误次数超过3次||
||1004||错误令牌||null 令牌或者伪造令牌||
||1005||令牌过期 ||access token 过期，需要刷新token ||
||1006 ||令牌刷新失败||需要重新登录 ||
||1007 ||验证码发送失败||||
||1008 ||Code无效或已过期||||
||9001||参数错误||-||
||9002||逻辑错误||-||
||9003||业务异常||-||
||9004||系统异常||-||
||9005||网络错误||-||
||9006||执行超时||-||
||9007||拒绝访问||访问接口时无权限 ||
||9008||系统未就绪||-||
||9009||异常中止||-||
||9010||请重试请求||-||
||9011||页面不存在||-||
||9012||服务不可用||-||
||9999||未知结果||-||

---");
        }

        private void ApiDoc(ServiceInfo serviceInfo, StringBuilder code, ApiActionInfo api)
        {
            code.AppendLine($@"
## {(api.Caption ?? api.Name)}

**简要描述：** 
{(api.Description ?? api.Caption)}

**请求URL：** 
` http://xx.com/{serviceInfo.Name}/{api.Routes[0]}`

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

||参数名||必选||类型||示例||说明||");

            if (!api.Arguments.Values.First().IsBaseType)
            {
                MarkdownArgument(code, new HashSet<TypeDocument>(), api.Arguments.Values.First(), "");
            }
            else
            {
                foreach (var field in api.Arguments.Values)
                {
                    code.AppendLine($"||{field.DocName.ToLWord()}||{(field.CanNull ? "否" : "是")}||{field.TypeName}||{(field.DocExample)}||{field.DocDesc}||");
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
                code.AppendLine($"||{space}{field.DocName.ToLWord()}||{(field.CanNull ? "否" : "是")}||{(field.IsEnum ? "int(枚举)" : field.TypeName)}||{(field.DocExample)}||{field.DocDesc}||");
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

||类型||说明||
||{reType.GetTypeName()}||{api.ResultInfo.Description}||

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
    'isSuccess': true,
    'businessCode': 0,
    'businessMessage': '操作成功'
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
    'isSuccess': true,
    'businessCode': 0,
    'businessMessage': '操作成功'
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
    'isSuccess': true,
    'businessCode': 0,
    'businessMessage': '操作成功',
    'returnObj': '{api.ResultInfo.DocExample}'
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
    'isSuccess': true,
    'businessCode': 0,
    'businessMessage': '操作成功',
    'returnObj': {}
}
```
");
                    return;
                }
                json = $@"{{
    'isSuccess': true,
    'businessCode': 0,
    'businessMessage': '操作成功',
    'returnObj': {doc.JsonExample(false).Trim()}
}}";
                code.AppendLine($@"

 **data格式说明** ");
            }
            else
            {
                json = doc.JsonExample();
            }

            code.AppendLine($@"
||参数名||类型||说明||");

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
                code.AppendLine($"||{space}{field.DocName}||{(field.IsEnum ? "int(枚举)" : field.TypeName)}||{field.DocDesc}||");
                MarkdownResult(code, hashSet, field, space + '-');
            }
        }
    }
}