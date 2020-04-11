using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Wechart
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WxPayResult
    {
        #region 原始字段

        [JsonProperty("wx_appid")] internal string _appid;


        [JsonProperty("wx_bank_type")] internal string _bankType;

        [JsonProperty("wx_cash_fee")] internal string _cashFee;


        [JsonProperty("wx_fee_type")] internal string _feeType;

        [JsonProperty("wx_is_subscribe")] internal string _isSubscribe;

        [JsonProperty("wx_mch_id")] internal string _mchId;


        [JsonProperty("wx_nonce_str")] internal string _nonceStr;

        [JsonProperty("wx_openid")] internal string _openid;

        [JsonProperty("wx_out_trade_no")] internal string _outTradeNo;

        [JsonProperty("wx_result_code")] internal string _resultCode;

        [JsonProperty("wx_return_code")] internal string _returnCode;


        [JsonProperty("wx_sign")] internal string _sign;

        [JsonProperty("wx_sub_appid")] internal string _subAppid;

        [JsonProperty("wx_sub_is_subscribe")] internal string _subIsSubscribe;

        [JsonProperty("wx_sub_mch_id")] internal string _subMchId;

        [JsonProperty("wx_sub_openid")] internal string _subOpenid;

        [JsonProperty("wx_time_end")] internal string _timeEnd;

        [JsonProperty("wx_total_fee")] internal string _totalFee;


        [JsonProperty("wx_trade_type")] internal string _tradeType;


        [JsonProperty("wx_transaction_id")] internal string _transactionId;

        /// <summary>
        ///     测试数据
        /// </summary>
        internal const string TestXml =
            @"<xml>
    <appid><![CDATA[wxae5986a3048e8443]]></appid>
    <bank_type><![CDATA[CFT]]></bank_type>
    <cash_fee><![CDATA[1]]></cash_fee>
    <fee_type><![CDATA[CNY]]></fee_type>
    <is_subscribe><![CDATA[Y]]></is_subscribe>
    <mch_id><![CDATA[1510078651]]></mch_id>
    <nonce_str><![CDATA[H9VUXLKCK41PiX4BL6UR]]></nonce_str>
    <openid><![CDATA[osLRJuBG9kxLavpxRflRzu2kLJdg]]></openid>
    <out_trade_no><![CDATA[6468390321188241409]]></out_trade_no>
    <result_code><![CDATA[SUCCESS]]></result_code>
    <return_code><![CDATA[SUCCESS]]></return_code>
    <sign><![CDATA[2978D3FB7930C9BE391D18DFD94AD1D4]]></sign>
    <sub_appid><![CDATA[wx55a2d735dcaba645]]></sub_appid>
    <sub_is_subscribe><![CDATA[N]]></sub_is_subscribe>
    <sub_mch_id><![CDATA[1516349581]]></sub_mch_id>
    <sub_openid><![CDATA[oQKU65CUgd7D2Sd6XjVz8LUavw6M]]></sub_openid>
    <time_end><![CDATA[20181114163407]]></time_end>
    <total_fee>1</total_fee>
    <trade_type><![CDATA[JSAPI]]></trade_type>
    <transaction_id><![CDATA[4200000222201811145632955158]]></transaction_id>
</xml>";
        #endregion

        #region 封装字段

        /// <summary>
        ///     连接字段
        /// </summary>
        [JsonProperty("signstrOrder")]
        public string SignstrOrder { get; set; }

        /// <summary>
        ///     微信开放平台审核通过的应用APPID
        /// </summary>
        public string Appid
            => _appid;

        /// <summary>
        ///     微信支付分配的商户号
        /// </summary>
        public string MchId
            => _mchId;

        /// <summary>
        ///     微信开放平台审核通过的应用APPID
        /// </summary>
        public string SubAppid
            => _subAppid;

        /// <summary>
        ///     微信支付分配的商户号
        /// </summary>
        public string SubMchId
            => _subMchId;

        /// <summary>
        ///     用户在商户appid下的唯一标识
        /// </summary>
        public string Openid
            => _openid;

        /// <summary>
        ///     用户在商户sub_appid下的唯一标识
        /// </summary>
        public string SubOpenid
            => _subOpenid;

        /// <summary>
        ///     随机字符串，不长于32位
        /// </summary>
        public string NonceStr
            => _nonceStr;

        /// <summary>
        ///     签名，详见签名算法
        /// </summary>
        public string Sign
            => _sign;

        /// <summary>
        ///     SUCCESS/FAIL
        /// </summary>
        public string ResultCode
            => _resultCode;

        /// <summary>
        ///     返回状态
        /// </summary>
        public string ReturnCode
            => _returnCode;

        /// <summary>
        ///     用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
        /// </summary>
        public string IsSubscribe
            => _isSubscribe;

        /// <summary>
        ///     用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
        /// </summary>
        public string SubIsSubscribe
            => _subIsSubscribe;

        /// <summary>
        ///     订单类型
        /// </summary>
        public string TradeType
            => _tradeType;

        /// <summary>
        ///     银行类型，采用字符串类型的银行标识，银行类型见银行列表
        /// </summary>
        public string BankType => _bankType;

        /// <summary>
        ///     货币类型，符合ISO4217标准的三位字母代码，默认人民币：CNY，其他值列表详见货币类型
        /// </summary>
        public string FeeType => _feeType;

        /// <summary>
        ///     微信支付订单号
        /// </summary>
        public string TransactionId => _transactionId;

        /// <summary>
        ///     商户系统的订单号，与请求一致。
        /// </summary>
        public string OutTradeNo => _outTradeNo;

        /// <summary>
        ///     支付完成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010。其他详见时间规则
        /// </summary>
        public string TimeEnd => _timeEnd;

        /// <summary>
        ///     订单总金额，单位为分
        /// </summary>
        public string TotalFee => _totalFee;

        /// <summary>
        ///     现金支付金额订单现金支付金额，详见支付金额
        /// </summary>
        public string CashFee => _cashFee;
        #endregion

    }
}

