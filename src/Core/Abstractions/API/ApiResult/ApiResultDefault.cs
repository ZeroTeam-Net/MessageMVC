using Agebull.Common;
using ZeroTeam.MessageMVC.Context;

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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = ErrorCode.GetMessage(errCode)
                }
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode)
                }
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage
                }
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage,
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage,
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = ErrorCode.GetMessage(errCode)
                }
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode)
                }
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage
                }
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage,
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
                Status = new OperatorStatus
                {
                    Code = errCode,
                    Message = message ?? ErrorCode.GetMessage(errCode),
                    InnerMessage = innerMessage,
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
            return new ApiResult
            {
                Success = false,
                Status =GlobalContext.CurrentNoLazy?.LastStatus
            };
        }

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult<TData> Error<TData>()
        {
            return new ApiResult<TData>
            {
                Success = false,
                Status =GlobalContext.CurrentNoLazy?.LastStatus
            };
        }

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult Succees()
        {
            return new ApiResult
            {
                Success = true,
                Status =GlobalContext.CurrentNoLazy?.LastStatus
            };
        }

        /// <summary>����һ���ɹ��ı�׼����</summary>
        /// <returns></returns>
        public IApiResult<TData> Succees<TData>()
        {
            return new ApiResult<TData>
            {
                Success = true,
                Status =GlobalContext.CurrentNoLazy?.LastStatus
            };
        }

        /// <summary>�ɹ�</summary>
        /// <remarks>�ɹ�</remarks>
        public IApiResult Ok => ErrorBuilder(ErrorCode.Success);

        /// <summary>ҳ�治����</summary>
        public IApiResult NoFind => ErrorBuilder(ErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>��֧�ֵĲ���</summary>
        public IApiResult NotSupport => ErrorBuilder(ErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>���������ַ���</summary>
        public IApiResult ArgumentError => ErrorBuilder(ErrorCode.ArgumentError, "��������");

        /// <summary>�߼������ַ���</summary>
        public IApiResult LogicalError => ErrorBuilder(ErrorCode.LogicalError, "�߼�����");

        /// <summary>�ܾ�����</summary>
        public IApiResult DenyAccess => ErrorBuilder(ErrorCode.DenyAccess);

        /// <summary>�������޷���ֵ���ַ���</summary>
        public IApiResult RemoteEmptyError => ErrorBuilder(ErrorCode.RemoteError, "*�������޷���ֵ*");

        /// <summary>�����������쳣</summary>
        public IApiResult NetworkError => ErrorBuilder(ErrorCode.NetworkError);

        /// <summary>���ش���</summary>
        public IApiResult LocalError => ErrorBuilder(ErrorCode.LocalError);

        /// <summary>���ط����쳣</summary>
        public IApiResult LocalException => ErrorBuilder(ErrorCode.LocalException);

        /// <summary>ϵͳδ����</summary>
        public IApiResult NoReady => ErrorBuilder(ErrorCode.NoReady);

        /// <summary>��ͣ����</summary>
        public IApiResult Pause => ErrorBuilder(ErrorCode.NoReady, "��ͣ����");

        /// <summary>δ֪����</summary>
        public IApiResult UnknowError => ErrorBuilder(ErrorCode.LocalError, "δ֪����");

        /// <summary>���糬ʱ</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public IApiResult NetTimeOut => ErrorBuilder(ErrorCode.NetworkError, "���糬ʱ");

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        public IApiResult ExecTimeOut => ErrorBuilder(ErrorCode.RemoteError, "ִ�г�ʱ");

        /// <summary>�ڲ�����</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public IApiResult InnerError => ErrorBuilder(ErrorCode.LocalError, "�ڲ�����");

        /// <summary>���񲻿���</summary>
        public IApiResult Unavailable => ErrorBuilder(ErrorCode.Unavailable, "���񲻿���");
    }
}