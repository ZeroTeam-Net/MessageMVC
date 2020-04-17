/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 14:32:57*/
#region
using Agebull.EntityModel.MySql;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.DataAccess
{
    /// <summary>
    /// 消息存储
    /// </summary>
    public partial class MessageDataAccess : MySqlTable<MessageData, TraceLinkDatabase>
    {
    }
}