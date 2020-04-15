namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// ApiResult�����⻯
    /// </summary>
    public interface IApiResultHelper
    {
        /// <summary>
        /// ���л�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        string Serialize<T>(T t);

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult Deserialize(string str);

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult<T> Deserialize<T>(string str);

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult<T> DeserializeInterface<T>(string str);

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        IApiResult Succees();

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        IApiResult Error(int errCode);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe);

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        IApiResult<TData> Succees<TData>(TData data);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message"></param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe);

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe);

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        IApiResult Error();

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        IApiResult<TData> Error<TData>();

        /// <summary>�ɹ�</summary>
        /// <remarks>�ɹ�</remarks>
        IApiResult Ok { get; }

        /// <summary>ҳ�治����</summary>
        IApiResult NoFind { get; }

        /// <summary>��֧�ֵĲ���</summary>
        IApiResult NonSupport { get; }

        /// <summary>��������</summary>
        IApiResult ArgumentError { get; }

        /// <summary>�ܾ�����</summary>
        IApiResult DenyAccess { get; }

        /// <summary>
        /// �����쳣
        /// </summary>
        IApiResult BusinessException { get; }

        /// <summary>
        /// ϵͳ�쳣
        /// </summary>
        IApiResult UnhandleException { get; }

        /// <summary>ϵͳδ����</summary>
        IApiResult NoReady { get; }

        /// <summary>��ͣ����</summary>
        IApiResult Pause { get; }

        /// <summary>ҵ�����</summary>
        IApiResult BusinessError { get; }

        /// <summary>�����쳣</summary>
        IApiResult NetworkError { get; }

        /// <summary>���糬ʱ</summary>
        IApiResult NetTimeOut { get; }

        /// <summary>Apiִ�г�ʱ</summary>
        IApiResult ExecTimeOut { get; }

        /// <summary>���񲻿���</summary>
        IApiResult Unavailable { get; }

        /// <summary>�ȴ�������</summary>
        IApiResult Waiting { get; }

    }
}