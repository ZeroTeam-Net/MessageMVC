// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// ����:bull2
// ����:CodeRefactor-Agebull.Common.WpfMvvmBase
// ����:2014-11-29
// �޸�:2014-11-29
// *****************************************************/

#region ����

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel.Common
{
    /// <summary>
    ///     ���������ֵ�
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public sealed class DependencyObjects
    {
        /// <summary>
        /// �ֵ�
        /// </summary>
        [JsonIgnore]
        public readonly Dictionary<Type, object> Dictionary = new Dictionary<Type, object>();

        /// <summary>
        ///     ɾ��һ�����Ͷ���
        /// </summary>
        public void Remove<T>()
        {
            Dictionary.Remove(typeof(T));
        }

        /// <summary>
        ///     ����һ�����Ͷ���
        /// </summary>
        /// <remarks>
        ///     ���ַ���ֻ����һ��,����θ���,ֻ�����һ������
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
        ///     ����һ�����Ͷ���
        /// </summary>
        /// <remarks>
        ///     ���ַ���ֻ����һ��,����θ���,ֻ�����һ������
        /// </remarks>
        public T TryAnnex<T>(T value)
        {
            var type = typeof(T);
            Dictionary.TryAdd(type, value);
            return value;
        }

        /// <summary>
        ///     ȡ��һ�����͵���չ����(�����Զ��������ǰ����)
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
        ///     ȡ��һ�����͵���չ����(��Ҫ����)
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
        ///     ȡ��һ�����͵���չ����(��Ҫ����)
        /// </summary>
        /// <returns></returns>
        public T Dependency<T>() where T : class
        {
            return Dictionary.TryGetValue(typeof(T), out var value1) ? value1 as T : null;
        }

        /// <summary>
        ///     ȡ��һ�����͵���չ����(��Ҫ����)
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