#region
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

#endregion

namespace MicroZero.Kafka.QueueStation
{
    /// <summary>
    ///  监控记录
    /// </summary>
    [Consumer("Test2")]
    public class DeviceDataController : IApiController
    {
        /// <summary>
        /// 设备消息
        /// </summary>
        [Route("Test")]
        public Task DeviceData()
        {
            return Task.CompletedTask;
        }
    }
}