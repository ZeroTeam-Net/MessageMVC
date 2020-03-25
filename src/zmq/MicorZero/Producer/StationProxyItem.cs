using System;
using ZeroTeam.ZeroMQ;
using ZeroTeam.ZeroMQ.ZeroRPC;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 站点代理节点
    /// </summary>
    public class StationProxyItem
    {
        /// <summary>
        /// 配置
        /// </summary>
        public StationConfig Config { get; set; }

        /// <summary>
        /// 远程连接
        /// </summary>
        public ZSocketEx Socket { get; set; }

        /// <summary>
        /// 打开时间
        /// </summary>
        public DateTime? Open { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? Close { get; set; }
    }
}

