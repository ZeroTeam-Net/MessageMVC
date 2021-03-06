﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel.Composition;
using ZeroTeam.MessageMVC.AddIn;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.ApiContract
{
    /// <summary>
    ///   组件注册
    /// </summary>
    [Export(typeof(IAutoRegister))]
    [ExportMetadata("Symbol", '%')]
    public sealed class AutoRegister : IAutoRegister
    {
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist(IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
        {
            //ApiResult构造
            services.TryAddTransient<IApiResultHelper, ApiResultDefault>();
            services.TryAddTransient<IOperatorStatus, OperatorStatus>();
            services.TryAddTransient<IApiResult, ApiResult>();
        }
    }
}
