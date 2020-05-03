using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agebull.Common.Ioc
{

    /// <summary>
    ///     简单的依赖注入器(框架内部使用,请不要调用)
    /// </summary>
    public static class DependencyHelper
    {
        #region ServiceCollection

        static ILoggerFactory loggerFactory;

        /// <summary>
        ///     全局依赖
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                return loggerFactory ??= Create<ILoggerFactory>() ?? new LoggerFactory();
            }
            set
            {
                loggerFactory = value;
            }
        }

        private static IServiceCollection _serviceCollection;

        private static IServiceProvider _rootProvider;
        /// <summary>
        ///     依赖注入代理
        /// </summary>
        public static IServiceProvider RootProvider => _rootProvider ??= ServiceCollection.BuildServiceProvider(true);

        static IServiceScopeFactory serviceScopeFactory;

        internal static IServiceScopeFactory ServiceScopeFactory => serviceScopeFactory ??= RootProvider.GetService<IServiceScopeFactory>();
        /// <summary>
        ///     依赖注入代理
        /// </summary>
        public static IServiceProvider ServiceProvider => DependencyScope.ServiceScope?.ServiceProvider ?? RootProvider;

        /// <summary>
        ///     全局依赖
        /// </summary>
        public static IServiceCollection ServiceCollection
        {
            get
            {
                if (_serviceCollection != null)
                    return _serviceCollection;
                _serviceCollection = new ServiceCollection();
                _serviceCollection.AddSingleton(p => ConfigurationManager.Builder);
                return _serviceCollection;
            }
            set
            {
                _serviceCollection = value;
                _serviceCollection.AddSingleton(p => ConfigurationManager.Builder);
                Update();
            }
        }

        /// <summary>
        ///     显示式设置配置对象(依赖)
        /// </summary>
        /// <param name="service"></param>
        public static void SetServiceCollection(IServiceCollection service)
        {
            var old = _serviceCollection;
            _serviceCollection = service;
            if (old != null)
                foreach (var dod in old.ToArray())
                    service.Add(dod);
            else
                _serviceCollection.AddSingleton(p => ConfigurationManager.Builder);
            Update();
        }

        /// <summary>
        ///     更新(请不要在可能调用构造的地方引用)
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider Update()
        {
            loggerFactory = null;
            serviceScopeFactory = null;
            _rootProvider = null;
            return _rootProvider = ServiceCollection.BuildServiceProvider();
        }

        #endregion

        #region 生成接口实例

        /// <summary>
        ///     生成接口实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface Create<TInterface>()
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

        /*// <summary>
        /// 注册一个服务依赖注册的扩展事件
        /// </summary>
        /// <param name="action"></param>
        public static void RegistJoinEvent(Action<Type> action)
        {
            if (!ServiceJoinEvents.Contains(action))
            {
                ServiceJoinEvents.Add(action);
                foreach (var type in Types)
                {
                    action(type);
                }
            }
        }

        static readonly List<Action<Type>> ServiceJoinEvents = new List<Action<Type>>();

        static readonly List<Type> Types = new List<Type>();

        static void //JoinEvents(Type type)
        {
            if (!Types.Contains(type))
            {
                Types.Add(type);
                foreach (var action in ServiceJoinEvents)
                {
                    action(type);
                }
            }
        }
        */
        /// <summary>
        ///     Adds a transient service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddTransient<TService, TImplementation>();
            //JoinEvents(typeof(TImplementation));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a transient service of the type specified in <paramref name="serviceType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient" />
        public static IServiceCollection AddTransient<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddTransient(implementationFactory);
            //JoinEvents(typeof(TImplementation));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddScoped<TService, TImplementation>();
            //JoinEvents(typeof(TImplementation));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a scoped service of the type specified in <paramref name="serviceType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped" />
        public static IServiceCollection AddScoped<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddScoped(implementationFactory);
            //JoinEvents(typeof(TImplementation));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddSingleton<TService, TImplementation>();
            //JoinEvents(typeof(TImplementation));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
            where TImplementation : class, TService
        {
            ServiceCollection.AddSingleton(implementationFactory);
            //JoinEvents(typeof(TImplementation));
            return ServiceCollection;
        }

        /// <summary>
        ///     Adds a singleton service of the type specified in <paramref name="serviceType" /> with an
        ///     instance specified in <paramref name="implementationInstance" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationInstance">The instance of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService>(TService implementationInstance) where TService : class
        {
            ServiceCollection.AddSingleton(typeof(TService), implementationInstance);
            //JoinEvents(typeof(TService));
            return ServiceCollection;
        }

        #endregion
    }
}