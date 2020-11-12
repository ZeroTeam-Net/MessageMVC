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
        static readonly ILogger logger;

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
            logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(ApiDiscover));
            XmlMember.Load(typeof(IMessageItem).Assembly);
            XmlMember.Load(typeof(ApiExecuter).Assembly);
        }

        #endregion

        #region 程序集发现

        private static readonly List<string> knowAssemblies = new List<string>();

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
                var name = Path.GetFileName(file);
                if (knowAssemblies.Contains(name))
                    continue;
                knowAssemblies.Add(file);
                var first = name.Split('.', 2)[0];
                switch (first)
                {
                    case "System":
                    case "Microsoft":
                    case "NuGet":
                    case "Newtonsoft":
                        break;
                    default:
                        FindApies(Assembly.LoadFile(file));
                        break;
                }
            }
        }

        /// <summary>
        ///     发现
        /// </summary>
        public static void FindApies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                FindApies(assembly);
            }
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public static void FindApies(Assembly asm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asm.Location))
                    return;
                var file = Path.GetFileName(asm.Location);
                if (knowAssemblies.Contains(file))
                {
                    return;
                }
                knowAssemblies.Add(file);
                if (asm.FullName == null ||
                    asm.FullName.Contains("netstandard") ||
                    asm.FullName.Contains("System.") ||
                    asm.FullName.Contains("Microsoft.") ||
                    asm.FullName.Contains("Newtonsoft.") ||
                    asm.FullName.Contains("Agebull.Common.") ||
                    asm.FullName.Contains("ZeroTeam.MessageMVC.Abstractions") ||
                    asm.FullName.Contains("ZeroTeam.MessageMVC.Core"))
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            try
            {
                var discover = new ApiDiscover
                {
                    Assembly = asm
                };
                XmlMember.Load(asm);
                discover.FindApies();
            }
            catch (Exception e2)
            {
                logger.Debug(e2.ToString());
            }
        }

        /// <summary>
        /// 查找API
        /// </summary>
        void FindApies()
        {
            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IApiController))).ToArray();
            if (types.Length == 0)
                return;
            logger.LogDebug("【解析程序集】{asm}", Assembly.FullName);
            foreach (var type in types)
            {
                FindApi(type);
            }
            RegistToZero();
        }

        /// <summary>
        /// 查找API
        /// </summary>
        public void Discover(Assembly assembly)
        {
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
            Assembly = type.Assembly;
            XmlMember.Load(Assembly);
            if (type.IsSupperInterface(typeof(IApiController)))
            {
                FindApi(type);
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
                    service = new ZeroService
                    {
                        IsAutoService = true,
                        ServiceName = serviceInfo.Name,
                        Receiver = serviceInfo.NetBuilder(serviceInfo.Name),
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
                        else if(!service.RegistAction(api.Key, info))
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
        /// <param name="type"></param>
        private void FindApi(Type type)
        {
            //泛型与纯虚类
            if (type.IsAbstract || type.IsGenericType)
            {
                return;
            }
            logger.LogDebug("【解析类型】 {asm}", type.FullName);

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