/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/16 23:46:23*/
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
    /// 消息存储
    /// </summary>
    public partial class MessageDataAccess
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MessageDataAccess()
        {
            Name = MessageData._DataStruct_.EntityName;
            Caption = MessageData._DataStruct_.EntityCaption;
            Description = MessageData._DataStruct_.EntityDescription;
        }
        

        #region 基本SQL语句

        /// <summary>
        /// 表的唯一标识
        /// </summary>
        public override int TableId => MessageData._DataStruct_.EntityIdentity;

        /// <summary>
        /// 读取表名
        /// </summary>
        protected sealed override string ReadTableName
        {
            get
            {
                return @"tb_trace_info";
            }
        }

        /// <summary>
        /// 写入表名
        /// </summary>
        protected sealed override string WriteTableName
        {
            get
            {
                return @"tb_trace_info";
            }
        }

        /// <summary>
        /// 主键
        /// </summary>
        protected sealed override string PrimaryKey => MessageData._DataStruct_.EntityPrimaryKey;

        /// <summary>
        /// 全表读取的SQL语句
        /// </summary>
        protected sealed override string FullLoadFields
        {
            get
            {
                return @"
    `id` AS `Id`,
    `trace_id` AS `TraceId`,
    `level` AS `Level`,
    `api_name` AS `ApiName`,
    `start` AS `Start`,
    `end` AS `End`,
    `local_id` AS `LocalId`,
    `local_app` AS `LocalApp`,
    `local_machine` AS `LocalMachine`,
    `call_id` AS `CallId`,
    `call_app` AS `CallApp`,
    `call_machine` AS `CallMachine`,
    `context` AS `Context`,
    `token` AS `Token`,
    `headers` AS `Headers`,
    `message` AS `Message`,
    `flow_step` AS `FlowStep`";
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
    `trace_id`,
    `level`,
    `api_name`,
    `start`,
    `end`,
    `local_id`,
    `local_app`,
    `local_machine`,
    `call_id`,
    `call_app`,
    `call_machine`,
    `context`,
    `token`,
    `headers`,
    `message`,
    `flow_step`
)
VALUES
(
    ?TraceId,
    ?Level,
    ?ApiName,
    ?Start,
    ?End,
    ?LocalId,
    ?LocalApp,
    ?LocalMachine,
    ?CallId,
    ?CallApp,
    ?CallMachine,
    ?Context,
    ?Token,
    ?Headers,
    ?Message,
    ?FlowStep
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
       `trace_id` = ?TraceId,
       `level` = ?Level,
       `api_name` = ?ApiName,
       `start` = ?Start,
       `end` = ?End,
       `local_id` = ?LocalId,
       `local_app` = ?LocalApp,
       `local_machine` = ?LocalMachine,
       `call_id` = ?CallId,
       `call_app` = ?CallApp,
       `call_machine` = ?CallMachine,
       `context` = ?Context,
       `token` = ?Token,
       `headers` = ?Headers,
       `message` = ?Message,
       `flow_step` = ?FlowStep
 WHERE `id` = ?Id;";
            }
        }

        /*// <summary>
        /// 取得仅更新的SQL语句
        /// </summary>
        public string GetModifiedSqlCode(MessageData data)
        {
            if (data.__EntityStatusNull || !data.__EntityStatus.IsModified)
                return ";";
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("UPDATE `{ContextWriteTable}` SET");
            //全局请求标识
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_TraceId] > 0)
                sql.AppendLine("       `trace_id` = ?TraceId");
            //调用层级
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_Level] > 0)
                sql.AppendLine("       `level` = ?Level");
            //接口名称
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_ApiName] > 0)
                sql.AppendLine("       `api_name` = ?ApiName");
            //开始时间
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_Start] > 0)
                sql.AppendLine("       `start` = ?Start");
            //结束时间
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_End] > 0)
                sql.AppendLine("       `end` = ?End");
            //本地的全局标识
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_LocalId] > 0)
                sql.AppendLine("       `local_id` = ?LocalId");
            //本地的应用
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_LocalApp] > 0)
                sql.AppendLine("       `local_app` = ?LocalApp");
            //本地的机器
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_LocalMachine] > 0)
                sql.AppendLine("       `local_machine` = ?LocalMachine");
            //请求方跟踪标识
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_CallId] > 0)
                sql.AppendLine("       `call_id` = ?CallId");
            //请求应用
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_CallApp] > 0)
                sql.AppendLine("       `call_app` = ?CallApp");
            //请求机器
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_CallMachine] > 0)
                sql.AppendLine("       `call_machine` = ?CallMachine");
            //上下文信息
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_Context] > 0)
                sql.AppendLine("       `context` = ?Context");
            //身份令牌
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_Token] > 0)
                sql.AppendLine("       `token` = ?Token");
            //请求头信息
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_Headers] > 0)
                sql.AppendLine("       `headers` = ?Headers");
            //消息序列化文本
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_Message] > 0)
                sql.AppendLine("       `message` = ?Message");
            //流程步骤记录
            if (data.__EntityStatus.ModifiedProperties[MessageData._DataStruct_.Real_FlowStep] > 0)
                sql.AppendLine("       `flow_step` = ?FlowStep");
            sql.Append(" WHERE `id` = ?Id;");
            return sql.ToString();
        }*/

        #endregion


        #region 字段

        /// <summary>
        ///  所有字段
        /// </summary>
        static string[] _fields = new string[]{ "Id","TraceId","Level","ApiName","Start","End","LocalId","LocalApp","LocalMachine","CallId","CallApp","CallMachine","Context","Token","Headers","Message","FlowStep" };

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
            { "TraceId" , "trace_id" },
            { "trace_id" , "trace_id" },
            { "Level" , "level" },
            { "ApiName" , "api_name" },
            { "api_name" , "api_name" },
            { "Start" , "start" },
            { "End" , "end" },
            { "LocalId" , "local_id" },
            { "local_id" , "local_id" },
            { "LocalApp" , "local_app" },
            { "local_app" , "local_app" },
            { "LocalMachine" , "local_machine" },
            { "local_machine" , "local_machine" },
            { "CallId" , "call_id" },
            { "call_id" , "call_id" },
            { "CallApp" , "call_app" },
            { "call_app" , "call_app" },
            { "CallMachine" , "call_machine" },
            { "call_machine" , "call_machine" },
            { "Context" , "context" },
            { "Token" , "token" },
            { "Headers" , "headers" },
            { "Message" , "message" },
            { "FlowStep" , "flow_step" },
            { "flow_step" , "flow_step" }
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
        protected sealed override void LoadEntity(MySqlDataReader reader,MessageData entity)
        {
            if (!reader.IsDBNull(0))
                entity._id = (long)reader.GetInt64(0);
            if (!reader.IsDBNull(1))
                entity._traceId = reader.GetString(1);
            if (!reader.IsDBNull(2))
                entity._level = (int)reader.GetInt32(2);
            if (!reader.IsDBNull(3))
                entity._apiName = reader.GetString(3);
            if (!reader.IsDBNull(4))
                try{entity._start = reader.GetMySqlDateTime(4).Value;}catch{}
            if (!reader.IsDBNull(5))
                try{entity._end = reader.GetMySqlDateTime(5).Value;}catch{}
            if (!reader.IsDBNull(6))
                entity._localId = reader.GetString(6);
            if (!reader.IsDBNull(7))
                entity._localApp = reader.GetString(7);
            if (!reader.IsDBNull(8))
                entity._localMachine = reader.GetString(8);
            if (!reader.IsDBNull(9))
                entity._callId = reader.GetString(9);
            if (!reader.IsDBNull(10))
                entity._callApp = reader.GetString(10);
            if (!reader.IsDBNull(11))
                entity._callMachine = reader.GetString(11);
            if (!reader.IsDBNull(12))
                entity._context = reader.GetString(12).ToString();
            if (!reader.IsDBNull(13))
                entity._token = reader.GetString(13);
            if (!reader.IsDBNull(14))
                entity._headers = reader.GetString(14);
            if (!reader.IsDBNull(15))
                entity._message = reader.GetString(15).ToString();
            if (!reader.IsDBNull(16))
                entity._flowStep = reader.GetString(16).ToString();
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
                case "trace_id":
                case "TraceId":
                    return MySqlDbType.VarString;
                case "level":
                case "Level":
                    return MySqlDbType.Int32;
                case "api_name":
                case "ApiName":
                    return MySqlDbType.VarString;
                case "start":
                case "Start":
                    return MySqlDbType.DateTime;
                case "end":
                case "End":
                    return MySqlDbType.DateTime;
                case "local_id":
                case "LocalId":
                    return MySqlDbType.VarString;
                case "local_app":
                case "LocalApp":
                    return MySqlDbType.VarString;
                case "local_machine":
                case "LocalMachine":
                    return MySqlDbType.VarString;
                case "call_id":
                case "CallId":
                    return MySqlDbType.VarString;
                case "call_app":
                case "CallApp":
                    return MySqlDbType.VarString;
                case "call_machine":
                case "CallMachine":
                    return MySqlDbType.VarString;
                case "context":
                case "Context":
                    return MySqlDbType.Text;
                case "token":
                case "Token":
                    return MySqlDbType.VarString;
                case "headers":
                case "Headers":
                    return MySqlDbType.VarString;
                case "message":
                case "Message":
                    return MySqlDbType.Text;
                case "flow_step":
                case "FlowStep":
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
        public void CreateFullSqlParameter(MessageData entity, MySqlCommand cmd)
        {
            //02:主键(Id)
            cmd.Parameters.Add(new MySqlParameter("Id",MySqlDbType.Int64){ Value = entity.Id});
            //03:全局请求标识(TraceId)
            var isNull = string.IsNullOrWhiteSpace(entity.TraceId);
            var parameter = new MySqlParameter("TraceId",MySqlDbType.VarString , isNull ? 10 : (entity.TraceId).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.TraceId;
            cmd.Parameters.Add(parameter);
            //04:调用层级(Level)
            cmd.Parameters.Add(new MySqlParameter("Level",MySqlDbType.Int32){ Value = entity.Level});
            //05:接口名称(ApiName)
            isNull = string.IsNullOrWhiteSpace(entity.ApiName);
            parameter = new MySqlParameter("ApiName",MySqlDbType.VarString , isNull ? 10 : (entity.ApiName).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.ApiName;
            cmd.Parameters.Add(parameter);
            //06:开始时间(Start)
            isNull = entity.Start.Year < 1900;
            parameter = new MySqlParameter("Start",MySqlDbType.DateTime);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Start;
            cmd.Parameters.Add(parameter);
            //07:结束时间(End)
            isNull = entity.End.Year < 1900;
            parameter = new MySqlParameter("End",MySqlDbType.DateTime);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.End;
            cmd.Parameters.Add(parameter);
            //08:本地的全局标识(LocalId)
            isNull = string.IsNullOrWhiteSpace(entity.LocalId);
            parameter = new MySqlParameter("LocalId",MySqlDbType.VarString , isNull ? 10 : (entity.LocalId).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.LocalId;
            cmd.Parameters.Add(parameter);
            //09:本地的应用(LocalApp)
            isNull = string.IsNullOrWhiteSpace(entity.LocalApp);
            parameter = new MySqlParameter("LocalApp",MySqlDbType.VarString , isNull ? 10 : (entity.LocalApp).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.LocalApp;
            cmd.Parameters.Add(parameter);
            //10:本地的机器(LocalMachine)
            isNull = string.IsNullOrWhiteSpace(entity.LocalMachine);
            parameter = new MySqlParameter("LocalMachine",MySqlDbType.VarString , isNull ? 10 : (entity.LocalMachine).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.LocalMachine;
            cmd.Parameters.Add(parameter);
            //11:请求方跟踪标识(CallId)
            isNull = string.IsNullOrWhiteSpace(entity.CallId);
            parameter = new MySqlParameter("CallId",MySqlDbType.VarString , isNull ? 10 : (entity.CallId).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.CallId;
            cmd.Parameters.Add(parameter);
            //12:请求应用(CallApp)
            isNull = string.IsNullOrWhiteSpace(entity.CallApp);
            parameter = new MySqlParameter("CallApp",MySqlDbType.VarString , isNull ? 10 : (entity.CallApp).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.CallApp;
            cmd.Parameters.Add(parameter);
            //13:请求机器(CallMachine)
            isNull = string.IsNullOrWhiteSpace(entity.CallMachine);
            parameter = new MySqlParameter("CallMachine",MySqlDbType.VarString , isNull ? 10 : (entity.CallMachine).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.CallMachine;
            cmd.Parameters.Add(parameter);
            //14:上下文信息(Context)
            isNull = string.IsNullOrWhiteSpace(entity.Context);
            parameter = new MySqlParameter("Context",MySqlDbType.Text , isNull ? 10 : (entity.Context).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Context;
            cmd.Parameters.Add(parameter);
            //15:身份令牌(Token)
            isNull = string.IsNullOrWhiteSpace(entity.Token);
            parameter = new MySqlParameter("Token",MySqlDbType.VarString , isNull ? 10 : (entity.Token).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Token;
            cmd.Parameters.Add(parameter);
            //16:请求头信息(Headers)
            isNull = string.IsNullOrWhiteSpace(entity.Headers);
            parameter = new MySqlParameter("Headers",MySqlDbType.VarString , isNull ? 10 : (entity.Headers).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Headers;
            cmd.Parameters.Add(parameter);
            //17:消息序列化文本(Message)
            isNull = string.IsNullOrWhiteSpace(entity.Message);
            parameter = new MySqlParameter("Message",MySqlDbType.Text , isNull ? 10 : (entity.Message).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.Message;
            cmd.Parameters.Add(parameter);
            //18:流程步骤记录(FlowStep)
            isNull = string.IsNullOrWhiteSpace(entity.FlowStep);
            parameter = new MySqlParameter("FlowStep",MySqlDbType.Text , isNull ? 10 : (entity.FlowStep).Length);
            if(isNull)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = entity.FlowStep;
            cmd.Parameters.Add(parameter);
        }


        /// <summary>
        /// 设置更新数据的命令
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="cmd">命令</param>
        protected sealed override void SetUpdateCommand(MessageData entity, MySqlCommand cmd)
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
        protected sealed override bool SetInsertCommand(MessageData entity, MySqlCommand cmd)
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
        /// 消息存储的结构语句
        /// </summary>
        private TableSql _MessageSql = new TableSql
        {
            TableName = "tb_trace_info",
            PimaryKey = "Id"
        };


        /// <summary>
        /// 消息存储数据访问对象
        /// </summary>
        private MessageDataAccess _messages;

        /// <summary>
        /// 消息存储数据访问对象
        /// </summary>
        public MessageDataAccess Messages
        {
            get
            {
                return this._messages ?? ( this._messages = new MessageDataAccess{ DataBase = this});
            }
        }


        /// <summary>
        /// 消息存储(tb_trace_info):消息存储
        /// </summary>
        public const int Table_Message = 0x0;
    }*/
}