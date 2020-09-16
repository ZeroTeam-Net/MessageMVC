// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel.Common
{
    /// <summary>
    ///     对象依赖字典
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public sealed class DependencyObjects
    {
        /// <summary>
        /// 字典
        /// </summary>
        [JsonIgnore]
        public readonly Dictionary<Type, object> Dictionary = new Dictionary<Type, object>();

        /// <summary>
        ///     删除一种类型对象
        /// </summary>
        public void Remove<T>()
        {
            Dictionary.Remove(typeof(T));
        }

        /// <summary>
        ///     附加一种类型对象
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public T Annex<T>(T value)
        {
            var type = typeof(T);
            if (Dictionary.ContainsKey(type))
            {
                if (Equals(value, default(T)))
                {
                    Dictionary.Remove(type);
                }
                else
                {
                    Dictionary[type] = value;
                }
            }
            else if (!Equals(value, default(T)))
            {
                Dictionary.Add(type, value);
            }
            return value;
        }

        /// <summary>
        ///     附加一种类型对象
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public T TryAnnex<T>(T value)
        {
            var type = typeof(T);
            Dictionary.TryAdd(type, value);
            return value;
        }

        /// <summary>
        ///     取得一种类型的扩展属性(可以自动构造或提前附加)
        /// </summary>
        /// <returns></returns>
        public T AutoDependency<T>() where T : class, new()
        {
            if (Dictionary.TryGetValue(typeof(T), out var value1))
            {
                return value1 as T;
            }
            var value = new T();
            Dictionary.Add(typeof(T), value);
            return value;
        }

        /// <summary>
        ///     取得一种类型的扩展属性(需要附加)
        /// </summary>
        /// <returns></returns>
        public T TryGetDependency<T>(Func<T> creater) where T : class
        {
            if (Dictionary.TryGetValue(typeof(T), out var value1))
                return (T)value1;
            var t = creater();
            Dictionary.TryAdd(typeof(T), t);
            return t;
        }

        /// <summary>
        ///     取得一种类型的扩展属性(需要附加)
        /// </summary>
        /// <returns></returns>
        public T Dependency<T>() where T : class
        {
            return Dictionary.TryGetValue(typeof(T), out var value1) ? value1 as T : null;
        }

        /// <summary>
        ///     取得一种类型的扩展属性(需要附加)
        /// </summary>
        /// <returns></returns>
        public T Dependency<T>(Func<T> def) where T : class
        {
            if (!Dictionary.TryGetValue(typeof(T), out var value1))
            {
                Dictionary.Add(typeof(T), value1 = def());
            }
            return value1 as T;
        }
    }
}