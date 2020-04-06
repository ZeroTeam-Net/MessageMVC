namespace ZeroTeam.MessageMVC.Sample
{
    /// <summary>
    /// 优惠券信息
    /// </summary>
    public class CouponInfo
    {
        /// <summary>
        /// 发放编号
        /// </summary>
        public string IssuedId { get; set; }

        /// <summary>
        /// 优惠券类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 发放名称--使用门槛
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 活动编码
        /// </summary>
        public string ActiveId { get; set; }

        /// <summary>
        /// 优惠码状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 优惠的价格--优惠力度
        /// </summary>
        public decimal SalesMoney { get; set; }

        /// <summary>
        /// 优惠券类型主键ID
        /// </summary>
        public string CouponTypeId { get; set; }

        /// <summary>
        /// 使用渠道
        /// </summary>
        public string OrgName { get; set; } = "";

        /// <summary>
        /// 使用商品范围
        /// </summary>
        public string ProductUseName { get; set; } = "";

        /// <summary>
        /// 使用日期
        /// </summary>
        public string DateUsed { get; set; } = "";

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 配置项
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// 折扣价格
        /// </summary>
        public decimal Amounts { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Success { get; set; }
    }
}