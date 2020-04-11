using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 消息交互格式
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class InlineMessage : MessageItem, IInlineMessage
    {
        /// <summary>
        ///     开始时间
        /// </summary>

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime? Start { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime? End { get; set; }

        private Dictionary<string, object> extend;
        /// <summary>
        /// 其他带外内容
        /// </summary>
        public Dictionary<string, object> Extend
        {
            get => extend; set
            {
                IsInline = true;
                ArgumentOutdated = true;
                extend = value;
            }
        }

        private object argumentData;

        /// <summary>
        /// 参数
        /// </summary>
        public object ArgumentData
        {
            get => argumentData;
            set
            {
                IsInline = true;
                ArgumentOutdated = true;
                argumentData = value;
            }
        }

        /// <summary>
        /// 是否已在线
        /// </summary>
        public bool IsInline { get; set; }

        /// <summary>
        /// 是否已离线
        /// </summary>
        public bool ArgumentOutdated { get; set; }

        /// <summary>
        /// 返回值已过时
        /// </summary>
        public bool ResultOutdated { get; set; }

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
        public object ResultData
        {
            get => resultData;
            set
            {
                resultData = value;
                ResultOutdated = true;
            }
        }

        /// <summary>
        /// 执行状态
        /// </summary>
        public IOperatorStatus RuntimeStatus { get; set; }


        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        public ISerializeProxy ResultSerializer { get; set; }


        /// <summary>
        ///     返回值构造对象
        /// </summary>
        public Func<int, string, object> ResultCreater { get; set; }


        /// <summary>
        /// 服务名称,即Topic
        /// </summary>
        public string ServiceName { get => Topic; set => Topic = value; }

        /// <summary>
        /// 接口名称,即Title
        /// </summary>
        public string ApiName { get => Title; set => Title = value; }

        /// <summary>
        /// 接口参数,即Content
        /// </summary>
        public string Argument { get => Content; set => Content = value; }


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
            return ArgumentData;
        }

        /// <summary>
        /// 取参数值(动态IL代码调用)  BUG
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        public object GetValueArgument(string name, int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            if (Extend == null || !Extend.TryGetValue(name, out var value))
                return null;
            return value?.ToString();//需要优化
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
