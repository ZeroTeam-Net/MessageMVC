namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    ///     命名辅助类
    /// </summary>
    internal static class ZeroAddressHelper
    {

        /*
        /// <summary>
        /// 是否本机
        /// </summary>
        /// <returns></returns>
        public static bool UseIpc { get; set; }

        string.IsNullOrWhiteSpace(ZeroFlowControl.Config.ZeroAddress) ||
                                          ZeroFlowControl.Config.ZeroAddress == "127.0.0.1" ||
                                          ZeroFlowControl.Config.ZeroAddress == "::1" ||
                                          ZeroFlowControl.Config.ZeroAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase)
     

        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetSubscriberAddress(string station, int port)
        {
            return !UseIpc
                ? $"tcp://{ZeroFlowControl.Config.ZeroAddress}:{port}"
                : $"ipc://{ZeroFlowControl.Config.RootPath}/ipc/{station}_sub.ipc";
        }

        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetWorkerAddress(string station, int port)
        {
            return $"tcp://{ZeroFlowControl.Config.ZeroAddress}:{port}";
        }

        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetAddress(string station, int port)
        {
            var cfg = ZeroFlowControl.Config[station];
            if (cfg == null)
                return null;
            if (string.IsNullOrWhiteSpace(cfg.Address))
                return $"tcp://{ZeroFlowControl.Config.ZeroAddress}:{port}";
            return $"tcp://{cfg.Address}:{port}";
        }*/
    }
}