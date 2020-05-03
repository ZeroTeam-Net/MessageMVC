/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/26 22:05:27*/
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
    partial class TaskInfoApiController
    {

        #region 基本扩展

        /// <summary>
        ///     读取查询条件
        /// </summary>
        /// <param name="filter">筛选器</param>
        public override void GetQueryFilter(LambdaItem<TaskInfoData> filter)
        {
            if (TryGet("_value_", out string value) && value != null)
            {
                var field = GetString("_field_");
                if (string.IsNullOrWhiteSpace(field) || field == "_any_")
                    filter.AddAnd(p => p.PlanId.Contains(value)
                                    || p.Description.Contains(value)
                                    || p.Message.Contains(value));
                else this[field] = value;
            }
            if(TryGetIDs("Id" , out var Id))
            {
                if (Id.Count == 1)
                    filter.AddAnd(p => p.Id == Id[0]);
                else
                    filter.AddAnd(p => Id.Contains(p.Id));
            }
            if(TryGet("PlanId" , out string PlanId))
            {
                filter.AddAnd(p => p.PlanId.Contains(PlanId));
            }
            if(TryGet("Description" , out string Description))
            {
                filter.AddAnd(p => p.Description.Contains(Description));
            }
            if(TryGet("PlanType" , out int PlanType))
            {
                filter.AddAnd(p => p.PlanType == (PlanTimeType)PlanType);
            }
            if(TryGet("PlanValue" , out int PlanValue))
            {
                filter.AddAnd(p => p.PlanValue == PlanValue);
            }
            if(TryGet("RetrySet" , out int RetrySet))
            {
                filter.AddAnd(p => p.RetrySet == RetrySet);
            }
            if(TryGet("SkipSet" , out int SkipSet))
            {
                filter.AddAnd(p => p.SkipSet == SkipSet);
            }
            if(TryGet("PlanRepet" , out int PlanRepet))
            {
                filter.AddAnd(p => p.PlanRepet == PlanRepet);
            }
            if(TryGet("QueuePassBy" , out bool QueuePassBy))
            {
                filter.AddAnd(p => p.QueuePassBy == QueuePassBy);
            }
            if(TryGet("AddTime" , out long AddTime))
            {
                filter.AddAnd(p => p.AddTime == AddTime);
            }
            if(TryGet("PlanTime" , out long PlanTime))
            {
                filter.AddAnd(p => p.PlanTime == PlanTime);
            }
            if(TryGet("IsAsync" , out bool IsAsync))
            {
                filter.AddAnd(p => p.IsAsync == IsAsync);
            }
            if(TryGet("CheckResultTime" , out int CheckResultTime))
            {
                filter.AddAnd(p => p.CheckResultTime == CheckResultTime);
            }
            if(TryGet("Message" , out string Message))
            {
                filter.AddAnd(p => p.Message.Contains(Message));
            }
            if(TryGet("State" , out int State))
            {
                filter.AddAnd(p => p.State == (PlanMessageState)State);
            }
            if(TryGet("CloseTime" , out DateTime CloseTime))
            {
                var day = CloseTime.Date;
                var nextDay = day.AddDays(1);
                filter.AddAnd(p => (p.CloseTime >= day && p.CloseTime < nextDay));
            }
            else 
            {
                if(TryGet("CloseTime_begin" , out DateTime CloseTime_begin))
                {
                    var day = CloseTime_begin.Date;
                    filter.AddAnd(p => p.CloseTime >= day);
                }
                if(TryGet("CloseTime_end" , out DateTime CloseTime_end))
                {
                    var day = CloseTime_end.Date.AddDays(1);
                    filter.AddAnd(p => p.CloseTime < day);
                }
            }
        }

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected void DefaultReadFormData(TaskInfoData data, FormConvert convert)
        {
            //普通字段
            if(convert.TryGetValue("PlanId" , out string PlanId))
                data.PlanId = PlanId;
            if(convert.TryGetValue("Description" , out string Description))
                data.Description = Description;
            if(convert.TryGetValue("PlanType" , out int PlanType))
                data.PlanType = (PlanTimeType)PlanType;
            if(convert.TryGetValue("PlanValue" , out int PlanValue))
                data.PlanValue = PlanValue;
            if(convert.TryGetValue("RetrySet" , out int RetrySet))
                data.RetrySet = RetrySet;
            if(convert.TryGetValue("SkipSet" , out int SkipSet))
                data.SkipSet = SkipSet;
            if(convert.TryGetValue("PlanRepet" , out int PlanRepet))
                data.PlanRepet = PlanRepet;
            if(convert.TryGetValue("QueuePassBy" , out bool QueuePassBy))
                data.QueuePassBy = QueuePassBy;
            if(convert.TryGetValue("AddTime" , out long AddTime))
                data.AddTime = AddTime;
            if(convert.TryGetValue("PlanTime" , out long PlanTime))
                data.PlanTime = PlanTime;
            if(convert.TryGetValue("IsAsync" , out bool IsAsync))
                data.IsAsync = IsAsync;
            if(convert.TryGetValue("CheckResultTime" , out int CheckResultTime))
                data.CheckResultTime = CheckResultTime;
            if(convert.TryGetValue("Message" , out string Message))
                data.Message = Message;
            if(convert.TryGetValue("State" , out int State))
                data.State = (PlanMessageState)State;
            if(convert.TryGetValue("CloseTime" , out DateTime CloseTime))
                data.CloseTime = CloseTime;
        }

        #endregion
    }
}