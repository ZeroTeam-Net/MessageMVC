using Agebull.Common;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API���ػ���</summary>
    internal class ApiResultDefault : IApiResultDefault
    {
        #region ���л�

        /// <summary>
        /// ���л�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public string SerializeObject<T>(T t)
        {
            return JsonHelper.SerializeObject(t);
        }

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
        #endregion

        #region ��������

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public IApiResult Build(int errCode, string message = null)
        {
            return new ApiResult
            {
                Success = errCode == DefaultErrorCode.Success,
                Code = errCode,
                Message = message ?? DefaultErrorCode.GetMessage(errCode)
            };
        }

        /// <summary>�ɹ�</summary>
        /// <remarks>�ɹ�</remarks>
        public IApiResult Ok => Build(DefaultErrorCode.Success);

        /// <summary>ҳ�治����</summary>
        public IApiResult NoFind => Build(DefaultErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>��֧�ֵĲ���</summary>
        public IApiResult NotSupport => Build(DefaultErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>���������ַ���</summary>
        public IApiResult ArgumentError => Build(DefaultErrorCode.ArgumentError, "��������");

        /// <summary>�߼������ַ���</summary>
        public IApiResult LogicalError => Build(DefaultErrorCode.LogicalError, "�߼�����");

        /// <summary>�ܾ�����</summary>
        public IApiResult DenyAccess => Build(DefaultErrorCode.DenyAccess);

        /// <summary>�������޷���ֵ���ַ���</summary>
        public IApiResult RemoteEmptyError => Build(DefaultErrorCode.RemoteError, "*�������޷���ֵ*");

        /// <summary>�����������쳣</summary>
        public IApiResult NetworkError => Build(DefaultErrorCode.NetworkError);

        /// <summary>���ش���</summary>
        public IApiResult LocalError => Build(DefaultErrorCode.LocalError);

        /// <summary>���ط����쳣</summary>
        public IApiResult LocalException => Build(DefaultErrorCode.LocalException);

        /// <summary>ϵͳδ����</summary>
        public IApiResult NoReady => Build(DefaultErrorCode.NoReady);

        /// <summary>��ͣ����</summary>
        public IApiResult Pause => Build(DefaultErrorCode.NoReady, "��ͣ����");

        /// <summary>δ֪����</summary>
        public IApiResult UnknowError => Build(DefaultErrorCode.LocalError, "δ֪����");

        /// <summary>���糬ʱ</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        public IApiResult NetTimeOut => Build(DefaultErrorCode.NetworkError, "���糬ʱ");

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        public IApiResult ExecTimeOut => Build(DefaultErrorCode.RemoteError, "ִ�г�ʱ");

        /// <summary>�ڲ�����</summary>
        /// <remarks>ִ�з���ʱ�׳�δ�����쳣</remarks>
        public IApiResult InnerError => Build(DefaultErrorCode.LocalError, "�ڲ�����");

        /// <summary>���񲻿���</summary>
        public IApiResult Unavailable => Build(DefaultErrorCode.Unavailable, "���񲻿���");

        #endregion

        #region ���췽��

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        public IApiResult Succees()
        {
            return new ApiResult
            {
                Success = true
            };
        }

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
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

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public IApiResult Error(int errCode, string message)
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
        public IApiResult Error(int errCode, string message, string innerMessage)
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
        public IApiResult Error(int errCode, string message, string innerMessage, string guide, string describe)
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
        public IApiResult Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
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
        public IApiResult<TData> Succees<TData>(TData data, string message = null)
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
        public IApiResult<TData> Error<TData>(int errCode)
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
        public IApiResult<TData> Error<TData>(int errCode, string message)
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
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage)
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
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string guide, string describe)
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
        public IApiResult<TData> Error<TData>(int errCode, string message, string innerMessage, string point, string guide, string describe)
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
        public IApiResult Error()
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
        public IApiResult<TData> Error<TData>()
        {
            var result = new ApiResult<TData>
            {
                Success = false
            };
            if (GlobalContext.CurrentNoLazy?.Status != null)
            {
                result.Success = GlobalContext.Current.Status.LastStatus.Success;
                result.Code = GlobalContext.Current.Status.LastStatus.Code;
                result.Message = GlobalContext.Current.Status.LastStatus.Message;
            }
            return result;
        }

        #endregion

    }
}

/*

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
            return Error(errCode, message);
        }

        /// <summary>����һ������������ı�׼����</summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public  IApiResult Error(int errCode, string message = null)
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
*/
