using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Agebull.Common.Ioc
{

    /// <summary>
    ///     简单的依赖注入器(框架内部使用,请不要调用)
    /// </summary>
    public static class DependencyHelper
    {
        #region ServiceCollection

        static DependencyHelper()
        {
            ServiceCollection = new ServiceCollection();
            ServiceCollection.AddLogging();
        }

        /// <summary>
        ///     全局依赖
        /// </summary>
        public static IServiceCollection ServiceCollection { get; private set; }

        /// <summary>
        ///     显示式设置配置对象(依赖)
        /// </summary>
        /// <param name="service"></param>
        public static void Binding(IServiceCollection service)
        {
            if (service == ServiceCollection)
                return;
            service.RemoveAll<IConfigurationBuilder>();
            service.RemoveAll<IConfigurationRoot>();
            service.AddTransient(p => ConfigurationHelper.Builder);
            service.AddTransient(p => ConfigurationHelper.Root);

            if (ServiceCollection != null)
            {
                foreach (var dod in ServiceCollection.ToArray())
                {
                    if (dod.ServiceType == typeof(ILoggerFactory))
                    {
                        continue;
                    }
                    else if (dod.ServiceType == typeof(IConfigurationBuilder))
                    {
                        continue;
                    }
                    else if (dod.ServiceType == typeof(IConfigurationRoot))
                    {
                        continue;
                    }
                    else if (dod.ServiceType == typeof(ILoggerFactory))
                    {
                        continue;
                    }
                    else
                    {
                        service.Add(dod);
                    }
                }
            }
            ServiceCollection = service;
            ConfigurationHelper.CreateBuilder();
            CheckLog();
        }

        /// <summary>
        ///     更新构造提供器
        /// </summary>
        /// <returns></returns>
        public static void SetRootProvider(IServiceProvider provider)
        {
            CheckLog();
            _rootProvider = provider;
            _serviceScopeFactory = provider.GetService<IServiceScopeFactory>();
            //ConfigurationHelper.UpdateDependency();
        }

        /// <summary>
        ///     更新(构架内使用)
        /// </summary>
        /// <returns></returns>
        public static void Flush()
        {
            CheckLog();
            _rootProvider = ServiceCollection.BuildServiceProvider(true);
            _serviceScopeFactory = _rootProvider.GetService<IServiceScopeFactory>();
            //if (ConfigurationHelper.UpdateDependency())
            //{
            //    _rootProvider = ServiceCollection.BuildServiceProvider(true);
            //    _serviceScopeFactory = _rootProvider.GetService<IServiceScopeFactory>();
            //}
            ConfigurationHelper.Flush();
        }

        private static IServiceProvider _rootProvider;

        /// <summary>
        ///     依赖注入构造器
        /// </summary>
        public static IServiceProvider RootProvider => _rootProvider ??= ServiceCollection.BuildServiceProvider(true);


        static IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        ///     范围工厂
        /// </summary>
        public static IServiceScopeFactory ServiceScopeFactory => _serviceScopeFactory ??= RootProvider.GetService<IServiceScopeFactory>();

        /// <summary>
        ///     依赖注入构造器
        /// </summary>
        public static IServiceProvider ServiceProvider => DependencyRun.ServiceScope?.ServiceProvider ?? RootProvider;

        #endregion

        #region ILoggerFactory

        static ILoggerFactory _loggerFactory;

        /// <summary>
        ///     全局依赖
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get => _loggerFactory ??= Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.Services.AddScoped(provider => ConfigurationHelper.Root);
                builder.AddConfiguration(ConfigurationHelper.Root.GetSection("Logging"));
                if (ConfigurationHelper.Root.GetValue("Logging:console", true))
                    builder.AddConsole();
            });
            set => _loggerFactory = value;
        }

        static void CheckLog()
        {
            ServiceCollection.RemoveAll<ILoggerFactory>();
            ServiceCollection.AddSingleton(pri => LoggerFactory);
        }
        #endregion

        #region 生成接口实例

        /// <summary>
        ///     生成接口实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static TInterface Create<TInterface>()
        {
            return ServiceProvider.GetService<TInterface>();
        }

        /// <summary>
        ///     生成接口实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface GetService<TInterface>()
        {
            return ServiceProvider.GetService<TInterface>();
        }

        /// <summary>
        ///     生成接口实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TInterface> GetServices<TInterface>()
        {
            return ServiceProvider.GetServices<TInterface>();
        }

        #endregion

        #region 自动构造与属性扩展

        /// <summary>
        /// 注册并使用自动构造
        /// </summary>
        /// <typeparam name="TService">基类型</typeparam>
        /// <typeparam name="TImplementation">实际类型</typeparam>
        /// <returns>构造后的类型</returns>
        /// <remarks>
        /// 构造参数构造方式，
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. 其余全部通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// 公开属性构造方式
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. FromServicesAttribute通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// </remarks>
        public static IServiceCollection AddAutoTransient<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            var func = new DynamicCreateBuilder().AutoCreate<TImplementation>();
            ServiceCollection.AddTransient<TService>(func);
            return ServiceCollection;
        }

        /// <summary>
        /// 注册并使用自动构造
        /// </summary>
        /// <typeparam name="TService">基类型</typeparam>
        /// <typeparam name="TImplementation">实际类型</typeparam>
        /// <returns>构造后的类型</returns>
        /// <remarks>
        /// 构造参数构造方式，
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. 其余全部通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// 公开属性构造方式
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. FromServicesAttribute通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// </remarks>
        public static IServiceCollection AddAutoScoped<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            var func = new DynamicCreateBuilder().AutoCreate<TImplementation>();
            ServiceCollection.AddScoped<TService>(func);
            return ServiceCollection;
        }

        /// <summary>
        /// 注册并使用动态构造
        /// </summary>
        /// <typeparam name="TService">基类型</typeparam>
        /// <typeparam name="TImplementation">实际类型</typeparam>
        /// <returns>构造后的类型</returns>
        /// <remarks>
        /// 构造参数构造方式，
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. 其余全部通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// 公开属性构造方式
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. FromServicesAttribute通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// </remarks>
        public static IServiceCollection AddAutoSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            var func = new DynamicCreateBuilder().AutoCreate<TImplementation>();
            ServiceCollection.AddSingleton<TService>(func);
            return ServiceCollection;
        }

        /// <summary>
        /// 注册并使用自动构造
        /// </summary>
        /// <typeparam name="TService">基类型</typeparam>
        /// <returns>构造后的类型</returns>
        /// <remarks>
        /// 构造参数构造方式，
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. 其余全部通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// 公开属性构造方式
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. FromServicesAttribute通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// </remarks>
        public static IServiceCollection AddAutoTransient<TService>() where TService : class
        {
            var func = new DynamicCreateBuilder().AutoCreate<TService>();
            ServiceCollection.AddTransient(func);
            return ServiceCollection;
        }

        /// <summary>
        /// 注册并使用自动构造
        /// </summary>
        /// <typeparam name="TService">类型</typeparam>
        /// <returns>构造后的类型</returns>
        /// <remarks>
        /// 构造参数构造方式，
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. 其余全部通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// 公开属性构造方式
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. FromServicesAttribute通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// </remarks>
        public static IServiceCollection AddAutoScoped<TService>() where TService : class
        {
            var func = new DynamicCreateBuilder().AutoCreate<TService>();
            ServiceCollection.AddScoped(func);
            return ServiceCollection;
        }

        /// <summary>
        /// 注册并使用动态构造
        /// </summary>
        /// <typeparam name="TService">类型</typeparam>
        /// <returns>构造后的类型</returns>
        /// <remarks>
        /// 构造参数构造方式，
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. 其余全部通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// 公开属性构造方式
        /// 1. ILogger构造成ILogger&lt;T&gt;
        /// 2. FromConfigAttribute特性标识的构造成从配置文件读取的对象
        /// 3. FromServicesAttribute通过依赖构造（如果无法依赖构造或未注册的，后果未知）
        /// </remarks>
        public static IServiceCollection AddAutoSingleton<TService>() where TService : class
        {
            var func = new DynamicCreateBuilder().AutoCreate<TService>();
            ServiceCollection.AddSingleton(func);
            return ServiceCollection;
        }
        #endregion

        #region 拿来主义

        /// <summary>
        ///     Adds a transient service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient(Type serviceType, Type implementationType)
        {

            ServiceCollection.AddTransient(serviceType, implementationType);
            //JoinEvents(implementationType);
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a transient service of the type specified in <paramref name="serviceType" /> with a
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient(Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            return ServiceCollection.AddTransient(serviceType, implementationFactory);
        }


        /// <summary>
        ///     Adds a transient service of the type specified in <typeparamref name="TService" /> with an
        ///     implementation type specified in <typeparamref name="TImplementation" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">目标类型</typeparam>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddTransient<TService, TImplementation>();

            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a transient service of the type specified in <paramref name="serviceType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient(Type serviceType)
        {
            ServiceCollection.AddTransient(serviceType);
            //JoinEvents(serviceType);
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a transient service of the type specified in <typeparamref name="TService" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient<TService>() where TService : class
        {
            ServiceCollection.AddTransient<TService>();
            //JoinEvents(typeof(TService));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a transient service of the type specified in <typeparamref name="TService" /> with a
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return ServiceCollection.AddTransient(implementationFactory);
        }

        /// <summary>
        ///     Adds a transient service of the type specified in <typeparamref name="TService" /> with an
        ///     implementation type specified in <typeparamref name="TImplementation" /> using the
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">目标类型</typeparam>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddTransient(implementationFactory);

            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped(Type serviceType, Type implementationType)
        {
            ServiceCollection.AddScoped(serviceType, implementationType);
            //JoinEvents(implementationType);
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <paramref name="serviceType" /> with a
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped(Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            return ServiceCollection.AddScoped(serviceType, implementationFactory);
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <typeparamref name="TService" /> with an
        ///     implementation type specified in <typeparamref name="TImplementation" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">目标类型</typeparam>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddScoped<TService, TImplementation>();

            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <paramref name="serviceType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped(Type serviceType)
        {
            ServiceCollection.AddScoped(serviceType);
            //JoinEvents(serviceType);
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <typeparamref name="TService" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped<TService>() where TService : class
        {
            ServiceCollection.AddScoped<TService>();
            //JoinEvents(typeof(TService));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <typeparamref name="TService" /> with a
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return ServiceCollection.AddScoped(implementationFactory);
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <typeparamref name="TService" /> with an
        ///     implementation type specified in <typeparamref name="TImplementation" /> using the
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">目标类型</typeparam>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddScoped(implementationFactory);

            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            ServiceCollection.AddSingleton(serviceType, implementationType);
            //JoinEvents(implementationType);
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> with a
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton(Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            return ServiceCollection.AddSingleton(serviceType, implementationFactory);
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <typeparamref name="TService" /> with an
        ///     implementation type specified in <typeparamref name="TImplementation" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">目标类型</typeparam>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddSingleton<TService, TImplementation>();

            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton(Type serviceType)
        {
            ServiceCollection.AddSingleton(serviceType);
            //JoinEvents(serviceType);
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <typeparamref name="TService" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService>() where TService : class
        {
            ServiceCollection.AddSingleton<TService>();
            //JoinEvents(typeof(TService));
            return ServiceCollection;
        }


        /// <summary>
        ///     Adds a singleton service of the type specified in <typeparamref name="TService" /> with a
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return ServiceCollection.AddSingleton(implementationFactory);
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <typeparamref name="TService" /> with an
        ///     implementation type specified in <typeparamref name="TImplementation" /> using the
        ///     factory specified in <paramref name="implementationFactory" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TImplementation">目标类型</typeparam>
        /// <param name="implementationFactory">工产方法</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddSingleton(implementationFactory);

            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> with an
        ///     instance specified in <paramref name="implementationInstance" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationInstance">The instance of the service.</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton(Type serviceType, object implementationInstance)
        {
            return ServiceCollection.AddSingleton(serviceType, implementationInstance);
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <typeparamref name="TService" /> with an
        ///     instance specified in <paramref name="implementationInstance" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="implementationInstance">The instance of the service.</param>
        /// <returns>IServiceCollection实例</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService>(TService implementationInstance) where TService : class
        {
            ServiceCollection.AddSingleton(typeof(TService), implementationInstance);
            //JoinEvents(typeof(TService));
            return ServiceCollection;
        }

        #endregion

        #region 删除指定类型构造器

        /// <summary>
        /// 删除指定类型构造器
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        public static void RemoveService<TService>()
        {
            var pres = ServiceCollection.Where(p => p.ServiceType == typeof(TService)).ToArray();
            foreach (var pre in pres)
            {
                ServiceCollection.Remove(pre);
            }
        }

        #endregion
    }
}