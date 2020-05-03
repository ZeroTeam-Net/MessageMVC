/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/26 22:33:23*/
#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Agebull.Common.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

using Agebull.Common;
using Agebull.Common.Ioc;

using Agebull.EntityModel.Common;
using Agebull.EntityModel.EasyUI;
using ZeroTeam.MessageMVC.ZeroApis;
using Agebull.MicroZero.ZeroApis;

using ZeroTeam.MessageMVC.Messages;

using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.PlanTasks.BusinessLogic;
using ZeroTeam.MessageMVC.PlanTasks.DataAccess;
#endregion

namespace ZeroTeam.MessageMVC.PlanTasks.WebApi.Entity
{
    partial class TaskExecutionApiController
    {

        #region 基本扩展

        /// <summary>
        ///     读取查询条件
        /// </summary>
        /// <param name="filter">筛选器</param>
        public override void GetQueryFilter(LambdaItem<TaskExecutionData> filter)
        {
            if (TryGet("_value_", out string value) && value != null)
            {
                var field = GetString("_field_");
                if (string.IsNullOrWhiteSpace(field) || field == "_any_")
                    filter.AddAnd(p => p.PlanId.Contains(value)
                                    || p.Result.Contains(value)
                                    || p.Log.Contains(value));
                else this[field] = value;
            }
            if(TryGetIDs("Id" , out var Id))
            {
                if (Id.Count == 1)
                    filter.AddAnd(p => p.Id == Id[0]);
                else
                    filter.AddAnd(p => Id.Contains(p.Id));
            }
            if(TryGet("TaskId" , out long TaskId))
            {
                filter.AddAnd(p => p.TaskId == TaskId);
            }
            if(TryGet("PlanId" , out string PlanId))
            {
                filter.AddAnd(p => p.PlanId.Contains(PlanId));
            }
            if(TryGet("ExecNum" , out int ExecNum))
            {
                filter.AddAnd(p => p.ExecNum == ExecNum);
            }
            if(TryGet("SuccessNum" , out int SuccessNum))
            {
                filter.AddAnd(p => p.SuccessNum == SuccessNum);
            }
            if(TryGet("ErrorNum" , out int ErrorNum))
            {
                filter.AddAnd(p => p.ErrorNum == ErrorNum);
            }
            if(TryGet("RetryNum" , out int RetryNum))
            {
                filter.AddAnd(p => p.RetryNum == RetryNum);
            }
            if(TryGet("SkipNum" , out int SkipNum))
            {
                filter.AddAnd(p => p.SkipNum == SkipNum);
            }
            if(TryGet("ExecState" , out int ExecState))
            {
                filter.AddAnd(p => p.ExecState == (MessageState)ExecState);
            }
            if(TryGet("PlanState" , out int PlanState))
            {
                filter.AddAnd(p => p.PlanState == (PlanMessageState)PlanState);
            }
            if(TryGet("PlanTime" , out long PlanTime))
            {
                filter.AddAnd(p => p.PlanTime == PlanTime);
            }
            if(TryGet("ExecStartTime" , out long ExecStartTime))
            {
                filter.AddAnd(p => p.ExecStartTime == ExecStartTime);
            }
            if(TryGet("ExecEndTime" , out long ExecEndTime))
            {
                filter.AddAnd(p => p.ExecEndTime == ExecEndTime);
            }
            if(TryGet("Result" , out string Result))
            {
                filter.AddAnd(p => p.Result.Contains(Result));
            }
            if(TryGet("Log" , out string Log))
            {
                filter.AddAnd(p => p.Log.Contains(Log));
            }
        }

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected void DefaultReadFormData(TaskExecutionData data, FormConvert convert)
        {
            //普通字段
            if(convert.TryGetValue("TaskId" , out long TaskId))
                data.TaskId = TaskId;
            if(convert.TryGetValue("PlanId" , out string PlanId))
                data.PlanId = PlanId;
            if(convert.TryGetValue("ExecNum" , out int ExecNum))
                data.ExecNum = ExecNum;
            if(convert.TryGetValue("SuccessNum" , out int SuccessNum))
                data.SuccessNum = SuccessNum;
            if(convert.TryGetValue("ErrorNum" , out int ErrorNum))
                data.ErrorNum = ErrorNum;
            if(convert.TryGetValue("RetryNum" , out int RetryNum))
                data.RetryNum = RetryNum;
            if(convert.TryGetValue("SkipNum" , out int SkipNum))
                data.SkipNum = SkipNum;
            if(convert.TryGetValue("ExecState" , out int ExecState))
                data.ExecState = (MessageState)ExecState;
            if(convert.TryGetValue("PlanState" , out int PlanState))
                data.PlanState = (PlanMessageState)PlanState;
            if(convert.TryGetValue("PlanTime" , out long PlanTime))
                data.PlanTime = PlanTime;
            if(convert.TryGetValue("ExecStartTime" , out long ExecStartTime))
                data.ExecStartTime = ExecStartTime;
            if(convert.TryGetValue("ExecEndTime" , out long ExecEndTime))
                data.ExecEndTime = ExecEndTime;
            if(convert.TryGetValue("Result" , out string Result))
                data.Result = Result;
            if(convert.TryGetValue("Log" , out string Log))
                data.Log = Log;
        }

        #endregion
    }
}