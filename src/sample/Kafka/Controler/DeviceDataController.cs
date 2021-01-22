#region
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

#endregion

namespace MicroZero.Kafka.QueueStation
{
    /// <summary>
    ///  监控记录
    /// </summary>
    [Consumer("DeviceDataEvent")]
    public class DeviceDataController : IApiController
    {
        /// <summary>
        /// 设备消息
        /// </summary>
        [Route("DeviceData")]
        public Task DeviceData()
        {
            return Task.CompletedTask;
        }
    }
}