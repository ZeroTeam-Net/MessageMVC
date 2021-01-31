using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API���ػ���</summary>
    public static class ApiResultHelper
    {
        #region ��̬����

        /// <summary>
        /// ApiResult�ĳ���
        /// </summary>
        private static IApiResultHelper helper;

        /// <summary>
        /// ApiResult�ĳ���
        /// </summary>
        public static IApiResultHelper Helper => helper ??= DependencyHelper.GetService<IApiResultHelper>();

        /// <summary>�ɹ���Json�ַ���</summary>
        /// <remarks>�ɹ�</remarks>
        public static string SucceesJson => Helper.Serialize(Helper.Ok);

        /// <summary>ҳ�治���ڵ�Json�ַ���</summary>
        public static string NoFindJson => Helper.Serialize(Helper.NoFind);

        /// <summary>ϵͳ��֧�ֵ�Json�ַ���</summary>
        public static string NotSupportJson => Helper.Serialize(Helper.NonSupport);

        /// <summary>���������ַ���</summary>
        public static string ArgumentErrorJson => Helper.Serialize(Helper.ArgumentError);

        /// <summary>�߼������ַ���</summary>
        public static string BusinessErrorJson => Helper.Serialize(Helper.BusinessError);

        /// <summary>�ܾ����ʵ�Json�ַ���</summary>
        public static string DenyAccessJson => Helper.Serialize(Helper.DenyAccess);


        /// <summary>�����������쳣</summary>
        public static string NetworkErrorJson => Helper.Serialize(Helper.NetworkError);

        /// <summary>���ط����쳣��Json�ַ���</summary>
        public static string BusinessExceptionJson => Helper.Serialize(Helper.BusinessException);

        /// <summary>ϵͳδ������Json�ַ���</summary>
        public static string NoReadyJson => Helper.Serialize(Helper.NoReady);

        /// <summary>��ͣ�����Json�ַ���</summary>
        public static string PauseJson => Helper.Serialize(Helper.Pause);

        /// <summary>δ֪�����Json�ַ���</summary>
        public static string UnknowErrorJson => Helper.Serialize(Helper.BusinessError);

        /// <summary>���糬ʱ��Json�ַ���</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public static string NetTimeOutJson => Helper.Serialize(Helper.NetTimeOut);

        /// <summary>���Ƴ�ʱ��Json�ַ���</summary>
        /// <remarks>���Ƴ�ʱ</remarks>
        public static string TokenTimeOutJson => Helper.Serialize(Helper.TokenTimeOut);

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        public static string ExecTimeOut => Helper.Serialize(Helper.ExecTimeOut);

        /// <summary>�ڲ������Json�ַ���</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public static string InnerErrorJson => Helper.Serialize(Helper.BusinessException);

        /// <summary>���񲻿��õ�Json�ַ���</summary>
        public static string UnavailableJson => Helper.Serialize(Helper.Unavailable);

        #endregion

        #region ���췽��

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static IApiResult Succees() => Helper.Succees();

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <returns></returns>
        public static IApiResult State(int code) => Helper.State(code);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message) => Helper.State(code, message);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message, string innerMessage) => Helper.State(code, message, innerMessage);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message, string innerMessage, string guide, string describe)
             => Helper.State(code, message, innerMessage, guide, describe);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static IApiResult State(int code, string message, string innerMessage, string point, string guide, string describe)
             => Helper.State(code, message, innerMessage, point, guide, describe);

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> Succees<TData>(TData data) => Helper.Succees<TData>(data);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code) => Helper.State<TData>(code);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message) => Helper.State<TData>(code, message);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message, string innerMessage) => Helper.State<TData>(code, message, innerMessage);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message, string innerMessage, string guide, string describe)
             => Helper.State<TData>(code, message, innerMessage, guide, describe);

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static IApiResult<TData> State<TData>(int code, string message, string innerMessage, string point, string guide, string describe)
             => Helper.State<TData>(code, message, point, innerMessage, guide, describe);


        #endregion

        #region ��̬����

        /// <summary>
        ///     ȡ���������еķ���
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> FromContext<TData>()
        {
            return Helper.State<TData>(GlobalContext.Current.Status.LastStatus.Code,
                GlobalContext.Current.Status.LastStatus.Message);
        }

        /// <summary>
        ///     ȡ���������еķ���
        /// </summary>
        /// <returns></returns>
        public static IApiResult FromContext()
        {
            return Helper.State(GlobalContext.Current.Status.LastStatus.Code,
                GlobalContext.Current.Status.LastStatus.Message);
        }

        #endregion
    }
}

