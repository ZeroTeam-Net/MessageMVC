using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Sample
{
    /// <summary>
    /// 订单明细
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UnionOrderItem
    {
        /// <summary>
        /// 序号
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Index { get; set; }

        /// <summary>
        /// SKU类型
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SkuType ItemType { get; set; }

        /// <summary>
        /// 购物车Id(为空不传,保持默认0)
        /// </summary>
        /// <example>
        ///     98562148562
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long CartId { get; set; }

        /// <summary>
        /// 对应平台的对象ID,根据SKU类型不同而不同-----（eshop里的skuID）
        /// </summary>
        /// <remarks>
        /// 如果为商品为SKUID，线上为线上SKUID，线下为tbProductSkuInOrg中的SID
        /// 如果为优惠券为优惠券发放中的IssueId
        /// 如果为会员为会员权益ID
        /// </remarks>
        /// <example>
        ///     98562148562
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long PlatformId { get; set; }

        /// <summary>
        /// 基础库里的skuID【tbProductsku表中的SID】
        /// </summary>
        /// <example>
        ///     98562148562
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long BaseSkuSid { get; set; }


        /// <summary>
        /// 商品Id
        /// </summary>
        /// <remarks>
        /// 如果为商品为商品（SPU）ID（线下目前不适用）
        /// 如果为优惠券为优惠券类型ID
        /// 如果为会员为会员类型ID
        /// </remarks>
        /// <example>
        ///     98562148562
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ProductId { get; set; }

        /// <summary>
        /// 商品条码
        /// </summary>
        /// <example>
        ///     690358796
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BarCode { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        /// <example>
        ///     XXX长裤
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SkuName { get; set; }

        /// <summary>
        /// 购买数量 必填
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Number { get; set; }

        /// <summary>
        ///  成本价
        /// </summary>
        /// <example>
        ///     42
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal CostPrice { get; set; }

        /// <summary>
        ///  销售价
        /// </summary>
        /// <example>
        ///     99
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal SalePrice { get; set; }

        /// <summary>
        ///  结算金额（优惠前）
        /// </summary>
        /// <example>
        ///     99
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Amount { get; set; }


        /// <summary>
        ///  实际应付金额（优惠后）
        /// </summary>
        /// <example>
        ///     89
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Pay { get; set; }


    }
}