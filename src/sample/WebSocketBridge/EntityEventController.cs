#region
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC;

#endregion

namespace ZeroTeam.ZhongJian.EnvironmentIOT.WebApi.Entity
{
    /// <summary>
    ///  监控记录
    /// </summary>
    [NetEvent("EntityEvent")]
    [Route("EnvironmentMonitor")]
    public partial class EntityEventController : IApiController
    {
        /// <summary>
        /// 实时记录
        /// </summary>
        [Route("RealRecord")]
        public void RealRecordEvent()
        {
            MessagePoster.Call("RealRecord", $"real", "");
        }
    }
}