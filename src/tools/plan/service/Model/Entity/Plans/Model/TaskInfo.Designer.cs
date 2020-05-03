/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/26 22:05:24*/
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

using ZeroTeam.MessageMVC.Messages;
#endregion

namespace ZeroTeam.MessageMVC.PlanTasks
{
    /// <summary>
    /// 任务信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class TaskInfoData : IIdentityData
    {
        #region 构造
        
        /// <summary>
        /// 构造
        /// </summary>
        public TaskInfoData()
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
        /// 消息标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _planId;

        partial void OnPlanIdGet();

        partial void OnPlanIdSet(ref string value);

        partial void OnPlanIdSeted();

        
        /// <summary>
        ///  消息标识
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"消息标识")]
        public  string PlanId
        {
            get
            {
                OnPlanIdGet();
                return this._planId;
            }
            set
            {
                if(this._planId == value)
                    return;
                OnPlanIdSet(ref value);
                this._planId = value;
                OnPlanIdSeted();
                this.OnPropertyChanged(_DataStruct_.Real_PlanId);
                this.OnPropertyChanged(nameof(PlanId));
            }
        }
        /// <summary>
        /// 计划说明
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _description;

        partial void OnDescriptionGet();

        partial void OnDescriptionSet(ref string value);

        partial void OnDescriptionSeted();

        
        /// <summary>
        ///  计划说明
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"计划说明")]
        public  string Description
        {
            get
            {
                OnDescriptionGet();
                return this._description;
            }
            set
            {
                if(this._description == value)
                    return;
                OnDescriptionSet(ref value);
                this._description = value;
                OnDescriptionSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Description);
                this.OnPropertyChanged(nameof(Description));
            }
        }
        /// <summary>
        /// 计划类型
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public PlanTimeType _planType;

        partial void OnPlanTypeGet();

        partial void OnPlanTypeSet(ref PlanTimeType value);

        partial void OnPlanTypeSeted();

        
        /// <summary>
        ///  计划类型
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanType", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"计划类型")]
        public  PlanTimeType PlanType
        {
            get
            {
                OnPlanTypeGet();
                return this._planType;
            }
            set
            {
                if(this._planType == value)
                    return;
                OnPlanTypeSet(ref value);
                this._planType = value;
                OnPlanTypeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_PlanType);
                this.OnPropertyChanged(nameof(PlanType));
            }
        }
        /// <summary>
        /// 计划类型的可读内容
        /// </summary>
        [IgnoreDataMember,JsonIgnore,DisplayName("计划类型")]
        public string PlanType_Content => PlanType.ToCaption();

        /// <summary>
        /// 计划类型的数字属性
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public  int PlanType_Number
        {
            get => (int)this.PlanType;
            set => this.PlanType = (PlanTimeType)value;
        }
        /// <summary>
        /// 计划值
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _planValue;

        partial void OnPlanValueGet();

        partial void OnPlanValueSet(ref int value);

        partial void OnPlanValueSeted();

        
        /// <summary>
        ///  计划值
        /// </summary>
        /// <remarks>
        ///     none time 无效  second minute hour day : 延时处理的 指定延时数量(单位为对应的plan_date_type)  week : 周日到周六(0-6),值无效系统自动放弃(无提示) month: 正数为指定号数(如当月不存在,则使用当月最后一天) 零或负数为月未倒推(0为最后一天,负数为减去的数字,减的结果为0或负数的,则为当前第一天)
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanValue", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"计划值")]
        public  int PlanValue
        {
            get
            {
                OnPlanValueGet();
                return this._planValue;
            }
            set
            {
                if(this._planValue == value)
                    return;
                OnPlanValueSet(ref value);
                this._planValue = value;
                OnPlanValueSeted();
                this.OnPropertyChanged(_DataStruct_.Real_PlanValue);
                this.OnPropertyChanged(nameof(PlanValue));
            }
        }
        /// <summary>
        /// 重试次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _retrySet;

        partial void OnRetrySetGet();

        partial void OnRetrySetSet(ref int value);

        partial void OnRetrySetSeted();

        
        /// <summary>
        ///  重试次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("RetrySet", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"重试次数")]
        public  int RetrySet
        {
            get
            {
                OnRetrySetGet();
                return this._retrySet;
            }
            set
            {
                if(this._retrySet == value)
                    return;
                OnRetrySetSet(ref value);
                this._retrySet = value;
                OnRetrySetSeted();
                this.OnPropertyChanged(_DataStruct_.Real_RetrySet);
                this.OnPropertyChanged(nameof(RetrySet));
            }
        }
        /// <summary>
        /// 跳过设置次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _skipSet;

        partial void OnSkipSetGet();

        partial void OnSkipSetSet(ref int value);

        partial void OnSkipSetSeted();

        
        /// <summary>
        ///  跳过设置次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("SkipSet", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"跳过设置次数")]
        public  int SkipSet
        {
            get
            {
                OnSkipSetGet();
                return this._skipSet;
            }
            set
            {
                if(this._skipSet == value)
                    return;
                OnSkipSetSet(ref value);
                this._skipSet = value;
                OnSkipSetSeted();
                this.OnPropertyChanged(_DataStruct_.Real_SkipSet);
                this.OnPropertyChanged(nameof(SkipSet));
            }
        }
        /// <summary>
        /// 重复次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _planRepet;

        partial void OnPlanRepetGet();

        partial void OnPlanRepetSet(ref int value);

        partial void OnPlanRepetSeted();

        
        /// <summary>
        ///  重复次数
        /// </summary>
        /// <remarks>
        ///     0不重复 &gt;0重复次数,-1永久重复
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanRepet", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"重复次数")]
        public  int PlanRepet
        {
            get
            {
                OnPlanRepetGet();
                return this._planRepet;
            }
            set
            {
                if(this._planRepet == value)
                    return;
                OnPlanRepetSet(ref value);
                this._planRepet = value;
                OnPlanRepetSeted();
                this.OnPropertyChanged(_DataStruct_.Real_PlanRepet);
                this.OnPropertyChanged(nameof(PlanRepet));
            }
        }
        /// <summary>
        /// 跳过无效时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public bool _queuePassBy;

        partial void OnQueuePassByGet();

        partial void OnQueuePassBySet(ref bool value);

        partial void OnQueuePassBySeted();

        
        /// <summary>
        ///  跳过无效时间
        /// </summary>
        /// <remarks>
        ///     如果为真，执行时并不检查时间是否已过去
        /// </remarks>
        /// <example>
        ///     true
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("QueuePassBy", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"跳过无效时间")]
        public  bool QueuePassBy
        {
            get
            {
                OnQueuePassByGet();
                return this._queuePassBy;
            }
            set
            {
                if(this._queuePassBy == value)
                    return;
                OnQueuePassBySet(ref value);
                this._queuePassBy = value;
                OnQueuePassBySeted();
                this.OnPropertyChanged(_DataStruct_.Real_QueuePassBy);
                this.OnPropertyChanged(nameof(QueuePassBy));
            }
        }
        /// <summary>
        /// 加入时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _addTime;

        partial void OnAddTimeGet();

        partial void OnAddTimeSet(ref long value);

        partial void OnAddTimeSeted();

        
        /// <summary>
        ///  加入时间
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("AddTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"加入时间")]
        public  long AddTime
        {
            get
            {
                OnAddTimeGet();
                return this._addTime;
            }
            set
            {
                if(this._addTime == value)
                    return;
                OnAddTimeSet(ref value);
                this._addTime = value;
                OnAddTimeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_AddTime);
                this.OnPropertyChanged(nameof(AddTime));
            }
        }
        /// <summary>
        /// 计划时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _planTime;

        partial void OnPlanTimeGet();

        partial void OnPlanTimeSet(ref long value);

        partial void OnPlanTimeSeted();

        
        /// <summary>
        ///  计划时间
        /// </summary>
        /// <remarks>
        ///     使用UNIX时间(1970年1月1日0时0分0秒起的总毫秒数)
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"计划时间")]
        public  long PlanTime
        {
            get
            {
                OnPlanTimeGet();
                return this._planTime;
            }
            set
            {
                if(this._planTime == value)
                    return;
                OnPlanTimeSet(ref value);
                this._planTime = value;
                OnPlanTimeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_PlanTime);
                this.OnPropertyChanged(nameof(PlanTime));
            }
        }
        /// <summary>
        /// 异步调用
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public bool _isAsync;

        partial void OnIsAsyncGet();

        partial void OnIsAsyncSet(ref bool value);

        partial void OnIsAsyncSeted();

        
        /// <summary>
        ///  异步调用
        /// </summary>
        /// <example>
        ///     true
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("IsAsync", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"异步调用")]
        public  bool IsAsync
        {
            get
            {
                OnIsAsyncGet();
                return this._isAsync;
            }
            set
            {
                if(this._isAsync == value)
                    return;
                OnIsAsyncSet(ref value);
                this._isAsync = value;
                OnIsAsyncSeted();
                this.OnPropertyChanged(_DataStruct_.Real_IsAsync);
                this.OnPropertyChanged(nameof(IsAsync));
            }
        }
        /// <summary>
        /// 异步结果检查时长
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _checkResultTime;

        partial void OnCheckResultTimeGet();

        partial void OnCheckResultTimeSet(ref int value);

        partial void OnCheckResultTimeSeted();

        
        /// <summary>
        ///  异步结果检查时长
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("CheckResultTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"异步结果检查时长")]
        public  int CheckResultTime
        {
            get
            {
                OnCheckResultTimeGet();
                return this._checkResultTime;
            }
            set
            {
                if(this._checkResultTime == value)
                    return;
                OnCheckResultTimeSet(ref value);
                this._checkResultTime = value;
                OnCheckResultTimeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_CheckResultTime);
                this.OnPropertyChanged(nameof(CheckResultTime));
            }
        }
        /// <summary>
        /// 消息内容
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _message;

        partial void OnMessageGet();

        partial void OnMessageSet(ref string value);

        partial void OnMessageSeted();

        
        /// <summary>
        ///  消息内容
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"消息内容")]
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
        /// 任务状态
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public PlanMessageState _state;

        partial void OnStateGet();

        partial void OnStateSet(ref PlanMessageState value);

        partial void OnStateSeted();

        
        /// <summary>
        ///  任务状态
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("State", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"任务状态")]
        public  PlanMessageState State
        {
            get
            {
                OnStateGet();
                return this._state;
            }
            set
            {
                if(this._state == value)
                    return;
                OnStateSet(ref value);
                this._state = value;
                OnStateSeted();
                this.OnPropertyChanged(_DataStruct_.Real_State);
                this.OnPropertyChanged(nameof(State));
            }
        }
        /// <summary>
        /// 任务状态的可读内容
        /// </summary>
        [IgnoreDataMember,JsonIgnore,DisplayName("任务状态")]
        public string State_Content => State.ToCaption();

        /// <summary>
        /// 任务状态的数字属性
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public  int State_Number
        {
            get => (int)this.State;
            set => this.State = (PlanMessageState)value;
        }
        /// <summary>
        /// 关闭时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public DateTime _closeTime;

        partial void OnCloseTimeGet();

        partial void OnCloseTimeSet(ref DateTime value);

        partial void OnCloseTimeSeted();

        
        /// <summary>
        ///  关闭时间
        /// </summary>
        /// <example>
        ///     2012-12-21
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("CloseTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , JsonConverter(typeof(MyDateTimeConverter)) , DisplayName(@"关闭时间")]
        public  DateTime CloseTime
        {
            get
            {
                OnCloseTimeGet();
                return this._closeTime;
            }
            set
            {
                if(this._closeTime == value)
                    return;
                OnCloseTimeSet(ref value);
                this._closeTime = value;
                OnCloseTimeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_CloseTime);
                this.OnPropertyChanged(nameof(CloseTime));
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
            case "planid":
                this.PlanId = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "description":
                this.Description = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "plantype":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (PlanTimeType.TryParse(value, out PlanTimeType val))
                    {
                        this.PlanType = val;
                        return true;
                    }
                    else if (int.TryParse(value, out int vl))
                    {
                        this.PlanType = (PlanTimeType)vl;
                        return true;
                    }
                }
                return false;
            case "planvalue":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.PlanValue = vl;
                        return true;
                    }
                }
                return false;
            case "retryset":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.RetrySet = vl;
                        return true;
                    }
                }
                return false;
            case "skipset":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.SkipSet = vl;
                        return true;
                    }
                }
                return false;
            case "planrepet":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.PlanRepet = vl;
                        return true;
                    }
                }
                return false;
            case "queuepassby":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (bool.TryParse(value, out var vl))
                    {
                        this.QueuePassBy = vl;
                        return true;
                    }
                }
                return false;
            case "addtime":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.AddTime = vl;
                        return true;
                    }
                }
                return false;
            case "plantime":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.PlanTime = vl;
                        return true;
                    }
                }
                return false;
            case "isasync":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (bool.TryParse(value, out var vl))
                    {
                        this.IsAsync = vl;
                        return true;
                    }
                }
                return false;
            case "checkresulttime":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.CheckResultTime = vl;
                        return true;
                    }
                }
                return false;
            case "message":
                this.Message = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "state":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (PlanMessageState.TryParse(value, out PlanMessageState val))
                    {
                        this.State = val;
                        return true;
                    }
                    else if (int.TryParse(value, out int vl))
                    {
                        this.State = (PlanMessageState)vl;
                        return true;
                    }
                }
                return false;
            case "closetime":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (DateTime.TryParse(value, out var vl))
                    {
                        this.CloseTime = vl;
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
            case "planid":
                this.PlanId = value == null ? null : value.ToString();
                return;
            case "description":
                this.Description = value == null ? null : value.ToString();
                return;
            case "plantype":
                if (value != null)
                {
                    if(value is int)
                    {
                        this.PlanType = (PlanTimeType)(int)value;
                    }
                    else if(value is PlanTimeType)
                    {
                        this.PlanType = (PlanTimeType)value;
                    }
                    else
                    {
                        var str = value.ToString();
                        PlanTimeType val;
                        if (PlanTimeType.TryParse(str, out val))
                        {
                            this.PlanType = val;
                        }
                        else
                        {
                            int vl;
                            if (int.TryParse(str, out vl))
                            {
                                this.PlanType = (PlanTimeType)vl;
                            }
                        }
                    }
                }
                return;
            case "planvalue":
                this.PlanValue = (int)Convert.ToDecimal(value);
                return;
            case "retryset":
                this.RetrySet = (int)Convert.ToDecimal(value);
                return;
            case "skipset":
                this.SkipSet = (int)Convert.ToDecimal(value);
                return;
            case "planrepet":
                this.PlanRepet = (int)Convert.ToDecimal(value);
                return;
            case "queuepassby":
                if (value != null)
                {
                    int vl;
                    if (int.TryParse(value.ToString(), out vl))
                    {
                        this.QueuePassBy = vl != 0;
                    }
                    else
                    {
                        this.QueuePassBy = Convert.ToBoolean(value);
                    }
                }
                return;
            case "addtime":
                this.AddTime = (long)Convert.ToDecimal(value);
                return;
            case "plantime":
                this.PlanTime = (long)Convert.ToDecimal(value);
                return;
            case "isasync":
                if (value != null)
                {
                    int vl;
                    if (int.TryParse(value.ToString(), out vl))
                    {
                        this.IsAsync = vl != 0;
                    }
                    else
                    {
                        this.IsAsync = Convert.ToBoolean(value);
                    }
                }
                return;
            case "checkresulttime":
                this.CheckResultTime = (int)Convert.ToDecimal(value);
                return;
            case "message":
                this.Message = value == null ? null : value.ToString();
                return;
            case "state":
                if (value != null)
                {
                    if(value is int)
                    {
                        this.State = (PlanMessageState)(int)value;
                    }
                    else if(value is PlanMessageState)
                    {
                        this.State = (PlanMessageState)value;
                    }
                    else
                    {
                        var str = value.ToString();
                        PlanMessageState val;
                        if (PlanMessageState.TryParse(str, out val))
                        {
                            this.State = val;
                        }
                        else
                        {
                            int vl;
                            if (int.TryParse(str, out vl))
                            {
                                this.State = (PlanMessageState)vl;
                            }
                        }
                    }
                }
                return;
            case "closetime":
                this.CloseTime = Convert.ToDateTime(value);
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
            case _DataStruct_.PlanId:
                this.PlanId = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Description:
                this.Description = value == null ? null : value.ToString();
                return;
            case _DataStruct_.PlanType:
                this.PlanType = (PlanTimeType)value;
                return;
            case _DataStruct_.PlanValue:
                this.PlanValue = Convert.ToInt32(value);
                return;
            case _DataStruct_.RetrySet:
                this.RetrySet = Convert.ToInt32(value);
                return;
            case _DataStruct_.SkipSet:
                this.SkipSet = Convert.ToInt32(value);
                return;
            case _DataStruct_.PlanRepet:
                this.PlanRepet = Convert.ToInt32(value);
                return;
            case _DataStruct_.QueuePassBy:
                this.QueuePassBy = Convert.ToBoolean(value);
                return;
            case _DataStruct_.AddTime:
                this.AddTime = Convert.ToInt64(value);
                return;
            case _DataStruct_.PlanTime:
                this.PlanTime = Convert.ToInt64(value);
                return;
            case _DataStruct_.IsAsync:
                this.IsAsync = Convert.ToBoolean(value);
                return;
            case _DataStruct_.CheckResultTime:
                this.CheckResultTime = Convert.ToInt32(value);
                return;
            case _DataStruct_.Message:
                this.Message = value == null ? null : value.ToString();
                return;
            case _DataStruct_.State:
                this.State = (PlanMessageState)value;
                return;
            case _DataStruct_.CloseTime:
                this.CloseTime = Convert.ToDateTime(value);
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
            case "planid":
                return this.PlanId;
            case "description":
                return this.Description;
            case "plantype":
                return this.PlanType.ToCaption();
            case "planvalue":
                return this.PlanValue;
            case "retryset":
                return this.RetrySet;
            case "skipset":
                return this.SkipSet;
            case "planrepet":
                return this.PlanRepet;
            case "queuepassby":
                return this.QueuePassBy;
            case "addtime":
                return this.AddTime;
            case "plantime":
                return this.PlanTime;
            case "isasync":
                return this.IsAsync;
            case "checkresulttime":
                return this.CheckResultTime;
            case "message":
                return this.Message;
            case "state":
                return this.State.ToCaption();
            case "closetime":
                return this.CloseTime;
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
                case _DataStruct_.PlanId:
                    return this.PlanId;
                case _DataStruct_.Description:
                    return this.Description;
                case _DataStruct_.PlanType:
                    return this.PlanType;
                case _DataStruct_.PlanValue:
                    return this.PlanValue;
                case _DataStruct_.RetrySet:
                    return this.RetrySet;
                case _DataStruct_.SkipSet:
                    return this.SkipSet;
                case _DataStruct_.PlanRepet:
                    return this.PlanRepet;
                case _DataStruct_.QueuePassBy:
                    return this.QueuePassBy;
                case _DataStruct_.AddTime:
                    return this.AddTime;
                case _DataStruct_.PlanTime:
                    return this.PlanTime;
                case _DataStruct_.IsAsync:
                    return this.IsAsync;
                case _DataStruct_.CheckResultTime:
                    return this.CheckResultTime;
                case _DataStruct_.Message:
                    return this.Message;
                case _DataStruct_.State:
                    return this.State;
                case _DataStruct_.CloseTime:
                    return this.CloseTime;
            }

            return null;
        }

        #endregion

        #region 复制
        

        partial void CopyExtendValue(TaskInfoData source);

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected override void CopyValueInner(DataObjectBase source)
        {
            var sourceEntity = source as TaskInfoData;
            if(sourceEntity == null)
                return;
            this._id = sourceEntity._id;
            this._planId = sourceEntity._planId;
            this._description = sourceEntity._description;
            this._planType = sourceEntity._planType;
            this._planValue = sourceEntity._planValue;
            this._retrySet = sourceEntity._retrySet;
            this._skipSet = sourceEntity._skipSet;
            this._planRepet = sourceEntity._planRepet;
            this._queuePassBy = sourceEntity._queuePassBy;
            this._addTime = sourceEntity._addTime;
            this._planTime = sourceEntity._planTime;
            this._isAsync = sourceEntity._isAsync;
            this._checkResultTime = sourceEntity._checkResultTime;
            this._message = sourceEntity._message;
            this._state = sourceEntity._state;
            this._closeTime = sourceEntity._closeTime;
            CopyExtendValue(sourceEntity);
            this.__status.IsModified = true;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void Copy(TaskInfoData source)
        {
                this.Id = source.Id;
                this.PlanId = source.PlanId;
                this.Description = source.Description;
                this.PlanType = source.PlanType;
                this.PlanValue = source.PlanValue;
                this.RetrySet = source.RetrySet;
                this.SkipSet = source.SkipSet;
                this.PlanRepet = source.PlanRepet;
                this.QueuePassBy = source.QueuePassBy;
                this.AddTime = source.AddTime;
                this.PlanTime = source.PlanTime;
                this.IsAsync = source.IsAsync;
                this.CheckResultTime = source.CheckResultTime;
                this.Message = source.Message;
                this.State = source.State;
                this.CloseTime = source.CloseTime;
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
                OnPlanIdModified(subsist,false);
                OnDescriptionModified(subsist,false);
                OnPlanTypeModified(subsist,false);
                OnPlanValueModified(subsist,false);
                OnRetrySetModified(subsist,false);
                OnSkipSetModified(subsist,false);
                OnPlanRepetModified(subsist,false);
                OnQueuePassByModified(subsist,false);
                OnAddTimeModified(subsist,false);
                OnPlanTimeModified(subsist,false);
                OnIsAsyncModified(subsist,false);
                OnCheckResultTimeModified(subsist,false);
                OnMessageModified(subsist,false);
                OnStateModified(subsist,false);
                OnCloseTimeModified(subsist,false);
                return;
            }
            else if (subsist == EntitySubsist.Adding || subsist == EntitySubsist.Added)
            {
                OnIdModified(subsist,true);
                OnPlanIdModified(subsist,true);
                OnDescriptionModified(subsist,true);
                OnPlanTypeModified(subsist,true);
                OnPlanValueModified(subsist,true);
                OnRetrySetModified(subsist,true);
                OnSkipSetModified(subsist,true);
                OnPlanRepetModified(subsist,true);
                OnQueuePassByModified(subsist,true);
                OnAddTimeModified(subsist,true);
                OnPlanTimeModified(subsist,true);
                OnIsAsyncModified(subsist,true);
                OnCheckResultTimeModified(subsist,true);
                OnMessageModified(subsist,true);
                OnStateModified(subsist,true);
                OnCloseTimeModified(subsist,true);
                return;
            }
            else if(modifieds != null && modifieds[16] > 0)
            {
                OnIdModified(subsist,modifieds[_DataStruct_.Real_Id] == 1);
                OnPlanIdModified(subsist,modifieds[_DataStruct_.Real_PlanId] == 1);
                OnDescriptionModified(subsist,modifieds[_DataStruct_.Real_Description] == 1);
                OnPlanTypeModified(subsist,modifieds[_DataStruct_.Real_PlanType] == 1);
                OnPlanValueModified(subsist,modifieds[_DataStruct_.Real_PlanValue] == 1);
                OnRetrySetModified(subsist,modifieds[_DataStruct_.Real_RetrySet] == 1);
                OnSkipSetModified(subsist,modifieds[_DataStruct_.Real_SkipSet] == 1);
                OnPlanRepetModified(subsist,modifieds[_DataStruct_.Real_PlanRepet] == 1);
                OnQueuePassByModified(subsist,modifieds[_DataStruct_.Real_QueuePassBy] == 1);
                OnAddTimeModified(subsist,modifieds[_DataStruct_.Real_AddTime] == 1);
                OnPlanTimeModified(subsist,modifieds[_DataStruct_.Real_PlanTime] == 1);
                OnIsAsyncModified(subsist,modifieds[_DataStruct_.Real_IsAsync] == 1);
                OnCheckResultTimeModified(subsist,modifieds[_DataStruct_.Real_CheckResultTime] == 1);
                OnMessageModified(subsist,modifieds[_DataStruct_.Real_Message] == 1);
                OnStateModified(subsist,modifieds[_DataStruct_.Real_State] == 1);
                OnCloseTimeModified(subsist,modifieds[_DataStruct_.Real_CloseTime] == 1);
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
        /// 消息标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 计划说明修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnDescriptionModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 计划类型修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanTypeModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 计划值修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanValueModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 重试次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnRetrySetModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 跳过设置次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnSkipSetModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 重复次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanRepetModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 跳过无效时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnQueuePassByModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 加入时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnAddTimeModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 计划时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanTimeModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 异步调用修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnIsAsyncModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 异步结果检查时长修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnCheckResultTimeModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 消息内容修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnMessageModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 任务状态修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnStateModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 关闭时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnCloseTimeModified(EntitySubsist subsist,bool isModified);
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
            public const string EntityName = @"TaskInfo";
            /// <summary>
            /// 实体标题
            /// </summary>
            public const string EntityCaption = @"任务信息";
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityDescription = @"任务信息";
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
            public const int Id = 14;
            
            /// <summary>
            /// 主键的实时记录顺序
            /// </summary>
            public const int Real_Id = 0;

            /// <summary>
            /// 消息标识的数字标识
            /// </summary>
            public const int PlanId = 15;
            
            /// <summary>
            /// 消息标识的实时记录顺序
            /// </summary>
            public const int Real_PlanId = 1;

            /// <summary>
            /// 计划说明的数字标识
            /// </summary>
            public const int Description = 16;
            
            /// <summary>
            /// 计划说明的实时记录顺序
            /// </summary>
            public const int Real_Description = 2;

            /// <summary>
            /// 计划类型的数字标识
            /// </summary>
            public const int PlanType = 17;
            
            /// <summary>
            /// 计划类型的实时记录顺序
            /// </summary>
            public const int Real_PlanType = 3;

            /// <summary>
            /// 计划值的数字标识
            /// </summary>
            public const int PlanValue = 18;
            
            /// <summary>
            /// 计划值的实时记录顺序
            /// </summary>
            public const int Real_PlanValue = 4;

            /// <summary>
            /// 重试次数的数字标识
            /// </summary>
            public const int RetrySet = 19;
            
            /// <summary>
            /// 重试次数的实时记录顺序
            /// </summary>
            public const int Real_RetrySet = 5;

            /// <summary>
            /// 跳过设置次数的数字标识
            /// </summary>
            public const int SkipSet = 20;
            
            /// <summary>
            /// 跳过设置次数的实时记录顺序
            /// </summary>
            public const int Real_SkipSet = 6;

            /// <summary>
            /// 重复次数的数字标识
            /// </summary>
            public const int PlanRepet = 21;
            
            /// <summary>
            /// 重复次数的实时记录顺序
            /// </summary>
            public const int Real_PlanRepet = 7;

            /// <summary>
            /// 跳过无效时间的数字标识
            /// </summary>
            public const int QueuePassBy = 22;
            
            /// <summary>
            /// 跳过无效时间的实时记录顺序
            /// </summary>
            public const int Real_QueuePassBy = 8;

            /// <summary>
            /// 加入时间的数字标识
            /// </summary>
            public const int AddTime = 23;
            
            /// <summary>
            /// 加入时间的实时记录顺序
            /// </summary>
            public const int Real_AddTime = 9;

            /// <summary>
            /// 计划时间的数字标识
            /// </summary>
            public const int PlanTime = 24;
            
            /// <summary>
            /// 计划时间的实时记录顺序
            /// </summary>
            public const int Real_PlanTime = 10;

            /// <summary>
            /// 异步调用的数字标识
            /// </summary>
            public const int IsAsync = 25;
            
            /// <summary>
            /// 异步调用的实时记录顺序
            /// </summary>
            public const int Real_IsAsync = 11;

            /// <summary>
            /// 异步结果检查时长的数字标识
            /// </summary>
            public const int CheckResultTime = 26;
            
            /// <summary>
            /// 异步结果检查时长的实时记录顺序
            /// </summary>
            public const int Real_CheckResultTime = 12;

            /// <summary>
            /// 消息内容的数字标识
            /// </summary>
            public const int Message = 27;
            
            /// <summary>
            /// 消息内容的实时记录顺序
            /// </summary>
            public const int Real_Message = 13;

            /// <summary>
            /// 任务状态的数字标识
            /// </summary>
            public const int State = 28;
            
            /// <summary>
            /// 任务状态的实时记录顺序
            /// </summary>
            public const int Real_State = 14;

            /// <summary>
            /// 关闭时间的数字标识
            /// </summary>
            public const int CloseTime = 29;
            
            /// <summary>
            /// 关闭时间的实时记录顺序
            /// </summary>
            public const int Real_CloseTime = 15;

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
                        Real_PlanId,
                        new PropertySturct
                        {
                            Index        = PlanId,
                            Name         = "PlanId",
                            Caption      = @"消息标识",
                            JsonName     = "PlanId",
                            ColumnName   = "plan_id",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"消息标识"
                        }
                    },
                    {
                        Real_Description,
                        new PropertySturct
                        {
                            Index        = Description,
                            Name         = "Description",
                            Caption      = @"计划说明",
                            JsonName     = "Description",
                            ColumnName   = "description",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"计划说明"
                        }
                    },
                    {
                        Real_PlanType,
                        new PropertySturct
                        {
                            Index        = PlanType,
                            Name         = "PlanType",
                            Caption      = @"计划类型",
                            JsonName     = "PlanType",
                            ColumnName   = "plan_type",
                            PropertyType = typeof(PlanTimeType),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"计划类型"
                        }
                    },
                    {
                        Real_PlanValue,
                        new PropertySturct
                        {
                            Index        = PlanValue,
                            Name         = "PlanValue",
                            Caption      = @"计划值",
                            JsonName     = "PlanValue",
                            ColumnName   = "plan_value",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"none time 无效  second minute hour day : 延时处理的 指定延时数量(单位为对应的plan_date_type)  week : 周日到周六(0-6),值无效系统自动放弃(无提示) month: 正数为指定号数(如当月不存在,则使用当月最后一天) 零或负数为月未倒推(0为最后一天,负数为减去的数字,减的结果为0或负数的,则为当前第一天)"
                        }
                    },
                    {
                        Real_RetrySet,
                        new PropertySturct
                        {
                            Index        = RetrySet,
                            Name         = "RetrySet",
                            Caption      = @"重试次数",
                            JsonName     = "RetrySet",
                            ColumnName   = "retry_set",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"重试次数"
                        }
                    },
                    {
                        Real_SkipSet,
                        new PropertySturct
                        {
                            Index        = SkipSet,
                            Name         = "SkipSet",
                            Caption      = @"跳过设置次数",
                            JsonName     = "SkipSet",
                            ColumnName   = "skip_set",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"跳过设置次数"
                        }
                    },
                    {
                        Real_PlanRepet,
                        new PropertySturct
                        {
                            Index        = PlanRepet,
                            Name         = "PlanRepet",
                            Caption      = @"重复次数",
                            JsonName     = "PlanRepet",
                            ColumnName   = "plan_repet",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"0不重复 >0重复次数,-1永久重复"
                        }
                    },
                    {
                        Real_QueuePassBy,
                        new PropertySturct
                        {
                            Index        = QueuePassBy,
                            Name         = "QueuePassBy",
                            Caption      = @"跳过无效时间",
                            JsonName     = "QueuePassBy",
                            ColumnName   = "queue_pass_by",
                            PropertyType = typeof(bool),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 1,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"如果为真，执行时并不检查时间是否已过去"
                        }
                    },
                    {
                        Real_AddTime,
                        new PropertySturct
                        {
                            Index        = AddTime,
                            Name         = "AddTime",
                            Caption      = @"加入时间",
                            JsonName     = "AddTime",
                            ColumnName   = "add_time",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"加入时间"
                        }
                    },
                    {
                        Real_PlanTime,
                        new PropertySturct
                        {
                            Index        = PlanTime,
                            Name         = "PlanTime",
                            Caption      = @"计划时间",
                            JsonName     = "PlanTime",
                            ColumnName   = "plan_time",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"使用UNIX时间(1970年1月1日0时0分0秒起的总毫秒数)"
                        }
                    },
                    {
                        Real_IsAsync,
                        new PropertySturct
                        {
                            Index        = IsAsync,
                            Name         = "IsAsync",
                            Caption      = @"异步调用",
                            JsonName     = "IsAsync",
                            ColumnName   = "is_async",
                            PropertyType = typeof(bool),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 1,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"异步调用"
                        }
                    },
                    {
                        Real_CheckResultTime,
                        new PropertySturct
                        {
                            Index        = CheckResultTime,
                            Name         = "CheckResultTime",
                            Caption      = @"异步结果检查时长",
                            JsonName     = "CheckResultTime",
                            ColumnName   = "check_result_time",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"异步结果检查时长"
                        }
                    },
                    {
                        Real_Message,
                        new PropertySturct
                        {
                            Index        = Message,
                            Name         = "Message",
                            Caption      = @"消息内容",
                            JsonName     = "Message",
                            ColumnName   = "message",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"消息内容"
                        }
                    },
                    {
                        Real_State,
                        new PropertySturct
                        {
                            Index        = State,
                            Name         = "State",
                            Caption      = @"任务状态",
                            JsonName     = "State",
                            ColumnName   = "state",
                            PropertyType = typeof(PlanMessageState),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"任务状态"
                        }
                    },
                    {
                        Real_CloseTime,
                        new PropertySturct
                        {
                            Index        = CloseTime,
                            Name         = "CloseTime",
                            Caption      = @"关闭时间",
                            JsonName     = "CloseTime",
                            ColumnName   = "close_time",
                            PropertyType = typeof(DateTime),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 12,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"关闭时间"
                        }
                    }
                }
            };
        }
        #endregion

    }
}