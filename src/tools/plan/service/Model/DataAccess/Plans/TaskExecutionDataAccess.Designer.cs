/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/26 22:35:36*/
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
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

using MySql.Data.MySqlClient;
using Agebull.EntityModel.MySql;

using Agebull.Common;

using Agebull.EntityModel.Common;
using Agebull.EntityModel.Interfaces;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks.DataAccess;

#endregion

namespace ZeroTeam.MessageMVC.PlanTasks.DataAccess
{
    /// <summary>
    /// 任务执行记录
    /// </summary>
    public partial class TaskExecutionDataAccess
    {
        /// <summary>
        /// 构造
        /// </summary>
        public TaskExecutionDataAccess()
        {
            Name = TaskExecutionData._DataStruct_.EntityName;
            Caption = TaskExecutionData._DataStruct_.EntityCaption;
            Description = TaskExecutionData._DataStruct_.EntityDescription;
        }
        

        #region 基本SQL语句

        /// <summary>
        /// 表的唯一标识
        /// </summary>
        public override int TableId => TaskExecutionData._DataStruct_.EntityIdentity;

        /// <summary>
        /// 读取表名
        /// </summary>
        protected sealed override string ReadTableName
        {
            get
            {
                return @"tb_task_execution";
            }
        }

        /// <summary>
        /// 写入表名
        /// </summary>
        protected sealed override string WriteTableName
        {
            get
            {
                return @"tb_task_execution";
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        protected sealed override string PrimaryKey => TaskExecutionData._DataStruct_.EntityPrimaryKey;

        /// <summary>
        /// 全表读取的SQL语句
        /// </summary>
        protected sealed override string FullLoadFields
        {
            get
            {
                return @"
    `id` AS `Id`,
    `task_id` AS `TaskId`,
    `plan_id` AS `PlanId`,
    `exec_num` AS `ExecNum`,
    `success_num` AS `SuccessNum`,
    `error_num` AS `ErrorNum`,
    `retry_num` AS `RetryNum`,
    `skip_num` AS `SkipNum`,
    `exec_state` AS `ExecState`,
    `plan_state` AS `PlanState`,
    `plan_time` AS `PlanTime`,
    `exec_start_time` AS `ExecStartTime`,
    `exec_end_time` AS `ExecEndTime`,
    `result` AS `Result`,
    `log` AS `Log`";
            }
        }

        

        /// <summary>
        /// 插入的SQL语句
        /// </summary>
        protected sealed override string InsertSqlCode
        {
            get
            {
                return $@"
INSERT INTO `{ContextWriteTable}`
(
    `task_id`,
    `plan_id`,
    `exec_num`,
    `success_num`,
    `error_num`,
    `retry_num`,
    `skip_num`,
    `exec_state`,
    `plan_state`,
    `plan_time`,
    `exec_start_time`,
    `exec_end_time`,
    `result`,
    `log`
)
VALUES
(
    ?TaskId,
    ?PlanId,
    ?ExecNum,
    ?SuccessNum,
    ?ErrorNum,
    ?RetryNum,
    ?SkipNum,
    ?ExecState,
    ?PlanState,
    ?PlanTime,
    ?ExecStartTime,
    ?ExecEndTime,
    ?Result,
    ?Log
);
SELECT @@IDENTITY;";
            }
        }

        /// <summary>
        /// 全部更新的SQL语句
        /// </summary>
        protected sealed override string UpdateSqlCode
        {
            get
            {
                return $@"
UPDATE `{ContextWriteTable}` SET
       `task_id` = ?TaskId,
       `plan_id` = ?PlanId,
       `exec_num` = ?ExecNum,
       `success_num` = ?SuccessNum,
       `error_num` = ?ErrorNum,
       `retry_num` = ?RetryNum,
       `skip_num` = ?SkipNum,
       `exec_state` = ?ExecState,
       `plan_state` = ?PlanState,
       `plan_time` = ?PlanTime,
       `exec_start_time` = ?ExecStartTime,
       `exec_end_time` = ?ExecEndTime,
       `result` = ?Result,
       `log` = ?Log
 WHERE `id` = ?Id;";
            }
        }

        /*// <summary>
        /// 取得仅更新的SQL语句
        /// </summary>
        public string GetModifiedSqlCode(TaskExecutionData data)
        {
            if (data.__EntityStatusNull || !data.__EntityStatus.IsModified)
                return ";";
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE `{ContextWriteTable}` SET");
            //任务标识
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_TaskId] > 0)
                sql.AppendLine("       `task_id` = ?TaskId");
            //计划标识
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_PlanId] > 0)
                sql.AppendLine("       `plan_id` = ?PlanId");
            //执行次数
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_ExecNum] > 0)
                sql.AppendLine("       `exec_num` = ?ExecNum");
            //返回次数
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_SuccessNum] > 0)
                sql.AppendLine("       `success_num` = ?SuccessNum");
            //错误次数
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_ErrorNum] > 0)
                sql.AppendLine("       `error_num` = ?ErrorNum");
            //重试次数
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_RetryNum] > 0)
                sql.AppendLine("       `retry_num` = ?RetryNum");
            //跳过次数
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_SkipNum] > 0)
                sql.AppendLine("       `skip_num` = ?SkipNum");
            //执行状态
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_ExecState] > 0)
                sql.AppendLine("       `exec_state` = ?ExecState");
            //计划状态
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_PlanState] > 0)
                sql.AppendLine("       `plan_state` = ?PlanState");
            //计划时间
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_PlanTime] > 0)
                sql.AppendLine("       `plan_time` = ?PlanTime");
            //开始时间
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_ExecStartTime] > 0)
                sql.AppendLine("       `exec_start_time` = ?ExecStartTime");
            //完成时间
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_ExecEndTime] > 0)
                sql.AppendLine("       `exec_end_time` = ?ExecEndTime");
            //返回内容
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_Result] > 0)
                sql.AppendLine("       `result` = ?Result");
            //执行日志
            if (data.__EntityStatus.ModifiedProperties[TaskExecutionData._DataStruct_.Real_Log] > 0)
                sql.AppendLine("       `log` = ?Log");
            sql.Append(" WHERE `id` = ?Id;");
            return sql.ToString();
        }*/

        #endregion


        #region 字段

        /// <summary>
        ///  所有字段
        /// </summary>
        static string[] _fields = new string[]{ "Id","TaskId","PlanId","ExecNum","SuccessNum","ErrorNum","RetryNum","SkipNum","ExecState","PlanState","PlanTime","ExecStartTime","ExecEndTime","Result","Log" };

        /// <summary>
        ///  所有字段
        /// </summary>
        public sealed override string[] Fields
        {
            get
            {
                return _fields;
            }
        }

        /// <summary>
        ///  字段字典
        /// </summary>
        public static Dictionary<string, string> fieldMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id" , "id" },
            { "TaskId" , "task_id" },
            { "task_id" , "task_id" },
            { "PlanId" , "plan_id" },
            { "plan_id" , "plan_id" },
            { "ExecNum" , "exec_num" },
            { "exec_num" , "exec_num" },
            { "SuccessNum" , "success_num" },
            { "success_num" , "success_num" },
            { "ErrorNum" , "error_num" },
            { "error_num" , "error_num" },
            { "RetryNum" , "retry_num" },
            { "retry_num" , "retry_num" },
            { "SkipNum" , "skip_num" },
            { "skip_num" , "skip_num" },
            { "ExecState" , "exec_state" },
            { "exec_state" , "exec_state" },
            { "PlanState" , "plan_state" },
            { "plan_state" , "plan_state" },
            { "PlanTime" , "plan_time" },
            { "plan_time" , "plan_time" },
            { "ExecStartTime" , "exec_start_time" },
            { "exec_start_time" , "exec_start_time" },
            { "ExecEndTime" , "exec_end_time" },
            { "exec_end_time" , "exec_end_time" },
            { "Result" , "result" },
            { "Log" , "log" }
        };

        /// <summary>
        ///  字段字典
        /// </summary>
        public sealed override Dictionary<string, string> FieldMap
        {
            get { return fieldMap ; }
        }
        #endregion
        #region 方法实现


        /// <summary>
        /// 载入数据
        /// </summary>
        /// <param name="reader">数据读取器</param>
        /// <param name="entity">读取数据的实体</param>
        protected sealed override void LoadEntity(MySqlDataReader reader,TaskExecutionData entity)
        {
            if (!reader.IsDBNull(0))
                entity._id = (long)reader.GetInt64(0);
            if (!reader.IsDBNull(1))
                entity._taskId = (long)reader.GetInt64(1);
            if (!reader.IsDBNull(2))
                entity._planId = reader.GetString(2);
            if (!reader.IsDBNull(3))
                entity._execNum = (int)reader.GetInt32(3);
            if (!reader.IsDBNull(4))
                entity._successNum = (int)reader.GetInt32(4);
            if (!reader.IsDBNull(5))
                entity._errorNum = (int)reader.GetInt32(5);
            if (!reader.IsDBNull(6))
                entity._retryNum = (int)reader.GetInt32(6);
            if (!reader.IsDBNull(7))
                entity._skipNum = (int)reader.GetInt32(7);
            if (!reader.IsDBNull(8))
                entity._execState = (MessageState)reader.GetInt32(8);
            if (!reader.IsDBNull(9))
                entity._planState = (PlanMessageState)reader.GetInt32(9);
            if (!reader.IsDBNull(10))
                entity._planTime = (long)reader.GetInt64(10);
            if (!reader.IsDBNull(11))
                entity._execStartTime = (long)reader.GetInt64(11);
            if (!reader.IsDBNull(12))
                entity._execEndTime = (long)reader.GetInt64(12);
            if (!reader.IsDBNull(13))
                entity._result = reader.GetString(13).ToString();
            if (!reader.IsDBNull(14))
                entity._log = reader.GetString(14).ToString();
        }

        /// <summary>
        /// 得到字段的DbType类型
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <returns>参数</returns>
        protected sealed override MySqlDbType GetDbType(string field)
        {
            switch (field)
            {
                case "id":
                case "Id":
                    return MySqlDbType.Int64;
                case "task_id":
                case "TaskId":
                    return MySqlDbType.Int64;
                case "plan_id":
                case "PlanId":
                    return MySqlDbType.VarString;
                case "exec_num":
                case "ExecNum":
                    return MySqlDbType.Int32;
                case "success_num":
                case "SuccessNum":
                    return MySqlDbType.Int32;
                case "error_num":
                case "ErrorNum":
                    return MySqlDbType.Int32;
                case "retry_num":
                case "RetryNum":
                    return MySqlDbType.Int32;
                case "skip_num":
                case "SkipNum":
                    return MySqlDbType.Int32;
                case "exec_state":
                case "ExecState":
                    return MySqlDbType.Int32;
                case "plan_state":
                case "PlanState":
                    return MySqlDbType.Int32;
                case "plan_time":
                case "PlanTime":
                    return MySqlDbType.Int64;
                case "exec_start_time":
                case "ExecStartTime":
                    return MySqlDbType.Int64;
                case "exec_end_time":
                case "ExecEndTime":
                    return MySqlDbType.Int64;
                case "result":
                case "Result":
                    return MySqlDbType.Text;
                case "log":
                case "Log":
                    return MySqlDbType.Text;
            }
            return MySqlDbType.VarChar;
        }


        /// <summary>
        /// 设置插入数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        /// <returns>返回真说明要取主键</returns>
        public void CreateFullSqlParameter(TaskExecutionData entity, MySqlCommand cmd)
        {
            //02:主键(Id)
            cmd.Parameters.Add(new MySqlParameter("Id",MySqlDbType.Int64){ Value = entity.Id});
            //03:任务标识(TaskId)
            cmd.Parameters.Add(new MySqlParameter("TaskId",MySqlDbType.Int64){ Value = entity.TaskId});
            //04:计划标识(PlanId)
            var isNull = string.IsNullOrWhiteSpace(entity.PlanId);
            var parameter = new MySqlParameter("PlanId",MySqlDbType.VarString , isNull ? 10 : (entity.PlanId).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.PlanId;
            cmd.Parameters.Add(parameter);
            //05:执行次数(ExecNum)
            cmd.Parameters.Add(new MySqlParameter("ExecNum",MySqlDbType.Int32){ Value = entity.ExecNum});
            //06:返回次数(SuccessNum)
            cmd.Parameters.Add(new MySqlParameter("SuccessNum",MySqlDbType.Int32){ Value = entity.SuccessNum});
            //07:错误次数(ErrorNum)
            cmd.Parameters.Add(new MySqlParameter("ErrorNum",MySqlDbType.Int32){ Value = entity.ErrorNum});
            //08:重试次数(RetryNum)
            cmd.Parameters.Add(new MySqlParameter("RetryNum",MySqlDbType.Int32){ Value = entity.RetryNum});
            //09:跳过次数(SkipNum)
            cmd.Parameters.Add(new MySqlParameter("SkipNum",MySqlDbType.Int32){ Value = entity.SkipNum});
            //10:执行状态(ExecState)
            cmd.Parameters.Add(new MySqlParameter("ExecState",MySqlDbType.Int32){ Value = (int)entity.ExecState});
            //11:计划状态(PlanState)
            cmd.Parameters.Add(new MySqlParameter("PlanState",MySqlDbType.Int32){ Value = (int)entity.PlanState});
            //12:计划时间(PlanTime)
            cmd.Parameters.Add(new MySqlParameter("PlanTime",MySqlDbType.Int64){ Value = entity.PlanTime});
            //13:开始时间(ExecStartTime)
            cmd.Parameters.Add(new MySqlParameter("ExecStartTime",MySqlDbType.Int64){ Value = entity.ExecStartTime});
            //14:完成时间(ExecEndTime)
            cmd.Parameters.Add(new MySqlParameter("ExecEndTime",MySqlDbType.Int64){ Value = entity.ExecEndTime});
            //15:返回内容(Result)
            isNull = string.IsNullOrWhiteSpace(entity.Result);
            parameter = new MySqlParameter("Result",MySqlDbType.Text , isNull ? 10 : (entity.Result).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Result;
            cmd.Parameters.Add(parameter);
            //16:执行日志(Log)
            isNull = string.IsNullOrWhiteSpace(entity.Log);
            parameter = new MySqlParameter("Log",MySqlDbType.Text , isNull ? 10 : (entity.Log).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Log;
            cmd.Parameters.Add(parameter);
        }


        /// <summary>
        /// 设置更新数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        protected sealed override void SetUpdateCommand(TaskExecutionData entity, MySqlCommand cmd)
        {
            //cmd.CommandText = UpdateSqlCode;
            CreateFullSqlParameter(entity,cmd);
        }


        /// <summary>
        /// 设置插入数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        /// <returns>返回真说明要取主键</returns>
        protected sealed override bool SetInsertCommand(TaskExecutionData entity, MySqlCommand cmd)
        {
            cmd.CommandText = InsertSqlCode;
            CreateFullSqlParameter(entity, cmd);
            return true;
        }

        #endregion


    }

    /*
    partial class PlanTaskDatabase
    {


        /// <summary>
        /// 任务执行记录的结构语句
        /// </summary>
        private TableSql _TaskExecutionSql = new TableSql
        {
            TableName = "tb_task_execution",
            PimaryKey = "Id"
        };


        /// <summary>
        /// 任务执行记录数据访问对象
        /// </summary>
        private TaskExecutionDataAccess _taskExecutions;

        /// <summary>
        /// 任务执行记录数据访问对象
        /// </summary>
        public TaskExecutionDataAccess TaskExecutions
        {
            get
            {
                return this._taskExecutions ?? ( this._taskExecutions = new TaskExecutionDataAccess{ DataBase = this});
            }
        }


        /// <summary>
        /// 任务执行记录(tb_task_execution):任务执行记录
        /// </summary>
        public const int Table_TaskExecution = 0x0;
    }*/
}