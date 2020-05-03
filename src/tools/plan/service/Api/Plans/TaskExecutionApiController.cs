/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/26 23:13:34*/
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
using Agebull.Common.Ioc;

using Agebull.EntityModel.Common;
using Agebull.EntityModel.EasyUI;
using ZeroTeam.MessageMVC.ZeroApis;
using Agebull.MicroZero.ZeroApis;

using ZeroTeam.MessageMVC.Messages;

using ZeroTeam.MessageMVC.PlanTasks;
using ZeroTeam.MessageMVC.PlanTasks.BusinessLogic;

#endregion

namespace ZeroTeam.MessageMVC.PlanTasks.WebApi.Entity
{
    /// <summary>
    ///  任务执行记录
    /// </summary>
    [Service("planEdit")]
    [Route("taskExecution/v1")]
    [ApiPage("/wwwroot/Plans/TaskExecution/index.htm")]
    public partial class TaskExecutionApiController 
         : ApiController<TaskExecutionData,TaskExecutionBusinessLogic>
    {
        #region 基本扩展

        /*// <summary>
        ///     取得列表数据
        /// </summary>
        protected override ApiPageData<TaskExecutionData> GetListData()
        {
            var filter = new LambdaItem<TaskExecutionData>();
            ReadQueryFilter(filter);
            return base.GetListData(filter);
        }*/

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected override void ReadFormData(TaskExecutionData data, FormConvert convert)
        {
            DefaultReadFormData(data,convert);
        }

        #endregion
    }
}