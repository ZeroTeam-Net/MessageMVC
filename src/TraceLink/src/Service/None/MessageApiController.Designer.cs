/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/16 23:46:25*/
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
    partial class MessageApiController
    {

        #region 基本扩展

        /// <summary>
        ///     读取查询条件
        /// </summary>
        /// <param name="filter">筛选器</param>
        public override void GetQueryFilter(LambdaItem<MessageData> filter)
        {
            if (TryGet("_value_", out string value) && value != null)
            {
                var field = GetString("_field_");
                if (string.IsNullOrWhiteSpace(field) || field == "_any_")
                    filter.AddAnd(p => p.TraceId.Contains(value)
                                    || p.ApiName.Contains(value)
                                    || p.LocalId.Contains(value)
                                    || p.LocalApp.Contains(value)
                                    || p.LocalMachine.Contains(value)
                                    || p.CallId.Contains(value)
                                    || p.CallApp.Contains(value)
                                    || p.CallMachine.Contains(value)
                                    || p.Context.Contains(value)
                                    || p.Token.Contains(value)
                                    || p.Headers.Contains(value)
                                    || p.Message.Contains(value)
                                    || p.FlowStep.Contains(value));
                else this[field] = value;
            }
            if(TryGetIDs("Id" , out var Id))
            {
                if (Id.Count == 1)
                    filter.AddAnd(p => p.Id == Id[0]);
                else
                    filter.AddAnd(p => Id.Contains(p.Id));
            }
            if(TryGet("TraceId" , out string TraceId))
            {
                filter.AddAnd(p => p.TraceId.Contains(TraceId));
            }
            if(TryGet("Level" , out int Level))
            {
                filter.AddAnd(p => p.Level == Level);
            }
            if(TryGet("ApiName" , out string ApiName))
            {
                filter.AddAnd(p => p.ApiName.Contains(ApiName));
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
            if(TryGet("LocalId" , out string LocalId))
            {
                filter.AddAnd(p => p.LocalId.Contains(LocalId));
            }
            if(TryGet("LocalApp" , out string LocalApp))
            {
                filter.AddAnd(p => p.LocalApp.Contains(LocalApp));
            }
            if(TryGet("LocalMachine" , out string LocalMachine))
            {
                filter.AddAnd(p => p.LocalMachine.Contains(LocalMachine));
            }
            if(TryGet("CallId" , out string CallId))
            {
                filter.AddAnd(p => p.CallId.Contains(CallId));
            }
            if(TryGet("CallApp" , out string CallApp))
            {
                filter.AddAnd(p => p.CallApp.Contains(CallApp));
            }
            if(TryGet("CallMachine" , out string CallMachine))
            {
                filter.AddAnd(p => p.CallMachine.Contains(CallMachine));
            }
            if(TryGet("Context" , out string Context))
            {
                filter.AddAnd(p => p.Context.Contains(Context));
            }
            if(TryGet("Token" , out string Token))
            {
                filter.AddAnd(p => p.Token.Contains(Token));
            }
            if(TryGet("Headers" , out string Headers))
            {
                filter.AddAnd(p => p.Headers.Contains(Headers));
            }
            if(TryGet("Message" , out string Message))
            {
                filter.AddAnd(p => p.Message.Contains(Message));
            }
            if(TryGet("FlowStep" , out string FlowStep))
            {
                filter.AddAnd(p => p.FlowStep.Contains(FlowStep));
            }
        }

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected void DefaultReadFormData(MessageData data, FormConvert convert)
        {
            //普通字段
            if(convert.TryGetValue("TraceId" , out string TraceId))
                data.TraceId = TraceId;
            if(convert.TryGetValue("Level" , out int Level))
                data.Level = Level;
            if(convert.TryGetValue("ApiName" , out string ApiName))
                data.ApiName = ApiName;
            if(convert.TryGetValue("Start" , out DateTime Start))
                data.Start = Start;
            if(convert.TryGetValue("End" , out DateTime End))
                data.End = End;
            if(convert.TryGetValue("LocalId" , out string LocalId))
                data.LocalId = LocalId;
            if(convert.TryGetValue("LocalApp" , out string LocalApp))
                data.LocalApp = LocalApp;
            if(convert.TryGetValue("LocalMachine" , out string LocalMachine))
                data.LocalMachine = LocalMachine;
            if(convert.TryGetValue("CallId" , out string CallId))
                data.CallId = CallId;
            if(convert.TryGetValue("CallApp" , out string CallApp))
                data.CallApp = CallApp;
            if(convert.TryGetValue("CallMachine" , out string CallMachine))
                data.CallMachine = CallMachine;
            if(convert.TryGetValue("Context" , out string Context))
                data.Context = Context;
            if(convert.TryGetValue("Token" , out string Token))
                data.Token = Token;
            if(convert.TryGetValue("Headers" , out string Headers))
                data.Headers = Headers;
            if(convert.TryGetValue("Message" , out string Message))
                data.Message = Message;
            if(convert.TryGetValue("FlowStep" , out string FlowStep))
                data.FlowStep = FlowStep;
        }

        #endregion
    }
}