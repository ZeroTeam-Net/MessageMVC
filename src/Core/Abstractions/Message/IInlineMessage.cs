using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 在线消息参数
    /// </summary>
    public interface IInlineMessage : IMessageItem
    {
        #region 数据

        /// <summary>
        /// 实体参数
        /// </summary>
        object ArgumentData { get; set; }

        /// <summary>
        /// 字典参数
        /// </summary>
        Dictionary<string, string> Dictionary { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ResultData { get; set; }

        #endregion

        #region 状态

        /// <summary>
        /// 过程状态
        /// </summary>
        public MessageState RealState
        {
            set
            {
                State = value;
                DataState &= ~(MessageDataState.ResultOffline | MessageDataState.ResultOffline);
            }
        }

        /// <summary>
        /// 数据状态
        /// </summary>
        MessageDataState DataState { get; set; }

        /// <summary>
        /// 重置
        /// </summary>
        void Reset()
        {
            Result = null;
            ResultData = null;

            DataState = MessageDataState.None;
            if (Content != null)
            {
                DataState |= MessageDataState.ArgumentOffline;
            }
            if (ArgumentData != null || Dictionary != null)
            {
                DataState |= MessageDataState.ArgumentInline;
            }
        }

        /// <summary>
        /// 准备在线(框架内调用)
        /// </summary>
        /// <returns></returns>
        Task PrepareInline()
        {
            DataState = MessageDataState.None;
            if (Content != null)
            {
                DataState |= MessageDataState.ArgumentOffline;
            }
            if (ArgumentData != null || Dictionary != null)
            {
                DataState |= MessageDataState.ArgumentInline;
            }
            if (ResultData != null)
            {
                DataState |= MessageDataState.ResultInline;
            }
            if (Result != null)
            {
                DataState |= MessageDataState.ResultOffline;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 转为离线序列化文本
        /// </summary>
        /// <returns></returns>
        IMessageItem Offline(ISerializeProxy serialize = null)
        {
            ArgumentOffline(serialize);
            OfflineResult(serialize);
            return this;
        }

        #endregion

        #region 参数值 

        /// <summary>
        /// 转为离线序列化文本
        /// </summary>
        /// <returns></returns>
        void ArgumentOffline(ISerializeProxy serialize = null)
        {
            if (!DataState.HasFlag(MessageDataState.ArgumentOffline))
            {
                if (DataState.HasFlag(MessageDataState.ArgumentInline))
                    Content = SmartSerializer.ToString(ArgumentData ?? Dictionary, serialize);
                else if (Dictionary != null)
                    Content = SmartSerializer.ToString(Dictionary, serialize);
                DataState |= MessageDataState.ArgumentOffline;
            }
        }

        /// <summary>
        /// 数据设置为上线状态
        /// </summary>
        Task ArgumentInline(Type argumentType, ISerializeProxy resultSerializer, Func<int, string, object> errResultCreater)
        {
            return ArgumentInline(null, argumentType, resultSerializer, errResultCreater);
        }

        /// <summary>
        /// 数据设置为上线状态
        /// </summary>
        Task ArgumentInline(ISerializeProxy argSerializer, Type argumentType, ISerializeProxy resultSerializer, Func<int, string, object> errResultCreater)
        {
            if (resultSerializer != null)
                ResultSerializer = resultSerializer;
            if (errResultCreater != null)
                ResultCreater = errResultCreater;

            if (!DataState.AnyFlags(MessageDataState.ArgumentInline))
            {
                if (argSerializer != null)
                {
                    if (argumentType == null || argumentType.IsBaseType())
                        Dictionary = argSerializer.ToObject<Dictionary<string, string>>(Content);
                    else
                        ArgumentData = argSerializer.ToObject(Content, argumentType);
                }
                else
                {
                    if (argumentType == null || argumentType.IsBaseType())
                        Dictionary = SmartSerializer.ToObject<Dictionary<string, string>>(Content);
                    else
                        ArgumentData = SmartSerializer.ToObject(Content, argumentType);
                }
                DataState |= MessageDataState.ArgumentInline | MessageDataState.ArgumentOffline;
            }
            return Task.CompletedTask;
        }

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

        /*// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>值</returns>
        byte[] GetBinaryArgument(string name)
        {
            if (Binary == null || !Binary.TryGetValue(name, out var bytes))
                return null;
            return bytes;
        }*/

        /// <summary>
        /// 取参数值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <returns>值</returns>
        string GetScopeArgument(string name, ArgumentScope scope = ArgumentScope.HttpArgument)
        {
            if (Dictionary == null || !Dictionary.TryGetValue(name, out var value))
                return null;
            return value;
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
        object GetValueArgument(string name, int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            if (Dictionary == null || !Dictionary.TryGetValue(name, out var value))
                return null;
            return value;
        }

        /// <summary>
        /// 取参数值(动态IL代码调用)
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="scope">参数范围</param>
        /// <param name="serializeType">序列化类型</param>
        /// <param name="serialize">序列化器</param>
        /// <param name="type">序列化对象</param>
        /// <returns>值</returns>
        object FrameGetValueArgument(string name, int scope, int serializeType, ISerializeProxy serialize, Type type)
        {
            if (Dictionary == null || !Dictionary.TryGetValue(name, out var value))
            {
                if (type != typeof(string))
                    throw new MessageArgumentNullException(name);
                return null;
            }
            return value;
        }
        #endregion

        #region 返回值

        /// <summary>
        ///     返回值构造对象
        /// </summary>
        Func<int, string, object> ResultCreater { get; set; }

        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        ISerializeProxy ResultSerializer { get; set; }


        /// <summary>
        /// 取得返回值
        /// </summary>
        /// <param name="serialize"></param>
        /// <returns></returns>
        IInlineMessage OfflineResult(ISerializeProxy serialize = null)
        {
            if (DataState.HasFlag(MessageDataState.ResultOffline))
                return this;

            if (DataState.HasFlag(MessageDataState.ResultInline))
            {
                Result = SmartSerializer.ToString(ResultData, ResultSerializer ?? serialize);
                DataState |= MessageDataState.ResultOffline;
                return this;
            }
            if (ResultCreater == null)
            {
                DataState |= MessageDataState.ResultOffline;
                return this;
            }
            ResultData = ResultCreater(State.ToErrorCode(), Result);
            Result = SmartSerializer.ToString(ResultData, ResultSerializer ?? serialize);
            DataState |= MessageDataState.ResultOffline;
            return this;
        }

        /// <summary>
        /// 取得返回值
        /// </summary>
        /// <typeparam name="TRes"></typeparam>
        /// <returns></returns>
        TRes GetResultData<TRes>()
        {
            if (DataState.HasFlag(MessageDataState.ResultInline))
            {
                if (ResultData is TRes res)
                    return res;
                if (!DataState.HasFlag(MessageDataState.ResultOffline))
                {
                    DataState |= MessageDataState.ResultOffline;
                    Result = SmartSerializer.ToString(ResultData, ResultSerializer);
                }
                return SmartSerializer.ToObject<TRes>(Result);
            }
            if (DataState.HasFlag(MessageDataState.ResultOffline))
            {
                var res = SmartSerializer.ToObject<TRes>(Result);
                ResultData = res;
                return res;
            }
            ResultData = ResultCreater?.Invoke(State.ToErrorCode(), Result);
            Result = SmartSerializer.ToString(ResultData, ResultSerializer);
            DataState |= MessageDataState.ResultOffline;
            return SmartSerializer.ToObject<TRes>(Result);
        }


        #endregion

        #region MessageResult

        /// <summary>
        /// 复制构造一个返回值对象
        /// </summary>
        /// <returns></returns>
        IMessageResult ToMessageResult(bool offline, ISerializeProxy serialize = null)
        {
            if (offline)
                OfflineResult(serialize);
            return offline
                ? new MessageResult
                {
                    ID = ID,
                    State = State,
                    Trace = Trace,
                    Result = Result,
                    DataState = MessageDataState.ResultOffline
                }
                : new MessageResult
                {
                    ID = ID,
                    State = State,
                    Trace = Trace,
                    Result = Result,
                    ResultData = ResultData,
                    DataState = DataState & (MessageDataState.ResultInline | MessageDataState.ResultOffline)
                };
        }

        /// <summary>
        /// 复制构造一个返回值对象,仅包含状态
        /// </summary>
        /// <returns></returns>
        IMessageResult ToStateResult()
        {
            return new MessageResult
            {
                ID = ID,
                State = State
            };
        }

        /// <summary>
        /// 复制返回值
        /// </summary>
        void CopyResult(IMessageResult message)
        {
            State = message.State;
            if (message.Trace != null)
                Trace = message.Trace;

            if (message.DataState.HasFlag(MessageDataState.ResultOffline))
            {
                ResultData = message.ResultData;
            }
            else
            {
                ResultData = null;
                DataState &= ~MessageDataState.ResultInline;
            }
            Result = message.Result;
            if (message.DataState.HasFlag(MessageDataState.ResultInline))
            {
                DataState |= MessageDataState.ResultOffline;
            }
            else
            {
                DataState &= ~MessageDataState.ResultOffline;
            }
        }

        #endregion

        #region 扩展名称

        /// <summary>
        /// 是否外部访问
        /// </summary>
        bool IsOutAccess { get; }

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

        #endregion
    }
}
