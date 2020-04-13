using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Wechart
{
    /// <summary>
    /// MPPayNotifyPage 的摘要说明
    /// </summary>
    public class WxPayRouter : IMessageMiddleware
    {
        #region IMessageMiddleware

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => int.MinValue;


        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope =>
            GatewayOption.Instance.EnableWxPay
                 ? MessageHandleScope.Prepare
                 : MessageHandleScope.None;
        /// <summary>
        /// 当前处理器
        /// </summary>
        MessageProcessor IMessageMiddleware.Processor { get; set; }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <returns></returns>
        async Task<bool> IMessageMiddleware.Prepare(IService service, IInlineMessage message, object tag)
        {
            if (message.Topic != WxPayOption.Instance.CallbackPath)
                return true;
            await OnCall(message as HttpMessage);
            return false;
        }
        #endregion

        #region 回调数据检查与结果发送

        /// <summary>
        ///     调用
        /// </summary>
        async Task OnCall(HttpMessage message)
        {
            try
            {
                message.Result = WxPayOption.Instance.SuccessXml;

                XmlDocument xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(message.Content.ToString());

                XmlElement item = (xml["xml"] ?? xml["XML"]) ?? xml["Xml"];
                if (!string.Equals("SUCCESS", item["return_code"].InnerText, StringComparison.OrdinalIgnoreCase))
                {
                    message.State = MessageState.Failed;
                    LogRecorder.MonitorTrace("Error: callback say error");
                    return;
                }
                var dic = ReadValue(item,out var payResult);

                var currentSign = GetSignString(dic);
                if (payResult.Sign != currentSign)
                {
                    message.State = MessageState.FormalError;
                    LogRecorder.MonitorTrace($"Error: Sign error({payResult.Sign}|{currentSign})");
                    return;
                }
                var postMessage = new MessageItem
                {
                    Topic = WxPayOption.Instance.Service,
                    Title = WxPayOption.Instance.Api,
                    Content = JsonHelper.SerializeObject(payResult)
                };
                var (msg, sec) = await MessagePoster.Post(postMessage);

                LogRecorder.MonitorTrace($"Publish : {msg.State} : {msg.Result})");
                message.State = msg.State;
            }
            catch (Exception ex)
            {
                message.RuntimeStatus = DependencyHelper.Create<IOperatorStatus>();
                message.RuntimeStatus.Exception = ex;
                if (message.ResultCreater != null)
                    message.ResultData = message.ResultCreater(DefaultErrorCode.UnhandleException, null);
                else
                    message.Result ??= ex.Message;

                message.State = MessageState.NetError;
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace($"Exception : {ex.Message})");
            }
        }


        /// <summary>
        ///     数据解析
        /// </summary>
        public Dictionary<string, string> ReadValue(XmlElement item,out WxPayResult result)
        {
            result = new WxPayResult();

            var dictionary = new Dictionary<string, string>();

            GetValue(item, dictionary, "appid", out result._appid);
            GetValue(item, dictionary, "bank_type", out result._bankType);
            GetValue(item, dictionary, "cash_fee", out result._cashFee);
            GetValue(item, dictionary, "fee_type", out result._feeType);
            GetValue(item, dictionary, "is_subscribe", out result._isSubscribe);
            GetValue(item, dictionary, "mch_id", out result._mchId);
            GetValue(item, dictionary, "nonce_str", out result._nonceStr);
            GetValue(item, dictionary, "openid", out result._openid);
            GetValue(item, dictionary, "out_trade_no", out result._outTradeNo);
            GetValue(item, dictionary, "result_code", out result._resultCode);
            GetValue(item, dictionary, "return_code", out result._returnCode);
            GetValue(item, dictionary, "sub_appid", out result._subAppid);
            GetValue(item, dictionary, "sub_is_subscribe", out result._subIsSubscribe);
            GetValue(item, dictionary, "sub_mch_id", out result._subMchId);
            GetValue(item, dictionary, "sub_openid", out result._subOpenid);
            GetValue(item, dictionary, "time_end", out result._timeEnd);
            GetValue(item, dictionary, "total_fee", out result._totalFee);
            GetValue(item, dictionary, "trade_type", out result._tradeType);
            GetValue(item, dictionary, "transaction_id", out result._transactionId);
            result._sign = item["sign"]?.InnerText;

            var fields = dictionary.OrderBy(p => p.Key).Aggregate("", (current, d) => $"{current}{d.Key}={d.Value}&");
            result.SignstrOrder = $"{fields}key={WxPayOption.Instance.Mchkey}";
            return dictionary;
        }

        internal void GetValue(XmlElement item, Dictionary<string, string> dic, string key, out string value)
        {
            value = item[key]?.InnerText;
            if (string.IsNullOrEmpty(value))
                return;
            //_signstr.Append("appid=" + value);
            dic.Add(key, value);
        }

        /// <summary>
        ///     将Dictionary转换为签名(场景：微信支付获取sign)
        /// </summary>
        /// <returns></returns>
        public string GetSignString(Dictionary<string, string> dic)
        {
            //string key = System.Web.Configuration.WebConfigurationManager.AppSettings["key"].ToString();//商户平台 API安全里面设置的KEY  32位长度  
            //排序  
            dic = dic.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            //连接字段  
            var sign = dic.Aggregate("", (current, d) => current + d.Key + "=" + d.Value + "&");
            sign += "key=" + WxPayOption.Instance.Mchkey;
            //MD5  
            // sign = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sign, "MD5").ToUpper();  
            var md5 = MD5.Create();
            sign = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(sign))).Replace("-", null);
            return sign;
        }

        #endregion
    }
}