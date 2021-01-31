#region
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

#endregion

namespace MicroZero.Kafka.QueueStation
{
    /// <summary>
    ///  监控记录
    /// </summary>
    [Consumer("DeviceReport")]
    public class DeviceDataController : IApiController
    {
        /// <summary>
        /// 设备消息
        /// </summary>
        [Route("Report")]
        public Task DeviceData()
        {
            return Task.CompletedTask;
        }
    }
}