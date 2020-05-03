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
    /// 任务执行记录
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class TaskExecutionData : IIdentityData
    {
        #region 构造
        
        /// <summary>
        /// 构造
        /// </summary>
        public TaskExecutionData()
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
        /// 任务标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _taskId;

        partial void OnTaskIdGet();

        partial void OnTaskIdSet(ref long value);

        partial void OnTaskIdSeted();

        
        /// <summary>
        ///  任务标识
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("TaskId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"任务标识")]
        public  long TaskId
        {
            get
            {
                OnTaskIdGet();
                return this._taskId;
            }
            set
            {
                if(this._taskId == value)
                    return;
                OnTaskIdSet(ref value);
                this._taskId = value;
                OnTaskIdSeted();
                this.OnPropertyChanged(_DataStruct_.Real_TaskId);
                this.OnPropertyChanged(nameof(TaskId));
            }
        }
        /// <summary>
        /// 计划标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _planId;

        partial void OnPlanIdGet();

        partial void OnPlanIdSet(ref string value);

        partial void OnPlanIdSeted();

        
        /// <summary>
        ///  计划标识
        /// </summary>
        /// <value>
        ///     可存储50个字符.合理长度应不大于50.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"计划标识")]
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
        /// 执行次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _execNum;

        partial void OnExecNumGet();

        partial void OnExecNumSet(ref int value);

        partial void OnExecNumSeted();

        
        /// <summary>
        ///  执行次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("ExecNum", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"执行次数")]
        public  int ExecNum
        {
            get
            {
                OnExecNumGet();
                return this._execNum;
            }
            set
            {
                if(this._execNum == value)
                    return;
                OnExecNumSet(ref value);
                this._execNum = value;
                OnExecNumSeted();
                this.OnPropertyChanged(_DataStruct_.Real_ExecNum);
                this.OnPropertyChanged(nameof(ExecNum));
            }
        }
        /// <summary>
        /// 返回次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _successNum;

        partial void OnSuccessNumGet();

        partial void OnSuccessNumSet(ref int value);

        partial void OnSuccessNumSeted();

        
        /// <summary>
        ///  返回次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("SuccessNum", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"返回次数")]
        public  int SuccessNum
        {
            get
            {
                OnSuccessNumGet();
                return this._successNum;
            }
            set
            {
                if(this._successNum == value)
                    return;
                OnSuccessNumSet(ref value);
                this._successNum = value;
                OnSuccessNumSeted();
                this.OnPropertyChanged(_DataStruct_.Real_SuccessNum);
                this.OnPropertyChanged(nameof(SuccessNum));
            }
        }
        /// <summary>
        /// 错误次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _errorNum;

        partial void OnErrorNumGet();

        partial void OnErrorNumSet(ref int value);

        partial void OnErrorNumSeted();

        
        /// <summary>
        ///  错误次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("ErrorNum", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"错误次数")]
        public  int ErrorNum
        {
            get
            {
                OnErrorNumGet();
                return this._errorNum;
            }
            set
            {
                if(this._errorNum == value)
                    return;
                OnErrorNumSet(ref value);
                this._errorNum = value;
                OnErrorNumSeted();
                this.OnPropertyChanged(_DataStruct_.Real_ErrorNum);
                this.OnPropertyChanged(nameof(ErrorNum));
            }
        }
        /// <summary>
        /// 重试次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _retryNum;

        partial void OnRetryNumGet();

        partial void OnRetryNumSet(ref int value);

        partial void OnRetryNumSeted();

        
        /// <summary>
        ///  重试次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("RetryNum", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"重试次数")]
        public  int RetryNum
        {
            get
            {
                OnRetryNumGet();
                return this._retryNum;
            }
            set
            {
                if(this._retryNum == value)
                    return;
                OnRetryNumSet(ref value);
                this._retryNum = value;
                OnRetryNumSeted();
                this.OnPropertyChanged(_DataStruct_.Real_RetryNum);
                this.OnPropertyChanged(nameof(RetryNum));
            }
        }
        /// <summary>
        /// 跳过次数
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _skipNum;

        partial void OnSkipNumGet();

        partial void OnSkipNumSet(ref int value);

        partial void OnSkipNumSeted();

        
        /// <summary>
        ///  跳过次数
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("SkipNum", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"跳过次数")]
        public  int SkipNum
        {
            get
            {
                OnSkipNumGet();
                return this._skipNum;
            }
            set
            {
                if(this._skipNum == value)
                    return;
                OnSkipNumSet(ref value);
                this._skipNum = value;
                OnSkipNumSeted();
                this.OnPropertyChanged(_DataStruct_.Real_SkipNum);
                this.OnPropertyChanged(nameof(SkipNum));
            }
        }
        /// <summary>
        /// 执行状态
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public MessageState _execState;

        partial void OnExecStateGet();

        partial void OnExecStateSet(ref MessageState value);

        partial void OnExecStateSeted();

        
        /// <summary>
        ///  执行状态
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("ExecState", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"执行状态")]
        public  MessageState ExecState
        {
            get
            {
                OnExecStateGet();
                return this._execState;
            }
            set
            {
                if(this._execState == value)
                    return;
                OnExecStateSet(ref value);
                this._execState = value;
                OnExecStateSeted();
                this.OnPropertyChanged(_DataStruct_.Real_ExecState);
                this.OnPropertyChanged(nameof(ExecState));
            }
        }
        /// <summary>
        /// 执行状态的可读内容
        /// </summary>
        [IgnoreDataMember,JsonIgnore,DisplayName("执行状态")]
        public string ExecState_Content => ExecState.ToCaption();

        /// <summary>
        /// 执行状态的数字属性
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public  int ExecState_Number
        {
            get => (int)this.ExecState;
            set => this.ExecState = (MessageState)value;
        }
        /// <summary>
        /// 计划状态
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public PlanMessageState _planState;

        partial void OnPlanStateGet();

        partial void OnPlanStateSet(ref PlanMessageState value);

        partial void OnPlanStateSeted();

        
        /// <summary>
        ///  计划状态
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("PlanState", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"计划状态")]
        public  PlanMessageState PlanState
        {
            get
            {
                OnPlanStateGet();
                return this._planState;
            }
            set
            {
                if(this._planState == value)
                    return;
                OnPlanStateSet(ref value);
                this._planState = value;
                OnPlanStateSeted();
                this.OnPropertyChanged(_DataStruct_.Real_PlanState);
                this.OnPropertyChanged(nameof(PlanState));
            }
        }
        /// <summary>
        /// 计划状态的可读内容
        /// </summary>
        [IgnoreDataMember,JsonIgnore,DisplayName("计划状态")]
        public string PlanState_Content => PlanState.ToCaption();

        /// <summary>
        /// 计划状态的数字属性
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public  int PlanState_Number
        {
            get => (int)this.PlanState;
            set => this.PlanState = (PlanMessageState)value;
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
        /// 开始时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _execStartTime;

        partial void OnExecStartTimeGet();

        partial void OnExecStartTimeSet(ref long value);

        partial void OnExecStartTimeSeted();

        
        /// <summary>
        ///  开始时间
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("ExecStartTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"开始时间")]
        public  long ExecStartTime
        {
            get
            {
                OnExecStartTimeGet();
                return this._execStartTime;
            }
            set
            {
                if(this._execStartTime == value)
                    return;
                OnExecStartTimeSet(ref value);
                this._execStartTime = value;
                OnExecStartTimeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_ExecStartTime);
                this.OnPropertyChanged(nameof(ExecStartTime));
            }
        }
        /// <summary>
        /// 完成时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _execEndTime;

        partial void OnExecEndTimeGet();

        partial void OnExecEndTimeSet(ref long value);

        partial void OnExecEndTimeSeted();

        
        /// <summary>
        ///  完成时间
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("ExecEndTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"完成时间")]
        public  long ExecEndTime
        {
            get
            {
                OnExecEndTimeGet();
                return this._execEndTime;
            }
            set
            {
                if(this._execEndTime == value)
                    return;
                OnExecEndTimeSet(ref value);
                this._execEndTime = value;
                OnExecEndTimeSeted();
                this.OnPropertyChanged(_DataStruct_.Real_ExecEndTime);
                this.OnPropertyChanged(nameof(ExecEndTime));
            }
        }
        /// <summary>
        /// 返回内容
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _result;

        partial void OnResultGet();

        partial void OnResultSet(ref string value);

        partial void OnResultSeted();

        
        /// <summary>
        ///  返回内容
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Result", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"返回内容")]
        public  string Result
        {
            get
            {
                OnResultGet();
                return this._result;
            }
            set
            {
                if(this._result == value)
                    return;
                OnResultSet(ref value);
                this._result = value;
                OnResultSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Result);
                this.OnPropertyChanged(nameof(Result));
            }
        }
        /// <summary>
        /// 执行日志
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _log;

        partial void OnLogGet();

        partial void OnLogSet(ref string value);

        partial void OnLogSeted();

        
        /// <summary>
        ///  执行日志
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Log", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"执行日志")]
        public  string Log
        {
            get
            {
                OnLogGet();
                return this._log;
            }
            set
            {
                if(this._log == value)
                    return;
                OnLogSet(ref value);
                this._log = value;
                OnLogSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Log);
                this.OnPropertyChanged(nameof(Log));
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
            case "taskid":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.TaskId = vl;
                        return true;
                    }
                }
                return false;
            case "planid":
                this.PlanId = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "execnum":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.ExecNum = vl;
                        return true;
                    }
                }
                return false;
            case "successnum":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.SuccessNum = vl;
                        return true;
                    }
                }
                return false;
            case "errornum":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.ErrorNum = vl;
                        return true;
                    }
                }
                return false;
            case "retrynum":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.RetryNum = vl;
                        return true;
                    }
                }
                return false;
            case "skipnum":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.SkipNum = vl;
                        return true;
                    }
                }
                return false;
            case "execstate":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (MessageState.TryParse(value, out MessageState val))
                    {
                        this.ExecState = val;
                        return true;
                    }
                    else if (int.TryParse(value, out int vl))
                    {
                        this.ExecState = (MessageState)vl;
                        return true;
                    }
                }
                return false;
            case "planstate":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (PlanMessageState.TryParse(value, out PlanMessageState val))
                    {
                        this.PlanState = val;
                        return true;
                    }
                    else if (int.TryParse(value, out int vl))
                    {
                        this.PlanState = (PlanMessageState)vl;
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
            case "execstarttime":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.ExecStartTime = vl;
                        return true;
                    }
                }
                return false;
            case "execendtime":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.ExecEndTime = vl;
                        return true;
                    }
                }
                return false;
            case "result":
                this.Result = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "log":
                this.Log = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
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
            case "taskid":
                this.TaskId = (long)Convert.ToDecimal(value);
                return;
            case "planid":
                this.PlanId = value == null ? null : value.ToString();
                return;
            case "execnum":
                this.ExecNum = (int)Convert.ToDecimal(value);
                return;
            case "successnum":
                this.SuccessNum = (int)Convert.ToDecimal(value);
                return;
            case "errornum":
                this.ErrorNum = (int)Convert.ToDecimal(value);
                return;
            case "retrynum":
                this.RetryNum = (int)Convert.ToDecimal(value);
                return;
            case "skipnum":
                this.SkipNum = (int)Convert.ToDecimal(value);
                return;
            case "execstate":
                if (value != null)
                {
                    if(value is int)
                    {
                        this.ExecState = (MessageState)(int)value;
                    }
                    else if(value is MessageState)
                    {
                        this.ExecState = (MessageState)value;
                    }
                    else
                    {
                        var str = value.ToString();
                        MessageState val;
                        if (MessageState.TryParse(str, out val))
                        {
                            this.ExecState = val;
                        }
                        else
                        {
                            int vl;
                            if (int.TryParse(str, out vl))
                            {
                                this.ExecState = (MessageState)vl;
                            }
                        }
                    }
                }
                return;
            case "planstate":
                if (value != null)
                {
                    if(value is int)
                    {
                        this.PlanState = (PlanMessageState)(int)value;
                    }
                    else if(value is PlanMessageState)
                    {
                        this.PlanState = (PlanMessageState)value;
                    }
                    else
                    {
                        var str = value.ToString();
                        PlanMessageState val;
                        if (PlanMessageState.TryParse(str, out val))
                        {
                            this.PlanState = val;
                        }
                        else
                        {
                            int vl;
                            if (int.TryParse(str, out vl))
                            {
                                this.PlanState = (PlanMessageState)vl;
                            }
                        }
                    }
                }
                return;
            case "plantime":
                this.PlanTime = (long)Convert.ToDecimal(value);
                return;
            case "execstarttime":
                this.ExecStartTime = (long)Convert.ToDecimal(value);
                return;
            case "execendtime":
                this.ExecEndTime = (long)Convert.ToDecimal(value);
                return;
            case "result":
                this.Result = value == null ? null : value.ToString();
                return;
            case "log":
                this.Log = value == null ? null : value.ToString();
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
            case _DataStruct_.TaskId:
                this.TaskId = Convert.ToInt64(value);
                return;
            case _DataStruct_.PlanId:
                this.PlanId = value == null ? null : value.ToString();
                return;
            case _DataStruct_.ExecNum:
                this.ExecNum = Convert.ToInt32(value);
                return;
            case _DataStruct_.SuccessNum:
                this.SuccessNum = Convert.ToInt32(value);
                return;
            case _DataStruct_.ErrorNum:
                this.ErrorNum = Convert.ToInt32(value);
                return;
            case _DataStruct_.RetryNum:
                this.RetryNum = Convert.ToInt32(value);
                return;
            case _DataStruct_.SkipNum:
                this.SkipNum = Convert.ToInt32(value);
                return;
            case _DataStruct_.ExecState:
                this.ExecState = (MessageState)value;
                return;
            case _DataStruct_.PlanState:
                this.PlanState = (PlanMessageState)value;
                return;
            case _DataStruct_.PlanTime:
                this.PlanTime = Convert.ToInt64(value);
                return;
            case _DataStruct_.ExecStartTime:
                this.ExecStartTime = Convert.ToInt64(value);
                return;
            case _DataStruct_.ExecEndTime:
                this.ExecEndTime = Convert.ToInt64(value);
                return;
            case _DataStruct_.Result:
                this.Result = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Log:
                this.Log = value == null ? null : value.ToString();
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
            case "taskid":
                return this.TaskId;
            case "planid":
                return this.PlanId;
            case "execnum":
                return this.ExecNum;
            case "successnum":
                return this.SuccessNum;
            case "errornum":
                return this.ErrorNum;
            case "retrynum":
                return this.RetryNum;
            case "skipnum":
                return this.SkipNum;
            case "execstate":
                return this.ExecState.ToCaption();
            case "planstate":
                return this.PlanState.ToCaption();
            case "plantime":
                return this.PlanTime;
            case "execstarttime":
                return this.ExecStartTime;
            case "execendtime":
                return this.ExecEndTime;
            case "result":
                return this.Result;
            case "log":
                return this.Log;
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
                case _DataStruct_.TaskId:
                    return this.TaskId;
                case _DataStruct_.PlanId:
                    return this.PlanId;
                case _DataStruct_.ExecNum:
                    return this.ExecNum;
                case _DataStruct_.SuccessNum:
                    return this.SuccessNum;
                case _DataStruct_.ErrorNum:
                    return this.ErrorNum;
                case _DataStruct_.RetryNum:
                    return this.RetryNum;
                case _DataStruct_.SkipNum:
                    return this.SkipNum;
                case _DataStruct_.ExecState:
                    return this.ExecState;
                case _DataStruct_.PlanState:
                    return this.PlanState;
                case _DataStruct_.PlanTime:
                    return this.PlanTime;
                case _DataStruct_.ExecStartTime:
                    return this.ExecStartTime;
                case _DataStruct_.ExecEndTime:
                    return this.ExecEndTime;
                case _DataStruct_.Result:
                    return this.Result;
                case _DataStruct_.Log:
                    return this.Log;
            }

            return null;
        }

        #endregion

        #region 复制
        

        partial void CopyExtendValue(TaskExecutionData source);

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected override void CopyValueInner(DataObjectBase source)
        {
            var sourceEntity = source as TaskExecutionData;
            if(sourceEntity == null)
                return;
            this._id = sourceEntity._id;
            this._taskId = sourceEntity._taskId;
            this._planId = sourceEntity._planId;
            this._execNum = sourceEntity._execNum;
            this._successNum = sourceEntity._successNum;
            this._errorNum = sourceEntity._errorNum;
            this._retryNum = sourceEntity._retryNum;
            this._skipNum = sourceEntity._skipNum;
            this._execState = sourceEntity._execState;
            this._planState = sourceEntity._planState;
            this._planTime = sourceEntity._planTime;
            this._execStartTime = sourceEntity._execStartTime;
            this._execEndTime = sourceEntity._execEndTime;
            this._result = sourceEntity._result;
            this._log = sourceEntity._log;
            CopyExtendValue(sourceEntity);
            this.__status.IsModified = true;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void Copy(TaskExecutionData source)
        {
                this.Id = source.Id;
                this.TaskId = source.TaskId;
                this.PlanId = source.PlanId;
                this.ExecNum = source.ExecNum;
                this.SuccessNum = source.SuccessNum;
                this.ErrorNum = source.ErrorNum;
                this.RetryNum = source.RetryNum;
                this.SkipNum = source.SkipNum;
                this.ExecState = source.ExecState;
                this.PlanState = source.PlanState;
                this.PlanTime = source.PlanTime;
                this.ExecStartTime = source.ExecStartTime;
                this.ExecEndTime = source.ExecEndTime;
                this.Result = source.Result;
                this.Log = source.Log;
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
                OnTaskIdModified(subsist,false);
                OnPlanIdModified(subsist,false);
                OnExecNumModified(subsist,false);
                OnSuccessNumModified(subsist,false);
                OnErrorNumModified(subsist,false);
                OnRetryNumModified(subsist,false);
                OnSkipNumModified(subsist,false);
                OnExecStateModified(subsist,false);
                OnPlanStateModified(subsist,false);
                OnPlanTimeModified(subsist,false);
                OnExecStartTimeModified(subsist,false);
                OnExecEndTimeModified(subsist,false);
                OnResultModified(subsist,false);
                OnLogModified(subsist,false);
                return;
            }
            else if (subsist == EntitySubsist.Adding || subsist == EntitySubsist.Added)
            {
                OnIdModified(subsist,true);
                OnTaskIdModified(subsist,true);
                OnPlanIdModified(subsist,true);
                OnExecNumModified(subsist,true);
                OnSuccessNumModified(subsist,true);
                OnErrorNumModified(subsist,true);
                OnRetryNumModified(subsist,true);
                OnSkipNumModified(subsist,true);
                OnExecStateModified(subsist,true);
                OnPlanStateModified(subsist,true);
                OnPlanTimeModified(subsist,true);
                OnExecStartTimeModified(subsist,true);
                OnExecEndTimeModified(subsist,true);
                OnResultModified(subsist,true);
                OnLogModified(subsist,true);
                return;
            }
            else if(modifieds != null && modifieds[15] > 0)
            {
                OnIdModified(subsist,modifieds[_DataStruct_.Real_Id] == 1);
                OnTaskIdModified(subsist,modifieds[_DataStruct_.Real_TaskId] == 1);
                OnPlanIdModified(subsist,modifieds[_DataStruct_.Real_PlanId] == 1);
                OnExecNumModified(subsist,modifieds[_DataStruct_.Real_ExecNum] == 1);
                OnSuccessNumModified(subsist,modifieds[_DataStruct_.Real_SuccessNum] == 1);
                OnErrorNumModified(subsist,modifieds[_DataStruct_.Real_ErrorNum] == 1);
                OnRetryNumModified(subsist,modifieds[_DataStruct_.Real_RetryNum] == 1);
                OnSkipNumModified(subsist,modifieds[_DataStruct_.Real_SkipNum] == 1);
                OnExecStateModified(subsist,modifieds[_DataStruct_.Real_ExecState] == 1);
                OnPlanStateModified(subsist,modifieds[_DataStruct_.Real_PlanState] == 1);
                OnPlanTimeModified(subsist,modifieds[_DataStruct_.Real_PlanTime] == 1);
                OnExecStartTimeModified(subsist,modifieds[_DataStruct_.Real_ExecStartTime] == 1);
                OnExecEndTimeModified(subsist,modifieds[_DataStruct_.Real_ExecEndTime] == 1);
                OnResultModified(subsist,modifieds[_DataStruct_.Real_Result] == 1);
                OnLogModified(subsist,modifieds[_DataStruct_.Real_Log] == 1);
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
        /// 任务标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnTaskIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 计划标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 执行次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnExecNumModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 返回次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnSuccessNumModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 错误次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnErrorNumModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 重试次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnRetryNumModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 跳过次数修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnSkipNumModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 执行状态修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnExecStateModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 计划状态修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnPlanStateModified(EntitySubsist subsist,bool isModified);

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
        /// 开始时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnExecStartTimeModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 完成时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnExecEndTimeModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 返回内容修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnResultModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 执行日志修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnLogModified(EntitySubsist subsist,bool isModified);
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
            public const string EntityName = @"TaskExecution";
            /// <summary>
            /// 实体标题
            /// </summary>
            public const string EntityCaption = @"任务执行记录";
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityDescription = @"任务执行记录";
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
            /// 任务标识的数字标识
            /// </summary>
            public const int TaskId = 2;
            
            /// <summary>
            /// 任务标识的实时记录顺序
            /// </summary>
            public const int Real_TaskId = 1;

            /// <summary>
            /// 计划标识的数字标识
            /// </summary>
            public const int PlanId = 3;
            
            /// <summary>
            /// 计划标识的实时记录顺序
            /// </summary>
            public const int Real_PlanId = 2;

            /// <summary>
            /// 执行次数的数字标识
            /// </summary>
            public const int ExecNum = 4;
            
            /// <summary>
            /// 执行次数的实时记录顺序
            /// </summary>
            public const int Real_ExecNum = 3;

            /// <summary>
            /// 返回次数的数字标识
            /// </summary>
            public const int SuccessNum = 5;
            
            /// <summary>
            /// 返回次数的实时记录顺序
            /// </summary>
            public const int Real_SuccessNum = 4;

            /// <summary>
            /// 错误次数的数字标识
            /// </summary>
            public const int ErrorNum = 6;
            
            /// <summary>
            /// 错误次数的实时记录顺序
            /// </summary>
            public const int Real_ErrorNum = 5;

            /// <summary>
            /// 重试次数的数字标识
            /// </summary>
            public const int RetryNum = 7;
            
            /// <summary>
            /// 重试次数的实时记录顺序
            /// </summary>
            public const int Real_RetryNum = 6;

            /// <summary>
            /// 跳过次数的数字标识
            /// </summary>
            public const int SkipNum = 8;
            
            /// <summary>
            /// 跳过次数的实时记录顺序
            /// </summary>
            public const int Real_SkipNum = 7;

            /// <summary>
            /// 执行状态的数字标识
            /// </summary>
            public const int ExecState = 9;
            
            /// <summary>
            /// 执行状态的实时记录顺序
            /// </summary>
            public const int Real_ExecState = 8;

            /// <summary>
            /// 计划状态的数字标识
            /// </summary>
            public const int PlanState = 10;
            
            /// <summary>
            /// 计划状态的实时记录顺序
            /// </summary>
            public const int Real_PlanState = 9;

            /// <summary>
            /// 计划时间的数字标识
            /// </summary>
            public const int PlanTime = 11;
            
            /// <summary>
            /// 计划时间的实时记录顺序
            /// </summary>
            public const int Real_PlanTime = 10;

            /// <summary>
            /// 开始时间的数字标识
            /// </summary>
            public const int ExecStartTime = 12;
            
            /// <summary>
            /// 开始时间的实时记录顺序
            /// </summary>
            public const int Real_ExecStartTime = 11;

            /// <summary>
            /// 完成时间的数字标识
            /// </summary>
            public const int ExecEndTime = 13;
            
            /// <summary>
            /// 完成时间的实时记录顺序
            /// </summary>
            public const int Real_ExecEndTime = 12;

            /// <summary>
            /// 返回内容的数字标识
            /// </summary>
            public const int Result = 14;
            
            /// <summary>
            /// 返回内容的实时记录顺序
            /// </summary>
            public const int Real_Result = 13;

            /// <summary>
            /// 执行日志的数字标识
            /// </summary>
            public const int Log = 15;
            
            /// <summary>
            /// 执行日志的实时记录顺序
            /// </summary>
            public const int Real_Log = 14;

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
                        Real_TaskId,
                        new PropertySturct
                        {
                            Index        = TaskId,
                            Name         = "TaskId",
                            Caption      = @"任务标识",
                            JsonName     = "TaskId",
                            ColumnName   = "task_id",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"任务标识"
                        }
                    },
                    {
                        Real_PlanId,
                        new PropertySturct
                        {
                            Index        = PlanId,
                            Name         = "PlanId",
                            Caption      = @"计划标识",
                            JsonName     = "PlanId",
                            ColumnName   = "plan_id",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"计划标识"
                        }
                    },
                    {
                        Real_ExecNum,
                        new PropertySturct
                        {
                            Index        = ExecNum,
                            Name         = "ExecNum",
                            Caption      = @"执行次数",
                            JsonName     = "ExecNum",
                            ColumnName   = "exec_num",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"执行次数"
                        }
                    },
                    {
                        Real_SuccessNum,
                        new PropertySturct
                        {
                            Index        = SuccessNum,
                            Name         = "SuccessNum",
                            Caption      = @"返回次数",
                            JsonName     = "SuccessNum",
                            ColumnName   = "success_num",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"返回次数"
                        }
                    },
                    {
                        Real_ErrorNum,
                        new PropertySturct
                        {
                            Index        = ErrorNum,
                            Name         = "ErrorNum",
                            Caption      = @"错误次数",
                            JsonName     = "ErrorNum",
                            ColumnName   = "error_num",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"错误次数"
                        }
                    },
                    {
                        Real_RetryNum,
                        new PropertySturct
                        {
                            Index        = RetryNum,
                            Name         = "RetryNum",
                            Caption      = @"重试次数",
                            JsonName     = "RetryNum",
                            ColumnName   = "retry_num",
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
                        Real_SkipNum,
                        new PropertySturct
                        {
                            Index        = SkipNum,
                            Name         = "SkipNum",
                            Caption      = @"跳过次数",
                            JsonName     = "SkipNum",
                            ColumnName   = "skip_num",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"跳过次数"
                        }
                    },
                    {
                        Real_ExecState,
                        new PropertySturct
                        {
                            Index        = ExecState,
                            Name         = "ExecState",
                            Caption      = @"执行状态",
                            JsonName     = "ExecState",
                            ColumnName   = "exec_state",
                            PropertyType = typeof(MessageState),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"执行状态"
                        }
                    },
                    {
                        Real_PlanState,
                        new PropertySturct
                        {
                            Index        = PlanState,
                            Name         = "PlanState",
                            Caption      = @"计划状态",
                            JsonName     = "PlanState",
                            ColumnName   = "plan_state",
                            PropertyType = typeof(PlanMessageState),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"计划状态"
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
                            Description  = @"计划时间"
                        }
                    },
                    {
                        Real_ExecStartTime,
                        new PropertySturct
                        {
                            Index        = ExecStartTime,
                            Name         = "ExecStartTime",
                            Caption      = @"开始时间",
                            JsonName     = "ExecStartTime",
                            ColumnName   = "exec_start_time",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"开始时间"
                        }
                    },
                    {
                        Real_ExecEndTime,
                        new PropertySturct
                        {
                            Index        = ExecEndTime,
                            Name         = "ExecEndTime",
                            Caption      = @"完成时间",
                            JsonName     = "ExecEndTime",
                            ColumnName   = "exec_end_time",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"完成时间"
                        }
                    },
                    {
                        Real_Result,
                        new PropertySturct
                        {
                            Index        = Result,
                            Name         = "Result",
                            Caption      = @"返回内容",
                            JsonName     = "Result",
                            ColumnName   = "result",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"返回内容"
                        }
                    },
                    {
                        Real_Log,
                        new PropertySturct
                        {
                            Index        = Log,
                            Name         = "Log",
                            Caption      = @"执行日志",
                            JsonName     = "Log",
                            ColumnName   = "log",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"执行日志"
                        }
                    }
                }
            };
        }
        #endregion

    }
}