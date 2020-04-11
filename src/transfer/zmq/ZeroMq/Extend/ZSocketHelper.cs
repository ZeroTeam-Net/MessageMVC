using Agebull.Common;
using Agebull.EntityModel.Common;
using System.Text;
using ZeroTeam.MessageMVC;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public static class ZSocketHelper
    {
        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static string CreateRealName(bool isService, params string[] ranges)
        {
            var sb = new StringBuilder();
            sb.Append(isService ? "+<" : "+>");
            sb.Append(ZeroAppOption.Instance.AppName);
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            sb.Append("-");
            sb.Append(RandomCode.Generate(4));
            return sb.ToString();
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <returns></returns>
        public static byte[] CreateIdentity(bool isService = false)
        {
            var sb = new StringBuilder();
            sb.Append(isService ? "+<" : "+>");
            sb.Append(ZeroAppOption.Instance.AppName);
            sb.Append("-");
            sb.Append(RandomCode.Generate(4));
            return sb.ToString().ToBytes();
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static byte[] CreateIdentity(bool isService, params string[] ranges)
        {
            var sb = new StringBuilder();
            sb.Append(isService ? "+<" : "+>");
            sb.Append(ZeroAppOption.Instance.AppName);
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            sb.Append("-");
            sb.Append(RandomCode.Generate(4));
            return sb.ToString().ToBytes();
        }
    }
}