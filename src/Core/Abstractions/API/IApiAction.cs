using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     Api方法
    /// </summary>
    public interface IApiAction
    {
        /// <summary>
        ///     访问控制
        /// </summary>
        ApiOption Access { get; set; }

        /// <summary>
        ///     Api名称
        /// </summary>
        string RouteName { get; set; }

        /// <summary>
        ///     是合符合API契约规定
        /// </summary>
        bool IsApiContract { get; }


        /// <summary>
        ///     参数类型
        /// </summary>
        Type ArgumentType { get; set; }

        /// <summary>
        ///     参数名称
        /// </summary>
        string ArgumentName { get; set; }

        /// <summary>
        ///     参数反序列化对象
        /// </summary>
        ISerializeProxy ArgumentSerializer { get; set; }
        /// <summary>
        /// 反序列化类型
        /// </summary>
        SerializeType ArgumentSerializeType { get; set; }

        /// <summary>
        ///     还原参数
        /// </summary>
        bool RestoreArgument(IInlineMessage message);

        /// <summary>
        ///     参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        bool ValidateArgument(IInlineMessage message,out string info);

        /// <summary>
        /// 反序列化类型
        /// </summary>
        SerializeType ResultSerializeType { get; set; }

        /// <summary>
        ///     返回值序列化对象
        /// </summary>
        ISerializeProxy ResultSerializer { get; set; }

        /// <summary>
        ///     返回值构造对象
        /// </summary>
        Func<int, string, object> ResultCreater { get; set; }

        /// <summary>
        ///     参数类型
        /// </summary>
        Type ResultType { get; set; }


        /// <summary>
        ///     执行
        /// </summary>
        /// <returns></returns>
        Task<(MessageState state, object result)> Execute(IInlineMessage message, ISerializeProxy serializer);

        /// <summary>
        /// 初始化检查
        /// </summary>
        void Initialize();
    }
}