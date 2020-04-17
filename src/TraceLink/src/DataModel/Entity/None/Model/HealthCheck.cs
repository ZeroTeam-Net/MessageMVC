/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 15:34:52*/
#region using
using Agebull.EntityModel.Common;
using System.Runtime.Serialization;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink
{
    /// <summary>
    /// 健康检查结果
    /// </summary>
    [DataContract]
    public sealed partial class HealthCheckData : EditDataObject
    {

        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize()
        {
            /*
                        _checkid = 0;
                        _level = 0;*/
        }

    }
}