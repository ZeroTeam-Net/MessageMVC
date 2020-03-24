using System;
using System.Runtime.Serialization;
namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     分组节点
    /// </summary>
    [Serializable]
    [DataContract]
    public class ZeroItem
    {
        /// <summary>
        ///     服务器唯一标识
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        ///     服务器唯一标识
        /// </summary>
        [DataMember]
        public string ServiceKey { get; set; }

        /// <summary>
        ///     短名称
        /// </summary>
        [DataMember]
        public string ShortName { get; set; }
        
        /// <summary>
        ///     ZeroCenter主机IP地址
        /// </summary>
        [DataMember]
        public string Address { get; set; }

        /// <summary>
        ///     ZeroCenter监测端口号
        /// </summary>
        [DataMember]
        public int MonitorPort { get; set; }

        /// <summary>
        ///     ZeroCenter管理端口号
        /// </summary>
        [DataMember]
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