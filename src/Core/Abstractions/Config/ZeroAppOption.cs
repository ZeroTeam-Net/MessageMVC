using Agebull.Common.Configuration;
using Agebull.EntityModel.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC
{
    /// <summary>
    ///     本地站点配置
    /// </summary>
    public class ZeroAppOption : ZeroAppConfig
    {
        #region RuntimeOption

        /// <summary>
        /// 是否开发模式
        /// </summary>
        public bool IsDevelopment { get; set; }

        /// <summary>
        ///     机器器名称
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        ///     应用所在的顶级目录
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        ///     程序所在地址
        /// </summary>
        public string BinPath { get; set; }

        /// <summary>
        ///     本机IP地址
        /// </summary>
        public string LocalIpAddress { get; set; }

        /// <summary>
        ///     是否在Linux黑环境下
        /// </summary>
        public bool IsLinux { get; set; }

        /// <summary>
        /// 全部自动发现
        /// </summary>
        public bool AutoDiscover { get; set; }

        /// <summary>
        /// 自定义发现方法
        /// </summary>
        public Action Discovery { get; set; }

        /// <summary>
        /// 本地应用名称
        /// </summary>
        public string LocalApp { get; set; }

        /// <summary>
        /// 本地机器名称
        /// </summary>
        public string LocalMachine { get; set; }

        /// <summary>
        ///     本地数据文件夹
        /// </summary>
        public string DataFolder { get; set; }

        /// <summary>
        ///     本地配置文件夹
        /// </summary>
        public string ConfigFolder { get; set; }

        /// <summary>
        ///     插件地址,如为空则与运行目录相同
        /// </summary>
        public string AddInPath { get; set; }

        /// <summary>
        ///     使用System.Text.Json序列化，而不是使用默认的Newtonsoft.Json
        /// </summary>
        public bool UsMsJson { get; set; }

        #endregion

        #region Flow

        internal static readonly List<NameValue<string, Func<CancellationToken, Task>>> StartActions = new List<NameValue<string, Func<CancellationToken, Task>>>();

        internal static readonly List<NameValue<string, Func<Task>>> StopActions = new List<NameValue<string, Func<Task>>>();

        /// <summary>
        /// 注册后台方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegistStartAction(string name, Func<CancellationToken, Task> action)
            => StartActions.Add(new NameValue<string, Func<CancellationToken, Task>>
            {
                Name = name,
                Value = action
            });

        /// <summary>
        /// 注册关机方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegistStopAction(string name, Func<Task> action)
            => StopActions.Add(new NameValue<string, Func<Task>>
            {
                Name = name,
                Value = action
            });

        internal static readonly List<Func<Task>> DestoryAction = new List<Func<Task>>();

        /// <summary>
        /// 注册析构方法
        /// </summary>
        /// <param name="action"></param>
        public static void RegistDestoryAction(Func<Task> action)
        {
            DestoryAction.Add(action);
        }

        #endregion

        #region TraceOption & ServiceMap

        /// <summary>
        ///     服务映射，即将对应服务名称替换成另一个服务
        /// </summary>
        public Dictionary<string, string> ServiceMap { get; set; }

        /// <summary>
        /// 跟踪信息内容配置
        /// </summary>
        public Dictionary<string, MessageTraceType> TraceOption { get; set; }

        /// <summary>
        /// 默认跟踪信息内容配置
        /// </summary>
        public MessageTraceType DefaultTraceOption { get; set; } = MessageTraceType.Simple;


        /// <summary>
        /// 获取跟踪信息内容配置
        /// </summary>
        /// <param name="service"></param>
        /// <returns>正确的内容</returns>
        public MessageTraceType GetTraceOption(string service)
        {
            if (TraceOption == null || TraceOption.Count == 0 || !TraceOption.TryGetValue(service, out var opt))
                return DefaultTraceOption;
            return opt;

        }

        /*// <summary>
        /// 检查跟踪信息内容配置
        /// </summary>
        /// <param name="traceInfo"></param>
        /// <returns>正确的内容</returns>
        public TraceInfoType Check(TraceInfoType traceInfo = TraceInfoType.Undefined)
        {
            if (traceInfo.HasFlag(TraceInfoType.Isolate))
                return traceInfo;
            else if (traceInfo.HasFlag(TraceInfoType.Undefined))
                return defaultTraceOption;
            else
                return traceInfo & defaultTraceOption;
        }*/

        #endregion

        #region Instance

        /// <summary>
        /// 实例
        /// </summary>
        public static ZeroAppOption Instance { get; internal set; }

        #endregion

        #region State

        /// <summary>
        ///     运行的API数量
        /// </summary>
        private long _requestSum;

        /// <summary>
        ///     运行的API数量
        /// </summary>
        public long RequestSum => Interlocked.Read(ref _requestSum);

        /// <summary>
        /// 设置应用状态
        /// </summary>
        public bool BeginRequest()
        {
            if (_appState == StationState.BeginRun || _appState == StationState.Run || ApplicationState == StationState.Pause)
            {
                Interlocked.Increment(ref _requestSum);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置应用状态
        /// </summary>
        public void EndRequest()
        {
            Interlocked.Decrement(ref _requestSum);
        }

        /// <summary>
        ///     运行状态
        /// </summary>
        private int _appState;

        /// <summary>
        ///     状态
        /// </summary>
        public int ApplicationState => _appState;

        /// <summary>
        /// 设置应用状态
        /// </summary>
        /// <param name="state"></param>
        public void SetApplicationState(int state) => Interlocked.Exchange(ref _appState, state);

        /// <summary>
        ///     本地应用是否正在运行
        /// </summary>
        public bool IsRuning => _appState == StationState.BeginRun || _appState == StationState.Run;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public bool IsAlive => _appState < StationState.Closing;

        /// <summary>
        ///     可以运行状态（本地正在运行或未关闭）
        /// </summary>
        public bool CanPost => _appState > StationState.Check && _appState < StationState.Destroy;

        /// <summary>
        ///     可以运行状态（本地正在运行或未关闭）
        /// </summary>
        public bool CanRun => _appState == StationState.BeginRun || _appState == StationState.Run || _appState == StationState.Pause;

        /// <summary>
        ///     已注销
        /// </summary>
        public bool IsDestroy => _appState == StationState.Destroy;

        /// <summary>
        ///     已关闭
        /// </summary>
        public bool IsClosed => _appState >= StationState.Closed;

        #endregion

    }
}