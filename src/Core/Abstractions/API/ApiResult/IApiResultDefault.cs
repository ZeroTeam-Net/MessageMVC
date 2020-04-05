namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// ApiResult�����⻯
    /// </summary>
    public interface IApiResultDefault
    {
        /// <summary>
        /// ���л�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        string SerializeObject<T>(T t);

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        IApiResult DeserializeObject(string json);

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        IApiResult<T> DeserializeObject<T>(string json);

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        IApiResult Succees(string message);

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
        IApiResult<TData> Succees<TData>(TData data, string message);

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
        IApiResult NotSupport { get; }

        /// <summary>���������ַ���</summary>
        IApiResult ArgumentError { get; }

        /// <summary>�߼������ַ���</summary>
        IApiResult LogicalError { get; }

        /// <summary>�ܾ�����</summary>
        IApiResult DenyAccess { get; }

        /// <summary>�������޷���ֵ���ַ���</summary>
        IApiResult RemoteEmptyError { get; }

        /// <summary>�����������쳣</summary>
        IApiResult NetworkError { get; }

        /// <summary>���ش���</summary>
        IApiResult LocalError { get; }

        /// <summary>���ط����쳣</summary>
        IApiResult LocalException { get; }

        /// <summary>ϵͳδ����</summary>
        IApiResult NoReady { get; }

        /// <summary>��ͣ����</summary>
        IApiResult Pause { get; }

        /// <summary>δ֪����</summary>
        IApiResult UnknowError { get; }

        /// <summary>���糬ʱ</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        IApiResult NetTimeOut { get; }

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        IApiResult ExecTimeOut { get; }

        /// <summary>�ڲ�����</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        IApiResult InnerError { get; }

        /// <summary>���񲻿���</summary>
        IApiResult Unavailable { get; }

    }
}