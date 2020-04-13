using Agebull.Common.Configuration;

namespace ZeroTeam.MessageMVC.Http
{

    /// <summary>
    ///     ��������
    /// </summary>
    public class GatewayOption
    {
        /// <summary>
        /// ���ð�ȫУ��
        /// </summary>
        public bool EnableSecurityChecker { get; set; }

        /// <summary>
        /// ����ǰ�˻���
        /// </summary>
        public bool EnableCache { get; set; }

        /// <summary>
        /// ����΢��֧���ص����
        /// </summary>
        public bool EnableWxPay { get; set; }

        #region �����Զ�����

        /// <summary>
        /// ��̬ʵ��
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