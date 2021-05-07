using Newtonsoft.Json;
using System;

namespace BeetleX.Zeroteam.WebSocketBus
{
    /// <summary>
    /// 监控数据汇总
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class MonitorCollectEntity
    {
        /// <summary>
        ///  主键
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        ///  企业名称
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [JsonProperty("organizationName")]
        public string OrganizationName { get; set; }

        /// <summary>
        ///  企业编码
        /// </summary>
        /// <value>
        ///     可存储50个字符.合理长度应不大于50.
        /// </value>
        [JsonProperty("organizationCode")]
        public string OrganizationCode { get; set; }

        /// <summary>
        ///  点位编码
        /// </summary>
        /// <value>
        ///     可存储50个字符.合理长度应不大于50.
        /// </value>
        [JsonProperty("deviceCode")]
        public string DeviceCode { get; set; }

        /// <summary>
        ///  点位名称
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }

        /// <summary>
        ///  因子编码
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [JsonProperty("factorCode")]
        public string FactorCode { get; set; }

        /// <summary>
        ///  因子名称
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [JsonProperty("factorName")]
        public string FactorName { get; set; }

        /// <summary>
        ///  汇总时间
        /// </summary>
        /// <example>
        ///     2012-12-21 23:59:59
        /// </example>
        [JsonProperty("collectionTime"), JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
        public DateTime CollectionTime { get; set; }

        /// <summary>
        ///  汇总类型
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("collectionType")]
        public MonitorDataType CollectionType { get; set; }

        /// <summary>
        ///  实时数据
        /// </summary>
        /// <remarks>
        ///     “xxxxxx”是污染因子编码,污染监测因子编码取值详见附录B
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("rtd")]
        public decimal Rtd { get; set; }

        /// <summary>
        ///  最小值
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("min")]
        public decimal Min { get; set; }

        /// <summary>
        ///  最大值
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("max")]
        public decimal Max { get; set; }

        /// <summary>
        ///  平均值
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("avg")]
        public decimal Avg { get; set; }

        /// <summary>
        ///  监控设备外键
        ///  --外键 : [MonitorDevice-Id]
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("monitorDeviceId")]
        public long MonitorDeviceId { get; set; }

        /// <summary>
        ///  组织机构外键
        /// </summary>
        /// <remarks>
        ///     标题
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("organizationId")]
        public long OrganizationId { get; set; }

        /// <summary>
        ///  数据包日志外键
        ///  --外键 : [PackageLog-Id]
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("packageLogId")]
        public long PackageLogId { get; set; }

        /// <summary>
        ///  监测因子编码表外键
        ///  --外键 : [MonitorFactor-Id]
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("monitorFactorId")]
        public long MonitorFactorId { get; set; }

        /// <summary>
        ///  组织边界代码
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [JsonProperty("boundaryCode")]
        public string BoundaryCode { get; set; }

    }
}
