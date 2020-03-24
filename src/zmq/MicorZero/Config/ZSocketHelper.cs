using Agebull.EntityModel.Common;
using System.Text;
using ZeroTeam.MessageMVC;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    internal static class ZSocketHelper
    {

        #region Identity

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
            sb.Append(ZeroFlowControl.Config.AppName);
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            sb.Append("-");
            sb.Append(RandomOperate.Generate(4));
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
            sb.Append(ZeroFlowControl.Config.AppName);
            sb.Append("-");
            sb.Append(RandomOperate.Generate(4));
            return sb.ToString().ToZeroBytes();
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
            sb.Append(ZeroFlowControl.Config.AppName);
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            sb.Append("-");
            sb.Append(RandomOperate.Generate(4));
            return sb.ToString().ToZeroBytes();
        }
        #endregion
    }
}