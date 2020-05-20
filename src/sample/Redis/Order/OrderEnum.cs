namespace ZeroTeam.MessageMVC.Sample
{
    /// <summary>
    /// 物流状态
    /// </summary>
    /// <remarks>
    ///  淘宝推荐物流的订单状态分为：
    ///     等待发货的订单、等待物流公司确认、等待物流公司揽件、等待对方签收的订单、对方已签收的订单。
    /// 其含义分别如下：
    ///     等待发货的订单：买家付款后的淘宝交易，会自动生成等待发货的物流订单。非支付宝交易的用户也可以创建等待发货的订单。
    ///     等待物流公司的确认：当您的订单通过淘宝推荐物流发送给物流公司后，物流公司会根据订单信息来确认是否可以揽收，在物流公司未确认是否能揽收前，
    /// 订单是等待物流公司确认的状态，在此状态下，您可以自主取消您的订单。
    ///     等待物流公司揽件：在此状态下表示，物流公司已经确认可以揽收您的订单，正在上门途中，或者您的物品已被揽收，但是物流公司正在返回公司扫描上传
    /// 揽收成功信息的过程中。
    ///     等待对方签收的订单：此状态下的订单，表示您的物品已在发往收件人的途中。
    ///     对方已签收的订单：此状态下的订单，表示对方已经成功签收，或者拒签，或者丢失。
    ///     是物流订单的一个结束状态。 
    /// </remarks>
    public enum OrderExpressStatus
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None = 0x0,

        /// <summary>
        /// 已下单
        /// </summary>
        Order = 0x10,

        /// <summary>
        /// 订单取消
        /// </summary>
        OrderCancel = 0x18,

        /// <summary>
        /// 已打包
        /// </summary>
        Packaged = 0x20,

        /// <summary>
        /// 待发货
        /// </summary>
        Waiting = 0x30,

        /// <summary>
        /// 快递公司已揽收
        /// </summary>
        ExpressLanShou = 0x40,

        /// <summary>
        /// 快递公司拒收
        /// </summary>
        ExpressRejection = 0x41,

        /// <summary>
        /// 快递公司运输中
        /// </summary>
        ExpressInTransit = 0x50,

        /// <summary>
        /// 快递公司送达
        /// </summary>
        ExpressReach = 0x60,

        /// <summary>
        /// 快递公司损失
        /// </summary>
        ExpressLost = 0x61,

        /// <summary>
        /// 客户已签收
        /// </summary>
        CustomrReceipt = 0x70,

        /// <summary>
        /// 客户已拒收
        /// </summary>
        CustomrRejection = 0x71,

        /// <summary>
        /// 客户已退回
        /// </summary>
        CustomrBack = 0x72,

        /// <summary>
        /// 已收回退货
        /// </summary>
        BackEnd = 0x73,

        /// <summary>
        /// 正常关闭
        /// </summary>
        Close = 0xFF,

        /// <summary>
        /// 非正常关闭
        /// </summary>
        Failed = 0xFFFF
    }

    /// <summary>
    /// 订单前端状态
    /// </summary>
    public enum OrderStatusForFront
    {
        /// <summary>
        /// 待支付
        /// </summary>
        NotPay = 10,

        /// <summary>
        /// 正在支付
        /// </summary>
        PreparePay = 20,

        /// <summary>
        /// 已付款
        /// </summary>
        Payed = 30,

        /// <summary>
        /// 已发货
        /// </summary>
        Deliver = 31,

        /// <summary>
        /// 退款申请中
        /// </summary>
        RefundApply = 33,

        /// <summary>
        /// 已退款
        /// </summary>
        Refund = 34,

        /// <summary>
        /// 退款退货
        /// </summary>
        BackAndRefund = 35,


        /// <summary>
        /// 交易关闭
        /// </summary>
        PayClosed = 40
    }

    /// <summary>
    /// 订单后端状态
    /// </summary>
    public enum OrderStatusType
    {
        /// <summary>
        /// 待支付
        /// </summary>
        PendingPayment = 110,

        /// <summary>
        /// 全额付款  
        /// </summary>
        FullPayment = 310,

        /// <summary>
        /// 退款申请中
        /// </summary>
        RefundApply = 320,

        /// <summary>
        /// 订单超时失败
        /// </summary>
        OrderOvertimeShutdown = 430,

        /// <summary>
        /// 商品信息变更失败
        /// </summary>
        ProductChangeShutown = 440,

        /// <summary>
        /// 库存不足失败
        /// </summary>
        StockShutown = 450,

        /// <summary>
        /// 主动退款完成关闭
        /// </summary>
        InitiativeRefund = 460,


        ///// <summary>
        ///// 生成订单待付款
        ///// </summary>
        //GenerateOrderToBePaid = 0x1,
        ///// <summary>
        ///// 首付支付成功待提交分期方审核
        ///// </summary>
        //SuccessfulPayment = 0xA,
        ///// <summary>
        ///// 首付支付成功已提交分期方审核，分期方审核中
        ///// </summary>
        //SubmitWaitInstalment = 0xB,
        ///// <summary>
        ///// 首付支付成功交分期方审核通过
        ///// </summary>
        //SubmitInstalmentALoan = 0xC,
        ///// <summary>
        ///// 交易关闭
        ///// </summary>
        //TransactionClosed = 0x29,
        ///// <summary>
        ///// 交易取消
        ///// </summary>
        //TransactionCanceled = 0x2A,
    }

    /// <summary>
    /// 订单类型(自定义类型
    /// </summary>
    /// <remark>
    /// 订单类型(1:正常订单,2:非正常)
    /// </remark>
    public enum OrderType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        None,

        /// <summary>
        /// 线上订单
        /// </summary>
        EShop = 0x1,

        /// <summary>
        /// 线下订单
        /// </summary>
        Offline = 0x2,
    }

    /// <summary>
    /// 发票类型(自定义类型
    /// </summary>
    /// <remark>
    /// 发票类型(1:普通发票，2:电子发票，3:增值税发票
    /// </remark>
    public enum InvoiceType
    {
        /// <summary>
        /// 个人
        /// </summary>
        OrdinaryInvoice = 0x1,

        /// <summary>
        /// 企业
        /// </summary>
        ElectronicInvoice = 0x2,

        /// <summary>
        /// 增值税发票
        /// </summary>
        ValueAddedTaxInvoice = 0x3,
    }


    /// <summary>
    /// 物流类型类型
    /// </summary>
    /// <remark>
    /// 物流类型类型
    /// </remark>
    public enum LogistcicsType
    {
        /// <summary>
        /// 未确定
        /// </summary>
        None = 0x0,

        /// <summary>
        /// 现场交易
        /// </summary>
        FaceToFace = 0x1,

        /// <summary>
        /// 快递
        /// </summary>
        Express = 0x2,

        /// <summary>
        /// 送货上门
        /// </summary>
        DoorToDoor = 0x3,

        /// <summary>
        /// 到店自提
        /// </summary>
        Store = 0x4,
    }

    /// <summary>
    /// SKU类型类型
    /// </summary>
    /// <remark>
    /// SKU类型类型
    /// </remark>
    public enum SkuType
    {
        /// <summary>
        /// 未确定
        /// </summary>
        None = 0x0,

        /// <summary>
        /// 普通商品
        /// </summary>
        GeneralProduct = 0x1,

        /// <summary>
        /// 虚拟商品
        /// </summary>
        VirtualProduct = 0x2,

        /// <summary>
        /// 运费
        /// </summary>
        Freight = 0x3,

        /// <summary>
        /// 优惠券 
        /// </summary>
        Coupon = 0x4,

        /// <summary>
        /// 会员
        /// </summary>
        Member = 0x5,

        /// <summary>
        /// 礼品
        /// </summary>
        Gift = 0x6,

        /// <summary>
        /// 价格策略
        /// </summary>
        PriceStrategy = 0x7
    }

    /// <summary>
    /// 枚举扩展
    /// </summary>
    public static class OrderEnumHelper
    {
        /// <summary>
        /// SKU类型类型名称转换
        /// </summary>
        public static string ToCaption(this SkuType value)
        {
            switch (value)
            {
                case SkuType.None:
                    return "未确定";
                case SkuType.GeneralProduct:
                    return "普通商品";
                case SkuType.VirtualProduct:
                    return "虚拟商品";
                case SkuType.Freight:
                    return "运费";
                case SkuType.Coupon:
                    return "优惠券";
                case SkuType.Member:
                    return "会员";
                case SkuType.Gift:
                    return "礼品";
                case SkuType.PriceStrategy:
                    return "价格策略";
                default:
                    return "SKU类型类型(错误)";
            }
        }


        /// <summary>
        /// 物流类型类型名称转换
        /// </summary>
        public static string ToCaption(this LogistcicsType value)
        {
            switch (value)
            {
                case LogistcicsType.None:
                    return "未确定";
                case LogistcicsType.FaceToFace:
                    return "现场交易";
                case LogistcicsType.Express:
                    return "快递";
                case LogistcicsType.DoorToDoor:
                    return "送货上门";
                case LogistcicsType.Store:
                    return "到店自提";
                default:
                    return "物流类型类型(错误)";
            }
        }


        /// <summary>
        ///     订单状态(自定义类型名称转换
        /// </summary>
        public static string ToCaption(this OrderStatusForFront value)
        {
            switch (value)
            {
                case OrderStatusForFront.NotPay:
                    return "待付款";
                case OrderStatusForFront.Payed:
                    return "已付款";
                case OrderStatusForFront.PayClosed:
                    return "交易关闭";
                case OrderStatusForFront.Refund:
                    return "已退款";
                case OrderStatusForFront.Deliver:
                    return "已发货";
                case OrderStatusForFront.RefundApply:
                    return "退款申请";
                case OrderStatusForFront.BackAndRefund:
                    return "退货退款";
                case OrderStatusForFront.PreparePay:
                    return "正在支付";
                default:
                    return "前端订单状态(自定义类型(未知))";
            }
        }

        /// <summary>
        ///     系统状态类型名称转换
        /// </summary>
        public static string ToCaption(this OrderStatusType value)
        {
            switch (value)
            {
                case OrderStatusType.PendingPayment:
                    return "待支付";
                case OrderStatusType.FullPayment:
                    return "全额付款";
                case OrderStatusType.RefundApply:
                    return "退款申请";
                case OrderStatusType.OrderOvertimeShutdown:
                    return "订单超时失败";
                case OrderStatusType.ProductChangeShutown:
                    return "商品信息变更失败";
                case OrderStatusType.StockShutown:
                    return "库存不足失败";
                case OrderStatusType.InitiativeRefund:
                    return "主动退款完成关闭";
                default:
                    return "系统状态类型(错误)";
            }
        }

        /// <summary>
        ///     物流状态类型名称转换
        /// </summary>
        public static string ToCaption(this OrderExpressStatus value)
        {
            switch (value)
            {
                case OrderExpressStatus.None:
                    return "无状态";
                case OrderExpressStatus.Order:
                    return "已下单";
                case OrderExpressStatus.OrderCancel:
                    return "订单取消";
                case OrderExpressStatus.Packaged:
                    return "已打包";
                case OrderExpressStatus.Waiting:
                    return "待发货";
                case OrderExpressStatus.ExpressLanShou:
                    return "快递公司已揽收";
                case OrderExpressStatus.ExpressRejection:
                    return "快递公司拒收";
                case OrderExpressStatus.ExpressInTransit:
                    return "快递公司运输中";
                case OrderExpressStatus.ExpressReach:
                    return "快递公司送达";
                case OrderExpressStatus.ExpressLost:
                    return "快递公司损失";
                case OrderExpressStatus.CustomrReceipt:
                    return "客户已签收";
                case OrderExpressStatus.CustomrRejection:
                    return "客户已拒收";
                case OrderExpressStatus.CustomrBack:
                    return "客户已退回";
                case OrderExpressStatus.BackEnd:
                    return "已收回退货";
                case OrderExpressStatus.Close:
                    return "非正常关闭";
                case OrderExpressStatus.Failed:
                    return "非正常关闭";
                default:
                    return "物流状态类型(错误)";
            }
        }


        /// <summary>
        ///     订单类型(自定义类型名称转换
        /// </summary>
        public static string ToCaption(this OrderType value)
        {
            switch (value)
            {
                case OrderType.EShop:
                    return "线上订单";
                case OrderType.Offline:
                    return "线下订单";
                default:
                    return "订单类型(自定义类型(未知)";
            }
        }

        /// <summary>
        ///     发票类型(自定义类型名称转换
        /// </summary>
        public static string ToCaption(this InvoiceType value)
        {
            switch (value)
            {
                case InvoiceType.OrdinaryInvoice:
                    return "普通发票";
                case InvoiceType.ElectronicInvoice:
                    return "电子发票";
                case InvoiceType.ValueAddedTaxInvoice:
                    return "增值税发票";
                default:
                    return "发票类型(自定义类型(未知)";
            }
        }

        /// <summary>
        /// 订单是否是有效订单
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsSuccess(this OrderStatusType value)
        {
            switch (value)
            {
                case OrderStatusType.OrderOvertimeShutdown:
                case OrderStatusType.ProductChangeShutown:
                case OrderStatusType.StockShutown:
                    return false;
                default:
                    return true;
            }
        }
    }
}