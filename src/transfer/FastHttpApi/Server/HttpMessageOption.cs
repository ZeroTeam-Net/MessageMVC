using Agebull.Common.Configuration;
using ZeroTeam.MessageMVC;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpMessageOption : IZeroOption
    {
        /// <summary>
        /// 服务配置
        /// </summary>
        public HttpOptions ServerOption { get; set; }


        #region IZeroOption

        /// <summary>
        /// 实例
        /// </summary>
        public static HttpMessageOption Instance = new HttpMessageOption();


        const string sectionName = "FastHttpApi";

        const string optionName = "FastHttpApi服务配置";

        const string supperUrl = "https://";

        /// <summary>
        /// 支持地址
        /// </summary>
        string IZeroOption.SupperUrl => supperUrl;

        /// <summary>
        /// 配置名称
        /// </summary>
        string IZeroOption.OptionName => optionName;


        /// <summary>
        /// 节点名称
        /// </summary>
        string IZeroOption.SectionName => sectionName;

        /// <summary>
        /// 是否动态配置
        /// </summary>
        bool IZeroOption.IsDynamic => false;

        void IZeroOption.Load(bool first)
        {
            ServerOption = ConfigurationHelper.Get<HttpOptions>(sectionName);
            if (ServerOption == null)
                throw new ZeroOptionException(optionName, sectionName);
            if (ServerOption.Port < 1024 || ServerOption.Port > 65535)
                throw new ZeroOptionException(optionName, sectionName, "端口号配置不正确，应在1025-65534之间");

        }
        #endregion
    }
}
