/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 16:45:24*/
#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;


using Agebull.Common;
using Agebull.EntityModel.Common;
using Agebull.EntityModel.Interfaces;


#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink
{
    /// <summary>
    /// 消息存储
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class MessageData : IIdentityData
    {
        #region 构造
        
        /// <summary>
        /// 构造
        /// </summary>
        public MessageData()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize();
        #endregion

        #region 基本属性


        /// <summary>
        /// 修改主键
        /// </summary>
        public void ChangePrimaryKey(long id)
        {
            _id = id;
        }
        /// <summary>
        /// 主键
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _id;

        partial void OnIdGet();

        partial void OnIdSet(ref long value);

        partial void OnIdLoad(ref long value);

        partial void OnIdSeted();

        
        /// <summary>
        ///  主键
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , ReadOnly(true) , DisplayName(@"主键")]
        public long Id
        {
            get
            {
                OnIdGet();
                return this._id;
            }
            set
            {
                if(this._id == value)
                    return;
                //if(this._id > 0)
                //    throw new Exception("主键一旦设置就不可以修改");
                OnIdSet(ref value);
                this._id = value;
                this.OnPropertyChanged(_DataStruct_.Real_Id);
                OnIdSeted();
            }
        }
        /// <summary>
        /// 全局请求标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _traceId;

        partial void OnTraceIdGet();

        partial void OnTraceIdSet(ref string value);

        partial void OnTraceIdSeted();

        
        /// <summary>
        ///  全局请求标识
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("TraceId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"全局请求标识")]
        public  string TraceId
        {
            get
            {
                OnTraceIdGet();
                return this._traceId;
            }
            set
            {
                if(this._traceId == value)
                    return;
                OnTraceIdSet(ref value);
                this._traceId = value;
                OnTraceIdSeted();
                this.OnPropertyChanged(_DataStruct_.Real_TraceId);
                this.OnPropertyChanged(nameof(TraceId));
            }
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public DateTime _start;

        partial void OnStartGet();

        partial void OnStartSet(ref DateTime value);

        partial void OnStartSeted();

        
        /// <summary>
        ///  开始时间
        /// </summary>
        /// <example>
        ///     2012-12-21
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Start", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , JsonConverter(typeof(MyDateTimeConverter)) , DisplayName(@"开始时间")]
        public  DateTime Start
        {
            get
            {
                OnStartGet();
                return this._start;
            }
            set
            {
                if(this._start == value)
                    return;
                OnStartSet(ref value);
                this._start = value;
                OnStartSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Start);
                this.OnPropertyChanged(nameof(Start));
            }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public DateTime _end;

        partial void OnEndGet();

        partial void OnEndSet(ref DateTime value);

        partial void OnEndSeted();

        
        /// <summary>
        ///  结束时间
        /// </summary>
        /// <example>
        ///     2012-12-21
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("End", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , JsonConverter(typeof(MyDateTimeConverter)) , DisplayName(@"结束时间")]
        public  DateTime End
        {
            get
            {
                OnEndGet();
                return this._end;
            }
            set
            {
                if(this._end == value)
                    return;
                OnEndSet(ref value);
                this._end = value;
                OnEndSeted();
                this.OnPropertyChanged(_DataStruct_.Real_End);
                this.OnPropertyChanged(nameof(End));
            }
        }
        /// <summary>
        /// 本地的全局标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _localId;

        partial void OnLocalIdGet();

        partial void OnLocalIdSet(ref string value);

        partial void OnLocalIdSeted();

        
        /// <summary>
        ///  本地的全局标识
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("LocalId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"本地的全局标识")]
        public  string LocalId
        {
            get
            {
                OnLocalIdGet();
                return this._localId;
            }
            set
            {
                if(this._localId == value)
                    return;
                OnLocalIdSet(ref value);
                this._localId = value;
                OnLocalIdSeted();
                this.OnPropertyChanged(_DataStruct_.Real_LocalId);
                this.OnPropertyChanged(nameof(LocalId));
            }
        }
        /// <summary>
        /// 本地的应用
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _localApp;

        partial void OnLocalAppGet();

        partial void OnLocalAppSet(ref string value);

        partial void OnLocalAppSeted();

        
        /// <summary>
        ///  本地的应用
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("LocalApp", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"本地的应用")]
        public  string LocalApp
        {
            get
            {
                OnLocalAppGet();
                return this._localApp;
            }
            set
            {
                if(this._localApp == value)
                    return;
                OnLocalAppSet(ref value);
                this._localApp = value;
                OnLocalAppSeted();
                this.OnPropertyChanged(_DataStruct_.Real_LocalApp);
                this.OnPropertyChanged(nameof(LocalApp));
            }
        }
        /// <summary>
        /// 本地的机器
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _localMachine;

        partial void OnLocalMachineGet();

        partial void OnLocalMachineSet(ref string value);

        partial void OnLocalMachineSeted();

        
        /// <summary>
        ///  本地的机器
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("LocalMachine", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"本地的机器")]
        public  string LocalMachine
        {
            get
            {
                OnLocalMachineGet();
                return this._localMachine;
            }
            set
            {
                if(this._localMachine == value)
                    return;
                OnLocalMachineSet(ref value);
                this._localMachine = value;
                OnLocalMachineSeted();
                this.OnPropertyChanged(_DataStruct_.Real_LocalMachine);
                this.OnPropertyChanged(nameof(LocalMachine));
            }
        }
        /// <summary>
        /// 请求方跟踪标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _callId;

        partial void OnCallIdGet();

        partial void OnCallIdSet(ref string value);

        partial void OnCallIdSeted();

        
        /// <summary>
        ///  请求方跟踪标识
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("CallId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"请求方跟踪标识")]
        public  string CallId
        {
            get
            {
                OnCallIdGet();
                return this._callId;
            }
            set
            {
                if(this._callId == value)
                    return;
                OnCallIdSet(ref value);
                this._callId = value;
                OnCallIdSeted();
                this.OnPropertyChanged(_DataStruct_.Real_CallId);
                this.OnPropertyChanged(nameof(CallId));
            }
        }
        /// <summary>
        /// 请求应用
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _callApp;

        partial void OnCallAppGet();

        partial void OnCallAppSet(ref string value);

        partial void OnCallAppSeted();

        
        /// <summary>
        ///  请求应用
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("CallApp", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"请求应用")]
        public  string CallApp
        {
            get
            {
                OnCallAppGet();
                return this._callApp;
            }
            set
            {
                if(this._callApp == value)
                    return;
                OnCallAppSet(ref value);
                this._callApp = value;
                OnCallAppSeted();
                this.OnPropertyChanged(_DataStruct_.Real_CallApp);
                this.OnPropertyChanged(nameof(CallApp));
            }
        }
        /// <summary>
        /// 请求机器
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _callMachine;

        partial void OnCallMachineGet();

        partial void OnCallMachineSet(ref string value);

        partial void OnCallMachineSeted();

        
        /// <summary>
        ///  请求机器
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("CallMachine", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"请求机器")]
        public  string CallMachine
        {
            get
            {
                OnCallMachineGet();
                return this._callMachine;
            }
            set
            {
                if(this._callMachine == value)
                    return;
                OnCallMachineSet(ref value);
                this._callMachine = value;
                OnCallMachineSeted();
                this.OnPropertyChanged(_DataStruct_.Real_CallMachine);
                this.OnPropertyChanged(nameof(CallMachine));
            }
        }
        /// <summary>
        /// 上下文信息
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _context;

        partial void OnContextGet();

        partial void OnContextSet(ref string value);

        partial void OnContextSeted();

        
        /// <summary>
        ///  上下文信息
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Context", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"上下文信息")]
        public  string Context
        {
            get
            {
                OnContextGet();
                return this._context;
            }
            set
            {
                if(this._context == value)
                    return;
                OnContextSet(ref value);
                this._context = value;
                OnContextSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Context);
                this.OnPropertyChanged(nameof(Context));
            }
        }
        /// <summary>
        /// 身份令牌
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _token;

        partial void OnTokenGet();

        partial void OnTokenSet(ref string value);

        partial void OnTokenSeted();

        
        /// <summary>
        ///  身份令牌
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Token", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"身份令牌")]
        public  string Token
        {
            get
            {
                OnTokenGet();
                return this._token;
            }
            set
            {
                if(this._token == value)
                    return;
                OnTokenSet(ref value);
                this._token = value;
                OnTokenSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Token);
                this.OnPropertyChanged(nameof(Token));
            }
        }
        /// <summary>
        /// 请求头信息
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _headers;

        partial void OnHeadersGet();

        partial void OnHeadersSet(ref string value);

        partial void OnHeadersSeted();

        
        /// <summary>
        ///  请求头信息
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Headers", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"请求头信息")]
        public  string Headers
        {
            get
            {
                OnHeadersGet();
                return this._headers;
            }
            set
            {
                if(this._headers == value)
                    return;
                OnHeadersSet(ref value);
                this._headers = value;
                OnHeadersSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Headers);
                this.OnPropertyChanged(nameof(Headers));
            }
        }
        /// <summary>
        /// 消息序列化文本
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _message;

        partial void OnMessageGet();

        partial void OnMessageSet(ref string value);

        partial void OnMessageSeted();

        
        /// <summary>
        ///  消息序列化文本
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"消息序列化文本")]
        public  string Message
        {
            get
            {
                OnMessageGet();
                return this._message;
            }
            set
            {
                if(this._message == value)
                    return;
                OnMessageSet(ref value);
                this._message = value;
                OnMessageSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Message);
                this.OnPropertyChanged(nameof(Message));
            }
        }
        /// <summary>
        /// 流程步骤记录
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _flowStep;

        partial void OnFlowStepGet();

        partial void OnFlowStepSet(ref string value);

        partial void OnFlowStepSeted();

        
        /// <summary>
        ///  流程步骤记录
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("FlowStep", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"流程步骤记录")]
        public  string FlowStep
        {
            get
            {
                OnFlowStepGet();
                return this._flowStep;
            }
            set
            {
                if(this._flowStep == value)
                    return;
                OnFlowStepSet(ref value);
                this._flowStep = value;
                OnFlowStepSeted();
                this.OnPropertyChanged(_DataStruct_.Real_FlowStep);
                this.OnPropertyChanged(nameof(FlowStep));
            }
        }
        /// <summary>
        /// 调用层级
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _level;

        partial void OnLevelGet();

        partial void OnLevelSet(ref int value);

        partial void OnLevelSeted();

        
        /// <summary>
        ///  调用层级
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Level", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"调用层级")]
        public  int Level
        {
            get
            {
                OnLevelGet();
                return this._level;
            }
            set
            {
                if(this._level == value)
                    return;
                OnLevelSet(ref value);
                this._level = value;
                OnLevelSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Level);
                this.OnPropertyChanged(nameof(Level));
            }
        }

        #region 接口属性

        #endregion
        #region 扩展属性

        #endregion
        #endregion


        #region 名称的属性操作

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected override bool SetValueInner(string property, string value)
        {
            if(property == null) return false;
            switch(property.Trim().ToLower())
            {
            case "id":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.Id = vl;
                        return true;
                    }
                }
                return false;
            case "traceid":
                this.TraceId = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "start":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (DateTime.TryParse(value, out var vl))
                    {
                        this.Start = vl;
                        return true;
                    }
                }
                return false;
            case "end":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (DateTime.TryParse(value, out var vl))
                    {
                        this.End = vl;
                        return true;
                    }
                }
                return false;
            case "localid":
                this.LocalId = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "localapp":
                this.LocalApp = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "localmachine":
                this.LocalMachine = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "callid":
                this.CallId = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "callapp":
                this.CallApp = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "callmachine":
                this.CallMachine = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "context":
                this.Context = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "token":
                this.Token = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "headers":
                this.Headers = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "message":
                this.Message = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "flowstep":
                this.FlowStep = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "level":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.Level = vl;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(string property, object value)
        {
            if(property == null) return;
            switch(property.Trim().ToLower())
            {
            case "id":
                this.Id = (long)Convert.ToDecimal(value);
                return;
            case "traceid":
                this.TraceId = value == null ? null : value.ToString();
                return;
            case "start":
                this.Start = Convert.ToDateTime(value);
                return;
            case "end":
                this.End = Convert.ToDateTime(value);
                return;
            case "localid":
                this.LocalId = value == null ? null : value.ToString();
                return;
            case "localapp":
                this.LocalApp = value == null ? null : value.ToString();
                return;
            case "localmachine":
                this.LocalMachine = value == null ? null : value.ToString();
                return;
            case "callid":
                this.CallId = value == null ? null : value.ToString();
                return;
            case "callapp":
                this.CallApp = value == null ? null : value.ToString();
                return;
            case "callmachine":
                this.CallMachine = value == null ? null : value.ToString();
                return;
            case "context":
                this.Context = value == null ? null : value.ToString();
                return;
            case "token":
                this.Token = value == null ? null : value.ToString();
                return;
            case "headers":
                this.Headers = value == null ? null : value.ToString();
                return;
            case "message":
                this.Message = value == null ? null : value.ToString();
                return;
            case "flowstep":
                this.FlowStep = value == null ? null : value.ToString();
                return;
            case "level":
                this.Level = (int)Convert.ToDecimal(value);
                return;
            }

            //System.Diagnostics.Trace.WriteLine(property + @"=>" + value);

        }

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(int index, object value)
        {
            switch(index)
            {
            case _DataStruct_.Id:
                this.Id = Convert.ToInt64(value);
                return;
            case _DataStruct_.TraceId:
                this.TraceId = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Start:
                this.Start = Convert.ToDateTime(value);
                return;
            case _DataStruct_.End:
                this.End = Convert.ToDateTime(value);
                return;
            case _DataStruct_.LocalId:
                this.LocalId = value == null ? null : value.ToString();
                return;
            case _DataStruct_.LocalApp:
                this.LocalApp = value == null ? null : value.ToString();
                return;
            case _DataStruct_.LocalMachine:
                this.LocalMachine = value == null ? null : value.ToString();
                return;
            case _DataStruct_.CallId:
                this.CallId = value == null ? null : value.ToString();
                return;
            case _DataStruct_.CallApp:
                this.CallApp = value == null ? null : value.ToString();
                return;
            case _DataStruct_.CallMachine:
                this.CallMachine = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Context:
                this.Context = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Token:
                this.Token = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Headers:
                this.Headers = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Message:
                this.Message = value == null ? null : value.ToString();
                return;
            case _DataStruct_.FlowStep:
                this.FlowStep = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Level:
                this.Level = Convert.ToInt32(value);
                return;
            }
        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="property"></param>
        protected override object GetValueInner(string property)
        {
            switch(property)
            {
            case "id":
                return this.Id;
            case "traceid":
                return this.TraceId;
            case "start":
                return this.Start;
            case "end":
                return this.End;
            case "localid":
                return this.LocalId;
            case "localapp":
                return this.LocalApp;
            case "localmachine":
                return this.LocalMachine;
            case "callid":
                return this.CallId;
            case "callapp":
                return this.CallApp;
            case "callmachine":
                return this.CallMachine;
            case "context":
                return this.Context;
            case "token":
                return this.Token;
            case "headers":
                return this.Headers;
            case "message":
                return this.Message;
            case "flowstep":
                return this.FlowStep;
            case "level":
                return this.Level;
            }

            return null;
        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="index"></param>
        protected override object GetValueInner(int index)
        {
            switch(index)
            {
                case _DataStruct_.Id:
                    return this.Id;
                case _DataStruct_.TraceId:
                    return this.TraceId;
                case _DataStruct_.Start:
                    return this.Start;
                case _DataStruct_.End:
                    return this.End;
                case _DataStruct_.LocalId:
                    return this.LocalId;
                case _DataStruct_.LocalApp:
                    return this.LocalApp;
                case _DataStruct_.LocalMachine:
                    return this.LocalMachine;
                case _DataStruct_.CallId:
                    return this.CallId;
                case _DataStruct_.CallApp:
                    return this.CallApp;
                case _DataStruct_.CallMachine:
                    return this.CallMachine;
                case _DataStruct_.Context:
                    return this.Context;
                case _DataStruct_.Token:
                    return this.Token;
                case _DataStruct_.Headers:
                    return this.Headers;
                case _DataStruct_.Message:
                    return this.Message;
                case _DataStruct_.FlowStep:
                    return this.FlowStep;
                case _DataStruct_.Level:
                    return this.Level;
            }

            return null;
        }

        #endregion

        #region 复制
        

        partial void CopyExtendValue(MessageData source);

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected override void CopyValueInner(DataObjectBase source)
        {
            var sourceEntity = source as MessageData;
            if(sourceEntity == null)
                return;
            this._id = sourceEntity._id;
            this._traceId = sourceEntity._traceId;
            this._start = sourceEntity._start;
            this._end = sourceEntity._end;
            this._localId = sourceEntity._localId;
            this._localApp = sourceEntity._localApp;
            this._localMachine = sourceEntity._localMachine;
            this._callId = sourceEntity._callId;
            this._callApp = sourceEntity._callApp;
            this._callMachine = sourceEntity._callMachine;
            this._context = sourceEntity._context;
            this._token = sourceEntity._token;
            this._headers = sourceEntity._headers;
            this._message = sourceEntity._message;
            this._flowStep = sourceEntity._flowStep;
            this._level = sourceEntity._level;
            CopyExtendValue(sourceEntity);
            this.__status.IsModified = true;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void Copy(MessageData source)
        {
                this.Id = source.Id;
                this.TraceId = source.TraceId;
                this.Start = source.Start;
                this.End = source.End;
                this.LocalId = source.LocalId;
                this.LocalApp = source.LocalApp;
                this.LocalMachine = source.LocalMachine;
                this.CallId = source.CallId;
                this.CallApp = source.CallApp;
                this.CallMachine = source.CallMachine;
                this.Context = source.Context;
                this.Token = source.Token;
                this.Headers = source.Headers;
                this.Message = source.Message;
                this.FlowStep = source.FlowStep;
                this.Level = source.Level;
        }
        #endregion

        #region 后期处理
        

        /// <summary>
        /// 单个属性修改的后期处理(保存后)
        /// </summary>
        /// <param name="subsist">当前实体生存状态</param>
        /// <param name="modifieds">修改列表</param>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        protected override void OnLaterPeriodBySignleModified(EntitySubsist subsist,byte[] modifieds)
        {
            if (subsist == EntitySubsist.Deleting)
            {
                OnIdModified(subsist,false);
                OnTraceIdModified(subsist,false);
                OnStartModified(subsist,false);
                OnEndModified(subsist,false);
                OnLocalIdModified(subsist,false);
                OnLocalAppModified(subsist,false);
                OnLocalMachineModified(subsist,false);
                OnCallIdModified(subsist,false);
                OnCallAppModified(subsist,false);
                OnCallMachineModified(subsist,false);
                OnContextModified(subsist,false);
                OnTokenModified(subsist,false);
                OnHeadersModified(subsist,false);
                OnMessageModified(subsist,false);
                OnFlowStepModified(subsist,false);
                OnLevelModified(subsist,false);
                return;
            }
            else if (subsist == EntitySubsist.Adding || subsist == EntitySubsist.Added)
            {
                OnIdModified(subsist,true);
                OnTraceIdModified(subsist,true);
                OnStartModified(subsist,true);
                OnEndModified(subsist,true);
                OnLocalIdModified(subsist,true);
                OnLocalAppModified(subsist,true);
                OnLocalMachineModified(subsist,true);
                OnCallIdModified(subsist,true);
                OnCallAppModified(subsist,true);
                OnCallMachineModified(subsist,true);
                OnContextModified(subsist,true);
                OnTokenModified(subsist,true);
                OnHeadersModified(subsist,true);
                OnMessageModified(subsist,true);
                OnFlowStepModified(subsist,true);
                OnLevelModified(subsist,true);
                return;
            }
            else if(modifieds != null && modifieds[16] > 0)
            {
                OnIdModified(subsist,modifieds[_DataStruct_.Real_Id] == 1);
                OnTraceIdModified(subsist,modifieds[_DataStruct_.Real_TraceId] == 1);
                OnStartModified(subsist,modifieds[_DataStruct_.Real_Start] == 1);
                OnEndModified(subsist,modifieds[_DataStruct_.Real_End] == 1);
                OnLocalIdModified(subsist,modifieds[_DataStruct_.Real_LocalId] == 1);
                OnLocalAppModified(subsist,modifieds[_DataStruct_.Real_LocalApp] == 1);
                OnLocalMachineModified(subsist,modifieds[_DataStruct_.Real_LocalMachine] == 1);
                OnCallIdModified(subsist,modifieds[_DataStruct_.Real_CallId] == 1);
                OnCallAppModified(subsist,modifieds[_DataStruct_.Real_CallApp] == 1);
                OnCallMachineModified(subsist,modifieds[_DataStruct_.Real_CallMachine] == 1);
                OnContextModified(subsist,modifieds[_DataStruct_.Real_Context] == 1);
                OnTokenModified(subsist,modifieds[_DataStruct_.Real_Token] == 1);
                OnHeadersModified(subsist,modifieds[_DataStruct_.Real_Headers] == 1);
                OnMessageModified(subsist,modifieds[_DataStruct_.Real_Message] == 1);
                OnFlowStepModified(subsist,modifieds[_DataStruct_.Real_FlowStep] == 1);
                OnLevelModified(subsist,modifieds[_DataStruct_.Real_Level] == 1);
            }
        }

        /// <summary>
        /// 主键修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 全局请求标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnTraceIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 开始时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnStartModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 结束时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnEndModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 本地的全局标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnLocalIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 本地的应用修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnLocalAppModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 本地的机器修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnLocalMachineModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 请求方跟踪标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnCallIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 请求应用修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnCallAppModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 请求机器修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnCallMachineModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 上下文信息修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnContextModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 身份令牌修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnTokenModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 请求头信息修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnHeadersModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 消息序列化文本修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnMessageModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 流程步骤记录修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnFlowStepModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 调用层级修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnLevelModified(EntitySubsist subsist,bool isModified);
        #endregion

        #region 数据结构

        /// <summary>
        /// 实体结构
        /// </summary>
        [IgnoreDataMember,Browsable (false)]
        public override EntitySturct __Struct
        {
            get
            {
                return _DataStruct_.Struct;
            }
        }
        /// <summary>
        /// 实体结构
        /// </summary>
        public class _DataStruct_
        {
            /// <summary>
            /// 实体名称
            /// </summary>
            public const string EntityName = @"Message";
            /// <summary>
            /// 实体标题
            /// </summary>
            public const string EntityCaption = @"消息存储";
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityDescription = @"消息存储";
            /// <summary>
            /// 实体标识
            /// </summary>
            public const int EntityIdentity = 0x0;
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityPrimaryKey = "Id";
            
            
            /// <summary>
            /// 主键的数字标识
            /// </summary>
            public const int Id = 1;
            
            /// <summary>
            /// 主键的实时记录顺序
            /// </summary>
            public const int Real_Id = 0;

            /// <summary>
            /// 全局请求标识的数字标识
            /// </summary>
            public const int TraceId = 2;
            
            /// <summary>
            /// 全局请求标识的实时记录顺序
            /// </summary>
            public const int Real_TraceId = 1;

            /// <summary>
            /// 开始时间的数字标识
            /// </summary>
            public const int Start = 3;
            
            /// <summary>
            /// 开始时间的实时记录顺序
            /// </summary>
            public const int Real_Start = 2;

            /// <summary>
            /// 结束时间的数字标识
            /// </summary>
            public const int End = 4;
            
            /// <summary>
            /// 结束时间的实时记录顺序
            /// </summary>
            public const int Real_End = 3;

            /// <summary>
            /// 本地的全局标识的数字标识
            /// </summary>
            public const int LocalId = 5;
            
            /// <summary>
            /// 本地的全局标识的实时记录顺序
            /// </summary>
            public const int Real_LocalId = 4;

            /// <summary>
            /// 本地的应用的数字标识
            /// </summary>
            public const int LocalApp = 6;
            
            /// <summary>
            /// 本地的应用的实时记录顺序
            /// </summary>
            public const int Real_LocalApp = 5;

            /// <summary>
            /// 本地的机器的数字标识
            /// </summary>
            public const int LocalMachine = 7;
            
            /// <summary>
            /// 本地的机器的实时记录顺序
            /// </summary>
            public const int Real_LocalMachine = 6;

            /// <summary>
            /// 请求方跟踪标识的数字标识
            /// </summary>
            public const int CallId = 8;
            
            /// <summary>
            /// 请求方跟踪标识的实时记录顺序
            /// </summary>
            public const int Real_CallId = 7;

            /// <summary>
            /// 请求应用的数字标识
            /// </summary>
            public const int CallApp = 9;
            
            /// <summary>
            /// 请求应用的实时记录顺序
            /// </summary>
            public const int Real_CallApp = 8;

            /// <summary>
            /// 请求机器的数字标识
            /// </summary>
            public const int CallMachine = 10;
            
            /// <summary>
            /// 请求机器的实时记录顺序
            /// </summary>
            public const int Real_CallMachine = 9;

            /// <summary>
            /// 上下文信息的数字标识
            /// </summary>
            public const int Context = 11;
            
            /// <summary>
            /// 上下文信息的实时记录顺序
            /// </summary>
            public const int Real_Context = 10;

            /// <summary>
            /// 身份令牌的数字标识
            /// </summary>
            public const int Token = 12;
            
            /// <summary>
            /// 身份令牌的实时记录顺序
            /// </summary>
            public const int Real_Token = 11;

            /// <summary>
            /// 请求头信息的数字标识
            /// </summary>
            public const int Headers = 13;
            
            /// <summary>
            /// 请求头信息的实时记录顺序
            /// </summary>
            public const int Real_Headers = 12;

            /// <summary>
            /// 消息序列化文本的数字标识
            /// </summary>
            public const int Message = 14;
            
            /// <summary>
            /// 消息序列化文本的实时记录顺序
            /// </summary>
            public const int Real_Message = 13;

            /// <summary>
            /// 流程步骤记录的数字标识
            /// </summary>
            public const int FlowStep = 15;
            
            /// <summary>
            /// 流程步骤记录的实时记录顺序
            /// </summary>
            public const int Real_FlowStep = 14;

            /// <summary>
            /// 调用层级的数字标识
            /// </summary>
            public const int Level = 16;
            
            /// <summary>
            /// 调用层级的实时记录顺序
            /// </summary>
            public const int Real_Level = 15;

            /// <summary>
            /// 实体结构
            /// </summary>
            public static readonly EntitySturct Struct = new EntitySturct
            {
                EntityName = EntityName,
                Caption    = EntityCaption,
                Description= EntityDescription,
                PrimaryKey = EntityPrimaryKey,
                EntityType = EntityIdentity,
                Properties = new Dictionary<int, PropertySturct>
                {
                    {
                        Real_Id,
                        new PropertySturct
                        {
                            Index        = Id,
                            Name         = "Id",
                            Caption      = @"主键",
                            JsonName     = "Id",
                            ColumnName   = "id",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"主键"
                        }
                    },
                    {
                        Real_TraceId,
                        new PropertySturct
                        {
                            Index        = TraceId,
                            Name         = "TraceId",
                            Caption      = @"全局请求标识",
                            JsonName     = "TraceId",
                            ColumnName   = "trace_id",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"全局请求标识"
                        }
                    },
                    {
                        Real_Start,
                        new PropertySturct
                        {
                            Index        = Start,
                            Name         = "Start",
                            Caption      = @"开始时间",
                            JsonName     = "Start",
                            ColumnName   = "start",
                            PropertyType = typeof(DateTime),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 12,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"开始时间"
                        }
                    },
                    {
                        Real_End,
                        new PropertySturct
                        {
                            Index        = End,
                            Name         = "End",
                            Caption      = @"结束时间",
                            JsonName     = "End",
                            ColumnName   = "end",
                            PropertyType = typeof(DateTime),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 12,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"结束时间"
                        }
                    },
                    {
                        Real_LocalId,
                        new PropertySturct
                        {
                            Index        = LocalId,
                            Name         = "LocalId",
                            Caption      = @"本地的全局标识",
                            JsonName     = "LocalId",
                            ColumnName   = "local_id",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"本地的全局标识"
                        }
                    },
                    {
                        Real_LocalApp,
                        new PropertySturct
                        {
                            Index        = LocalApp,
                            Name         = "LocalApp",
                            Caption      = @"本地的应用",
                            JsonName     = "LocalApp",
                            ColumnName   = "local_app",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"本地的应用"
                        }
                    },
                    {
                        Real_LocalMachine,
                        new PropertySturct
                        {
                            Index        = LocalMachine,
                            Name         = "LocalMachine",
                            Caption      = @"本地的机器",
                            JsonName     = "LocalMachine",
                            ColumnName   = "local_machine",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"本地的机器"
                        }
                    },
                    {
                        Real_CallId,
                        new PropertySturct
                        {
                            Index        = CallId,
                            Name         = "CallId",
                            Caption      = @"请求方跟踪标识",
                            JsonName     = "CallId",
                            ColumnName   = "call_id",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"请求方跟踪标识"
                        }
                    },
                    {
                        Real_CallApp,
                        new PropertySturct
                        {
                            Index        = CallApp,
                            Name         = "CallApp",
                            Caption      = @"请求应用",
                            JsonName     = "CallApp",
                            ColumnName   = "call_app",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"请求应用"
                        }
                    },
                    {
                        Real_CallMachine,
                        new PropertySturct
                        {
                            Index        = CallMachine,
                            Name         = "CallMachine",
                            Caption      = @"请求机器",
                            JsonName     = "CallMachine",
                            ColumnName   = "call_machine",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"请求机器"
                        }
                    },
                    {
                        Real_Context,
                        new PropertySturct
                        {
                            Index        = Context,
                            Name         = "Context",
                            Caption      = @"上下文信息",
                            JsonName     = "Context",
                            ColumnName   = "context",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"上下文信息"
                        }
                    },
                    {
                        Real_Token,
                        new PropertySturct
                        {
                            Index        = Token,
                            Name         = "Token",
                            Caption      = @"身份令牌",
                            JsonName     = "Token",
                            ColumnName   = "token",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"身份令牌"
                        }
                    },
                    {
                        Real_Headers,
                        new PropertySturct
                        {
                            Index        = Headers,
                            Name         = "Headers",
                            Caption      = @"请求头信息",
                            JsonName     = "Headers",
                            ColumnName   = "headers",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"请求头信息"
                        }
                    },
                    {
                        Real_Message,
                        new PropertySturct
                        {
                            Index        = Message,
                            Name         = "Message",
                            Caption      = @"消息序列化文本",
                            JsonName     = "Message",
                            ColumnName   = "message",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"消息序列化文本"
                        }
                    },
                    {
                        Real_FlowStep,
                        new PropertySturct
                        {
                            Index        = FlowStep,
                            Name         = "FlowStep",
                            Caption      = @"流程步骤记录",
                            JsonName     = "FlowStep",
                            ColumnName   = "flow_step",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"流程步骤记录"
                        }
                    },
                    {
                        Real_Level,
                        new PropertySturct
                        {
                            Index        = Level,
                            Name         = "Level",
                            Caption      = @"调用层级",
                            JsonName     = "Level",
                            ColumnName   = "level",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"调用层级"
                        }
                    }
                }
            };
        }
        #endregion

    }
}