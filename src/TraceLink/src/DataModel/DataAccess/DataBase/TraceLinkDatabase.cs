/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 14:32:57*/
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
    sealed partial class TraceLinkDatabase : MySqlDataBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize()
        {
            ConnectionStringName = "TraceLinkDatabase";
        }
    }
}