/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 14:32:57*/
#region
using Agebull.EntityModel.MySql;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.DataAccess
{
    /// <summary>
    /// 本地数据库
    /// </summary>
    public sealed partial class TraceLinkDatabase : MySqlDataBase
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