/*


        #region Ԥ����

        /// <summary>
        ///     �ɹ�
        /// </summary>
        /// <remarks>�ɹ�</remarks>
        public static ApiResult Ok => Succees();

        /// <summary>
        ///     ҳ�治����
        /// </summary>
        public static ApiResult NoFind => Error(ErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>
        ///     ��֧�ֵĲ���
        /// </summary>
        public static ApiResult NonSupport => Error(ErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>
        ///     ���������ַ���
        /// </summary>
        public static ApiResult ArgumentError => Error(ErrorCode.ArgumentError, "��������");

        /// <summary>
        ///     �߼������ַ���
        /// </summary>
        public static ApiResult LogicalError => Error(ErrorCode.LogicalError, "�߼�����");

        /// <summary>
        ///     �ܾ�����
        /// </summary>
        public static ApiResult DenyAccess => Error(ErrorCode.DenyAccess);

        /// <summary>
        ///     �������޷���ֵ���ַ���
        /// </summary>
        public static ApiResult RemoteEmptyError => Error(ErrorCode.RemoteError, "*�������޷���ֵ*");

        /// <summary>
        ///     �����������쳣
        /// </summary>
        public static ApiResult NetworkError => Error(ErrorCode.NetworkError);

        /// <summary>
        ///     ���ش���
        /// </summary>
        public static ApiResult LocalError => Error(ErrorCode.LocalError);

        /// <summary>
        ///     ���ط����쳣
        /// </summary>
        public static ApiResult LocalException => Error(ErrorCode.LocalException);

        /// <summary>
        ///     ϵͳδ����
        /// </summary>
        public static ApiResult NoReady => Error(ErrorCode.NoReady);

        /// <summary>
        ///     ��ͣ����
        /// </summary>
        public static ApiResult Pause => Error(ErrorCode.NoReady, "��ͣ����");

        /// <summary>
        ///     δ֪����
        /// </summary>
        public static ApiResult UnknowError => Error(ErrorCode.LocalError, "δ֪����");

        /// <summary>
        ///     ���糬ʱ
        /// </summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public static ApiResult TimeOut => Error(ErrorCode.NetworkError, "���糬ʱ");

        /// <summary>
        ///     �ڲ�����
        /// </summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public static ApiResult InnerError => Error(ErrorCode.LocalError, "�ڲ�����");

        /// <summary>
        ///     ���񲻿���
        /// </summary>
        public static ApiResult Unavailable => Error(ErrorCode.Unavailable, "���񲻿���");


        #endregion

        #region JSON

        /// <summary>
        ///     �ɹ���Json�ַ���
        /// </summary>
        /// <remarks>�ɹ�</remarks>
        public static string SucceesJson => JsonConvert.SerializeObject(Ok);

        /// <summary>
        ///     ҳ�治���ڵ�Json�ַ���
        /// </summary>
        public static string NoFindJson => JsonConvert.SerializeObject(NoFind);

        /// <summary>
        ///     ϵͳ��֧�ֵ�Json�ַ���
        /// </summary>
        public static string NotSupportJson => JsonConvert.SerializeObject(NonSupport);

        /// <summary>
        ///     ���������ַ���
        /// </summary>
        public static string ArgumentErrorJson => JsonConvert.SerializeObject(ArgumentError);

        /// <summary>
        ///     �߼������ַ���
        /// </summary>
        public static string LogicalErrorJson => JsonConvert.SerializeObject(LogicalError);

        /// <summary>
        ///     �ܾ����ʵ�Json�ַ���
        /// </summary>
        public static string DenyAccessJson => JsonConvert.SerializeObject(DenyAccess);

        /// <summary>
        ///     �������޷���ֵ���ַ���
        /// </summary>
        public static string RemoteEmptyErrorJson => JsonConvert.SerializeObject(RemoteEmptyError);

        /// <summary>
        ///     �����������쳣
        /// </summary>
        public static string NetworkErrorJson => JsonConvert.SerializeObject(NetworkError);

        /// <summary>
        ///     ���ش���
        /// </summary>
        public static string LocalErrorJson => JsonConvert.SerializeObject(LocalError);

        /// <summary>
        ///     ���ط����쳣��Json�ַ���
        /// </summary>
        public static string LocalExceptionJson => JsonConvert.SerializeObject(LocalException);

        /// <summary>
        ///     ϵͳδ������Json�ַ���
        /// </summary>
        public static string NoReadyJson => JsonConvert.SerializeObject(NoReady);

        /// <summary>
        ///     ��ͣ�����Json�ַ���
        /// </summary>
        public static string PauseJson => JsonConvert.SerializeObject(Pause);

        /// <summary>
        ///     δ֪�����Json�ַ���
        /// </summary>
        public static string UnknowErrorJson => JsonConvert.SerializeObject(UnknowError);

        /// <summary>
        ///     ���糬ʱ��Json�ַ���
        /// </summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public static string TimeOutJson => JsonConvert.SerializeObject(TimeOut);

        /// <summary>
        ///     �ڲ������Json�ַ���
        /// </summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public static string InnerErrorJson => JsonConvert.SerializeObject(InnerError);

        /// <summary>
        ///     ���񲻿��õ�Json�ַ���
        /// </summary>
        public static string UnavailableJson => JsonConvert.SerializeObject(Unavailable);


        #endregion
*/
