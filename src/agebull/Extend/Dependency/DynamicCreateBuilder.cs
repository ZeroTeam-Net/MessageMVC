using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Agebull.Common.Ioc
{
    /// <summary>
    /// 接口方法生成器
    /// </summary>
    internal class DynamicCreateBuilder
    {
        #region Api调用方法生成

        ILGenerator ilGenerator;

        /// <summary>生成动态匿名调用内部方法（参数由TArg转为实际类型后调用，并将调用返回值转为TRes）</summary>
        public Func<IServiceProvider,T> AutoCreate<T>()
        {
            var type = typeof(T);
            var dynamicMethod = new DynamicMethod($"{typeof(T).Name}_{RandomCode.Generate(6)}", type, 
                new Type[] { typeof(IServiceProvider)});
            ilGenerator = dynamicMethod.GetILGenerator();

            //如果修补操作码，则填充空间。 尽管可能消耗处理周期，但未执行任何有意义的操作。
            ilGenerator.Emit(OpCodes.Nop);
            //构造
            var res = Ctor(type);
            //构造属性
            Properties(type,res);

            //返回值入栈
            ilGenerator.Emit(OpCodes.Stloc, res);
            ilGenerator.Emit(OpCodes.Ldloc, res);

            return dynamicMethod.CreateDelegate(typeof(Func<IServiceProvider, T>)) as Func<IServiceProvider, T>;
        }

        protected LocalBuilder Ctor(Type type)
        {
            //new Controler;
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                ilGenerator.Emit(OpCodes.Newobj, constructor);
            }
            else
            { //构造参数
                var info = type.GetConstructors()[0];
                List<LocalBuilder> locals = new List<LocalBuilder>();
                foreach (var parameter in info.GetParameters())
                {
                    var ca = parameter.GetCustomAttribute<FromConfigAttribute>();
                    if (ca != null)
                    {
                        ConfigCreate(ilGenerator, ca.Name, parameter.ParameterType);
                    }
                    else
                    {
                        if (parameter.ParameterType == typeof(IServiceCollection))
                        {
                            ServiceCollection(ilGenerator);
                        }
                        else if (parameter.ParameterType.IsSupperInterface(typeof(ILogger)))
                        {
                            Logger(ilGenerator, parameter.ParameterType);
                        }
                        else
                        {
                            IocCreate(ilGenerator, parameter.ParameterType);
                        }
                    }
                    var builder = ilGenerator.DeclareLocal(parameter.ParameterType);
                    ilGenerator.Emit(OpCodes.Stloc, builder);
                    locals.Add(builder);
                }
                foreach (var builder in locals)
                {
                    ilGenerator.Emit(OpCodes.Ldloc, builder);
                }
                ilGenerator.Emit(OpCodes.Newobj, info);
            }
            //从计算堆栈的顶部弹出当前值并将其存储到指定索引处的局部变量列表中。
            var obj = ilGenerator.DeclareLocal(type);
            ilGenerator.Emit(OpCodes.Stloc, obj);
            return obj;
        }

        protected void Properties(Type type,LocalBuilder obj)
        {
            foreach (var pro in type.GetProperties())
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
                    else if (pro.PropertyType.IsSupperInterface(typeof(ILogger)))
                    {
                        Logger(ilGenerator, pro.PropertyType);
                    }
                    else
                    {
                        continue;
                    }
                }
                var b = ilGenerator.DeclareLocal(pro.PropertyType);
                ilGenerator.Emit(OpCodes.Stloc, b);
                ilGenerator.Emit(OpCodes.Ldloc, obj);
                ilGenerator.Emit(OpCodes.Ldloc, b);
                ilGenerator.Emit(OpCodes.Callvirt, pro.GetSetMethod());
            }
        }

        #endregion

        #region Helper

        private static void ServiceCollection(ILGenerator ilGenerator)
        {
            var method = typeof(DependencyHelper).GetProperty($"ServiceCollection").GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private static void Logger(ILGenerator ilGenerator, Type type)
        {
            var method = typeof(DependencyHelper).GetProperty($"LoggerFactory").GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
            var local = ilGenerator.DeclareLocal(typeof(ILoggerFactory));
            ilGenerator.Emit(OpCodes.Stloc, local);
            ilGenerator.Emit(OpCodes.Ldloc, local);
            var methods = typeof(LoggerFactoryExtensions).GetMethods();
            method = methods.First(p => p.Name == "CreateLogger" && p.GetParameters().Length == 1);
            method = method.MakeGenericMethod(type);
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private static void ConfigCreate(ILGenerator ilGenerator, string name, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldstr, name);
            var method = typeof(ConfigurationManager)
                .GetMethod(nameof(ConfigurationManager.Option), new[] { typeof(string) })
                .MakeGenericMethod(type);
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private static void IocCreate(ILGenerator ilGenerator, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var method = typeof(DependencyHelper).GetMethod($"GetServices").MakeGenericMethod(type.GetGenericArguments()[0]);
                ilGenerator.Emit(OpCodes.Call, method);
            }
            else
            {
                var method = typeof(DependencyHelper).GetMethod($"Create").MakeGenericMethod(type);
                ilGenerator.Emit(OpCodes.Call, method);
            }
        }
        #endregion
    }
}