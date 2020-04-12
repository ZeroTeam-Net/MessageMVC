using Agebull.Common.Ioc;
using Agebull.Common.Reflection;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// Controler发现工具,解析并生成服务对象及路由对象
    /// </summary>
    public class ApiDiscover
    {
        #region API发现

        /// <summary>
        /// 主调用程序集
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// 站点文档信息
        /// </summary>
        public Dictionary<string, ServiceInfo> ServiceInfos = new Dictionary<string, ServiceInfo>();

        /// <summary>
        /// 构造
        /// </summary>
        static ApiDiscover()
        {
            XmlMember.Load(typeof(IMessageItem).Assembly);
            XmlMember.Load(typeof(ApiExecuter).Assembly);
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies()
        {
            XmlMember.Load(Assembly);

            TransportDiscories ??= IocHelper.RootProvider.GetServices<IReceiverDiscover>().ToArray();

            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IApiControler))).ToArray();
            foreach (var type in types)
            {
                FindApi(type);
            }
            RegistToZero();

            RegistDocument();
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public void Discover(Assembly assembly)
        {
            XmlMember.Load(assembly);
            Assembly = assembly;
            TransportDiscories ??= IocHelper.RootProvider.GetServices<IReceiverDiscover>().ToArray();

            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IApiControler))).ToArray();
            foreach (var type in types)
            {
                FindApi(type);
            }
        }
        private void RegistToZero()
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }

                var service = ZeroFlowControl.TryGetZeroObject(serviceInfo.Name);
                if (service == null)
                {
                    service = new ZeroService
                    {
                        IsAutoService = true,
                        ServiceName = serviceInfo.Name,
                        Serialize = SelectSerialize(serviceInfo.Serialize),
                        Receiver = serviceInfo.NetBuilder(serviceInfo.Name)
                    };// IocHelper.Create<IService>();

                    ZeroFlowControl.RegistService(service);
                }
                foreach (var api in serviceInfo.Aips)
                {
                    var info = (ApiActionInfo)api.Value;
                    service.RegistAction(api.Key, info);
                }
            }
        }

        private IReceiverDiscover[] TransportDiscories;

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        private void FindApi(Type type)
        {
            //泛型与纯虚类
            if (type.IsAbstract || type.IsGenericType)
            {
                return;
            }

            var docx = XmlMember.Find(type) ?? new XmlMember
            {
                Name = type.Name
            };
            ServiceInfo service = null;
            #region 服务类型检测

            var serializeType = type.GetCustomAttributes<SerializeTypeAttribute>().FirstOrDefault()?.Type ?? SerializeType.Json;

            foreach (var dis in TransportDiscories)
            {
                var builder = dis.DiscoverNetTransport(type, out var serviceName);
                if (builder == null)
                {
                    continue;
                }

                if (!ServiceInfos.TryGetValue(serviceName, out service))
                {
                    ServiceInfos.Add(serviceName, service = new ServiceInfo
                    {
                        Name = serviceName,
                        Type = type,
                        Serialize = serializeType,
                        NetBuilder = builder
                    });
                }
                service.Copy(docx);
                break;
            }
            if (service == null)
            {
                return;
            }
            #endregion

            #region API发现

            string routeHead = type.GetCustomAttributes<RouteAttribute>().FirstOrDefault()?.Name.SafeTrim(' ', '\t', '\r', '\n', '/');
            if (routeHead != null)
            {
                routeHead += "/";
            }

            var defPage = type.GetCustomAttributes<ApiPageAttribute>().FirstOrDefault()?.PageUrl?.SafeTrim();
            var defOption = type.GetCustomAttributes<ApiAccessOptionFilterAttribute>().FirstOrDefault()?.Option ?? ApiAccessOption.OpenAccess;
            var defCategory = type.GetCustomAttributes<CategoryAttribute>().FirstOrDefault()?.Category.SafeTrim();

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                CheckMethod(type, docx, service, routeHead, defPage, defOption, defCategory, method);
            }
            #endregion
        }
        ISerializeProxy SelectSerialize(SerializeType type)
        {
            switch (type)
            {
                case SerializeType.Json:
                    return IocHelper.Create<IJsonSerializeProxy>();

                case SerializeType.NewtonJson:
                    return new NewtonJsonSerializeProxy();

                case SerializeType.Xml:
                    return IocHelper.Create<IXmlSerializeProxy>();

                case SerializeType.Bson:
                    return IocHelper.Create<IBsonSerializeProxy>();
                default:
                    throw new NotSupportedException($"{type}序列化方式暂不支持");
            };
        }
        private void CheckMethod(Type type, XmlMember docx, ServiceInfo serviceInfo, string routeHead, string defPage, ApiAccessOption defOption, string defCategory, MethodInfo method)
        {
            var routeAttribute = method.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
            if (routeAttribute == null)
            {
                return;
            }
            if (method.Name.Length > 4 && (method.Name.IndexOf("get_", StringComparison.Ordinal) == 0 || method.Name.IndexOf("set_", StringComparison.Ordinal) == 0))
            {
                return;
            }
            var head = routeHead;
            if (routeHead == null && method.DeclaringType != type)//基类方法,即增加自动前缀
            {
                head = type.Name;
            }
            var route = routeAttribute.Name == null
                ? $"{head}{method.Name}"
                : $"{head}{routeAttribute.Name.Trim(' ', '\t', '\r', '\n', '/')}";
            var accessOption = method.GetCustomAttributes<ApiAccessOptionFilterAttribute>().FirstOrDefault();
            ApiAccessOption option;
            if (accessOption != null)
            {
                option = accessOption.Option;
            }
            else
            {
                option = defOption;
            }

            var category = method.GetCustomAttributes<CategoryAttribute>().FirstOrDefault()?.Category.SafeTrim();
            var page = method.GetCustomAttributes<ApiPageAttribute>().FirstOrDefault()?.PageUrl.SafeTrim();

            var api = new ApiActionInfo
            {
                Name = method.Name,
                ApiName = route,
                Category = category ?? defCategory ?? docx?.Caption ?? docx?.Name,
                AccessOption = option,
                ResultInfo = ReadEntity(method.ReturnType, "result"),
                PageUrl = page ?? defPage
            };
            var doc = XmlMember.Find(type, method.Name, "M");
            api.Copy(doc);
            var arg = method.GetParameters().FirstOrDefault();
            api.HaseArgument = arg != null;
            api.ResultType = method.ReturnType;

            var serializeType = method.GetCustomAttributes<SerializeTypeAttribute>().FirstOrDefault();
            var resultSerializeType = method.GetCustomAttributes<ResultSerializeTypeAttribute>().FirstOrDefault();
            var argumentSerializeType = method.GetCustomAttributes<ArgumentSerializeTypeAttribute>().FirstOrDefault();

            api.ArgumentSerializeType = argumentSerializeType?.Type ?? serializeType?.Type ?? serviceInfo.Serialize;
            api.ResultSerializeType = resultSerializeType?.Type ?? serializeType?.Type ?? serviceInfo.Serialize;

            //动态生成并编译
            var builder = new ApiMethodBuilder
            {
                Method = method,
                TypeInfo = type.GetTypeInfo(),
                ActionInfo = api
            };
            api.Action = builder.CreateMethod();
            api.HaseArgument = builder.ParameterInfo != null;
            api.IsAsync = builder.isAsync;

            if (api.HaseArgument)
            {
                api.ArgumentInfo = ReadEntity(builder.ParameterInfo.ParameterType, builder.ParameterInfo.Name ?? "argument") ?? new TypeDocument();
                api.ArgumentInfo.Name = arg.Name;
                api.ArgumentName = arg.Name;
                api.ArgumentType = arg.ParameterType;
                if (doc != null)
                {
                    api.ArgumentInfo.Caption = doc.Arguments.Values.FirstOrDefault();
                }
            }
            var paras = method.GetParameters();
            api.Arguments = paras.Length == 0
                ? new Dictionary<string, Type>()
                : paras.ToDictionary(p => p.Name, p => p.ParameterType);
            serviceInfo.Aips.Add(api.ApiName, api);
        }

        #endregion


        #region UnitTest

        /// <summary>
        /// 生成单元测试代码
        /// </summary>
        /// <param name="path"></param>
        public void NUnitCode(string path)
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                var file = Path.Combine(path, $"{serviceInfo.Name}.cs");
                if (File.Exists(file))
                    continue;
                var code = new StringBuilder();
                code.AppendLine($@"using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace {serviceInfo.Type.Namespace}.UnitTest
{{
    [TestFixture]
    public class {serviceInfo.Type.GetTypeName()}UnitTest
    {{
        [SetUp]
        public void Setup()
        {{
            ZeroApp.UseTest(IocHelper.ServiceCollection, typeof({serviceInfo.Type.GetTypeName()}).Assembly);
        }}

        [TearDown]
        public void TearDown()
        {{
            ZeroFlowControl.Shutdown();
        }}

");
                foreach (var api in serviceInfo.Aips.Values.Cast<ApiActionInfo>())
                {
                    var argCode = new StringBuilder();
                    string json = ArgumentJson(argCode, api);
                    code.AppendLine($@"

        /// <summary>
        /// {api.Caption}
        /// </summary>
        [Test]
        public async Task {api.Name}()
        {{
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {{
                ServiceName = ""{serviceInfo.Name}"",
                ApiName = ""{api.ApiName}"",
                Content = 
@""{json}""
            }});
            Assert.IsTrue(msg.State == MessageState.Success , msg.Result);
        }}
");
                }
                code.AppendLine("    }");
                code.AppendLine("}");
                File.WriteAllText(file, code.ToString());
            }
        }
        /// <summary>
        /// 生成单元测试代码
        /// </summary>
        /// <param name="path"></param>
        public void xUnitCode(string path)
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                var file = Path.Combine(path, $"{serviceInfo.Name}.cs");
                if (File.Exists(file))
                    continue;
                var code = new StringBuilder();
                code.AppendLine($@"using System;
using System.Threading.Tasks;
using Xunit;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace {serviceInfo.Type.Namespace}.UnitTest
{{
    public class {serviceInfo.Type.GetTypeName()}UnitTest : IDisposable
    {{
        public {serviceInfo.Type.GetTypeName()}UnitTest()
        {{
            ZeroApp.UseTest(IocHelper.ServiceCollection, typeof({serviceInfo.Type.GetTypeName()}).Assembly);
        }}
        void IDisposable.Dispose()
        {{
            ZeroFlowControl.Shutdown();
        }}

");
                foreach (var api in serviceInfo.Aips.Values.Cast<ApiActionInfo>())
                {
                    var argCode = new StringBuilder();
                    string json = ArgumentJson(argCode, api);
                    code.AppendLine($@"

        /// <summary>
        /// {api.Caption}
        /// </summary>
        [Fact]
        public async Task {api.Name}()
        {{
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {{
                ServiceName = ""{serviceInfo.Name}"",
                ApiName = ""{api.ApiName}"",
                Content = 
@""{json}""
            }});
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }}
");
                }
                code.AppendLine("    }");
                code.AppendLine("}");
                File.WriteAllText(file, code.ToString());
            }
        }

        private string ArgumentJson(StringBuilder argCode, ApiActionInfo info)
        {
            string json;
            if (info.Arguments.Count != 1)
            {
                foreach (var arg in info.Arguments)
                {
                    if (!arg.Value.IsBaseType())
                    {
                        json = ClassArgumentJson(arg.Value).Trim(',', ' ', '\t', '\r', '\n');
                    }
                    else
                    {
                        json = JsonValue(arg.Value).Trim(',', ' ', '\t', '\r', '\n');
                    }
                    argCode.AppendLine($"\"{ arg.Key }\" : {json},");
                }
                json = argCode.ToString().Replace("\"", "\"\"").Trim(',', ' ', '\t', '\r', '\n');
                return $@"{{
{json.SpaceLine(4)}
}}";
            }
            else
            {
                var arg = info.Arguments.First();
                if (!arg.Value.IsBaseType())
                {
                    return ClassArgumentJson(arg.Value).Replace("\"", "\"\"");
                }
                else
                {
                    json = JsonValue(arg.Value).Replace("\"", "\"\"").Trim(',', ' ', '\t', '\r', '\n');
                    return $@"{{
    ""{ arg.Key }"" : {json}
}}";
                }
            }
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        string ClassArgumentJson(Type type)
        {
            var argCode = new StringBuilder();
            argCode.AppendLine("{");
            foreach (var field in type.GetFields(flags))
            {
                var json = field.GetAttribute<JsonPropertyAttribute>();
                if (json == null && !field.IsPublic)
                    continue;
                argCode.Append(JsonValue(field.FieldType, json?.PropertyName ?? field.Name));
                argCode.AppendLine(",");
            }
            foreach (var field in type.GetProperties(flags))
            {
                var json = field.GetAttribute<JsonPropertyAttribute>();
                if (json == null && (!field.CanWrite || !field.SetMethod.IsPublic))
                    continue;
                argCode.Append(JsonValue(field.PropertyType, json?.PropertyName ?? field.Name));
                argCode.AppendLine(",");
            }
            return argCode.ToString().Trim(',', ' ', '\t', '\r', '\n') + "\r\n}";
        }

        string JsonValue(Type type, string name)
        {
            return $"    \"{name}\" : {JsonValue(type)}";
        }

        string JsonValue(Type type)
        {
            if (type == typeof(string))
                return "\"string\"";
            if (type == typeof(bool))
                return "true";
            if (type == typeof(Guid))
                return $"\"{Guid.NewGuid().ToString()}\"";
            if (type.IsValueType)
                return "0";
            else
                return ClassArgumentJson(type).SpaceLine(4);
        }

        string ClassArgumentCode(Type type)
        {
            var argCode = new StringBuilder();
            argCode.AppendLine($"ArgumentData = new {type.GetFullTypeName()}{{");
            foreach (var field in type.GetFields(flags))
            {
                var json = field.GetAttribute<JsonPropertyAttribute>();
                if (json == null || !field.IsPublic)
                    continue;
                argCode.Append(' ', 4);
                argCode.Append($"");

            }
            return argCode.ToString();
        }

        #endregion

        #region XML文档

        private void RegistDocument()
        {
            //foreach (var serviceInfo in StationInfo.Values)
            //{
            //    if (serviceInfo.Aips.Count == 0)
            //        continue;
            //    if (!ZeroAppOption.Instance.Documents.TryGetValue(serviceInfo.Name, out var doc))
            //    {
            //        ZeroAppOption.Instance.Documents.Add(serviceInfo.Name, serviceInfo);
            //        continue;
            //    }
            //    foreach (var api in serviceInfo.Aips)
            //    {
            //        if (!doc.Aips.ContainsKey(api.Key))
            //        {
            //            doc.Aips.Add(api.Key, api.Value);
            //        }
            //        else
            //        {
            //            doc.Aips[api.Key] = api.Value;
            //        }
            //    }
            //}
        }

        private bool IsLetter(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

        private readonly Dictionary<Type, TypeDocument> _typeDocs2 = new Dictionary<Type, TypeDocument>();

        private readonly Dictionary<Type, TypeDocument> _typeDocs = new Dictionary<Type, TypeDocument>();


        private TypeDocument ReadEntity(Type type, string name)
        {
            var typeDocument = new TypeDocument
            {
                Name = name,
                TypeName = ReflectionHelper.GetTypeName(type),
                ClassName = ReflectionHelper.GetTypeName(type),
                ObjectType = ObjectType.Object
            };

            //if (typeDocs.TryGetValue(type, out var doc))
            //    return doc;
            ReadEntity(typeDocument, type);
            return typeDocument;
        }
        private void ReadEntity(TypeDocument typeDocument, Type type)
        {
            if (type == null || type.IsAutoClass || !IsLetter(type.Name[0]) ||
                type.IsInterface || type.IsMarshalByRef || type.IsCOMObject ||
                type == typeof(object) || type == typeof(void) ||
                type == typeof(ValueType) || type == typeof(Type) || type == typeof(Enum) ||
                type.Namespace == "System" || type.Namespace?.Contains("System.") == true)
            {
                return;
            }

            if (_typeDocs.TryGetValue(type, out var doc))
            {
                foreach (var field in doc.Fields)
                {
                    if (typeDocument.Fields.ContainsKey(field.Key))
                    {
                        typeDocument.Fields[field.Key] = field.Value;
                    }
                    else
                    {
                        typeDocument.Fields.Add(field.Key, field.Value);
                    }
                }
                return;
            }
            if (_typeDocs2.ContainsKey(type))
            {
                return;
            }

            _typeDocs2.Add(type, typeDocument);
            if (type.IsArray)
            {
                ReadEntity(typeDocument, type.Assembly.GetType(type.FullName.Split('[')[0]));
                return;
            }
            if (type.IsGenericType && !type.IsValueType &&
                type.GetGenericTypeDefinition().GetInterface(typeof(IEnumerable<>).FullName) != null)
            {
                ReadEntity(typeDocument, type.GetGenericArguments().Last());
                return;
            }

            XmlMember.Find(type);
            if (type.IsEnum)
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (field.IsSpecialName)
                    {
                        continue;
                    }
                    var info = CheckMember(typeDocument, type, field, field.FieldType, false, false, false);
                    if (info != null)
                    {
                        info.TypeName = "int";
                        info.Example = ((int)field.GetValue(null)).ToString();
                        info.JsonName = null;
                    }
                }
                _typeDocs.Add(type, new TypeDocument
                {
                    fields = typeDocument.fields?.ToDictionary(p => p.Key, p => p.Value)
                });
                typeDocument.Copy(XmlMember.Find(type));
                return;
            }

            var dc = type.GetCustomAttributes<DataContractAttribute>();
            var jo = type.GetCustomAttributes<JsonObjectAttribute>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, property, property.PropertyType, jo != null, dc != null);
            }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!char.IsLetter(field.Name[0]) || field.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, field, field.FieldType, jo != null, dc != null);
            }

            _typeDocs.Add(type, new TypeDocument
            {
                fields = typeDocument.fields?.ToDictionary(p => p.Key, p => p.Value)
            });
            typeDocument.Copy(XmlMember.Find(type));
        }

        private TypeDocument CheckMember(TypeDocument document, Type parent, MemberInfo member, Type memType, bool json, bool dc, bool checkBase = true)
        {
            if (document.Fields.ContainsKey(member.Name))
            {
                return null;
            }

            var jp = member.GetCustomAttributes<JsonPropertyAttribute>().FirstOrDefault();
            var dm = member.GetCustomAttributes<DataMemberAttribute>().FirstOrDefault();
            if (json)
            {
                var ji = member.GetCustomAttributes<JsonIgnoreAttribute>();
                if (ji != null)
                {
                    return null;
                }
                if (jp == null)
                {
                    return null;
                }
            }
            else if (dc)
            {
                var id = member.GetCustomAttributes<IgnoreDataMemberAttribute>();
                if (id != null)
                {
                    return null;
                }
            }

            var field = new TypeDocument();
            var doc = XmlMember.Find(parent, member.Name);
            field.Copy(doc);
            var isArray = false;
            var isDictionary = false;
            try
            {
                var type = memType;
                if (memType.IsArray)
                {
                    isArray = true;
                    type = type.Assembly.GetType(type.FullName.Split('[')[0]);
                }
                else if (type.IsGenericType)
                {
                    if (memType.GetGenericTypeDefinition().IsSupperInterface(typeof(IEnumerable<>)))
                    {
                        isArray = true;
                        type = type.GetGenericArguments()[0];
                    }
                    else if (memType.GetGenericTypeDefinition().IsSupperInterface(typeof(ICollection<>)))
                    {
                        isArray = true;
                        type = type.GetGenericArguments()[0];
                    }
                    else if (memType.GetGenericTypeDefinition().IsSupperInterface(typeof(IDictionary<,>)))
                    {
                        var fields = type.GetGenericArguments();
                        field.Fields.Add("Key", ReadEntity(fields[0], "Key"));
                        field.Fields.Add("Value", ReadEntity(fields[1], "Value"));
                        isDictionary = true;
                        checkBase = false;
                    }
                }
                if (type.IsEnum)
                {
                    if (checkBase)
                    {
                        field = ReadEntity(type, member.Name);
                    }

                    field.ObjectType = ObjectType.Base;
                    field.IsEnum = true;
                }
                else if (type.IsBaseType())
                {
                    field.ObjectType = ObjectType.Base;
                }
                else if (!isDictionary)
                {
                    if (checkBase)
                    {
                        field = ReadEntity(type, member.Name);
                    }

                    field.ObjectType = ObjectType.Object;
                }
                field.TypeName = ReflectionHelper.GetTypeName(type);
            }
            catch
            {
                field.TypeName = "object";
            }
            if (isArray)
            {
                field.TypeName += "[]";
                field.ObjectType = ObjectType.Array;
            }
            else if (isDictionary)
            {
                field.TypeName = "Dictionary";
                field.ObjectType = ObjectType.Dictionary;
            }

            field.Name = member.Name;
            field.JsonName = member.Name;
            field.ClassName = ReflectionHelper.GetTypeName(memType);

            if (!string.IsNullOrWhiteSpace(dm?.Name))
            {
                field.JsonName = dm.Name;
            }

            if (!string.IsNullOrWhiteSpace(jp?.PropertyName))
            {
                field.JsonName = jp.PropertyName;
            }

            var rule = member.GetCustomAttributes<DataRuleAttribute>().FirstOrDefault();
            if (rule != null)
            {
                field.CanNull = rule.CanNull;
                field.Regex = rule.Regex;
                field.Min = rule.Min;
                field.Max = rule.Max;
            }
            document.Fields.Add(member.Name, field);

            return field;
        }
        #endregion
    }
}