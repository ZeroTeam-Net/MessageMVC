#region ����

using System;
using System.Collections.Generic;

#endregion

namespace Agebull.Common.Ioc
{
    /// <summary>
    ///     ��Χ����
    /// </summary>
    public sealed class ScopeAttachData
    {
        /// <summary>
        /// �ֵ�
        /// </summary>
        internal readonly Dictionary<Type, object> dictionary = new Dictionary<Type, object>();

        /// <summary>
        /// ��¡�¸���
        /// </summary>
        /// <returns></returns>
        public ScopeAttachData Clone()
        {
            var n = new ScopeAttachData();
            foreach (var kv in dictionary)
                n.dictionary.Add(kv.Key, kv.Value);
            return n;
        }

        /// <summary>
        ///     ����
        /// </summary>
        public void Detach<T>()
        {
            dictionary.Remove(typeof(T));
        }

        /// <summary>
        ///     ����һ�����Ͷ���
        /// </summary>
        /// <remarks>
        ///     ���ַ���ֻ����һ��,����θ���,ֻ�����һ������
        /// </remarks>
        public T Attach<T>(T value)
        {
            var type = typeof(T);
            if (dictionary.ContainsKey(type))
            {
                if (Equals(value, default(T)))
                {
                    dictionary.Remove(type);
                }
                else
                {
                    dictionary[type] = value;
                }
            }
            else if (!Equals(value, default(T)))
            {
                dictionary.Add(type, value);
            }
            return value;
        }

        /// <summary>
        ///     ����һ�����Ͷ���
        /// </summary>
        /// <remarks>
        ///     ���ַ���ֻ����һ��,����θ���,ֻ�����һ������
        /// </remarks>
        public T TryAttach<T>(T value)
        {
            var type = typeof(T);
            dictionary.TryAdd(type, value);
            return value;
        }

        /// <summary>
        ///     ȡ��һ�����͵���չ����(�����Զ��������ǰ����)
        /// </summary>
        /// <returns></returns>
        public T AutoDependency<T>() where T : class, new()
        {
            if (dictionary.TryGetValue(typeof(T), out var value1))
            {
                return value1 as T;
            }
            var value = new T();
            dictionary.Add(typeof(T), value);
            return value;
        }

        /// <summary>
        ///     ȡ��һ�����͵���չ����(��Ҫ����)
        /// </summary>
        /// <returns></returns>
        public T TryGetDependency<T>(Func<T> creater) where T : class
        {
            if (dictionary.TryGetValue(typeof(T), out var value1))
                return (T)value1;
            var t = creater();
            dictionary.TryAdd(typeof(T), t);
            return t;
        }

        /// <summary>
        ///     ȡ��һ�����͵���չ����(��Ҫ����)
        /// </summary>
        /// <returns></returns>
        public T Get<T>()
        {
            return dictionary.TryGetValue(typeof(T), out var value1) && value1 is T t ? t : default;
        }

        /// <summary>
        ///     ȡ��һ�����͵���չ����(��Ҫ����)
        /// </summary>
        /// <returns></returns>
        public T Dependency<T>(Func<T> def) where T : class
        {
            if (!dictionary.TryGetValue(typeof(T), out var value1))
            {
                dictionary.Add(typeof(T), value1 = def());
            }
            return value1 as T;
        }
    }
}