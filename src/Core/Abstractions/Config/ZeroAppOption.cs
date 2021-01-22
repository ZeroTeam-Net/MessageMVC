using Agebull.Common.Configuration;
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
        /// <summary>
        /// 是否开发模式
        /// </summary>
        public bool IsDevelopment { get; set; }

        /// <summary>
        ///     当前应用版本号
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        ///     服务器名称
        /// </summary>
        public string ServiceName { get; set; }

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
        public bool AutoDiscover { get; set; }

        #region State

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
        public bool IsRuning => ApplicationState == StationState.BeginRun || ApplicationState == StationState.Run;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public bool IsAlive => ApplicationState < StationState.Closing;

        /// <summary>
        ///     已注销
        /// </summary>
        public bool IsDestroy => ApplicationState == StationState.Destroy;

        /// <summary>
        ///     已关闭
        /// </summary>
        public bool IsClosed => ApplicationState >= StationState.Closed;

        #endregion
        /// <summary>
        /// 实例
        /// </summary>
        public static ZeroAppOption Instance { get; }

        static readonly List<Func<Task>> DestoryAction = new List<Func<Task>>();

        /// <summary>
        /// 注册析构方法
        /// </summary>
        /// <param name="action"></param>
        public static void RegistDestoryAction(Func<Task> action)
        {
            DestoryAction.Add(action);
        }

        /// <summary>
        /// 执行析构方法
        /// </summary>
        public static async Task Destory()
        {
            foreach (var action in DestoryAction)
            {
                await action();
            }
        }

        static ZeroAppOption()
        {
            var asName = Assembly.GetEntryAssembly().GetName();
            Instance = new ZeroAppOption
            {
                AppName = asName.Name,
                AppVersion = asName.Version?.ToString(),
                BinPath = Environment.CurrentDirectory,
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            };
        }
        /// <summary>
        /// 载入配置
        /// </summary>
        public static void LoadConfig()
        {
            Instance.CopyByHase(ConfigurationHelper.Get<ZeroAppConfig>("MessageMVC:Option"));
            if (Instance.TraceInfo == TraceInfoType.None)
                Instance.TraceInfo = TraceInfoType.LinkTrace | TraceInfoType.App;
        }
    }
}