using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using ZeroTeam.MessageMVC.ApiDocuments;
using Agebull.Common.Reflection;
using Agebull.EntityModel.Common;
using Newtonsoft.Json;
using Agebull.Common.Ioc;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// Controler发现工具,解析并生成服务对象及路由对象
    /// </summary>
    internal class ApiDiscover
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
        public ApiDiscover()
        {
            XmlMember.Load(GetType().Assembly);
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies(Type type)
        {
            XmlMember.Load(type.Assembly);

            TransportDiscories = IocHelper.RootProvider.GetServices<ITransportDiscory>().ToArray();
            if (TransportDiscories.Length == 0)
                TransportDiscories = new[] { new TransportDiscory() };

            FindApi(type, false);
            RegistToZero();

            RegistDocument();
        }
        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies()
        {
            XmlMember.Load(Assembly);
            //if (!StationInfo.TryGetValue(StationName, out _defStation))
            //    StationInfo.Add(StationName, _defStation = new StationInfo
            //    {
            //        Name = StationName
            //    });
            TransportDiscories = IocHelper.RootProvider.GetServices<ITransportDiscory>().ToArray();
            if (TransportDiscories.Length == 0)
                TransportDiscories = new[] { new TransportDiscory() };
            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IApiControler))).ToArray();
            foreach (var type in types)
            {
                FindApi(type, false);
            }
            RegistToZero();

            RegistDocument();
        }

        private void RegistToZero()
        {
            foreach (var sta in ServiceInfos.Values)
            {
                if (sta.Aips.Count == 0)
                    continue;
                var station = (ZeroService)ZeroFlowControl.TryGetZeroObject(sta.Name);
                if (station == null)
                {
                    ZeroFlowControl.RegistService(station = new ZeroService
                    {
                        InstanceName = sta.Name,
                        ServiceName = sta.Name,
                        TransportBuilder = sta.NetBuilder
                    });
                }
                foreach (var api in sta.Aips)
                {
                    var info = (ApiActionInfo)api.Value;
                    var a = new ApiAction
                    {
                        Name = api.Key,
                        Function = info.Action,
                        Access = info.AccessOption,
                        ArgumentType = info.ArgumentType,
                        ResultType = info.ResultType,
                        IsAsync = info.IsAsync
                    };
                    station.RegistAction(api.Key, a, info);
                }
            }
        }
        ITransportDiscory[] TransportDiscories;

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        /// <param name="onlyDoc"></param>
        private void FindApi(Type type, bool onlyDoc)
        {
            //泛型与纯虚类
            if (type.IsAbstract || type.IsGenericType)
                return;

            var docx = XmlMember.Find(type) ?? new XmlMember
            {
                Name = type.Name
            };
            ServiceInfo service = null;
            #region 服务类型检测
            foreach (var dis in TransportDiscories)
            {
                var builder = dis.DiscoryNetTransport(type, out var name);
                if (builder == null)
                    continue;

                if (!ServiceInfos.TryGetValue(name, out service))
                {
                    ServiceInfos.Add(name, service = new ServiceInfo
                    {
                        Name = name,
                        NetBuilder = builder
                    });
                }
                service.Copy(docx);
                break;
            }
            if (service == null)
                return;
            #endregion

            #region API发现
            string routeHead = type.GetCustomAttribute<RouteAttribute>()?.Name.SafeTrim(' ', '\t', '\r', '\n', '/');
            if (routeHead != null)
                routeHead += "/";
            var defPage = type.GetCustomAttribute<ApiPageAttribute>()?.PageUrl?.SafeTrim();
            var defOption = type.GetCustomAttribute<ApiAccessOptionFilterAttribute>()?.Option;
            var defCategory = type.GetCustomAttribute<CategoryAttribute>()?.Category.SafeTrim();

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                CheckMethod(type, onlyDoc, docx, service, routeHead, defPage, defOption, defCategory, method);
            }
            #endregion
        }

        private void CheckMethod(Type type, bool onlyDoc, XmlMember docx, ServiceInfo station, string routeHead, string defPage, ApiAccessOption? defOption, string defCategory, MethodInfo method)
        {
            var route = method.GetCustomAttribute<RouteAttribute>();
            if (route == null)
            {
                //ZeroTrace.WriteError("ApiDiscover", "exclude", station.Name, type.Name, method.Name);
                return;
            }
            if (method.Name.Length > 4 && (method.Name.IndexOf("get_", StringComparison.Ordinal) == 0 || method.Name.IndexOf("set_", StringComparison.Ordinal) == 0))
                return;
            //if (method.GetParameters().Length > 1)
            //{
            //    ZeroTrace.WriteError("ApiDiscover", "argument size must 0 or 1", station.Name, type.Name, method.Name);
            //    return;
            //}

            var head = routeHead;
            if (routeHead == null && method.DeclaringType != type)//基类方法,即增加自动前缀
            {
                head = type.Name;
            }
            var name = route.Name == null
                ? $"{head}{method.Name}"
                : $"{head}{route.Name.Trim(' ', '\t', '\r', '\n', '/')}";
            var accessOption = method.GetCustomAttribute<ApiAccessOptionFilterAttribute>();
            ApiAccessOption option;
            if (accessOption != null)
                option = accessOption.Option;
            else if (defOption != null)
                option = defOption.Value;
            else
                option = ApiAccessOption.Internal | ApiAccessOption.Customer | ApiAccessOption.Employe | ApiAccessOption.ArgumentCanNil;

            var category = method.GetCustomAttribute<CategoryAttribute>()?.Category.SafeTrim();
            var page = method.GetCustomAttribute<ApiPageAttribute>()?.PageUrl.SafeTrim();

            var api = new ApiActionInfo
            {
                Name = method.Name,
                ApiName = name,
                RouteName = name,
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

            //动态生成并编译
            api.Action = CreateMethod(type.GetTypeInfo(),
                    method.Name,
                    out var argInfo,
                    out api.IsAsync);
            api.HaseArgument = argInfo != null;
            if (api.HaseArgument)
            {
                api.ArgumentInfo = ReadEntity(argInfo.ParameterType, argInfo.Name ?? "argument") ?? new TypeDocument();
                api.ArgumentInfo.Name = arg.Name;
                api.ArgumentType = arg.ParameterType;
                if (doc != null)
                    api.ArgumentInfo.Caption = doc.Arguments.Values.FirstOrDefault();
            }
            station.Aips.Add(api.RouteName, api);
        }

        #endregion

        #region Api调用方法生成


        /// <summary>生成动态匿名调用内部方法（参数由TArg转为实际类型后调用，并将调用返回值转为TRes）</summary>
        /// <param name="callInfo">调用对象类型</param>
        /// <param name="isAsync">此调用是否异步方法</param>
        /// <param name="methodName">原始调用方法</param>
        /// <param name="argInfo">参数类型</param>
        /// <returns>匿名委托</returns>
        public static Func<object, object> CreateMethod(TypeInfo callInfo, string methodName, out ParameterInfo argInfo, out bool isAsync)
        {
            var method = callInfo.GetMethod(methodName);
            if (method == null)
                throw new ArgumentException("类型" + callInfo.FullName + "没有名称为" + methodName + "的方法");

            var parameters = method.GetParameters();
            //if (parameters.Length > 1)
            //    throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "参数不是一个");
            //if (argInfo != null && parameters[0].ParameterType != argInfo)
            //    throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "唯一参数不为" + argInfo.FullName);

            //TypeInfo argInfo,

            var dynamicMethod = new DynamicMethod(methodName, typeof(object), new[]
            {
                typeof(object)
            });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            //如果修补操作码，则填充空间。 尽管可能消耗处理周期，但未执行任何有意义的操作。
            ilGenerator.Emit(OpCodes.Nop);


            //new Controler;
            var constructor = callInfo.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                ilGenerator.Emit(OpCodes.Newobj, constructor);
            }
            else
            {
                //构造参数
                var info = callInfo.GetConstructors()[0];
                List<LocalBuilder> locals = new List<LocalBuilder>();
                foreach (var parameter in info.GetParameters())
                {
                    var ca = parameter.GetCustomAttribute<FromConfigAttribute>();
                    if (ca != null)
                        ConfigCreate(ilGenerator, ca.Name, parameter.ParameterType);
                    else
                    {
                        var sa = parameter.GetCustomAttribute<FromServicesAttribute>();
                        if (sa != null)
                            IocCreate(ilGenerator, parameter.ParameterType);
                        else if (parameter.ParameterType == typeof(IServiceCollection))
                            ServiceCollection(ilGenerator);
                        else if (parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(ILogger<>))
                            Logger(ilGenerator, parameter.ParameterType);
                        else
                            IocCreate(ilGenerator, parameter.ParameterType);
                    }
                    var builder = ilGenerator.DeclareLocal(parameter.ParameterType.GetTypeInfo());
                    ilGenerator.Emit(OpCodes.Stloc, builder);
                    locals.Add(builder);
                }
                foreach (var builder in locals)
                {
                    ilGenerator.Emit(OpCodes.Ldloc, builder);
                }
                ilGenerator.Emit(OpCodes.Newobj, info);
            }
            //声明局部变量。
            var controler = ilGenerator.DeclareLocal(callInfo);
            //从计算堆栈的顶部弹出当前值并将其存储到指定索引处的局部变量列表中。
            ilGenerator.Emit(OpCodes.Stloc, controler);

            //构造属性
            var properties = callInfo.GetProperties();
            foreach (var pro in properties)
            {
                var ca = pro.GetCustomAttribute<FromConfigAttribute>();
                if (ca != null)
                {
                    ConfigCreate(ilGenerator, ca.Name, pro.PropertyType);
                }
                else
                {
                    var sa = pro.GetCustomAttribute<FromServicesAttribute>();
                    if (sa != null)
                    {
                        IocCreate(ilGenerator, pro.PropertyType);
                    }
                    else
                    {
                        continue;
                    }
                }
                var b = ilGenerator.DeclareLocal(pro.PropertyType);
                ilGenerator.Emit(OpCodes.Stloc, b);
                ilGenerator.Emit(OpCodes.Ldloc, controler);
                ilGenerator.Emit(OpCodes.Ldloc, b);
                ilGenerator.Emit(OpCodes.Callvirt, pro.GetSetMethod());
            }

            argInfo = null;
            //调用方法 controler.Api();
            List<LocalBuilder> paras = new List<LocalBuilder>();
            foreach (var parameter in parameters)
            {
                //第一个不特殊构造的
                LocalBuilder arg;
                //第一个不特殊构造的
                if (argInfo == null &&
                    !parameter.ParameterType.IsInterface &&
                    parameter.GetCustomAttribute<FromConfigAttribute>() == null &&
                    parameter.GetCustomAttribute<FromServicesAttribute>() == null)
                {
                    argInfo = parameter;
                    ilGenerator.Emit(OpCodes.Ldarg, 0);
                    ilGenerator.Emit(OpCodes.Castclass, parameter.ParameterType.GetTypeInfo());
                }
                else
                {
                    var ca = parameter.GetCustomAttribute<FromConfigAttribute>();
                    if (ca != null)
                    {
                        ConfigCreate(ilGenerator, ca.Name, parameter.ParameterType);
                    }
                    else
                    {
                        var sa = parameter.GetCustomAttribute<FromServicesAttribute>();
                        if (sa != null)
                        {
                            IocCreate(ilGenerator, parameter.ParameterType);
                        }
                        else
                        {
                            ilGenerator.Emit(OpCodes.Newobj, parameter.ParameterType.GetConstructor(Type.EmptyTypes));
                        }
                    }
                }
                arg = ilGenerator.DeclareLocal(parameter.ParameterType.GetTypeInfo());
                ilGenerator.Emit(OpCodes.Stloc, arg);
                paras.Add(arg);
            }

            ilGenerator.Emit(OpCodes.Ldloc, controler);
            foreach (var builder in paras)
            {
                ilGenerator.Emit(OpCodes.Ldloc, builder);
            }
            ilGenerator.Emit(OpCodes.Callvirt, method);
            //对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
            var resInfo = typeof(object).GetTypeInfo();
            if (method.ReturnType == null || method.ReturnType == typeof(void))
            {
                isAsync = false;
                //空值入栈
                var local4 = ilGenerator.DeclareLocal(resInfo);
                ilGenerator.Emit(OpCodes.Ldnull, local4);
            }
            else
            {
                isAsync = method.ReturnType.IsSubclassOf(typeof(Task));
                //取返回值
                var local3 = ilGenerator.DeclareLocal(method.ReturnType);
                ilGenerator.Emit(OpCodes.Stloc, local3);
                ilGenerator.Emit(OpCodes.Ldloc, local3);
                ilGenerator.Emit(OpCodes.Castclass, resInfo);
                //返回值入栈
                var local4 = ilGenerator.DeclareLocal(resInfo);
                ilGenerator.Emit(OpCodes.Stloc, local4);
                ilGenerator.Emit(OpCodes.Ldloc, local4);
            }
            ilGenerator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
        }

        static void ServiceCollection(ILGenerator ilGenerator)
        {
            var method = typeof(IocHelper).GetProperty($"ServiceCollection").GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
        }

        static void Logger(ILGenerator ilGenerator, Type type)
        {
            var method = typeof(IocHelper).GetProperty($"LoggerFactory").GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
            var local = ilGenerator.DeclareLocal(typeof(ILoggerFactory));
            ilGenerator.Emit(OpCodes.Stloc, local);
            ilGenerator.Emit(OpCodes.Ldloc, local);
            var methods = typeof(LoggerFactoryExtensions).GetMethods();
            method = methods.First(p => p.Name == "CreateLogger" && p.GetParameters().Length == 1);
            method = method.MakeGenericMethod(type);
            ilGenerator.Emit(OpCodes.Call, method);
        }

        static void ConfigCreate(ILGenerator ilGenerator, string name, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldstr, name);
            var method = typeof(ConfigurationManager).GetMethod($"Option").MakeGenericMethod(type);
            ilGenerator.Emit(OpCodes.Call, method);
        }
        static void IocCreate(ILGenerator ilGenerator, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var method = typeof(IocHelper).GetMethod($"GetServices").MakeGenericMethod(type.GetGenericArguments()[0]);
                ilGenerator.Emit(OpCodes.Call, method);
            }
            else
            {
                var method = typeof(IocHelper).GetMethod($"Create").MakeGenericMethod(type);
                ilGenerator.Emit(OpCodes.Call, method);
            }
        }

        #endregion

        #region XML文档

        private void RegistDocument()
        {
            //foreach (var sta in StationInfo.Values)
            //{
            //    if (sta.Aips.Count == 0)
            //        continue;
            //    if (!ZeroFlowControl.Config.Documents.TryGetValue(sta.Name, out var doc))
            //    {
            //        ZeroFlowControl.Config.Documents.Add(sta.Name, sta);
            //        continue;
            //    }
            //    foreach (var api in sta.Aips)
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
                return;
            if (_typeDocs.TryGetValue(type, out var doc))
            {
                foreach (var field in doc.Fields)
                {
                    if (typeDocument.Fields.ContainsKey(field.Key))
                        typeDocument.Fields[field.Key] = field.Value;
                    else
                        typeDocument.Fields.Add(field.Key, field.Value);
                }
                return;
            }
            if (_typeDocs2.ContainsKey(type))
            {
                ZeroTrace.WriteError("ReadEntity", "over flow", type.Name);

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

            var dc = type.GetCustomAttribute<DataContractAttribute>();
            var jo = type.GetCustomAttribute<JsonObjectAttribute>();

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
                return null;
            var jp = member.GetCustomAttribute<JsonPropertyAttribute>();
            var dm = member.GetCustomAttribute<DataMemberAttribute>();
            if (json)
            {
                var ji = member.GetCustomAttribute<JsonIgnoreAttribute>();
                if (ji != null)
                {
                    return null;
                }
                if (jp == null)
                    return null;
            }
            else if (dc)
            {
                var id = member.GetCustomAttribute<IgnoreDataMemberAttribute>();
                if (id != null)
                    return null;
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
                        field = ReadEntity(type, member.Name);
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
                        field = ReadEntity(type, member.Name);
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
                field.JsonName = dm.Name;
            if (!string.IsNullOrWhiteSpace(jp?.PropertyName))
                field.JsonName = jp.PropertyName;
            var rule = member.GetCustomAttribute<DataRuleAttribute>();
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