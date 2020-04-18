using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
{
    /// <summary>API���ػ���</summary>
    public class ApiResultDefault : IApiResultHelper
    {
        #region ���л�

        static readonly ISerializeProxy Serializer = DependencyHelper.Create<ISerializeProxy>();

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
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <returns></returns>
        public IApiResult Generate(int code, string message = null)
        {
            return new ApiResult
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code)
            };
        }

        ///<inheritdoc/>
        IApiResult IApiResultHelper.Waiting => Generate(OperatorStatusCode.Queue);


        /// <summary>�ɹ�</summary>
        /// <remarks>�ɹ�</remarks>
        IApiResult IApiResultHelper.Ok => Generate(OperatorStatusCode.Success);

        /// <summary>ҳ�治����</summary>
        IApiResult IApiResultHelper.NoFind => Generate(OperatorStatusCode.NoFind, "*ҳ�治����*");

        /// <summary>��֧�ֵĲ���</summary>
        IApiResult IApiResultHelper.NonSupport => Generate(OperatorStatusCode.Ignore, "*��֧�ֵĲ���*");

        /// <summary>���������ַ���</summary>
        IApiResult IApiResultHelper.ArgumentError => Generate(OperatorStatusCode.ArgumentError, "��������");

        /// <summary>�ܾ�����</summary>
        IApiResult IApiResultHelper.DenyAccess => Generate(OperatorStatusCode.DenyAccess);

        /// <summary>ϵͳδ����</summary>
        IApiResult IApiResultHelper.NoReady => Generate(OperatorStatusCode.NoReady);

        /// <summary>��ͣ����</summary>
        IApiResult IApiResultHelper.Pause => Generate(OperatorStatusCode.NoReady, "��ͣ����");

        /// <summary>�߼�����</summary>
        IApiResult IApiResultHelper.BusinessError => Generate(OperatorStatusCode.BusinessError, "�߼�����");
        /// <summary>
        /// �����쳣
        /// </summary>
        IApiResult IApiResultHelper.BusinessException => Generate(OperatorStatusCode.BusinessException, "�����쳣");

        IApiResult IApiResultHelper.UnhandleException => Generate(OperatorStatusCode.UnhandleException, "ϵͳ�쳣");

        /// <summary>�������</summary>
        IApiResult IApiResultHelper.NetworkError => Generate(OperatorStatusCode.NetworkError);

        /// <summary>���糬ʱ</summary>
        /// <remarks>��������Apiʱʱ�׳�δ�����쳣</remarks>
        IApiResult IApiResultHelper.NetTimeOut => Generate(OperatorStatusCode.NetworkError, "���糬ʱ");

        /// <summary>ִ�г�ʱ</summary>
        /// <remarks>Apiִ�г�ʱ</remarks>
        IApiResult IApiResultHelper.ExecTimeOut => Generate(OperatorStatusCode.TimeOut, "ִ�г�ʱ");

        /// <summary>���񲻿���</summary>
        IApiResult IApiResultHelper.Unavailable => Generate(OperatorStatusCode.Unavailable, "���񲻿���");

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
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <returns></returns>
        IApiResult IApiResultHelper.State(int code)
        {
            return new ApiResult
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = OperatorStatusCode.GetMessage(code)
            };
        }

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <returns></returns>
        IApiResult IApiResultHelper.State(int code, string message)
        {
            return new ApiResult
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code)
            };
        }

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        IApiResult IApiResultHelper.State(int code, string message, string innerMessage)
        {
            return new ApiResult
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code),
                InnerMessage = innerMessage
            };
        }
        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        IApiResult IApiResultHelper.State(int code, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Guide = guide,
                    Describe = describe
                }
            };
        }

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
        IApiResult IApiResultHelper.State(int code, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code),
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
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <returns></returns>
        public IApiResult<TData> State<TData>(int code)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = code,
                Message = OperatorStatusCode.GetMessage(code)
            };
        }

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public IApiResult<TData> State<TData>(int code, string message)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code)
            };
        }

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <returns></returns>
        public IApiResult<TData> State<TData>(int code, string message, string innerMessage)
        {
            return new ApiResult<TData>
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code),
                InnerMessage = innerMessage
            };
        }

        /// <summary>
        ///     ����һ������״̬��ı�׼����
        /// </summary>
        /// <param name="code">״̬��</param>
        /// <param name="message">��ʾ��Ϣ</param>
        /// <param name="innerMessage">�ڲ�˵��</param>
        /// <param name="guide">����ָ��</param>
        /// <param name="describe">�������</param>
        /// <returns></returns>
        public IApiResult<TData> State<TData>(int code, string message, string innerMessage, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Guide = guide,
                    Describe = describe
                }
            };
        }

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
        public IApiResult<TData> State<TData>(int code, string message, string innerMessage, string point, string guide, string describe)
        {
            return new ApiResult<TData>
            {
                Success = code == OperatorStatusCode.Success || code == OperatorStatusCode.Queue,
                Code = code,
                Message = message ?? OperatorStatusCode.GetMessage(code),
                InnerMessage = innerMessage,
                Trace = new OperatorTrace
                {
                    Point = point,
                    Guide = guide,
                    Describe = describe
                }
            };
        }

        #endregion

    }
}
