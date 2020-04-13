using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 在线消息参数
    /// </summary>
    public interface IInlineMessage : IMessageItem
    {
        /// <summary>
        /// 服务名称,即Topic
        /// </summary>
        string ServiceName => Topic;

        /// <summary>
        /// 接口名称,即Title
        /// </summary>
        string ApiName => Title;

        /// <summary>
        /// 接口参数,即Content
        /// </summary>
        string Argument => Content;

        /// <summary>
        /// 实体参数
        /// </summary>
        object ArgumentData { get; set; }

        /// <summary>
        /// 字典参数
        /// </summary>
        Dictionary<string, string> Dictionary { get; set; }

        /// <summary>
        /// 返回值已过时
        /// </summary>
        bool ResultOutdated { get; set; }

        /// <summary>
        /// 是否已在线
        /// </summary>
        bool IsInline { get; set; }

        /// <summary>
        /// 是否已离线
        /// </summary>
        bool ArgumentOutdated { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ResultData { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public IOperatorStatus RuntimeStatus { get; set; }

        #region 状态


        /// <summary>
        /// 重置
        /// </summary>
        void Reset()
        {
            Result = null;
            ResultData = null;
            RuntimeStatus = null;
            ResultOutdated = false;
            State = MessageState.None;
        }

        /// <summary>
        /// 重置
        /// </summary>
        void CopyResult(IInlineMessage message)
        {
            if (message == this)
                return;
            Dictionary = message.Dictionary;
            ArgumentData = message.ArgumentData;
            ResultData = message.ResultData;
            Result = message.Result;
            ResultData = message.ResultData;
            RuntimeStatus = message.RuntimeStatus;
            State = message.State;
            Trace = message.Trace;
        }

        /// <summary>
        /// 取得返回值
        /// </summary>
        /// <param name="serialize"></param>
        /// <returns></returns>
        string GetResult(ISerializeProxy serialize)
        {
            if (!ResultOutdated)
                return Result;
            if (ResultData == null && this.RuntimeStatus != null)
                ResultData = this.RuntimeStatus;
            ResultOutdated = false;
            if (ResultData == null)
                return Result;
            if (ResultSerializer != null)
                serialize = ResultSerializer;
            return Result = serialize.ToString(this.ResultData);
        }

        /// <summary>
        /// 取得返回值
        /// </summary>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="serialize"></param>
        /// <returns></returns>
        TRes GetResultData<TRes>(ISerializeProxy serialize)
        {
            if (this.ResultData is TRes res)
                return res;
            if (ResultSerializer != null)
                serialize = ResultSerializer;
            if (!ResultOutdated)
                return serialize.ToObject<TRes>(this.Result);
            if (this.ResultData != null)
            {
                if (ResultOutdated)
                {
                    ResultOutdated = false;
                    Result = serialize.ToString(this.ResultData);
                }
                return serialize.ToObject<TRes>(Result);
            }
            return default;
        }

        /// <summary>
        /// 准备离线(框架内调用)
        /// </summary>
        /// <returns></returns>
        void PrepareOffline()
        {
            Trace ??= GlobalContext.CurrentNoLazy?.Trace;
            if (Trace != null)
                Trace.End = DateTime.Now;

            RuntimeStatus ??= GlobalContext.CurrentNoLazy?.Status?.LastStatus;
        }

        /// <summary>
        /// 转为离线序列化文本
        /// </summary>
        /// <returns></returns>
        IMessageItem Offline(ISerializeProxy serialize)
        {
            if (ArgumentOutdated)
            {
                //BUG:会多次调用参数序列化
                Content = ArgumentData == null
                    ? null
                    : (ResultSerializer ?? serialize).ToString(ArgumentData ?? Dictionary);
            }
            GetResult(serialize);
            return this;
        }

        /// <summary>
        /// 返回值转为离线序列化文本
        /// </summary>
        /// <returns></returns>
        IMessageItem OfflineResult(ISerializeProxy serialize)
        {
            GetResult(serialize);
            return this;
        }
        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        ISerializeProxy ResultSerializer { get; set; }


        /// <summary>
        ///     返回值构造对象
        /// </summary>
        Func<int, string, object> ResultCreater { get; set; }

        /// <summary>
        /// 如果未上线且还原参数为字典,否则什么也不做
        /// </summary>
        Task Inline(ISerializeProxy serialize, Type type, ISerializeProxy resultSerializer, Func<int, string, object> errResultCreater)
        {
            if (resultSerializer != null)
                ResultSerializer = resultSerializer;
            if (errResultCreater != null)
                ResultCreater = errResultCreater;
            if (!IsInline)
            {
                IsInline = true;
                if (type == null || type.IsBaseType())
                    Dictionary = serialize.ToObject<Dictionary<string, string>>(Content);
                else
                    ArgumentData = serialize.ToObject(Content, type);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 准备在线(框架内调用)
        /// </summary>
        /// <returns></returns>
        Task PrepareInline()
        {
            return Task.CompletedTask;
        }

        #endregion
        #region 方法 


        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        object GetArgument(int scope, int serializeType, ISerializeProxy serialize, Type type);

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        string GetStringArgument(string name)
        {
            if (Dictionary == null || !Dictionary.TryGetValue(name, out var value) || !(value is string str))
                return null;
            return str;
        }

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        byte[] GetBinaryArgument(string name)
        {
            if (Binary == null || !Binary.TryGetValue(name, out var bytes))
                return null;
            return bytes;
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
        object GetValueArgument(string name, int scope, int serializeType, ISerializeProxy serialize, Type type);

        #endregion
    }
}
