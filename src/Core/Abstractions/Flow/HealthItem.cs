using System.Collections.Generic;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    /// 健康信息
    /// </summary>
    public class HealthInfo : HealthItem
    {
        /// <summary>
        /// 检查详情
        /// </summary>
        public List<HealthItem> Items { get; set; }
    }

    /// <summary>
    /// 健康节点
    /// </summary>
    public class HealthItem
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 健康等级,1-5级,5为最好,5,-1为发生异常,0为失败
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 健康值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 检查详情
        /// </summary>
        public string Details { get; set; }
    }
}
