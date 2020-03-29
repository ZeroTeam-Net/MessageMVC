using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZeroTeam.MessageMVC.Sample
{
    /// <summary>
    /// 订单
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UnionOrder
    {

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

        /// <summary>
        /// 订单实时状态
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public OrderRealState RealState { get; set; }

        private decimal _preferential;

        /// <summary>
        ///  优惠
        /// </summary>
        /// <example>
        ///     10
        /// </example>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal Preferential
        {
            get => _preferential;
            set
            {
                _preferential = value;
                Pay = Amount - value;
                if (Pay < 0)
                    Pay = 0;
            }
        }

        /// <summary>
        ///  订单编码
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [JsonProperty("OrderCode", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OrderCode { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        [JsonProperty("OrderType", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        public OrderType OrderType { get; set; }

        /// <summary>
        /// 订单明细
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<UnionOrderItem> Items { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long UserId { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime OrderDate { get; set; }


        /// <summary>
        /// 门店组织ID
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long OrgID { get; set; }

        /// <summary>
        /// 用户备注
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Remark { get; set; }

    }
}