#region 引用

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     动态方法参数转换器
    /// </summary>
    public class ActionArgumentConvert
    {
        #region 基本属性

        /// <summary>
        ///     是否发生解析错误
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        ///     字段
        /// </summary>
        private readonly Dictionary<string, string> _messages = new Dictionary<string, string>();

        /// <summary>
        /// 设置错误字段
        /// </summary>
        /// <param name="field"></param>
        /// <param name="msg"></param>
        private void AddMessage(string field, string msg)
        {
            if (_messages.TryGetValue(field, out var val))
                _messages[field] = $"{val};{msg}";
            else
                _messages.Add(field, msg);
        }


        /// <summary>
        ///     是否发生解析错误
        /// </summary>
        public string Message
        {
            get
            {
                StringBuilder msg = new StringBuilder();
                foreach (var kv in _messages)
                {
                    msg.AppendLine($"{kv.Key} : {kv.Value}");
                }
                return msg.ToString();
            }
        }

        #endregion

        #region 字段值转换 

        /// <summary>
        /// 字段值转换(框架调用)
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="convert">转换器</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool FrameTryGetNullable<T>(string field, Func<string, T> convert, out T? value)
            where T : struct
        {
            if (MessageArgumentConvert.TryGet<T>(field, convert, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换(框架调用)
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <param name="convert">转换器</param>
        /// <returns>是否接收值</returns>
        public bool FrameTryGet<T>(string field, Func<string, T> convert, out T value)
        {
            if (!MessageArgumentConvert.FrameTryGetValue(field, out var str))
            {
                value = default;
                return false;
            }
            if (string.IsNullOrWhiteSpace(str))
            {
                value = default;
                return true;
            }
            try
            {
                value = convert(str);
                return true;
            }
            catch
            {
            }
            value = default;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out string value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetEnumNullable<TEnum>(string field, out TEnum? value)
            where TEnum : struct
        {
            if (!MessageArgumentConvert.FrameTryGetValue(field, out var str))
            {
                value = default;
                return false;
            }
            if(string.IsNullOrWhiteSpace(str))
            {
                value = default;
                return true;
            }
            try
            {
                if(Enum.TryParse(str, true, out TEnum value1))
                {
                    value = value1;
                    return true;
                }
            }
            catch
            {
            }

            value = default;
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetEnum<TEnum>(string field, out TEnum value)
            where TEnum : struct
        {
            if (MessageArgumentConvert.TryGetEnum(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out byte value)
        {
            if (MessageArgumentConvert.TryGet(field, byte.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out byte? value)
        {
            if (MessageArgumentConvert.TryGet(field, byte.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out sbyte value)
        {
            if (MessageArgumentConvert.TryGet(field, sbyte.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out sbyte? value)
        {
            if (MessageArgumentConvert.TryGet(field, sbyte.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out short value)
        {
            if (MessageArgumentConvert.TryGet(field, short.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out short? value)
        {
            if (MessageArgumentConvert.TryGet(field, short.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ushort value)
        {
            if (MessageArgumentConvert.TryGet(field, ushort.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ushort? value)
        {
            if (MessageArgumentConvert.TryGet(field, ushort.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out bool value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out bool? value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out int value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out int? value)
        {
            if (MessageArgumentConvert.TryGet(field, int.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out uint value)
        {
            if (MessageArgumentConvert.TryGet(field, uint.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out uint? value)
        {
            if (MessageArgumentConvert.TryGet(field, uint.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out long value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out long? value)
        {
            if (MessageArgumentConvert.TryGet(field, long.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ulong value)
        {
            if (MessageArgumentConvert.TryGet(field, ulong.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out ulong? value)
        {
            if (MessageArgumentConvert.TryGet(field, ulong.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out float value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out float? value)
        {
            if (MessageArgumentConvert.TryGet(field, float.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out double value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out double? value)
        {
            if (MessageArgumentConvert.TryGet(field, double.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out DateTime value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out DateTime? value)
        {
            if (MessageArgumentConvert.TryGet(field, DateTime.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out decimal value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out decimal? value)
        {
            if (MessageArgumentConvert.TryGet(field, decimal.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out Guid value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out Guid? value)
        {
            if (MessageArgumentConvert.TryGet(field, Guid.Parse, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }


        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out List<int> value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }
        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetValue(string field, out List<long> value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }

        /// <summary>
        /// 字段值转换
        /// </summary>
        /// <param name="field">名称</param>
        /// <param name="value">字段名称</param>
        /// <returns>是否接收值</returns>
        public bool TryGetIDs(string field, out List<long> value)
        {
            if (MessageArgumentConvert.TryGet(field, out value))
            {
                return true;
            }
            AddMessage(field, "参数值转换出错");
            Failed = true;
            return false;
        }
        #endregion
    }
}
