using Agebull.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Agebull.Common.Ioc
{

    /// <summary>
    ///     �򵥵�����ע����(����ڲ�ʹ��,�벻Ҫ����)
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
        ///     ȫ������
        /// </summary>
        public static IServiceCollection ServiceCollection { get; private set; }

        /// <summary>
        ///     ��ʾʽ�������ö���(����)
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
        ///     ���¹����ṩ��
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
        ///     ����(������ʹ��)
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
            //ConfigurationHelper.Flush();
        }

        private static IServiceProvider _rootProvider;

        /// <summary>
        ///     ����ע�빹����
        /// </summary>
        public static IServiceProvider RootProvider => _rootProvider ??= ServiceCollection.BuildServiceProvider(true);


        static IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        ///     ��Χ����
        /// </summary>
        public static IServiceScopeFactory ServiceScopeFactory => _serviceScopeFactory ??= RootProvider.GetService<IServiceScopeFactory>();

        /// <summary>
        ///     ����ע�빹����
        /// </summary>
        public static IServiceProvider ServiceProvider => ScopeRuner.ServiceScope?.ServiceProvider ?? RootProvider;

        #endregion

        #region ILoggerFactory

        static ILogger logger;
        /// <summary>
        /// Ĭ����־��¼��
        /// </summary>
        internal static ILogger Logger => logger;

        static ILoggerFactory _loggerFactory;

        /// <summary>
        ///     ��־����
        /// </summary>
        public static ILoggerFactory LoggerFactory => _loggerFactory;

        /// <summary>
        /// ��־����
        /// </summary>
        /// <param name="builder"></param>
        public static void LoggingConfig(ILoggingBuilder builder)
        {
            builder.Services.AddTransient(provider => ConfigurationHelper.Root);
            var config = ConfigurationHelper.Root.GetSection("Logging");
            builder.AddConfiguration(config);
            if (config.GetValue("Console", true))
                builder.AddConsole();
            builder.AddConsole();
        }

        /// <summary>
        /// ����������־����
        /// </summary>
        /// <param name="factory">��־����</param>
        public static void BindingLoggerFactory(ILoggerFactory factory)
        {
            if (factory == null)
                return;
            _loggerFactory = factory;
            logger = _loggerFactory.CreateLogger("Agebull.Common.Ioc");
            ServiceCollection.RemoveAll<ILoggerFactory>();
            ServiceCollection.AddSingleton(pri => LoggerFactory);
        }

        static void CheckLog()
        {
            if (_loggerFactory != null)
            {
                return;
            }
            ResetLoggerFactory(LoggingConfig);
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="action"></param>
        public static void ResetLoggerFactory(Action<ILoggingBuilder> action)
        {
            _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(action);
            logger = _loggerFactory.CreateLogger("Agebull.Common.Ioc");
            ServiceCollection.RemoveAll<ILoggerFactory>();
            ServiceCollection.AddSingleton(pri => LoggerFactory);
        }
        #endregion

        #region ���ɽӿ�ʵ��

        /// <summary>
        ///     ���ɽӿ�ʵ��
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static TInterface Create<TInterface>()
        {
            return ServiceProvider.GetService<TInterface>();
        }

        /// <summary>
        ///     ���ɽӿ�ʵ��
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface GetService<TInterface>()
        {
            return ServiceProvider.GetService<TInterface>();
        }

        /// <summary>
        ///     ���ɽӿ�ʵ��
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TInterface> GetServices<TInterface>()
        {
            return ServiceProvider.GetServices<TInterface>();
        }

        #endregion

        #region �Զ�������������չ

        /// <summary>
        /// ע�Ტʹ���Զ�����
        /// </summary>
        /// <typeparam name="TService">������</typeparam>
        /// <typeparam name="TImplementation">ʵ������</typeparam>
        /// <returns>����������</returns>
        /// <remarks>
        /// ����������췽ʽ��
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. ����ȫ��ͨ���������죨����޷����������δע��ģ����δ֪��
        /// �������Թ��췽ʽ
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. FromServicesAttributeͨ���������죨����޷����������δע��ģ����δ֪��
        /// </remarks>
        public static IServiceCollection AddAutoTransient<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            var func = new DynamicCreateBuilder().AutoCreate<TImplementation>();
            ServiceCollection.AddTransient<TService>(func);
            return ServiceCollection;
        }

        /// <summary>
        /// ע�Ტʹ���Զ�����
        /// </summary>
        /// <typeparam name="TService">������</typeparam>
        /// <typeparam name="TImplementation">ʵ������</typeparam>
        /// <returns>����������</returns>
        /// <remarks>
        /// ����������췽ʽ��
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. ����ȫ��ͨ���������죨����޷����������δע��ģ����δ֪��
        /// �������Թ��췽ʽ
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. FromServicesAttributeͨ���������죨����޷����������δע��ģ����δ֪��
        /// </remarks>
        public static IServiceCollection AddAutoScoped<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            var func = new DynamicCreateBuilder().AutoCreate<TImplementation>();
            ServiceCollection.AddScoped<TService>(func);
            return ServiceCollection;
        }

        /// <summary>
        /// ע�Ტʹ�ö�̬����
        /// </summary>
        /// <typeparam name="TService">������</typeparam>
        /// <typeparam name="TImplementation">ʵ������</typeparam>
        /// <returns>����������</returns>
        /// <remarks>
        /// ����������췽ʽ��
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. ����ȫ��ͨ���������죨����޷����������δע��ģ����δ֪��
        /// �������Թ��췽ʽ
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. FromServicesAttributeͨ���������죨����޷����������δע��ģ����δ֪��
        /// </remarks>
        public static IServiceCollection AddAutoSingleton<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            var func = new DynamicCreateBuilder().AutoCreate<TImplementation>();
            ServiceCollection.AddSingleton<TService>(func);
            return ServiceCollection;
        }

        /// <summary>
        /// ע�Ტʹ���Զ�����
        /// </summary>
        /// <typeparam name="TService">������</typeparam>
        /// <returns>����������</returns>
        /// <remarks>
        /// ����������췽ʽ��
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. ����ȫ��ͨ���������죨����޷����������δע��ģ����δ֪��
        /// �������Թ��췽ʽ
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. FromServicesAttributeͨ���������죨����޷����������δע��ģ����δ֪��
        /// </remarks>
        public static IServiceCollection AddAutoTransient<TService>() where TService : class
        {
            var func = new DynamicCreateBuilder().AutoCreate<TService>();
            ServiceCollection.AddTransient(func);
            return ServiceCollection;
        }

        /// <summary>
        /// ע�Ტʹ���Զ�����
        /// </summary>
        /// <typeparam name="TService">����</typeparam>
        /// <returns>����������</returns>
        /// <remarks>
        /// ����������췽ʽ��
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. ����ȫ��ͨ���������죨����޷����������δע��ģ����δ֪��
        /// �������Թ��췽ʽ
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. FromServicesAttributeͨ���������죨����޷����������δע��ģ����δ֪��
        /// </remarks>
        public static IServiceCollection AddAutoScoped<TService>() where TService : class
        {
            var func = new DynamicCreateBuilder().AutoCreate<TService>();
            ServiceCollection.AddScoped(func);
            return ServiceCollection;
        }

        /// <summary>
        /// ע�Ტʹ�ö�̬����
        /// </summary>
        /// <typeparam name="TService">����</typeparam>
        /// <returns>����������</returns>
        /// <remarks>
        /// ����������췽ʽ��
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. ����ȫ��ͨ���������죨����޷����������δע��ģ����δ֪��
        /// �������Թ��췽ʽ
        /// 1. ILogger�����ILogger&lt;T&gt;
        /// 2. FromConfigAttribute���Ա�ʶ�Ĺ���ɴ������ļ���ȡ�Ķ���
        /// 3. FromServicesAttributeͨ���������죨����޷����������δע��ģ����δ֪��
        /// </remarks>
        public static IServiceCollection AddAutoSingleton<TService>() where TService : class
        {
            var func = new DynamicCreateBuilder().AutoCreate<TService>();
            ServiceCollection.AddSingleton(func);
            return ServiceCollection;
        }
        #endregion

        #region ���ƹ���

        static readonly Dictionary<string, Type> NameTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        /// <summary>
        ///     ע��˲ʱ��������
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <paramref name="name">����</paramref>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services, string name)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddTransient<TImplementation>();
            NameTypes[name] = typeof(TImplementation);
            return services;
        }

        /// <summary>
        ///     ע�᷶Χ��������
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <paramref name="name">����</paramref>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddScope<TService, TImplementation>(this IServiceCollection services, string name)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddScoped<TService, TImplementation>();
            services.AddScoped<TImplementation>();
            NameTypes[name] = typeof(TImplementation);
            return services;
        }

        /// <summary>
        ///     ע�ᵥ����������
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <paramref name="name">����</paramref>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, string name)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddSingleton<TImplementation>();
            NameTypes[name] = typeof(TImplementation);
            return services;
        }
        /// <summary>
        ///     ע��˲ʱ��������,����ΪTImplementation�����Ͷ�����
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddNameTransient<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddTransient<TImplementation>();
            var type = typeof(TImplementation);
            NameTypes[type.Name] = type;
            return services;
        }

        /// <summary>
        ///     ע�᷶Χ��������,����ΪTImplementation�����Ͷ�����
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddNameScope<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddScoped<TService, TImplementation>();
            services.AddScoped<TImplementation>();
            var type = typeof(TImplementation);
            NameTypes[type.Name] = type;
            return services;
        }

        /// <summary>
        ///     ע�ᵥ����������,����ΪTImplementation�����Ͷ�����
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddNameSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>(implementationFactory);
            services.AddSingleton(implementationFactory);
            var type = typeof(TImplementation);
            NameTypes[type.Name] = type;
            return services;
        }

        /// <summary>
        ///     ע�ᵥ����������,����ΪTImplementation�����Ͷ�����
        /// </summary>
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
        public static IServiceCollection AddNameSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddSingleton<TImplementation>();
            var type = typeof(TImplementation);
            NameTypes[type.Name] = type;
            return services;
        }
        /// <summary>
        ///     ���ɽӿ�ʵ��
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <paramref name="name">����</paramref>
        /// <returns>����</returns>
        public static TInterface GetService<TInterface>(string name)
        {
            if (!NameTypes.TryGetValue(name, out var type))
                return default;
            return (TInterface)ServiceProvider.GetService(type);
        }
        #endregion

        #region ��������

        /// <summary>
        ///     Adds a transient service of the type specified in <paramref name="serviceType" /> with an
        ///     implementation of the type specified in <paramref name="implementationType" /> to the
        ///     specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <param name="serviceType">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <param name="serviceType">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <param name="serviceType">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <typeparam name="TService">��������</typeparam>
        /// <typeparam name="TImplementation">Ŀ������</typeparam>
        /// <param name="implementationFactory">��������</param>
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <returns>IServiceCollectionʵ��</returns>
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
        /// <returns>IServiceCollectionʵ��</returns>
        /// <seealso cref="F:Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton" />
        public static IServiceCollection AddSingleton<TService>(TService implementationInstance) where TService : class
        {
            ServiceCollection.AddSingleton(typeof(TService), implementationInstance);
            //JoinEvents(typeof(TService));
            return ServiceCollection;
        }

        #endregion

        #region ɾ��ָ�����͹�����

        /// <summary>
        /// ɾ��ָ�����͹�����
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