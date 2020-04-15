/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 14:26:44*/
#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Agebull.Common.Configuration;
using Agebull.EntityModel.Common;
using Agebull.EntityModel.Interfaces;
using Agebull.EntityModel.Events;

using Agebull.EntityModel.MySql;


#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.DataAccess
{
    /// <summary>
    /// 本地数据库
    /// </summary>
    public partial class TraceLinkDatabase
    {
        /// <summary>
        /// 构造
        /// </summary>
        static TraceLinkDatabase()
        {
            /*tableSql = new Dictionary<string, TableSql>(StringComparer.OrdinalIgnoreCase)
            {TraceLinkDatabase
            };
            DataUpdateHandler.RegisterUpdateHandler(new RedisDataTrigger());*/
            DataUpdateHandler.RegisterUpdateHandler(new MySqlDataTrigger());
        }

        /// <summary>
        /// 构造
        /// </summary>
        public TraceLinkDatabase()
        {
            Name = @"trace_link";
            Caption = @"消息链路跟踪";
            Description = @"消息链路跟踪";
            Initialize();
            //RegistToEntityPool();
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize();
    }
}