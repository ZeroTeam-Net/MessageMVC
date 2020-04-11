using Agebull.Common.Configuration;
using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC.Wechart
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WxPayOption
    {
        /// <summary>
        /// 微信回调的路径
        /// </summary>
        public string CallbackPath { get; set; }

        /// <summary>
        /// 掩码
        /// </summary>
        public string Mchkey { get; set; }

        /// <summary>
        /// 实际处理的服务
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// 实际处理的接口
        /// </summary>
        public string Api { get; set; }

        /// <summary>
        /// 成功返回微信的文本
        /// </summary>
        public string SuccessXml { get; set; } =
            @"<xml>
   <return_code><![CDATA[SUCCESS]]></return_code>
   <return_msg><![CDATA[OK]]></return_msg>
</xml>";


        public static WxPayOption Instance = new WxPayOption();



        #region 配置自动更新

        static WxPayOption()
        {
            ConfigurationManager.RegistOnChange(LoadOption, true);

        }
        static void LoadOption()
        {
            var option = ConfigurationManager.Option<WxPayOption>("WxPay");
            if (option == null)
                return;
            Instance.CallbackPath = option.CallbackPath;
            Instance.Mchkey = option.Mchkey;
            Instance.Service = option.Service;
            Instance.Api = option.Api;
            Instance.SuccessXml = option.SuccessXml;
        }

        #endregion
    }
}