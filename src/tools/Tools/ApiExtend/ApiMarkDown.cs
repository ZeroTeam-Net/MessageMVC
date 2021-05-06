using Agebull.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Documents;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// API扩展Markdown文档生成工具
    /// </summary>
    public class ApiMarkDown
    {
        /// <summary>
        /// 生成Markdown文档(在当前目录的md目录下)
        /// </summary>
        public static void CreateMarkdown(params Type[] types)
        {
            foreach (var type in types)
            {
                ApiDiscover discover = new();
                discover.Discover(type);

                var path = IOHelper.CheckPath(Environment.CurrentDirectory, "md");

                var extend = new ApiMarkDown
                {
                    ServiceInfos = discover.ServiceInfos
                };
                extend.MarkDown(path);
            }
        }
        /// <summary>
        /// 生成Markdown文档(在当前目录的md目录下)
        /// </summary>
        public static void CreateMarkdown(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                ApiDiscover discover = new();
                discover.Discover(assembly);

                var path = IOHelper.CheckPath(Environment.CurrentDirectory, "md");

                var extend = new ApiMarkDown
                {
                    ServiceInfos = discover.ServiceInfos
                };
                extend.MarkDown(path);
            }
        }

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

                foreach (var apis in serviceInfo.Aips.Values.Cast<ApiActionInfo>().GroupBy(p => p.ControllerCaption ?? p.ControllerName))
                {
                    var first = apis.First();
                    HeadDoc(serviceInfo, first, code);
                    foreach (var api in apis)
                        ApiDoc(serviceInfo, code, api);
                }
                File.WriteAllText(file, code.ToString());
            }
        }

        private static void HeadDoc(ServiceInfo serviceInfo, ApiActionInfo api, StringBuilder code)
        {
            code.AppendLine($@"# {(api.ControllerCaption ?? api.ControllerName)} 
---
");
        }

        private void ApiDoc(ServiceInfo serviceInfo, StringBuilder code, ApiActionInfo api)
        {
            code.Append($@"
## {api.ControllerCaption ?? api.ControllerName}-{(api.Caption ?? api.Name)}
> {api.ControllerName}

**简要描述：** 
{(api.Description ?? api.Caption)}


**请求URL：**");
            foreach (var path in api.Routes)
                code.AppendLine($@"
`http://xx.com/{serviceInfo.Name}/{path}`");

            code.AppendLine($@"
**请求方式：**
- POST
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
|:----|:---|:-----|:-----|-----|");

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