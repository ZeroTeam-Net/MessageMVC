using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
        #region 过程参数

        ILogger logger;
        /// <summary>
        /// 构造
        /// </summary>
        public ApiDiscover()
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger<ApiDiscover>();
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public void Discover(Assembly assembly)
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger<ApiDiscover>();
            XmlMember.Load(assembly);
            Assembly = assembly;
            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IApiController))).ToArray();
            foreach (var type in types)
            {
                FindApi(type);
            }
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public void Discover(Type type)
        {
            logger = DependencyHelper.LoggerFactory.CreateLogger<ApiDiscover>();
            Assembly = type.Assembly;
            XmlMember.Load(Assembly);
            if (type.IsSupperInterface(typeof(IApiController)))
            {
                FindApi(type);
            }
        }

        /// <summary>
        /// 主调用程序集
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// 站点文档信息
        /// </summary>
        public readonly Dictionary<string, ServiceInfo> ServiceInfos = new Dictionary<string, ServiceInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 构造
        /// </summary>
        static ApiDiscover()
        {
            XmlMember.Load(typeof(IMessageItem).Assembly);
            XmlMember.Load(typeof(ApiExecuter).Assembly);
        }

        #endregion

        #region 程序集排除

        private static readonly HashSet<string> knowAssemblies = new HashSet<string>();

        static bool CheckAssembly(string file)
        {
            try
            {
                if (file.IsBlank())
                {
                    return false;
                }
                if (knowAssemblies.Contains(file))
                {
                    return false;
                }
                knowAssemblies.Add(file);

                var name = Path.GetFileName(file);
                if (knowAssemblies.Contains(name))
                {
                    return false;
                }
                knowAssemblies.Add(name);

                switch (name.Split('.', 2)[0])
                {
                    case "System":
                    case "Microsoft":
                    case "NuGet":
                    case "Newtonsoft":
                        return false;
                }

                if (name.IsFirst("netstandard") ||
                    name.IsFirst("BeetleX") ||
                    name.IsFirst("Agebull.Common.") ||
                    name.IsMe("CSRedis") ||
                    name.IsMe("RabbitMQ.Client") ||
                    name.IsMe("Confluent.Kafka") ||
                    name.IsMe("ZeroTeam.MessageMVC.Core") ||
                    name.IsMe("ZeroTeam.MessageMVC.Abstractions") ||
                    name.IsMe("ZeroTeam.MessageMVC.Tools") ||
                    name.IsMe("ZeroTeam.MessageMVC.Consul") ||
                    name.IsMe("ZeroTeam.MessageMVC.ApiContract") ||
                    name.IsMe("ZeroTeam.MessageMVC.Kafka") ||
                    name.IsMe("ZeroTeam.MessageMVC.Tcp") ||
                    name.IsMe("ZeroTeam.MessageMVC.Http") ||
                    name.IsMe("ZeroTeam.MessageMVC.RabbitMQ") ||
                    name.IsMe("ZeroTeam.MessageMVC.RedisMQ") ||
                    name.IsMe("ZeroTeam.MessageMVC.PageInfoAutoSave")
                    )
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                
                return false;
            }
            return true;
        }
        #endregion

        #region 程序集发现

        /// <summary>
        ///     发现
        /// </summary>
        public static void FindAppDomain()
        {
            FindApies(AppDomain.CurrentDomain.GetAssemblies());

            var path = Path.GetDirectoryName(typeof(ApiDiscover).Assembly.Location);
            var files = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (!CheckAssembly(file))
                    continue;
                FindApies(Assembly.LoadFile(file));
            }
        }

        /// <summary>
        ///     发现
        /// </summary>
        public static void FindApies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic)
                    continue;
                if (!CheckAssembly( assembly.Location))
                    continue;
                FindApies(assembly);
            }
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public static void FindApies(Assembly asm)
        {
            var discover = new ApiDiscover
            {
                Assembly = asm
            };
            try
            {
                XmlMember.Load(asm);
                discover.FindApies();
            }
            catch (Exception e2)
            {
                discover.logger.Debug(e2.ToString());
            }
        }

        #endregion

        #region 注册API

        private void RegistToZero()
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                logger.Debug(() => $"【注册API】 {serviceInfo.Name}");

                var service = ZeroFlowControl.TryGetZeroObject(serviceInfo.Name);
                if (service == null)
                {
                    var receiver = serviceInfo.NetBuilder(serviceInfo.Name);
                    if (receiver == null || receiver is EmptyService)
                        service = new EmptyService
                        {
                            IsAutoService = true,
                            ServiceName = serviceInfo.Name,
                            Serialize = serviceInfo.Serialize switch
                            {
                                SerializeType.Json => DependencyHelper.GetService<IJsonSerializeProxy>(),
                                SerializeType.NewtonJson => new NewtonJsonSerializeProxy(),
                                SerializeType.Xml => DependencyHelper.GetService<IXmlSerializeProxy>(),
                                SerializeType.Bson => DependencyHelper.GetService<IBsonSerializeProxy>(),
                                _ => DependencyHelper.GetService<IJsonSerializeProxy>(),
                            }
                        };
                    else
                        service = new ZeroService
                        {
                            IsAutoService = true,
                            ServiceName = serviceInfo.Name,
                            Receiver = receiver,
                            Serialize = serviceInfo.Serialize switch
                            {
                                SerializeType.Json => DependencyHelper.GetService<IJsonSerializeProxy>(),
                                SerializeType.NewtonJson => new NewtonJsonSerializeProxy(),
                                SerializeType.Xml => DependencyHelper.GetService<IXmlSerializeProxy>(),
                                SerializeType.Bson => DependencyHelper.GetService<IBsonSerializeProxy>(),
                                _ => DependencyHelper.GetService<IJsonSerializeProxy>(),
                            }
                        };

                    ZeroFlowControl.RegistService(ref service);
                }
                foreach (var api in serviceInfo.Aips)
                {
                    try
                    {
                        var info = (ApiActionInfo)api.Value;
                        if (api.Key == "*")
                        {
                            logger.Debug(() => $"[注册接口] {serviceInfo.Name}/* => {info.Caption} {info.ControllerName}.{info.Name}");
                            service.RegistWildcardAction(info);
                        }
                        else if (!service.RegistAction(api.Key, info))
                        {
                            logger.Error($"[注册接口]失败，因为路由名称已存在 {serviceInfo.Name}/{api.Key} => {info.Caption} {info.ControllerName}.{info.Name}");
                        }
                        else
                        {
                            logger.Debug(() => $"[注册接口] {serviceInfo.Name}/{api.Key} => {info.Caption} {info.ControllerName}.{info.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Exception(ex, api.Key);
                    }
                }

            }
        }
        #endregion

        #region 查找API

        /// <summary>
        /// 查找API
        /// </summary>
        void FindApies()
        {
            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IApiController))).ToArray();
            if (types.Length == 0)
                return;
            logger.Information("【解析程序集】{asm}", Assembly.FullName);
            foreach (var type in types)
            {
                FindApi(type);
            }
            RegistToZero();
        }

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
            logger.Debug("【解析类型】 {asm}", type.FullName);

            #region 服务类型检测

            var serializeType = GetCustomAttribute<SerializeTypeAttribute>(type)?.Type ?? SerializeType.Json;

            var builder = type.DiscoverNetTransport(out var serviceName);
            if (builder == null)
            {
                return;
            }

            if (!ServiceInfos.TryGetValue(serviceName, out ServiceInfo service))
            {
                ServiceInfos.Add(serviceName, service = new ServiceInfo
                {
                    Name = serviceName,
                    Type = type,
                    Serialize = serializeType,
                    NetBuilder = builder
                });
            }
            var docx = XmlMember.Find(type) ?? new XmlMember
            {
                Name = type.Name
            };

            service.Copy(docx);

            #endregion

            #region API发现

            string routeHead = GetCustomAttribute<RouteAttribute>(type)?.Name.SafeTrim(' ', '\t', '\r', '\n', '/');
            if (routeHead != null)
            {
                routeHead += "/";
            }

            var defPage = GetCustomAttribute<ApiPageAttribute>(type)?.PageUrl?.SafeTrim();
            var defOption = GetCustomAttribute<ApiOptionAttribute>(type)?.Option ?? ApiOption.None;
            var defCategory = GetCustomAttribute<CategoryAttribute>(type)?.Category.SafeTrim();

            foreach (var method in type.GetMethods(PublicFlags))
            {
                CheckMethod(type, docx, service, routeHead, defPage, defOption, defCategory, method);
            }
            #endregion
        }

        private void CheckMethod(Type type, XmlMember docx, ServiceInfo serviceInfo, string routeHead, string defPage, ApiOption defOption, string defCategory, MethodInfo method)
        {
            var routeAttribute = GetCustomAttribute<RouteAttribute>(method);
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

            var accessOption = GetCustomAttribute<ApiOptionAttribute>(method);
            ApiOption option;
            if (accessOption != null)
            {
                option = accessOption.Option;
            }
            else
            {
                option = defOption;
            }

            var category = GetCustomAttribute<CategoryAttribute>(method)?.Category.SafeTrim();
            var page = GetCustomAttribute<ApiPageAttribute>(method)?.PageUrl.SafeTrim();

            var api = new ApiActionInfo
            {
                Name = method.Name,
                Route = route,
                ControllerName = type.GetTypeName(),
                ControllerCaption = docx?.Caption,
                Category = category ?? defCategory ?? docx?.Caption ?? docx?.Name,
                AccessOption = option,

                PageUrl = page ?? defPage
            };
            var doc = XmlMember.Find(type, method.Name, "M");
            api.Copy(doc);
            var arg = method.GetParameters().FirstOrDefault();
            if (method.ReturnType != typeof(void))
            {
                api.ResultType = method.ReturnType;
                api.ResultInfo = XmlDocumentDiscover.ReadEntity(method.ReturnType, "result");

                if (doc?.Returns != null)
                {
                    api.ResultInfo.Caption = doc.Returns;
                    api.ResultInfo.Description = doc.Returns;
                }
            }


            var serializeType = GetCustomAttribute<SerializeTypeAttribute>(method);
            var resultSerializeType = GetCustomAttribute<ResultSerializeTypeAttribute>(method);
            var argumentSerializeType = GetCustomAttribute<ArgumentSerializeTypeAttribute>(method);

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

            api.IsAsync = builder.isAsync;

            if (api.HaseArgument)
            {
                foreach (var argument in api.Arguments.Values)
                {
                    argument.Name = argument.ParameterInfo.Name;
                    argument.IsEnum = argument.ParameterInfo.ParameterType.IsEnum;
                    argument.IsArray = argument.ParameterInfo.ParameterType.IsArray;
                    argument.TypeName = argument.ParameterInfo.ParameterType.Name;
                    if (!argument.IsBaseType || argument.IsEnum)
                    {
                        var info = XmlDocumentDiscover.ReadEntity(argument.ParameterInfo.ParameterType, argument.ParameterInfo.Name);
                        if (info != null)
                        {
                            argument.Copy(info);
                        }
                    }
                    if (doc?.Arguments != null && doc.Arguments.TryGetValue(argument.ParameterInfo.Name, out var pdoc))
                    {
                        argument.Caption = argument.Description = pdoc;
                    }
                    var rule = argument.ParameterInfo.GetCustomAttribute<DataRuleAttribute>();
                    if (rule != null)
                    {
                        argument.Min = rule.Min;
                        argument.Max = rule.Max;
                        argument.Regex = rule.Regex;
                        argument.CanNull = rule.CanNull;
                    }
                }
            }

            serviceInfo.Aips.Add(api.Route, api);
            logger.Debug(() => $"【找到接口方法】{api.Route}=>{api.Name} {api.Caption}");
        }


        #endregion

        #region 基础支持

        const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        const BindingFlags PublicFlags = BindingFlags.Public | BindingFlags.Instance;

        static T GetCustomAttribute<T>(Type type) where T : Attribute => type.GetCustomAttributes<T>().FirstOrDefault();
        static T GetCustomAttribute<T>(MemberInfo type) where T : Attribute => type.GetCustomAttributes<T>().FirstOrDefault();

        #endregion
    }
}