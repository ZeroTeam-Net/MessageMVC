using Agebull.Common;
using Agebull.Common.Ioc;
using System.Collections.Generic;
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
        private static IApiResultDefault _ioc;

        /// <summary>
        /// ApiResult�ĳ���
        /// </summary>
        public static IApiResultDefault Ioc => _ioc ??= IocHelper.Create<IApiResultDefault>();

        /// <summary>�ɹ���Json�ַ���</summary>
        /// <remarks>�ɹ�</remarks>
        public static string SucceesJson => Ioc.SerializeObject(Ioc.Ok);

        /// <summary>ҳ�治���ڵ�Json�ַ���</summary>
        public static string NoFindJson => Ioc.SerializeObject(Ioc.NoFind);

        /// <summary>ϵͳ��֧�ֵ�Json�ַ���</summary>
        public static string NotSupportJson => Ioc.SerializeObject(Ioc.NotSupport);

        /// <summary>���������ַ���</summary>
        public static string ArgumentErrorJson => Ioc.SerializeObject(Ioc.ArgumentError);

        /// <summary>�߼������ַ���</summary>
        public static string LogicalErrorJson => Ioc.SerializeObject(Ioc.LogicalError);

        /// <summary>�ܾ����ʵ�Json�ַ���</summary>
        public static string DenyAccessJson => Ioc.SerializeObject(Ioc.DenyAccess);

        /// <summary>�������޷���ֵ���ַ���</summary>
        public static string RemoteEmptyErrorJson => Ioc.SerializeObject(Ioc.RemoteEmptyError);

        /// <summary>�����������쳣</summary>
        public static string NetworkErrorJson => Ioc.SerializeObject(Ioc.NetworkError);

        /// <summary>���ش���</summary>
        public static string LocalErrorJson => Ioc.SerializeObject(Ioc.LocalError);

        /// <summary>���ط����쳣��Json�ַ���</summary>
        public static string LocalExceptionJson => Ioc.SerializeObject(Ioc.LocalException);

        /// <summary>ϵͳδ������Json�ַ���</summary>
        public static string NoReadyJson => Ioc.SerializeObject(Ioc.NoReady);

        /// <summary>��ͣ�����Json�ַ���</summary>
        public static string PauseJson => Ioc.SerializeObject(Ioc.Pause);

        /// <summary>δ֪�����Json�ַ���</summary>
        public static string UnknowErrorJson => Ioc.SerializeObject(Ioc.UnknowError);

        /// <summary>���糬ʱ��Json�ַ���</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public static string TimeOutJson => Ioc.SerializeObject(Ioc.NetTimeOut);

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        public static string ExecTimeOut => Ioc.SerializeObject(Ioc.ExecTimeOut);

        /// <summary>�ڲ������Json�ַ���</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public static string InnerErrorJson => Ioc.SerializeObject(Ioc.InnerError);

        /// <summary>���񲻿��õ�Json�ַ���</summary>
        public static string UnavailableJson => Ioc.SerializeObject(Ioc.Unavailable);

        #endregion

        #region ���췽��

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static IApiResult Succees() => Ioc.Succees();

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode) => Ioc.Error(errCode);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode, string message) => Ioc.Error(errCode, message);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode, string message, string innerMessage) => Ioc.Error(errCode, message, innerMessage);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
             => Ioc.Error(errCode, message, innerMessage, guide, describe);

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
        public static IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
             => Ioc.Error(errCode, message, innerMessage, point, guide, describe);

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> Succees<TData>(TData data, string message = null) => Ioc.Succees<TData>(data, message);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode) => Ioc.Error<TData>(errCode);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode, string message) => Ioc.Error<TData>(errCode, message);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage) => Ioc.Error<TData>(errCode, message, innerMessage);

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public static IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
             => Ioc.Error<TData>(errCode, message, innerMessage, guide, describe);

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
        public static IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
             => Ioc.Error<TData>(errCode, message, point, innerMessage, guide, describe);


        #endregion

        #region ��̬����

        /// <summary>
        ///     ȡ���������еķ���
        /// </summary>
        /// <returns></returns>
        public static IApiResult<TData> FromContext<TData>()
        {
            return new ApiResult<TData>
            {
                Success = GlobalContext.Current.Status.LastStatus.Success,
                Code = GlobalContext.Current.Status.LastStatus.Code,
                Message = GlobalContext.Current.Status.LastStatus.Message
            };
        }

        /// <summary>
        ///     ȡ���������еķ���
        /// </summary>
        /// <returns></returns>
        public static IApiResult FromContext()
        {
            return new ApiResult
            {
                Success = GlobalContext.Current.Status.LastStatus.Success,
                Code = GlobalContext.Current.Status.LastStatus.Code,
                Message = GlobalContext.Current.Status.LastStatus.Message
            };
        }

        #region Array

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArrayError<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiArrayResult<TData>
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
        public static ApiArrayResult<TData> ArraySuccees<TData>(List<TData> data, string message = null)
        {
            return new ApiArrayResult<TData>
            {
                Success = true,
                ResultData = data,
                Message = message
            };
        }

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiArrayResult<TData> ArrayError<TData>()
        {
            var result = new ApiArrayResult<TData>();
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Success = GlobalContext.Current.Status.LastStatus.Success;
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }
        #endregion

        #region Value

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiValueResult ValueSuccees(string data, string message = null)
        {
            return new ApiValueResult
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
        public static ApiValueResult ValueError(int errCode)
        {
            return new ApiValueResult
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
        public static ApiValueResult ValueError(int errCode, string message)
        {
            return new ApiValueResult
            {
                Success = false,
                Code = errCode,
                Message = message
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message"></param>
        /// <param name="innerMessage"></param>
        /// <returns></returns>
        public static ApiValueResult ValueError(int errCode, string message, string innerMessage)
        {
            return new ApiValueResult
            {
                Success = false,
                Code = errCode,
                Message = message,
                InnerMessage = innerMessage
            };
        }

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public static ApiValueResult<TData> ValueSuccees<TData>(TData data, string message = null)
        {
            return new ApiValueResult<TData>
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
        public static ApiValueResult<TData> ValueError<TData>(int errCode)
        {
            return new ApiValueResult<TData>
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
        public static ApiValueResult<TData> ValueError<TData>(int errCode, string message)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message"></param>
        /// <param name="innerMessage"></param>
        /// <returns></returns>
        public static ApiValueResult<TData> ValueError<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message,
                InnerMessage = innerMessage
            };
        }

        #endregion

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
