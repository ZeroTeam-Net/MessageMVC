using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.Http
{

    /// <summary>
    ///     缓存设置
    /// </summary>
    public class GatewayOption
    {
        /// <summary>
        /// 启用安全校验
        /// </summary>
        public bool EnableSecurityChecker { get; set; }

        /// <summary>
        /// 启用前端缓存
        /// </summary>
        public bool EnableCache { get; set; }

        /// <summary>
        /// 启用微信支付回调入口
        /// </summary>
        public bool EnableWxPay { get; set; }

        #region 配置自动更新

        /// <summary>
        /// 静态实例
        /// </summary>
        public readonly static GatewayOption Instance = new GatewayOption
        {
            EnableSecurityChecker = true,
            EnableCache = true,
        };

        static GatewayOption()
        {
            ConfigurationManager.RegistOnChange("Gateway:Tools",Instance.LoadOption, true);

        }
        void LoadOption()
        {
            var option = ConfigurationManager.Option<GatewayOption>("Gateway:Tools");
            EnableSecurityChecker = option.EnableSecurityChecker;
            EnableCache = option.EnableCache;
            EnableWxPay = option.EnableWxPay;
        }
        #endregion

    }
}