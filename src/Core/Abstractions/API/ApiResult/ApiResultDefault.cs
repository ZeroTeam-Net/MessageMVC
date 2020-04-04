using Agebull.Common;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API���ػ���</summary>
    public class ApiResultDefault : IApiResultDefault
    {
        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IApiResult DeserializeObject(string json)
        {
            return JsonHelper.DeserializeObject<ApiResult>(json);
        }

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IApiResult<T> DeserializeObject<T>(string json)
        {
            return JsonHelper.DeserializeObject<ApiResult<T>>(json);
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public IApiResult Error(int errCode)
        {
            return new ApiResult
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message)
        {
            return ErrorBuilder(errCode, message);
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public static IApiResult ErrorBuilder(int errCode, string message = null)
        {
            return new ApiResult
            {
                Success = errCode == 0,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }
        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message, string innerMessage)
        {
            return new ApiResult
            {
                Success = errCode == 0,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == 0,
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

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult
            {
                Success = errCode == 0,
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

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult<TData> Succees<TData>(TData data)
        {
            return new ApiResult<TData>
            {
                Success = true,
                ResultData = data
            };
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage)
        {
            return new ApiResult<TData>
            {
                Success = errCode == 0,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode),
                InnerMessage = innerMessage
            };
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == 0,
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

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="point">�����</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = errCode == 0,
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

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult Error()
        {
            return ApiResultHelper.Error();
        }

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>()
        {
            return ApiResultHelper.Error<TData>();
        }

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult Succees()
        {
            return ApiResultHelper.Succees();
        }

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult<TData> Succees<TData>()
        {
            return ApiResultHelper.Succees<TData>();
        }

        /// <summary>�ɹ�</summary>
        /// <remarks>�ɹ�</remarks>
        public IApiResult Ok => ErrorBuilder(DefaultErrorCode.Success);

        /// <summary>ҳ�治����</summary>
        public IApiResult NoFind => ErrorBuilder(DefaultErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>��֧�ֵĲ���</summary>
        public IApiResult NotSupport => ErrorBuilder(DefaultErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>���������ַ���</summary>
        public IApiResult ArgumentError => ErrorBuilder(DefaultErrorCode.ArgumentError, "��������");

        /// <summary>�߼������ַ���</summary>
        public IApiResult LogicalError => ErrorBuilder(DefaultErrorCode.LogicalError, "�߼�����");

        /// <summary>�ܾ�����</summary>
        public IApiResult DenyAccess => ErrorBuilder(DefaultErrorCode.DenyAccess);

        /// <summary>�������޷���ֵ���ַ���</summary>
        public IApiResult RemoteEmptyError => ErrorBuilder(DefaultErrorCode.RemoteError, "*�������޷���ֵ*");

        /// <summary>�����������쳣</summary>
        public IApiResult NetworkError => ErrorBuilder(DefaultErrorCode.NetworkError);

        /// <summary>���ش���</summary>
        public IApiResult LocalError => ErrorBuilder(DefaultErrorCode.LocalError);

        /// <summary>���ط����쳣</summary>
        public IApiResult LocalException => ErrorBuilder(DefaultErrorCode.LocalException);

        /// <summary>ϵͳδ����</summary>
        public IApiResult NoReady => ErrorBuilder(DefaultErrorCode.NoReady);

        /// <summary>��ͣ����</summary>
        public IApiResult Pause => ErrorBuilder(DefaultErrorCode.NoReady, "��ͣ����");

        /// <summary>δ֪����</summary>
        public IApiResult UnknowError => ErrorBuilder(DefaultErrorCode.LocalError, "δ֪����");

        /// <summary>���糬ʱ</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public IApiResult NetTimeOut => ErrorBuilder(DefaultErrorCode.NetworkError, "���糬ʱ");

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        public IApiResult ExecTimeOut => ErrorBuilder(DefaultErrorCode.RemoteError, "ִ�г�ʱ");

        /// <summary>�ڲ�����</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public IApiResult InnerError => ErrorBuilder(DefaultErrorCode.LocalError, "�ڲ�����");

        /// <summary>���񲻿���</summary>
        public IApiResult Unavailable => ErrorBuilder(DefaultErrorCode.Unavailable, "���񲻿���");
    }
}