using System.Runtime.Serialization;
namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     分组节点
    /// </summary>
    public class ZeroItem
    {
        /// <summary>
        ///     服务器唯一标识
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        ///     服务器唯一标识
        /// </summary>

        public string ServiceKey { get; set; }

        /// <summary>
        ///     短名称
        /// </summary>

        public string ShortName { get; set; }

        /// <summary>
        ///     ZeroCenter主机IP地址
        /// </summary>

        public string Address { get; set; }

        /// <summary>
        ///     ZeroCenter监测端口号
        /// </summary>

        public int MonitorPort { get; set; }

        /// <summary>
        ///     ZeroCenter管理端口号
        /// </summary>

        public int ManagePort { get; set; }


        /// <summary>
        ///     ZeroCenter管理地址
        /// </summary>
        [IgnoreDataMember]
        public string ManageAddress => $"tcp://{Address}:{ManagePort}";

        /// <summary>
        ///     ZeroCenter监测地址
        /// </summary>
        [IgnoreDataMember]
        public string MonitorAddress => $"tcp://{Address}:{MonitorPort}";

    }
}