namespace System
{
    /// <summary>
    /// Unix时间互转
    /// </summary>
    public static class UnixTimeConvert
    {
        /// <summary>
        /// 将Unix时间戳转换为DateTime类型时间
        /// </summary>
        /// <param name="unix">double 型数字</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(this int unix)
        {
            return new DateTime(1970, 1, 1).AddSeconds(unix);
        }

        /// <summary>
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>long</returns>
        public static int ToTimestamp(this DateTime time)
        {
            return (int)(time - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
