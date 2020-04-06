namespace ZeroTeam.MessageMVC.Sample
{
    /// <summary>
    /// 发票信息
    /// </summary>
    public class OrderInvoce
    {
        /// <summary>
        /// 发票类型 必填
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 发票抬头 必填
        /// </summary>
        public string InvoiceTitle { get; set; }

        /// <summary>
        /// 发票税号 非必填
        /// </summary>
        public string InvoiceTaxNo { get; set; }
    }
}