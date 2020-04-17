/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 15:42:49*/
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



using ZeroTeam.MessageMVC.MessageTraceLink;
using ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.WebApi.Entity
{
    partial class HealthCheckApiController
    {

        #region 基本扩展

        /// <summary>
        ///     读取查询条件
        /// </summary>
        /// <param name="filter">筛选器</param>
        public override void GetQueryFilter(LambdaItem<HealthCheckData> filter)
        {
            if (TryGet("_value_", out string value) && value != null)
            {
                var field = GetString("_field_");
                if (string.IsNullOrWhiteSpace(field) || field == "_any_")
                    filter.AddAnd(p => p.Service.Contains(value)
                                    || p.Url.Contains(value)
                                    || p.Machine.Contains(value)
                                    || p.Details.Contains(value));
                else this[field] = value;
            }
            if(TryGetIDs("Id" , out var Id))
            {
                if (Id.Count == 1)
                    filter.AddAnd(p => p.Id == Id[0]);
                else
                    filter.AddAnd(p => Id.Contains(p.Id));
            }
            if(TryGet("CheckID" , out int CheckID))
            {
                filter.AddAnd(p => p.CheckID == CheckID);
            }
            if(TryGet("Service" , out string Service))
            {
                filter.AddAnd(p => p.Service.Contains(Service));
            }
            if(TryGet("Url" , out string Url))
            {
                filter.AddAnd(p => p.Url.Contains(Url));
            }
            if(TryGet("Machine" , out string Machine))
            {
                filter.AddAnd(p => p.Machine.Contains(Machine));
            }
            if(TryGet("Start" , out DateTime Start))
            {
                var day = Start.Date;
                var nextDay = day.AddDays(1);
                filter.AddAnd(p => (p.Start >= day && p.Start < nextDay));
            }
            else 
            {
                if(TryGet("Start_begin" , out DateTime Start_begin))
                {
                    var day = Start_begin.Date;
                    filter.AddAnd(p => p.Start >= day);
                }
                if(TryGet("Start_end" , out DateTime Start_end))
                {
                    var day = Start_end.Date.AddDays(1);
                    filter.AddAnd(p => p.Start < day);
                }
            }
            if(TryGet("End" , out DateTime End))
            {
                var day = End.Date;
                var nextDay = day.AddDays(1);
                filter.AddAnd(p => (p.End >= day && p.End < nextDay));
            }
            else 
            {
                if(TryGet("End_begin" , out DateTime End_begin))
                {
                    var day = End_begin.Date;
                    filter.AddAnd(p => p.End >= day);
                }
                if(TryGet("End_end" , out DateTime End_end))
                {
                    var day = End_end.Date.AddDays(1);
                    filter.AddAnd(p => p.End < day);
                }
            }
            if(TryGet("Level" , out int Level))
            {
                filter.AddAnd(p => p.Level == Level);
            }
            if(TryGet("Details" , out string Details))
            {
                filter.AddAnd(p => p.Details.Contains(Details));
            }
        }

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected void DefaultReadFormData(HealthCheckData data, FormConvert convert)
        {
            //普通字段
            if(convert.TryGetValue("CheckID" , out int CheckID))
                data.CheckID = CheckID;
            if(convert.TryGetValue("Service" , out string Service))
                data.Service = Service;
            if(convert.TryGetValue("Url" , out string Url))
                data.Url = Url;
            if(convert.TryGetValue("Machine" , out string Machine))
                data.Machine = Machine;
            if(convert.TryGetValue("Start" , out DateTime Start))
                data.Start = Start;
            if(convert.TryGetValue("End" , out DateTime End))
                data.End = End;
            if(convert.TryGetValue("Level" , out int Level))
                data.Level = Level;
            if(convert.TryGetValue("Details" , out string Details))
                data.Details = Details;
        }

        #endregion
    }
}