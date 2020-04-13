using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>API���ػ���</summary>
    internal class ApiResultDefault : IApiResultHelper
    {
        #region ���л�

        static ISerializeProxy Serializer = DependencyHelper.Create<ISerializeProxy>();

        /// <summary>
        /// ���л�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public string Serialize<T>(T t)
        {
            return Serializer.ToString(t);
        }

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IApiResult IApiResultHelper.Deserialize(string str)
        {
            return Serializer.ToObject<ApiResult>(str);
        }

        /// <summary>
        /// �����л�
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public IApiResult<T> Deserialize<T>(string str)
        {
            return Serializer.ToObject<ApiResult<T>>(str);
        }

        /// <summary>
        /// �����л�(BUG:interface����)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IApiResult<T> DeserializeInterface<T>(string json)
        {
            return Serializer.ToObject<ApiResult<T>>(json);
        }

        #endregion

        #region ��������

        /// <summary>
        ///     ����һ������������ı�׼����
        /// </summary>
        /// <param name="errCode">������</param>
        /// <param name="message">������Ϣ</param>
        /// <returns></returns>
        public IApiResult Generate(int errCode, string message = null)
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
        IApiResult IApiResultHelper.Ok => Generate(DefaultErrorCode.Success);

        /// <summary>ҳ�治����</summary>
        IApiResult IApiResultHelper.NoFind => Generate(DefaultErrorCode.NoFind, "*ҳ�治����*");

        /// <summary>��֧�ֵĲ���</summary>
        IApiResult IApiResultHelper.NotSupport => Generate(DefaultErrorCode.Ignore, "*��֧�ֵĲ���*");

        /// <summary>���������ַ���</summary>
        IApiResult IApiResultHelper.ArgumentError => Generate(DefaultErrorCode.ArgumentError, "��������");

        /// <summary>�ܾ�����</summary>
        IApiResult IApiResultHelper.DenyAccess => Generate(DefaultErrorCode.DenyAccess);

        /// <summary>ϵͳδ����</summary>
        IApiResult IApiResultHelper.NoReady => Generate(DefaultErrorCode.NoReady);

        /// <summary>��ͣ����</summary>
        IApiResult IApiResultHelper.Pause => Generate(DefaultErrorCode.NoReady, "��ͣ����");

        /// <summary>�߼�����</summary>
        IApiResult IApiResultHelper.BusinessError => Generate(DefaultErrorCode.BusinessError, "�߼�����");
        /// <summary>
        /// �����쳣
        /// </summary>
        IApiResult IApiResultHelper.BusinessException => Generate(DefaultErrorCode.BusinessException, "�����쳣");

        IApiResult IApiResultHelper.UnhandleException => Generate(DefaultErrorCode.UnhandleException, "ϵͳ�쳣");

        /// <summary>�������</summary>
        IApiResult IApiResultHelper.NetworkError => Generate(DefaultErrorCode.NetworkError);

        /// <summary>���糬ʱ</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        IApiResult IApiResultHelper.NetTimeOut => Generate(DefaultErrorCode.NetworkError, "���糬ʱ");

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        IApiResult IApiResultHelper.ExecTimeOut => Generate(DefaultErrorCode.TimeOut, "ִ�г�ʱ");

        /// <summary>���񲻿���</summary>
        IApiResult IApiResultHelper.Unavailable => Generate(DefaultErrorCode.Unavailable, "���񲻿���");

        #endregion

        #region ���췽��

        /// <summary>
        ///     ����һ���ɹ��ı�׼����
        /// </summary>
        /// <returns></returns>
        IApiResult IApiResultHelper.Succees()
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
        IApiResult IApiResultHelper.Error(int errCode)
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
        IApiResult IApiResultHelper.Error(int errCode, string message)
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
        IApiResult IApiResultHelper.Error(int errCode, string message, string innerMessage)
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
        IApiResult IApiResultHelper.Error(int errCode, string message, string innerMessage, string guide, string describe)
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
        IApiResult IApiResultHelper.Error(int errCode, string message, string innerMessage, string point, string guide, string describe)
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
        public IApiResult<TData> Succees<TData>(TData data)
        {
            return new ApiResult<TData>
            {
                Success = true,
                ResultData = data
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
        IApiResult IApiResultHelper.Error()
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
