/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/15 14:26:44*/
#region using
using Agebull.EntityModel.Common;
using System.Runtime.Serialization;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink
{
    /// <summary>
    /// 消息存储
    /// </summary>
    [DataContract]
    public sealed partial class MessageData : EditDataObject
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