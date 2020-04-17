/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 15:42:48*/
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

using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.DataAccess
{
    /// <summary>
    /// 健康检查结果
    /// </summary>
    public partial class HealthCheckDataAccess
    {
        /// <summary>
        /// 构造
        /// </summary>
        public HealthCheckDataAccess()
        {
            Name = HealthCheckData._DataStruct_.EntityName;
            Caption = HealthCheckData._DataStruct_.EntityCaption;
            Description = HealthCheckData._DataStruct_.EntityDescription;
        }
        

        #region 基本SQL语句

        /// <summary>
        /// 表的唯一标识
        /// </summary>
        public override int TableId => HealthCheckData._DataStruct_.EntityIdentity;

        /// <summary>
        /// 读取表名
        /// </summary>
        protected sealed override string ReadTableName
        {
            get
            {
                return @"tb_health_check";
            }
        }

        /// <summary>
        /// 写入表名
        /// </summary>
        protected sealed override string WriteTableName
        {
            get
            {
                return @"tb_health_check";
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        protected sealed override string PrimaryKey => HealthCheckData._DataStruct_.EntityPrimaryKey;

        /// <summary>
        /// 全表读取的SQL语句
        /// </summary>
        protected sealed override string FullLoadFields
        {
            get
            {
                return @"
    `id` AS `Id`,
    `check_id` AS `CheckID`,
    `service` AS `Service`,
    `url` AS `Url`,
    `machine` AS `Machine`,
    `start` AS `Start`,
    `end` AS `End`,
    `level` AS `Level`,
    `details` AS `Details`";
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
    `check_id`,
    `service`,
    `url`,
    `machine`,
    `start`,
    `end`,
    `level`,
    `details`
)
VALUES
(
    ?CheckID,
    ?Service,
    ?Url,
    ?Machine,
    ?Start,
    ?End,
    ?Level,
    ?Details
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
       `check_id` = ?CheckID,
       `service` = ?Service,
       `url` = ?Url,
       `machine` = ?Machine,
       `start` = ?Start,
       `end` = ?End,
       `level` = ?Level,
       `details` = ?Details
 WHERE `id` = ?Id;";
            }
        }

        /*// <summary>
        /// 取得仅更新的SQL语句
        /// </summary>
        public string GetModifiedSqlCode(HealthCheckData data)
        {
            if (data.__EntityStatusNull || !data.__EntityStatus.IsModified)
                return ";";
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE `{ContextWriteTable}` SET");
            //检查标识
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_CheckID] > 0)
                sql.AppendLine("       `check_id` = ?CheckID");
            //服务名称
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_Service] > 0)
                sql.AppendLine("       `service` = ?Service");
            //服务地址
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_Url] > 0)
                sql.AppendLine("       `url` = ?Url");
            //机器名称
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_Machine] > 0)
                sql.AppendLine("       `machine` = ?Machine");
            //开始时间
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_Start] > 0)
                sql.AppendLine("       `start` = ?Start");
            //结束时间
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_End] > 0)
                sql.AppendLine("       `end` = ?End");
            //健康等级
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_Level] > 0)
                sql.AppendLine("       `level` = ?Level");
            //检查详情
            if (data.__EntityStatus.ModifiedProperties[HealthCheckData._DataStruct_.Real_Details] > 0)
                sql.AppendLine("       `details` = ?Details");
            sql.Append(" WHERE `id` = ?Id;");
            return sql.ToString();
        }*/

        #endregion


        #region 字段

        /// <summary>
        ///  所有字段
        /// </summary>
        static string[] _fields = new string[]{ "Id","CheckID","Service","Url","Machine","Start","End","Level","Details" };

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
            { "CheckID" , "check_id" },
            { "check_id" , "check_id" },
            { "Service" , "service" },
            { "Url" , "url" },
            { "Machine" , "machine" },
            { "Start" , "start" },
            { "End" , "end" },
            { "Level" , "level" },
            { "Details" , "details" }
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
        protected sealed override void LoadEntity(MySqlDataReader reader,HealthCheckData entity)
        {
            if (!reader.IsDBNull(0))
                entity._id = (long)reader.GetInt64(0);
            if (!reader.IsDBNull(1))
                entity._checkID = (int)reader.GetInt32(1);
            if (!reader.IsDBNull(2))
                entity._service = reader.GetString(2);
            if (!reader.IsDBNull(3))
                entity._url = reader.GetString(3);
            if (!reader.IsDBNull(4))
                entity._machine = reader.GetString(4);
            if (!reader.IsDBNull(5))
                try{entity._start = reader.GetMySqlDateTime(5).Value;}catch{}
            if (!reader.IsDBNull(6))
                try{entity._end = reader.GetMySqlDateTime(6).Value;}catch{}
            if (!reader.IsDBNull(7))
                entity._level = (int)reader.GetInt32(7);
            if (!reader.IsDBNull(8))
                entity._details = reader.GetString(8).ToString();
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
                case "check_id":
                case "CheckID":
                    return MySqlDbType.Int32;
                case "service":
                case "Service":
                    return MySqlDbType.VarString;
                case "url":
                case "Url":
                    return MySqlDbType.VarString;
                case "machine":
                case "Machine":
                    return MySqlDbType.VarString;
                case "start":
                case "Start":
                    return MySqlDbType.DateTime;
                case "end":
                case "End":
                    return MySqlDbType.DateTime;
                case "level":
                case "Level":
                    return MySqlDbType.Int32;
                case "details":
                case "Details":
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
        public void CreateFullSqlParameter(HealthCheckData entity, MySqlCommand cmd)
        {
            //02:主键(Id)
            cmd.Parameters.Add(new MySqlParameter("Id",MySqlDbType.Int64){ Value = entity.Id});
            //03:检查标识(CheckID)
            cmd.Parameters.Add(new MySqlParameter("CheckID",MySqlDbType.Int32){ Value = entity.CheckID});
            //04:服务名称(Service)
            var isNull = string.IsNullOrWhiteSpace(entity.Service);
            var parameter = new MySqlParameter("Service",MySqlDbType.VarString , isNull ? 10 : (entity.Service).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Service;
            cmd.Parameters.Add(parameter);
            //05:服务地址(Url)
            isNull = string.IsNullOrWhiteSpace(entity.Url);
            parameter = new MySqlParameter("Url",MySqlDbType.VarString , isNull ? 10 : (entity.Url).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Url;
            cmd.Parameters.Add(parameter);
            //06:机器名称(Machine)
            isNull = string.IsNullOrWhiteSpace(entity.Machine);
            parameter = new MySqlParameter("Machine",MySqlDbType.VarString , isNull ? 10 : (entity.Machine).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Machine;
            cmd.Parameters.Add(parameter);
            //07:开始时间(Start)
            isNull = entity.Start.Year < 1900;
            parameter = new MySqlParameter("Start",MySqlDbType.DateTime);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Start;
            cmd.Parameters.Add(parameter);
            //08:结束时间(End)
            isNull = entity.End.Year < 1900;
            parameter = new MySqlParameter("End",MySqlDbType.DateTime);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.End;
            cmd.Parameters.Add(parameter);
            //09:健康等级(Level)
            cmd.Parameters.Add(new MySqlParameter("Level",MySqlDbType.Int32){ Value = entity.Level});
            //10:检查详情(Details)
            isNull = string.IsNullOrWhiteSpace(entity.Details);
            parameter = new MySqlParameter("Details",MySqlDbType.Text , isNull ? 10 : (entity.Details).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Details;
            cmd.Parameters.Add(parameter);
        }


        /// <summary>
        /// 设置更新数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        protected sealed override void SetUpdateCommand(HealthCheckData entity, MySqlCommand cmd)
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
        protected sealed override bool SetInsertCommand(HealthCheckData entity, MySqlCommand cmd)
        {
            cmd.CommandText = InsertSqlCode;
            CreateFullSqlParameter(entity, cmd);
            return true;
        }

        #endregion


    }

    /*
    partial class TraceLinkDatabase
    {


        /// <summary>
        /// 健康检查结果的结构语句
        /// </summary>
        private TableSql _HealthCheckSql = new TableSql
        {
            TableName = "tb_health_check",
            PimaryKey = "Id"
        };


        /// <summary>
        /// 健康检查结果数据访问对象
        /// </summary>
        private HealthCheckDataAccess _healthChecks;

        /// <summary>
        /// 健康检查结果数据访问对象
        /// </summary>
        public HealthCheckDataAccess HealthChecks
        {
            get
            {
                return this._healthChecks ?? ( this._healthChecks = new HealthCheckDataAccess{ DataBase = this});
            }
        }


        /// <summary>
        /// 健康检查(tb_health_check):健康检查
        /// </summary>
        public const int Table_HealthCheck = 0x0;
    }*/
}