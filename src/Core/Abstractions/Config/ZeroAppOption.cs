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
        ///     当前应用版本号
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        ///     机器器名称
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        ///     当前服务器的跟踪名称
        /// </summary>
        public string TraceName { get; set; }

        /// <summary>
        ///     应用所在的顶级目录
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        ///     程序所在地址
        /// </summary>
        [IgnoreDataMember]
        public string BinPath { get; set; }

        /// <summary>
        ///     本机IP地址
        /// </summary>
        [IgnoreDataMember]
        public string LocalIpAddress { get; set; }

        /// <summary>
        ///     是否在Linux黑环境下
        /// </summary>
        [IgnoreDataMember]
        public bool IsLinux { get; set; }

        /// <summary>
        /// 全部自动发现
        /// </summary>
        [IgnoreDataMember]
        public bool AutoDiscover { get; set; }

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

        #region TraceOption

        /// <summary>
        /// 默认跟踪信息内容配置
        /// </summary>
        internal MessageTraceType defaultTraceOption = MessageTraceType.Simple;


        /// <summary>
        /// 获取跟踪信息内容配置
        /// </summary>
        /// <param name="service"></param>
        /// <returns>正确的内容</returns>
        public MessageTraceType GetTraceOption(string service)
        {
            if (TraceOption == null || TraceOption.Count == 0 || !TraceOption.TryGetValue(service,out var opt))
                return defaultTraceOption;
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
        public static ZeroAppOption Instance { get; }

        static ZeroAppOption()
        {
            var asName = Assembly.GetEntryAssembly().GetName();
            Instance = new ZeroAppOption
            {
                AppName = asName.Name,
                AppVersion = asName.Version?.ToString(),
                BinPath = Environment.CurrentDirectory,
                MaxCloseSecond = 30,
                defaultTraceOption = MessageTraceType.Simple,
                TraceOption = new Dictionary<string, MessageTraceType>(StringComparer.OrdinalIgnoreCase),
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            };
        }
        /// <summary>
        /// 载入配置
        /// </summary>
        public static void LoadConfig()
        {
            Instance.CopyByHase(ConfigurationHelper.Get<ZeroAppConfig>("MessageMVC:Option"));
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Flush()
        {

        }
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