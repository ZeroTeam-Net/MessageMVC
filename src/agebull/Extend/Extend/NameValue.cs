// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:AgebullProjectDesigner-Agebull.EntityModel.Common
// 建立:2014-08-21
// 修改:2014-08-25
// *****************************************************/

#region 引用

using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel.Common
{
    /// <summary>
    ///     名称内容对象
    /// </summary>
    /// <typeparam name="TName"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class NameValue<TName, TValue>
    {
        /// <summary>
        ///     名称
        /// </summary>
        [JsonProperty ("name")]
        public TName Name;

        /// <summary>
        ///     值
        /// </summary>
        [JsonProperty("value")]
        public TValue Value;
    }


    /// <summary>
    ///     名称内容对象
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class NameValue<TValue> : NameValue<string, TValue>
    {
        /// <summary>
        ///     构造
        /// </summary>
        public NameValue()
        {
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        public NameValue(string n, TValue v)
        {
            Name = n;
            Value = v;
        }
    }

    /// <summary>
    ///     名称内容对象
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class NameValue : NameValue<string, string>
    {
        /// <summary>
        ///     构造
        /// </summary>
        public NameValue()
        {
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        public NameValue(string n, object v)
        {
            Name = n;
            Value = v?.ToString();
        }
    }

}
