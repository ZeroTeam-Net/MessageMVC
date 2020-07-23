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
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ApiFunc = System.Func<ZeroTeam.MessageMVC.Messages.IInlineMessage, ZeroTeam.MessageMVC.Messages.ISerializeProxy, object, object>;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 接口方法生成器
    /// </summary>
    internal class ApiMethodBuilder
    {
        /// <summary>
        /// 调用对象类型
        /// </summary>
        public TypeInfo TypeInfo;

        public MethodInfo Method;

        /// <summary>
        /// 调用对象类型
        /// </summary>
        public ApiActionInfo ActionInfo;

        bool ParameterClass = false;

        /// <summary>
        /// 此调用是否异步方法
        /// </summary>
        public bool isAsync;

        #region Api调用方法生成

        ILGenerator ilGenerator;
        LocalBuilder controler;

        /// <summary>生成动态匿名调用内部方法（参数由TArg转为实际类型后调用，并将调用返回值转为TRes）</summary>
        public ApiFunc CreateMethod()
        {
            var dynamicMethod = new DynamicMethod($"{Method.Name}_{RandomCode.Generate(6)}"
                , typeof(object), new[]
                {
                    typeof(IInlineMessage),
                    typeof(ISerializeProxy),
                    typeof(object)
                });
            ilGenerator = dynamicMethod.GetILGenerator();

            //如果修补操作码，则填充空间。 尽管可能消耗处理周期，但未执行任何有意义的操作。
            ilGenerator.Emit(OpCodes.Nop);
            //构造
            Ctor();
            //构造属性
            Properties();
            //调用方法 controler.Api();
            Call();
            //对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
            Result();

            return dynamicMethod.CreateDelegate(typeof(ApiFunc)) as ApiFunc;
        }

        void Call()
        {
            List<LocalBuilder> paras = new List<LocalBuilder>();
            foreach (var parameter in Method.GetParameters())
            {
                var res = Parameter(parameter);
                var arg = ilGenerator.DeclareLocal(parameter.ParameterType);
                ilGenerator.Emit(OpCodes.Stloc, arg);
                paras.Add(arg);
                if (res == null)
                    continue;
                ActionInfo.Arguments ??= new Dictionary<string, ApiArgument>();
                ActionInfo.Arguments.Add(parameter.Name, new ApiArgument
                {
                    Name = parameter.Name,
                    ParameterInfo = parameter,
                    IsBaseType = res.Value
                });
            }

            ilGenerator.Emit(OpCodes.Ldloc, controler);
            foreach (var builder in paras)
            {
                ilGenerator.Emit(OpCodes.Ldloc, builder);
            }
            ilGenerator.Emit(OpCodes.Callvirt, Method);

        }

        private bool? Parameter(ParameterInfo parameter)
        {
            if (parameter.ParameterType.IsSupperInterface(typeof(IZeroContext)))
            {
                Context(ilGenerator);
                return null;
            }
            if (parameter.ParameterType.IsSupperInterface(typeof(IUser)))
            {
                User(ilGenerator);
                return null;
            }

            var ca = parameter.GetCustomAttribute<FromConfigAttribute>();
            if (ca != null)
            {
                ConfigCreate(ilGenerator, ca.Name, parameter.ParameterType);
                return null;
            }
            var fa = parameter.GetCustomAttribute<FromServicesAttribute>();
            if (fa != null)
            {
                IocCreate(ilGenerator, parameter.ParameterType);
                return null;
            }

            var scope = (int)(parameter.GetCustomAttribute<ArgumentScopeAttribute>()?.Scope ?? ArgumentScope.Content);

            if (parameter.ParameterType == typeof(string) || parameter.ParameterType.IsValueType)
            {
                ReadValueArgument(ilGenerator, parameter, scope, 0);//serializeType
                return true;
            }

            //第一个不特殊构造的
            if (!ParameterClass)
            {
                ParameterClass = true;
                ilGenerator.Emit(OpCodes.Ldarg, 2);
                ilGenerator.Emit(OpCodes.Castclass, parameter.ParameterType);
                return false;
            }
            IocCreate(ilGenerator, parameter.ParameterType);
            return null;
        }

        void Result()
        {
            var resInfo = typeof(object);
            if (Method.ReturnType == null || Method.ReturnType == typeof(void))
            {
                isAsync = false;
                //空值入栈
                var local4 = ilGenerator.DeclareLocal(resInfo);
                ilGenerator.Emit(OpCodes.Ldnull, local4);
            }
            else
            {
                isAsync = Method.ReturnType.IsSubclassOf(typeof(Task));
                //取返回值
                var local3 = ilGenerator.DeclareLocal(Method.ReturnType);
                ilGenerator.Emit(OpCodes.Stloc, local3);
                ilGenerator.Emit(OpCodes.Ldloc, local3);
                ilGenerator.Emit(OpCodes.Castclass, resInfo);
                //返回值入栈
                var local4 = ilGenerator.DeclareLocal(resInfo);
                ilGenerator.Emit(OpCodes.Stloc, local4);
                ilGenerator.Emit(OpCodes.Ldloc, local4);
            }
            ilGenerator.Emit(OpCodes.Ret);
        }
        void Ctor()
        {
            //new Controler;
            var constructor = TypeInfo.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                ilGenerator.Emit(OpCodes.Newobj, constructor);
            }
            else
            {
                //构造参数
                var info = TypeInfo.GetConstructors()[0];
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
                        else if (parameter.ParameterType == typeof(IServiceCollection))
                        {
                            Logger(ilGenerator);
                        }
                        else if (parameter.ParameterType == typeof(IServiceCollection))
                        {
                            User(ilGenerator);
                        }
                        else if (parameter.ParameterType == typeof(IServiceCollection))
                        {
                            Context(ilGenerator);
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
            controler = ilGenerator.DeclareLocal(TypeInfo);
            ilGenerator.Emit(OpCodes.Stloc, controler);
        }

        private void Properties()
        {
            foreach (var pro in TypeInfo.GetProperties())
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
                    else if (pro.PropertyType== typeof(IZeroContext))
                    {
                        Context(ilGenerator);
                    }
                    else if (pro.PropertyType== typeof(IUser))
                    {
                        User(ilGenerator);
                    }
                    else if (pro.PropertyType== typeof(ILogger))
                    {
                        Logger(ilGenerator);
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
        }



        #endregion

        #region Helper
        /*
        private static void ReadArgument(ILGenerator ilGenerator, ParameterInfo parameter, int scope, int serializeType)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, parameter.ParameterType);
            ilGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));

            var typ = ilGenerator.DeclareLocal(typeof(Type));
            ilGenerator.Emit(OpCodes.Stloc, typ);

            //object arg = message.GetArgument(scope, serializeType,ser,typ);
            ilGenerator.Emit(OpCodes.Ldarg, 0);
            ilGenerator.Emit(OpCodes.Ldc_I4, scope);
            ilGenerator.Emit(OpCodes.Ldc_I4, serializeType);
            ilGenerator.Emit(OpCodes.Ldarg, 1);
            ilGenerator.Emit(OpCodes.Ldloc, typ);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(IInlineMessage).GetMethod(nameof(IInlineMessage.GetArgument)));


            //serialize.Deserialize(arg, typeof());
            //var obj = ilGenerator.DeclareLocal(objInfo);
            //ilGenerator.Emit(OpCodes.Stloc, obj);
            //ilGenerator.Emit(OpCodes.Ldloc, obj);
            //ilGenerator.Emit(OpCodes.Ldobj, parameter.ParameterType);

            //类型转换
            var obj = ilGenerator.DeclareLocal(typeof(object));
            ilGenerator.Emit(OpCodes.Stloc, obj);

            ilGenerator.Emit(OpCodes.Ldloc, obj);
            ilGenerator.Emit(OpCodes.Castclass, parameter.ParameterType);
        }*/

        private static void ReadValueArgument(ILGenerator ilGenerator, ParameterInfo parameter, int scope, int serializeType)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, parameter.ParameterType);
            ilGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));

            var typ = ilGenerator.DeclareLocal(typeof(Type));
            ilGenerator.Emit(OpCodes.Stloc, typ);
            //object arg = message.GetOnceArgument(ArgumentName, DeserializeType);
            ilGenerator.Emit(OpCodes.Ldarg, 0);
            ilGenerator.Emit(OpCodes.Ldstr, parameter.Name);
            ilGenerator.Emit(OpCodes.Ldc_I4, scope);
            ilGenerator.Emit(OpCodes.Ldc_I4, serializeType);
            ilGenerator.Emit(OpCodes.Ldarg, 1);
            ilGenerator.Emit(OpCodes.Ldloc, typ);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(IInlineMessage).GetMethod(nameof(IInlineMessage.FrameGetValueArgument)));
            //object arg = 
            var obj = ilGenerator.DeclareLocal(typeof(object));
            ilGenerator.Emit(OpCodes.Stloc, obj);

            ilGenerator.Emit(OpCodes.Ldloc, obj);
            ilGenerator.Emit(OpCodes.Castclass, typeof(string));

            if (parameter.ParameterType == typeof(string))
            {
                return;
            }
            var str = ilGenerator.DeclareLocal(typeof(string));
            ilGenerator.Emit(OpCodes.Stloc, str);

            //ilGenerator.Emit(OpCodes.Ldloc, str);
            //ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod(nameof(Console.WriteLine), new[] { typeof(string) }));

            if (parameter.ParameterType.IsEnum)
            {
                ilGenerator.Emit(OpCodes.Ldloc, typ);
                ilGenerator.Emit(OpCodes.Ldloc, str);


                ilGenerator.Emit(OpCodes.Call, typeof(Enum).GetMethod(nameof(Enum.Parse), new[] { typeof(Type), typeof(string) }));

                ilGenerator.Emit(OpCodes.Unbox_Any, parameter.ParameterType);
                //ilGenerator.Emit(OpCodes.Castclass, parameter.ParameterType);
                //var emobj = ilGenerator.DeclareLocal(typeof(object));
                //ilGenerator.Emit(OpCodes.Stloc, emobj);

                //ilGenerator.Emit(OpCodes.Ldloc, emobj);
                //ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod(nameof(Console.WriteLine), new[] { typeof(object) }));

                //ilGenerator.Emit(OpCodes.Ldloc, emobj);
                //ilGenerator.Emit(OpCodes.Castclass, parameter.ParameterType);

                //var em = ilGenerator.DeclareLocal(parameter.ParameterType);
                //ilGenerator.Emit(OpCodes.Stloc, em);

                //ilGenerator.Emit(OpCodes.Ldloc, em);
                //ilGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod(nameof(Console.WriteLine), new[] { typeof(object) }));

                //ilGenerator.Emit(OpCodes.Ldloc, em);

                return;
            }
            try
            {
                ilGenerator.Emit(OpCodes.Ldloc, str);
                ilGenerator.Emit(OpCodes.Call, parameter.ParameterType.GetMethod(nameof(int.Parse), new[] { typeof(string) }));
            }
            catch
            {
                throw new NotSupportedException($"类型{parameter.ParameterType}没有Parse方法");
            }
        }

        private static void ServiceCollection(ILGenerator ilGenerator)
        {
            var method = typeof(DependencyHelper).GetProperty($"ServiceCollection").GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private static void User(ILGenerator ilGenerator)
        {
            var method = typeof(GlobalContext).GetProperty(nameof(GlobalContext.User)).GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private static void Context(ILGenerator ilGenerator)
        {
            var method = typeof(GlobalContext).GetProperty(nameof(GlobalContext.Current)).GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private void Logger(ILGenerator ilGenerator)
        {
            var method = typeof(DependencyHelper).GetProperty($"LoggerFactory").GetGetMethod();
            ilGenerator.Emit(OpCodes.Call, method);
            var local = ilGenerator.DeclareLocal(typeof(ILoggerFactory));
            ilGenerator.Emit(OpCodes.Stloc, local);
            ilGenerator.Emit(OpCodes.Ldloc, local);
            var methods = typeof(LoggerFactoryExtensions).GetMethods();
            method = methods.First(p => p.Name == "CreateLogger" && p.GetParameters().Length == 1);
            method = method.MakeGenericMethod(TypeInfo.AsType());
            ilGenerator.Emit(OpCodes.Call, method);
        }

        private static void ConfigCreate(ILGenerator ilGenerator, string name, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldstr, name);
            var method = typeof(ConfigurationHelper)
                .GetMethod(nameof(ConfigurationHelper.Option), new[] { typeof(string) })
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