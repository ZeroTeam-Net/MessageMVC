namespace BeetleX.Zeroteam.WebSocketBus
{
    /// <summary>
    /// 监控数据类型
    /// </summary>
    /// <remark>
    /// 监控数据类型
    /// </remark>
    public enum MonitorDataType
    {
        /// <summary>
        /// 实时
        /// </summary>
        Real = 0x0,
        /// <summary>
        /// 5分钟
        /// </summary>
        Minute5 = 0x1,
        /// <summary>
        /// 30分钟
        /// </summary>
        Minute30 = 0x2,
        /// <summary>
        /// 1小时
        /// </summary>
        Hour1 = 0x3,
        /// <summary>
        /// 4小时
        /// </summary>
        Hour4 = 0x4,
        /// <summary>
        /// 12小时
        /// </summary>
        Hour12 = 0x5,
        /// <summary>
        /// 日
        /// </summary>
        Day = 0x6,
        /// <summary>
        /// 周
        /// </summary>
        Week = 0x7,
        /// <summary>
        /// 旬
        /// </summary>
        TenDay = 0x8,
        /// <summary>
        /// 月
        /// </summary>
        Month = 0x9,
        /// <summary>
        /// 季度
        /// </summary>
        Quarter = 0xA,
        /// <summary>
        /// 半年
        /// </summary>
        HalfYear = 0xB,
        /// <summary>
        /// 年
        /// </summary>
        Year = 0xC,
    }
}
