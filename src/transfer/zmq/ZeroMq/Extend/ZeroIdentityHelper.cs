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

        string.IsNullOrWhiteSpace(ZeroAppOption.Instance.ZeroAddress) ||
                                          ZeroAppOption.Instance.ZeroAddress == "127.0.0.1" ||
                                          ZeroAppOption.Instance.ZeroAddress == "::1" ||
                                          ZeroAppOption.Instance.ZeroAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase)
     

        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetSubscriberAddress(string station, int port)
        {
            return !UseIpc
                ? $"tcp://{ZeroAppOption.Instance.ZeroAddress}:{port}"
                : $"ipc://{ZeroAppOption.Instance.RootPath}/ipc/{station}_sub.ipc";
        }

        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetWorkerAddress(string station, int port)
        {
            return $"tcp://{ZeroAppOption.Instance.ZeroAddress}:{port}";
        }

        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetAddress(string station, int port)
        {
            var cfg = ZeroAppOption.Instance[station];
            if (cfg == null)
                return null;
            if (string.IsNullOrWhiteSpace(cfg.Address))
                return $"tcp://{ZeroAppOption.Instance.ZeroAddress}:{port}";
            return $"tcp://{cfg.Address}:{port}";
        }*/
    }
}