/*design by:agebull designer date:2020/4/26 18:26:10*/
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

using MySql.Data.MySqlClient;

using Agebull.Common;

using Agebull.EntityModel.Common;

using Agebull.EntityModel.MySql;
using Agebull.EntityModel.BusinessLogic;

using ZeroTeam.MessageMVC.Messages;

using ZeroTeam.MessageMVC.PlanTasks.DataAccess;
using ZeroTeam.MessageMVC.PlanTasks.DataAccess;
using System.Threading.Tasks;
using Agebull.Common.Logging;
#endregion

namespace ZeroTeam.MessageMVC.PlanTasks.BusinessLogic
{
    /// <summary>
    /// 任务执行记录
    /// </summary>
    public partial class TaskExecutionBusinessLogic : UiBusinessLogicBase<TaskExecutionData,TaskExecutionDataAccess>
    {

        #region 设计器命令

        #endregion


        #region 树形数据

        #endregion
        #region CURD扩展
        /*// <summary>
        ///     保存前的操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool OnSaving(TaskExecutionData data, bool isAdd)
        {
             return base.OnSaving(data, isAdd);
        }

        /// <summary>
        ///     保存完成后的操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool OnSaved(TaskExecutionData data, bool isAdd)
        {
             return base.OnSaved(data, isAdd);
        }
        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool LastSavedByUser(TaskExecutionData data, bool isAdd)
        {
            return base.LastSavedByUser(data, isAdd);
        }

        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool PrepareSaveByUser(TaskExecutionData data, bool isAdd)
        {
            return base.PrepareSaveByUser(data, isAdd);
        }*/
        #endregion


        public static Task SaveToDatabase(PlanItem item,IInlineMessage message)
        {
            var log = item.Monitor.End();
            var bl = new TaskExecutionBusinessLogic();
            return bl.Access.InsertAsync(new TaskExecutionData
            {
                PlanId = item.Option.PlanId,
                ExecNum = item.RealInfo.ExecNum,
                SuccessNum = item.RealInfo.SuccessNum,
                ErrorNum = item.RealInfo.ErrorNum,
                RetryNum = item.RealInfo.RetryNum,
                SkipNum = item.RealInfo.SkipNum,
                ExecState = item.RealInfo.ExecState,
                PlanState = item.RealInfo.PlanState,
                PlanTime = item.RealInfo.PlanTime,
                ExecStartTime = item.RealInfo.ExecStartTime,
                ExecEndTime = item.RealInfo.ExecEndTime,
                Result = SmartSerializer.SerializeResult(message),
                Log = SmartSerializer.ToInnerString(log)
            });
        }
    }
}