using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     操作状态码
    /// </summary>
    public class OperatorStatusCode
    {
        /// <summary>
        ///     正在排队
        /// </summary>
        public const int Queue = 1;

        /// <summary>
        ///     成功
        /// </summary>
        public const int Success = 0;

        /// <summary>
        ///     参数错误
        /// </summary>
        public const int ArgumentError = -1;

        /// <summary>
        ///     发生处理业务错误
        /// </summary>
        public const int BusinessError = -2;

        /// <summary>
        ///     发生未处理业务异常
        /// </summary>
        public const int BusinessException = -3;

        /// <summary>
        ///     发生未处理系统异常
        /// </summary>
        public const int UnhandleException = -4;

        /// <summary>
        ///     网络错误
        /// </summary>
        public const int NetworkError = -5;

        /// <summary>
        ///     执行超时
        /// </summary>
        public const int TimeOut = -6;

        /// <summary>
        ///     拒绝访问
        /// </summary>
        public const int DenyAccess = -7;

        /// <summary>
        ///     未知的令牌
        /// </summary>
        public const int TokenUnknow = -8;

        /// <summary>
        ///     令牌过期
        /// </summary>
        public const int TokenTimeOut = -9;

        /// <summary>
        ///     系统未就绪
        /// </summary>
        public const int NoReady = -0xA;

        /// <summary>
        ///     异常中止
        /// </summary>
        public const int Ignore = -0xB;

        /// <summary>
        ///     重试
        /// </summary>
        public const int ReTry = -0xC;

        /// <summary>
        ///     方法不存在
        /// </summary>
        public const int NoFind = -0xD;

        /// <summary>
        ///     服务不可用
        /// </summary>
        public const int Unavailable = -0xE;

        /// <summary>
        ///     未知结果
        /// </summary>
        public const int Unknow = 0xF;

        #region 消息字典

        private static readonly Dictionary<int, string> Map = new Dictionary<int, string>
        {
            {Success, "操作成功"},
            {ArgumentError, "参数错误"},
            {BusinessError, "逻辑错误"},
            {TimeOut, "执行超时"},
            {BusinessException, "业务异常"},
            {UnhandleException, "系统异常"},
            {NetworkError, "网络错误"},
            {DenyAccess, "拒绝访问"},
            {NoReady, "系统未就绪"},
            {ReTry, "请重试请求"},
            {Ignore, "异常中止"},
            {NoFind, "页面不存在"},
            {TokenUnknow, "未知令牌"},
            {TokenTimeOut, "令牌过期"},
            {Unavailable, "服务不可用"},
            {Queue, "正在排队"},
            {Unknow, "未知结果"}
        };

        /// <summary>
        ///     取得错误码对应的消息文本
        /// </summary>
        /// <param name="eid">错误码</param>
        /// <returns>消息文本</returns>
        public static string GetMessage(int eid)
        {
            return Map.TryGetValue(eid, out var result) ? result : "发生未知错误";
        }

        #endregion
    }
}