using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks.BusinessLogic;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    public static class EnumHelper
    {
        /// <summary>
        ///     任务状态类型名称转换
        /// </summary>
        public static string ToCaption(this PlanMessageState value)
        {
            return value switch
            {
                PlanMessageState.none => "无状态",
                PlanMessageState.queue => "排队",
                PlanMessageState.execute => "正常执行",
                PlanMessageState.retry => "重试执行",
                PlanMessageState.skip => "跳过",
                PlanMessageState.pause => "暂停",
                PlanMessageState.waiting => "远程等待",
                PlanMessageState.error => "错误关闭",
                PlanMessageState.close => "正常关闭",
                PlanMessageState.remove => "删除",
                _ => "任务状态类型(错误)",
            };
        }

        /// <summary>
        ///     执行状态类型名称转换
        /// </summary>
        public static string ToCaption(this MessageState value)
        {
            return value switch
            {
                MessageState.None => "未消费",
                MessageState.Cancel => "取消处理",
                MessageState.NonSupport => "不支持处理",
                MessageState.Accept => "已接受",
                MessageState.Unsend => "未发送",
                MessageState.Send => "已发送",
                MessageState.Recive => "已接收",
                MessageState.Processing => "正在处理",
                MessageState.AsyncQueue => "异步排队",
                MessageState.Success => "处理成功",
                MessageState.Failed => "处理失败",
                MessageState.Unhandled => "无处理结果",
                MessageState.Deny => "拒绝处理",
                MessageState.FormalError => "格式错误",
                MessageState.NetworkError => "网络错误",
                MessageState.BusinessError => "处理错误",
                MessageState.NoUs => "并非MessageMVC服务",
                MessageState.FrameworkError => "框架错误",
                _ => "执行状态类型(错误)",
            };
        }

        /// <summary>
        ///     计划类型名称转换
        /// </summary>
        public static string ToCaption(this PlanTimeType value)
        {
            return value switch
            {
                PlanTimeType.none => "无计划，立即发送",
                PlanTimeType.time => "在指定的时间发送",
                PlanTimeType.second => "秒间隔后发送",
                PlanTimeType.minute => "分钟间隔后发送",
                PlanTimeType.hour => "小时间隔后发送",
                PlanTimeType.day => "日间隔后发送",
                PlanTimeType.week => "每周几",
                PlanTimeType.month => "每月几号",
                _ => "计划类型(错误)",
            };
        }



    }
}