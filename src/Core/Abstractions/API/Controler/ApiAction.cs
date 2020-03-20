using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

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
        public ApiAccessOption Access { get; set; }

        /// <summary>
        ///     Api名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     参数
        /// </summary>
        object Argument { get; }

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumentType { get; set; }

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        ///     还原参数
        /// </summary>
        bool RestoreArgument(string argument);

        /// <summary>
        ///     参数校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Validate(out string message);

        /// <summary>
        ///     执行
        /// </summary>
        /// <returns></returns>
        Task<Tuple<MessageState, string>> Execute();
    }
}