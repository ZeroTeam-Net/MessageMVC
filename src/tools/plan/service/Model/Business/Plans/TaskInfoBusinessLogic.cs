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
#endregion

namespace ZeroTeam.MessageMVC.PlanTasks.BusinessLogic
{
    /// <summary>
    /// 任务信息
    /// </summary>
    public partial class TaskInfoBusinessLogic : UiBusinessLogicBase<TaskInfoData, TaskInfoDataAccess>
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
        protected override bool OnSaving(TaskInfoData data, bool isAdd)
        {
             return base.OnSaving(data, isAdd);
        }

        /// <summary>
        ///     保存完成后的操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool OnSaved(TaskInfoData data, bool isAdd)
        {
             return base.OnSaved(data, isAdd);
        }
        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool LastSavedByUser(TaskInfoData data, bool isAdd)
        {
            return base.LastSavedByUser(data, isAdd);
        }

        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool PrepareSaveByUser(TaskInfoData data, bool isAdd)
        {
            return base.PrepareSaveByUser(data, isAdd);
        }*/
        #endregion

        public static async Task<long> SaveToDatabase(PlanItem item)
        {
            var bl = new TaskInfoBusinessLogic();
            var data = new TaskInfoData
            {
                PlanId = item.Option.PlanId,
                Description = item.Option.Description,
                PlanType = item.Option.PlanType,
                PlanValue = item.Option.PlanValue,
                RetrySet = item.Option.RetrySet,
                SkipSet = item.Option.SkipSet,
                PlanRepet = item.Option.PlanRepet,
                QueuePassBy = item.Option.QueuePassBy,
                AddTime = item.Option.AddTime,
                PlanTime = item.Option.PlanTime,
                IsAsync = item.Option.IsAsync,
                CheckResultTime = item.Option.CheckResultTime,
                Message = SmartSerializer.SerializeMessage(item.Message)
            };
            await bl.Access.InsertAsync(data);
            return data.Id;
        }


        public static async Task OnClose(PlanItem item)
        {
            var bl = new TaskInfoBusinessLogic();
            var data = await bl.Access.FirstAsync(p => p.PlanId == item.Option.PlanId);
            if (data == null)
                return;
            data.State = PlanMessageState.close;
            data.CloseTime = DateTime.Now;
            await bl.Access.UpdateAsync(data);
        }
    }
}