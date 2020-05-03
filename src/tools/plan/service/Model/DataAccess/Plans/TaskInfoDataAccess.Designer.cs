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
    /// 任务信息
    /// </summary>
    public partial class TaskInfoDataAccess
    {
        /// <summary>
        /// 构造
        /// </summary>
        public TaskInfoDataAccess()
        {
            Name = TaskInfoData._DataStruct_.EntityName;
            Caption = TaskInfoData._DataStruct_.EntityCaption;
            Description = TaskInfoData._DataStruct_.EntityDescription;
        }
        

        #region 基本SQL语句

        /// <summary>
        /// 表的唯一标识
        /// </summary>
        public override int TableId => TaskInfoData._DataStruct_.EntityIdentity;

        /// <summary>
        /// 读取表名
        /// </summary>
        protected sealed override string ReadTableName
        {
            get
            {
                return @"tb_task_info";
            }
        }

        /// <summary>
        /// 写入表名
        /// </summary>
        protected sealed override string WriteTableName
        {
            get
            {
                return @"tb_task_info";
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        protected sealed override string PrimaryKey => TaskInfoData._DataStruct_.EntityPrimaryKey;

        /// <summary>
        /// 全表读取的SQL语句
        /// </summary>
        protected sealed override string FullLoadFields
        {
            get
            {
                return @"
    `id` AS `Id`,
    `plan_id` AS `PlanId`,
    `description` AS `Description`,
    `plan_type` AS `PlanType`,
    `plan_value` AS `PlanValue`,
    `retry_set` AS `RetrySet`,
    `skip_set` AS `SkipSet`,
    `plan_repet` AS `PlanRepet`,
    `queue_pass_by` AS `QueuePassBy`,
    `add_time` AS `AddTime`,
    `plan_time` AS `PlanTime`,
    `is_async` AS `IsAsync`,
    `check_result_time` AS `CheckResultTime`,
    `message` AS `Message`,
    `state` AS `State`,
    `close_time` AS `CloseTime`";
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
    `plan_id`,
    `description`,
    `plan_type`,
    `plan_value`,
    `retry_set`,
    `skip_set`,
    `plan_repet`,
    `queue_pass_by`,
    `add_time`,
    `plan_time`,
    `is_async`,
    `check_result_time`,
    `message`,
    `state`,
    `close_time`
)
VALUES
(
    ?PlanId,
    ?Description,
    ?PlanType,
    ?PlanValue,
    ?RetrySet,
    ?SkipSet,
    ?PlanRepet,
    ?QueuePassBy,
    ?AddTime,
    ?PlanTime,
    ?IsAsync,
    ?CheckResultTime,
    ?Message,
    ?State,
    ?CloseTime
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
       `plan_id` = ?PlanId,
       `description` = ?Description,
       `plan_type` = ?PlanType,
       `plan_value` = ?PlanValue,
       `retry_set` = ?RetrySet,
       `skip_set` = ?SkipSet,
       `plan_repet` = ?PlanRepet,
       `queue_pass_by` = ?QueuePassBy,
       `add_time` = ?AddTime,
       `plan_time` = ?PlanTime,
       `is_async` = ?IsAsync,
       `check_result_time` = ?CheckResultTime,
       `message` = ?Message,
       `state` = ?State,
       `close_time` = ?CloseTime
 WHERE `id` = ?Id;";
            }
        }

        /*// <summary>
        /// 取得仅更新的SQL语句
        /// </summary>
        public string GetModifiedSqlCode(TaskInfoData data)
        {
            if (data.__EntityStatusNull || !data.__EntityStatus.IsModified)
                return ";";
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE `{ContextWriteTable}` SET");
            //消息标识
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_PlanId] > 0)
                sql.AppendLine("       `plan_id` = ?PlanId");
            //计划说明
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_Description] > 0)
                sql.AppendLine("       `description` = ?Description");
            //计划类型
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_PlanType] > 0)
                sql.AppendLine("       `plan_type` = ?PlanType");
            //计划值
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_PlanValue] > 0)
                sql.AppendLine("       `plan_value` = ?PlanValue");
            //重试次数
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_RetrySet] > 0)
                sql.AppendLine("       `retry_set` = ?RetrySet");
            //跳过设置次数
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_SkipSet] > 0)
                sql.AppendLine("       `skip_set` = ?SkipSet");
            //重复次数
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_PlanRepet] > 0)
                sql.AppendLine("       `plan_repet` = ?PlanRepet");
            //跳过无效时间
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_QueuePassBy] > 0)
                sql.AppendLine("       `queue_pass_by` = ?QueuePassBy");
            //加入时间
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_AddTime] > 0)
                sql.AppendLine("       `add_time` = ?AddTime");
            //计划时间
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_PlanTime] > 0)
                sql.AppendLine("       `plan_time` = ?PlanTime");
            //异步调用
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_IsAsync] > 0)
                sql.AppendLine("       `is_async` = ?IsAsync");
            //异步结果检查时长
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_CheckResultTime] > 0)
                sql.AppendLine("       `check_result_time` = ?CheckResultTime");
            //消息内容
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_Message] > 0)
                sql.AppendLine("       `message` = ?Message");
            //任务状态
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_State] > 0)
                sql.AppendLine("       `state` = ?State");
            //关闭时间
            if (data.__EntityStatus.ModifiedProperties[TaskInfoData._DataStruct_.Real_CloseTime] > 0)
                sql.AppendLine("       `close_time` = ?CloseTime");
            sql.Append(" WHERE `id` = ?Id;");
            return sql.ToString();
        }*/

        #endregion


        #region 字段

        /// <summary>
        ///  所有字段
        /// </summary>
        static string[] _fields = new string[]{ "Id","PlanId","Description","PlanType","PlanValue","RetrySet","SkipSet","PlanRepet","QueuePassBy","AddTime","PlanTime","IsAsync","CheckResultTime","Message","State","CloseTime" };

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
            { "PlanId" , "plan_id" },
            { "plan_id" , "plan_id" },
            { "Description" , "description" },
            { "PlanType" , "plan_type" },
            { "plan_type" , "plan_type" },
            { "PlanValue" , "plan_value" },
            { "plan_value" , "plan_value" },
            { "RetrySet" , "retry_set" },
            { "retry_set" , "retry_set" },
            { "SkipSet" , "skip_set" },
            { "skip_set" , "skip_set" },
            { "PlanRepet" , "plan_repet" },
            { "plan_repet" , "plan_repet" },
            { "QueuePassBy" , "queue_pass_by" },
            { "queue_pass_by" , "queue_pass_by" },
            { "AddTime" , "add_time" },
            { "add_time" , "add_time" },
            { "PlanTime" , "plan_time" },
            { "plan_time" , "plan_time" },
            { "IsAsync" , "is_async" },
            { "is_async" , "is_async" },
            { "CheckResultTime" , "check_result_time" },
            { "check_result_time" , "check_result_time" },
            { "Message" , "message" },
            { "State" , "state" },
            { "CloseTime" , "close_time" },
            { "close_time" , "close_time" }
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
        protected sealed override void LoadEntity(MySqlDataReader reader,TaskInfoData entity)
        {
            if (!reader.IsDBNull(0))
                entity._id = (long)reader.GetInt64(0);
            if (!reader.IsDBNull(1))
                entity._planId = reader.GetString(1);
            if (!reader.IsDBNull(2))
                entity._description = reader.GetString(2);
            if (!reader.IsDBNull(3))
                entity._planType = (PlanTimeType)reader.GetInt32(3);
            if (!reader.IsDBNull(4))
                entity._planValue = (int)reader.GetInt32(4);
            if (!reader.IsDBNull(5))
                entity._retrySet = (int)reader.GetInt32(5);
            if (!reader.IsDBNull(6))
                entity._skipSet = (int)reader.GetInt32(6);
            if (!reader.IsDBNull(7))
                entity._planRepet = (int)reader.GetInt32(7);
            if (!reader.IsDBNull(8))
                entity._queuePassBy = (bool)reader.GetBoolean(8);
            if (!reader.IsDBNull(9))
                entity._addTime = (long)reader.GetInt64(9);
            if (!reader.IsDBNull(10))
                entity._planTime = (long)reader.GetInt64(10);
            if (!reader.IsDBNull(11))
                entity._isAsync = (bool)reader.GetBoolean(11);
            if (!reader.IsDBNull(12))
                entity._checkResultTime = (int)reader.GetInt32(12);
            if (!reader.IsDBNull(13))
                entity._message = reader.GetString(13).ToString();
            if (!reader.IsDBNull(14))
                entity._state = (PlanMessageState)reader.GetInt32(14);
            if (!reader.IsDBNull(15))
                try{entity._closeTime = reader.GetMySqlDateTime(15).Value;}catch{}
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
                case "plan_id":
                case "PlanId":
                    return MySqlDbType.VarString;
                case "description":
                case "Description":
                    return MySqlDbType.VarString;
                case "plan_type":
                case "PlanType":
                    return MySqlDbType.Int32;
                case "plan_value":
                case "PlanValue":
                    return MySqlDbType.Int32;
                case "retry_set":
                case "RetrySet":
                    return MySqlDbType.Int32;
                case "skip_set":
                case "SkipSet":
                    return MySqlDbType.Int32;
                case "plan_repet":
                case "PlanRepet":
                    return MySqlDbType.Int32;
                case "queue_pass_by":
                case "QueuePassBy":
                    return MySqlDbType.Byte;
                case "add_time":
                case "AddTime":
                    return MySqlDbType.Int64;
                case "plan_time":
                case "PlanTime":
                    return MySqlDbType.Int64;
                case "is_async":
                case "IsAsync":
                    return MySqlDbType.Byte;
                case "check_result_time":
                case "CheckResultTime":
                    return MySqlDbType.Int32;
                case "message":
                case "Message":
                    return MySqlDbType.Text;
                case "state":
                case "State":
                    return MySqlDbType.Int32;
                case "close_time":
                case "CloseTime":
                    return MySqlDbType.DateTime;
            }
            return MySqlDbType.VarChar;
        }


        /// <summary>
        /// 设置插入数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        /// <returns>返回真说明要取主键</returns>
        public void CreateFullSqlParameter(TaskInfoData entity, MySqlCommand cmd)
        {
            //02:主键(Id)
            cmd.Parameters.Add(new MySqlParameter("Id",MySqlDbType.Int64){ Value = entity.Id});
            //03:消息标识(PlanId)
            var isNull = string.IsNullOrWhiteSpace(entity.PlanId);
            var parameter = new MySqlParameter("PlanId",MySqlDbType.VarString , isNull ? 10 : (entity.PlanId).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.PlanId;
            cmd.Parameters.Add(parameter);
            //04:计划说明(Description)
            isNull = string.IsNullOrWhiteSpace(entity.Description);
            parameter = new MySqlParameter("Description",MySqlDbType.VarString , isNull ? 10 : (entity.Description).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Description;
            cmd.Parameters.Add(parameter);
            //05:计划类型(PlanType)
            cmd.Parameters.Add(new MySqlParameter("PlanType",MySqlDbType.Int32){ Value = (int)entity.PlanType});
            //06:计划值(PlanValue)
            cmd.Parameters.Add(new MySqlParameter("PlanValue",MySqlDbType.Int32){ Value = entity.PlanValue});
            //07:重试次数(RetrySet)
            cmd.Parameters.Add(new MySqlParameter("RetrySet",MySqlDbType.Int32){ Value = entity.RetrySet});
            //08:跳过设置次数(SkipSet)
            cmd.Parameters.Add(new MySqlParameter("SkipSet",MySqlDbType.Int32){ Value = entity.SkipSet});
            //09:重复次数(PlanRepet)
            cmd.Parameters.Add(new MySqlParameter("PlanRepet",MySqlDbType.Int32){ Value = entity.PlanRepet});
            //10:跳过无效时间(QueuePassBy)
            cmd.Parameters.Add(new MySqlParameter("QueuePassBy",MySqlDbType.Byte) { Value = entity.QueuePassBy ? (byte)1 : (byte)0 });
            //11:加入时间(AddTime)
            cmd.Parameters.Add(new MySqlParameter("AddTime",MySqlDbType.Int64){ Value = entity.AddTime});
            //12:计划时间(PlanTime)
            cmd.Parameters.Add(new MySqlParameter("PlanTime",MySqlDbType.Int64){ Value = entity.PlanTime});
            //13:异步调用(IsAsync)
            cmd.Parameters.Add(new MySqlParameter("IsAsync",MySqlDbType.Byte) { Value = entity.IsAsync ? (byte)1 : (byte)0 });
            //14:异步结果检查时长(CheckResultTime)
            cmd.Parameters.Add(new MySqlParameter("CheckResultTime",MySqlDbType.Int32){ Value = entity.CheckResultTime});
            //15:消息内容(Message)
            isNull = string.IsNullOrWhiteSpace(entity.Message);
            parameter = new MySqlParameter("Message",MySqlDbType.Text , isNull ? 10 : (entity.Message).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Message;
            cmd.Parameters.Add(parameter);
            //16:任务状态(State)
            cmd.Parameters.Add(new MySqlParameter("State",MySqlDbType.Int32){ Value = (int)entity.State});
            //17:关闭时间(CloseTime)
            isNull = entity.CloseTime.Year < 1900;
            parameter = new MySqlParameter("CloseTime",MySqlDbType.DateTime);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.CloseTime;
            cmd.Parameters.Add(parameter);
        }


        /// <summary>
        /// 设置更新数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        protected sealed override void SetUpdateCommand(TaskInfoData entity, MySqlCommand cmd)
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
        protected sealed override bool SetInsertCommand(TaskInfoData entity, MySqlCommand cmd)
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
        /// 任务信息的结构语句
        /// </summary>
        private TableSql _TaskInfoSql = new TableSql
        {
            TableName = "tb_task_info",
            PimaryKey = "Id"
        };


        /// <summary>
        /// 任务信息数据访问对象
        /// </summary>
        private TaskInfoDataAccess _taskInfoes;

        /// <summary>
        /// 任务信息数据访问对象
        /// </summary>
        public TaskInfoDataAccess TaskInfoes
        {
            get
            {
                return this._taskInfoes ?? ( this._taskInfoes = new TaskInfoDataAccess{ DataBase = this});
            }
        }


        /// <summary>
        /// 任务信息(tb_task_info):任务信息
        /// </summary>
        public const int Table_TaskInfo = 0x0;
    }*/
}