/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 14:02:36*/
#region
using Agebull.EntityModel.MySql;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.DataAccess
{
    /// <summary>
    /// 健康检查结果
    /// </summary>
    public partial class HealthCheckDataAccess : MySqlTable<HealthCheckData, TraceLinkDatabase>
    {
    }
}