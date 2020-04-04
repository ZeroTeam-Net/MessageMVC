using Agebull.Common;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API���ػ���</summary>
    public static class ApiResultHelper
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

        #region ���췽��

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode)
        {
            return new ApiResult
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }
        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Point = point,
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Succees<TData>(TData data, string message = null)
        {
            return new ApiResult<TData>
            {
                Success = true,
                ResultData = data,
                Message = message
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiResult<TData>
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Point = point,
                    Guide = guide,
                    Describe = describe
                }
            };
        }
        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiResult Error()
        {
            var result = new ApiResult
            {
                Success = false
            };
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Error<TData>()
        {
            var result = new ApiResult<TData>
            {
                Success = false
            };
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }
        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiResult Succees()
        {
            var result = new ApiResult();
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Succees<TData>()
        {
            var result = new ApiResult<TData>();
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
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
        public static ApiResult NotSupport => Error(ErrorCode.NoFind, "*ҳ�治����*");

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
        public static string NotSupportJson => JsonConvert.SerializeObject(NotSupport);

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
