namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// ApiResult的虚拟化
    /// </summary>
    public interface IApiResultHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        string Serialize<T>(T t);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult Deserialize(string str);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult<T> Deserialize<T>(string str);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult<T> DeserializeInterface<T>(string str);

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult Succees();

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        IApiResult Error(int errCode);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe);

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult<TData> Succees<TData>(TData data);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe);

        /// <summary>生成一个包含错误码的标准返回</summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="innerMessage">内部说明</param>
        /// <param name="point">错误点</param>
        /// <param name="guide">错误指导</param>
        /// <param name="describe">错误解释</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe);

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult Error();

        /// <summary>生成一个成功的标准返回</summary>
        /// <returns></returns>
        IApiResult<TData> Error<TData>();

        /// <summary>成功</summary>
        /// <remarks>成功</remarks>
        IApiResult Ok { get; }

        /// <summary>页面不存在</summary>
        IApiResult NoFind { get; }

        /// <summary>不支持的操作</summary>
        IApiResult NonSupport { get; }

        /// <summary>参数错误</summary>
        IApiResult ArgumentError { get; }

        /// <summary>拒绝访问</summary>
        IApiResult DenyAccess { get; }

        /// <summary>
        /// 服务异常
        /// </summary>
        IApiResult BusinessException { get; }

        /// <summary>
        /// 系统异常
        /// </summary>
        IApiResult UnhandleException { get; }

        /// <summary>系统未就绪</summary>
        IApiResult NoReady { get; }

        /// <summary>暂停服务</summary>
        IApiResult Pause { get; }

        /// <summary>业务错误</summary>
        IApiResult BusinessError { get; }

        /// <summary>网络异常</summary>
        IApiResult NetworkError { get; }

        /// <summary>网络超时</summary>
        IApiResult NetTimeOut { get; }

        /// <summary>Api执行超时</summary>
        IApiResult ExecTimeOut { get; }

        /// <summary>服务不可用</summary>
        IApiResult Unavailable { get; }

        /// <summary>等待处理中</summary>
        IApiResult Waiting { get; }

    }
}