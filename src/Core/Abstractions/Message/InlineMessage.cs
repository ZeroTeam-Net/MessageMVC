using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息交互格式
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class InlineMessage : MessageItem, IInlineMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        string IInlineMessage.MessageType => "InlineMessage";

        /// <summary>
        /// 请求ID
        /// </summary>
        [JsonIgnore]
        public string RequestId { get; set; }

        /// <summary>
        /// 是否外部访问
        /// </summary>
        [JsonIgnore]
        public bool IsOutAccess => false;

        /// <summary>
        /// 字典参数
        /// </summary>
        private Dictionary<string, string> dictionary;

        /// <summary>
        /// 字典参数
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> ExtensionDictionary
        {
            get => dictionary;
            set
            {
                dictionary = value;
                DataState |= MessageDataState.ExtensionInline;
                if (Extension == null && value == null)
                    DataState |= MessageDataState.ExtensionOffline;
                else
                    DataState &= ~MessageDataState.ExtensionOffline;
            }
        }

        private object argumentData;

        /// <summary>
        /// 二进制字典参数
        /// </summary>
        public Dictionary<string, byte[]> BinaryDictionary { get; set; }

        /// <summary>
        /// 实体参数
        /// </summary>
        [JsonIgnore]
        public object ArgumentData
        {
            get => argumentData;
            set
            {
                argumentData = value;
                DataState |= MessageDataState.ArgumentInline;
                if (base.Argument == null && value == null)
                    DataState |= MessageDataState.ArgumentOffline;
                else
                    DataState &= ~MessageDataState.ArgumentOffline;
            }
        }

        private object resultData;
        /// <summary>
        /// 处理结果,对应状态的解释信息
        /// </summary>
        /// <remarks>
        /// 未消费:无内容
        /// 已接受:无内容
        /// 格式错误 : 无内容
        /// 无处理方法 : 无内容
        /// 处理异常 : 异常信息
        /// 处理失败 : 失败内容或原因
        /// 处理成功 : 结果信息或无
        /// </remarks>
        [JsonIgnore]
        public object ResultData
        {
            get => resultData;
            set
            {
                resultData = value;
                DataState |= MessageDataState.ResultInline;
                if (Result == null && value == null)
                    DataState |= MessageDataState.ResultOffline;
                else
                    DataState &= ~MessageDataState.ResultOffline;
            }
        }

        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        [JsonIgnore]
        public ISerializeProxy ResultSerializer { get; set; }


        /// <summary>
        ///     返回值构造对象
        /// </summary>
        [JsonIgnore]
        public Func<int, string, object> ResultCreater { get; set; }

        /// <summary>
        /// 参数序列化器
        /// </summary>
        [JsonIgnore]
        public ISerializeProxy ArgumentSerializer { get; set; }

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        public object GetArgument(int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            if (type != null)
                ((IInlineMessage)this).RestoryContent(serialize, type);
            return ArgumentData;
        }

    }
}

/*// <summary>
///     文件内容二进制数据
/// </summary>
public byte[] Bytes { get; set; }

/// <summary>
/// 生产者信息
/// </summary>
[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
public string ProducerInfo { get; set; }

/// <summary>
/// 构造
/// </summary>
/// <returns></returns>
public MessageItem()
{

}*/
