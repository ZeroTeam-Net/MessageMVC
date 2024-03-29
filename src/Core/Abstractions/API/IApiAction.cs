﻿using System;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     Api方法
    /// </summary>
    public interface IApiAction
    {
        /// <summary>
        /// 接口信息
        /// </summary>
        ApiActionInfo Info { get; set; }

        /// <summary>
        ///     API配置
        /// </summary>
        ApiOption Option { get; }

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
        /// <param name="status"></param>
        /// <returns></returns>
        bool ValidateArgument(IInlineMessage message, out IOperatorStatus status);

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
        void Execute(IInlineMessage message, ISerializeProxy serializer);

        /// <summary>
        /// 初始化检查
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// 方法执行检查器
    /// </summary>
    public interface IApiActionChecker
    {
        /// <summary>
        /// 检查接口是否可执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Check(IApiAction action, IInlineMessage message);
    }
}