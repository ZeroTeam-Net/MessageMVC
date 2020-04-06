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
        ///     是合符合API契约规定
        /// </summary>
        public bool IsApiContract { get; }

        /// <summary>
        ///     还原参数
        /// </summary>
        bool RestoreArgument(string argument);

        /// <summary>
        ///     基本数据,即按参数名称取值
        /// </summary>
        bool IsBaseValue { get; set; }


        /// <summary>
        ///     参数名称
        /// </summary>
        string ArgumentName { get; set; }

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
        Task<(MessageState state, string result)> Execute();
    }
}