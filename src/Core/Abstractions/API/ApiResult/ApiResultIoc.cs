using Agebull.Common.Ioc;
namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API���ػ���</summary>
    public static class ApiResultIoc
    {
        /// <summary>
        /// ApiResult�ĳ���
        /// </summary>
        private static IApiResultDefault _ioc;

        /// <summary>
        /// ApiResult�ĳ���
        /// </summary>
        public static IApiResultDefault Ioc =>
            _ioc ?? (_ioc = IocHelper.Create<IApiResultDefault>() ?? new ApiResultDefault());



        /// <summary>�ɹ���Json�ַ���</summary>
        /// <remarks>�ɹ�</remarks>
        public static string SucceesJson => JsonHelper.SerializeObject(Ioc.Ok);

        /// <summary>ҳ�治���ڵ�Json�ַ���</summary>
        public static string NoFindJson => JsonHelper.SerializeObject(Ioc.NoFind);

        /// <summary>ϵͳ��֧�ֵ�Json�ַ���</summary>
        public static string NotSupportJson => JsonHelper.SerializeObject(Ioc.NotSupport);

        /// <summary>���������ַ���</summary>
        public static string ArgumentErrorJson => JsonHelper.SerializeObject(Ioc.ArgumentError);

        /// <summary>�߼������ַ���</summary>
        public static string LogicalErrorJson => JsonHelper.SerializeObject(Ioc.LogicalError);

        /// <summary>�ܾ����ʵ�Json�ַ���</summary>
        public static string DenyAccessJson => JsonHelper.SerializeObject(Ioc.DenyAccess);

        /// <summary>�������޷���ֵ���ַ���</summary>
        public static string RemoteEmptyErrorJson => JsonHelper.SerializeObject(Ioc.RemoteEmptyError);

        /// <summary>�����������쳣</summary>
        public static string NetworkErrorJson => JsonHelper.SerializeObject(Ioc.NetworkError);

        /// <summary>���ش���</summary>
        public static string LocalErrorJson => JsonHelper.SerializeObject(Ioc.LocalError);

        /// <summary>���ط����쳣��Json�ַ���</summary>
        public static string LocalExceptionJson => JsonHelper.SerializeObject(Ioc.LocalException);

        /// <summary>ϵͳδ������Json�ַ���</summary>
        public static string NoReadyJson => JsonHelper.SerializeObject(Ioc.NoReady);

        /// <summary>��ͣ�����Json�ַ���</summary>
        public static string PauseJson => JsonHelper.SerializeObject(Ioc.Pause);

        /// <summary>δ֪�����Json�ַ���</summary>
        public static string UnknowErrorJson => JsonHelper.SerializeObject(Ioc.UnknowError);

        /// <summary>���糬ʱ��Json�ַ���</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public static string TimeOutJson => JsonHelper.SerializeObject(Ioc.NetTimeOut);

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        public static string ExecTimeOut => JsonHelper.SerializeObject(Ioc.ExecTimeOut);

        /// <summary>�ڲ������Json�ַ���</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public static string InnerErrorJson => JsonHelper.SerializeObject(Ioc.InnerError);

        /// <summary>���񲻿��õ�Json�ַ���</summary>
        public static string UnavailableJson => JsonHelper.SerializeObject(Ioc.Unavailable);
    }
}