using Agebull.Common.Ioc;
using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     操作状态码
    /// </summary>
    public class OperatorStatusCode
    {
        static IOperatorStatusCode statusCode;

        static IOperatorStatusCode StatusCode => statusCode ??= DependencyHelper.GetService<IOperatorStatusCode>() ?? new DefaultOperatorStatusCode();

        /// <summary>
        ///     正在排队
        /// </summary>
        public static int Queue => StatusCode.Queue; // 1;

        /// <summary>
        ///     成功
        /// </summary>
        public static int Success => StatusCode.Success; // 0;

        /// <summary>
        ///     参数错误
        /// </summary>
        public static int ArgumentError => StatusCode.ArgumentError; // -1;

        /// <summary>
        ///     发生处理业务错误
        /// </summary>
        public static int BusinessError => StatusCode.BusinessError; // -2;

        /// <summary>
        ///     发生未处理业务异常
        /// </summary>
        public static int BusinessException => StatusCode.BusinessException; // -3;

        /// <summary>
        ///     发生未处理系统异常
        /// </summary>
        public static int UnhandleException => StatusCode.UnhandleException; // -4;

        /// <summary>
        ///     网络错误
        /// </summary>
        public static int NetworkError => StatusCode.NetworkError; // -5;

        /// <summary>
        ///     执行超时
        /// </summary>
        public static int TimeOut => StatusCode.TimeOut; // -6;

        /// <summary>
        ///     拒绝访问
        /// </summary>
        public static int DenyAccess => StatusCode.DenyAccess; // -7;

        /// <summary>
        ///     未知的令牌
        /// </summary>
        public static int TokenUnknow => StatusCode.TokenUnknow; // -8;

        /// <summary>
        ///     令牌过期
        /// </summary>
        public static int TokenTimeOut => StatusCode.TokenTimeOut; // -9;

        /// <summary>
        ///     系统未就绪
        /// </summary>
        public static int NoReady => StatusCode.NoReady; // -0xA;

        /// <summary>
        ///     异常中止
        /// </summary>
        public static int Ignore => StatusCode.Ignore; // -0xB;

        /// <summary>
        ///     重试
        /// </summary>
        public static int ReTry => StatusCode.ReTry; // -0xC;

        /// <summary>
        ///     方法不存在
        /// </summary>
        public static int NoFind => StatusCode.NoFind; // -0xD;

        /// <summary>
        ///     服务不可用
        /// </summary>
        public static int Unavailable => StatusCode.Unavailable; // -0xE;

        /// <summary>
        ///     未知结果
        /// </summary>
        public static int Unknow => StatusCode.Unknow; // 0xF;

        #region 消息字典

        /// <summary>
        ///     取得错误码对应的消息文本
        /// </summary>
        /// <param name="eid">错误码</param>
        /// <returns>消息文本</returns>
        public static string GetMessage(int eid)
        {
            return StatusCode.Map.TryGetValue(eid, out var result) ? result : "发生未知错误";
        }

        #endregion
    }


    /// <summary>
    ///     操作状态码
    /// </summary>
    public interface IOperatorStatusCode
    {
        /// <summary>
        /// 文本表
        /// </summary>
        Dictionary<int, string> Map { get; }

        /// <summary>
        ///     正在排队
        /// </summary>
        int Queue { get; } // 1;

        /// <summary>
        ///     成功
        /// </summary>
        int Success { get; } // 0;

        /// <summary>
        ///     参数错误
        /// </summary>
        int ArgumentError { get; } // -1;

        /// <summary>
        ///     发生处理业务错误
        /// </summary>
        int BusinessError { get; } // -2;

        /// <summary>
        ///     发生未处理业务异常
        /// </summary>
        int BusinessException { get; } // -3;

        /// <summary>
        ///     发生未处理系统异常
        /// </summary>
        int UnhandleException { get; } // -4;

        /// <summary>
        ///     网络错误
        /// </summary>
        int NetworkError { get; } // -5;

        /// <summary>
        ///     执行超时
        /// </summary>
        int TimeOut { get; } // -6;

        /// <summary>
        ///     拒绝访问
        /// </summary>
        int DenyAccess { get; } // -7;

        /// <summary>
        ///     未知的令牌
        /// </summary>
        int TokenUnknow { get; } // -8;

        /// <summary>
        ///     令牌过期
        /// </summary>
        int TokenTimeOut { get; } // -9;

        /// <summary>
        ///     系统未就绪
        /// </summary>
        int NoReady { get; } // -0xA;

        /// <summary>
        ///     异常中止
        /// </summary>
        int Ignore { get; } // -0xB;

        /// <summary>
        ///     重试
        /// </summary>
        int ReTry { get; } // -0xC;

        /// <summary>
        ///     方法不存在
        /// </summary>
        int NoFind { get; } // -0xD;

        /// <summary>
        ///     服务不可用
        /// </summary>
        int Unavailable { get; } // -0xE;

        /// <summary>
        ///     未知结果
        /// </summary>
        int Unknow { get; } // 0xF;

    }

    /// <summary>
    ///     操作状态码
    /// </summary>
    internal class DefaultOperatorStatusCode : IOperatorStatusCode
    {
        /// <summary>
        /// 文本表
        /// </summary>
        public Dictionary<int, string> Map { get; } = new Dictionary<int, string>
        {
            {1, "正在排队"},
            {0, "操作成功"},
            {-1, "参数错误"},
            {-2, "逻辑错误"},
            {-3, "业务异常"},
            {-4, "系统异常"},
            {-5, "网络错误"},
            {-6, "执行超时"},
            {-7, "拒绝访问"},
            {-8, "未知令牌"},
            {-9, "令牌过期"},
            {-0xA, "系统未就绪"},
            {-0xC, "请重试请求"},
            {-0xB, "异常中止"},
            {-0xD, "页面不存在"},
            {-0xE, "服务不可用"},
            {-0xF, "未知结果"}
        };

        /// <summary>
        ///     正在排队
        /// </summary>
        public int Queue => 1;

        /// <summary>
        ///     成功
        /// </summary>
        public int Success => 0;

        /// <summary>
        ///     参数错误
        /// </summary>
        public int ArgumentError => -1;

        /// <summary>
        ///     发生处理业务错误
        /// </summary>
        public int BusinessError => -2;

        /// <summary>
        ///     发生未处理业务异常
        /// </summary>
        public int BusinessException => -3;

        /// <summary>
        ///     发生未处理系统异常
        /// </summary>
        public int UnhandleException => -4;

        /// <summary>
        ///     网络错误
        /// </summary>
        public int NetworkError => -5;

        /// <summary>
        ///     执行超时
        /// </summary>
        public int TimeOut => -6;

        /// <summary>
        ///     拒绝访问
        /// </summary>
        public int DenyAccess => -7;

        /// <summary>
        ///     未知的令牌
        /// </summary>
        public int TokenUnknow => -8;

        /// <summary>
        ///     令牌过期
        /// </summary>
        public int TokenTimeOut => -9;

        /// <summary>
        ///     系统未就绪
        /// </summary>
        public int NoReady => -0xA;

        /// <summary>
        ///     异常中止
        /// </summary>
        public int Ignore => -0xB;

        /// <summary>
        ///     重试
        /// </summary>
        public int ReTry => -0xC;

        /// <summary>
        ///     方法不存在
        /// </summary>
        public int NoFind => -0xD;

        /// <summary>
        ///     服务不可用
        /// </summary>
        public int Unavailable => -0xE;

        /// <summary>
        ///     未知结果
        /// </summary>
        public int Unknow => -0xF;

    }
}