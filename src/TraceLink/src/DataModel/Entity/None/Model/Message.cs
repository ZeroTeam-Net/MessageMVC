﻿/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 14:26:44*/
#region using
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
using Agebull.EntityModel.Common;
using Agebull.EntityModel.Interfaces;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink
{
    /// <summary>
    /// 消息存储
    /// </summary>
    [DataContract]
    sealed partial class MessageData : EditDataObject
    {
        
        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize()
        {
/**/
        }

    }
}