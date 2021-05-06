using Agebull.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Services
{
    /// <summary>
    /// 一个服务
    /// </summary>
    public class EmptyService : IService
    {
        #region 基础信息

        string IZeroDependency.Name => ServiceName;

        /// <summary>
        ///     是否可以运行的判断方法
        /// </summary>
        public Func<bool> CanRun { get; set; }

        /// <summary>
        /// 等级,用于确定中间件优先级
        /// </summary>
        int IZeroMiddleware.Level => MiddlewareLevel.General;

        /// <summary>
        /// 站点名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 消息接收对象
        /// </summary>
        public IMessageReceiver Receiver { get; set; }

        /// <summary>
        /// 是否自动发现对象
        /// </summary>
        public bool IsAutoService { get; set; }

        #endregion

        #region 状态管理

        /// <summary>
        ///     运行状态
        /// </summary>
        public int RealState { get; set; }

        /// <summary>
        ///     配置状态
        /// </summary>
        public StationStateType ConfigState { get; set; }

        #endregion

        #region 方法注册

        ISerializeProxy serialize;

        /// <summary>
        /// 序列化对象
        /// </summary>
        public ISerializeProxy Serialize
        {
            get => serialize ??= DependencyHelper.GetService<ISerializeProxy>();
            set => serialize = value;
        }
        /// <summary>
        /// 通配事件
        /// </summary>
        IApiAction WildcardAction;

        /// <summary>
        /// 注册的方法
        /// </summary>
        readonly Dictionary<string, IApiAction> ApiActions = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///  取得API信息
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public IApiAction GetApiAction(string api)
        {
            return api != null && ApiActions.TryGetValue(api, out var info) ? info : WildcardAction;
        }

        /// <summary>
        ///     注册通配方法
        /// </summary>
        /// <param name="info">反射信息</param>
        void IService.RegistWildcardAction(ApiActionInfo info)
        {
            WildcardAction = new ApiAction
            {
                Info = info,
                Function = info.Action,
                ResultType = info.ResultType,
                IsAsync = info.IsAsync,
                ResultSerializeType = info.ResultSerializeType,
                ArgumentSerializeType = info.ArgumentSerializeType
            };

            WildcardAction.Initialize();
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="route">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="info">反射信息</param>
        bool IService.RegistAction(string route, ApiActionInfo info)
        {
            var apis = route.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var api in apis)
            {
                var action = new ApiAction
                {
                    Info = info,
                    IsAsync = info.IsAsync,
                    RouteName = api,
                    Function = info.Action,
                    ResultType = info.ResultType,
                    ResultSerializeType = info.ResultSerializeType,
                    ArgumentSerializeType = info.ArgumentSerializeType
                };
                if (!ApiActions.TryAdd(api, action))
                    return false;
                if (info.HaseArgument)
                {
                    var arg = info.Arguments.Values.First();
                    if (!arg.IsBaseType)
                    {
                        action.ArgumentName = arg.Name;
                        action.ArgumentType = arg.ParameterInfo.ParameterType;
                    }
                }
                action.Initialize();
            }
            return true;
        }

        #endregion
    }
}