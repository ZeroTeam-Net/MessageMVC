namespace Agebull.MicroZero
{
    /// <summary>
    /// 中心事件
    /// </summary>
    public enum ZeroNetEventType
    {
        /// <summary>
        /// 没有事件
        /// </summary>
        None,
        /// <summary>
        /// 
        /// </summary>
        CenterSystemStart,
        /// <summary>
        /// 
        /// </summary>
        CenterSystemClosing,
        /// <summary>
        /// 
        /// </summary>
        CenterSystemStop,
        /// <summary>
        /// 
        /// </summary>
        CenterWorkerSoundOff,
        /// <summary>
        /// 
        /// </summary>
        CenterStationJoin,
        /// <summary>
        /// 
        /// </summary>
        CenterStationLeft,
        /// <summary>
        /// 
        /// </summary>
        CenterStationPause,
        /// <summary>
        /// 
        /// </summary>
        CenterStationResume,
        /// <summary>
        /// 
        /// </summary>
        CenterStationClosing,
        /// <summary>
        /// 
        /// </summary>
        CenterStationInstall,
        /// <summary>
        /// 
        /// </summary>
        CenterStationStop,
        /// <summary>
        /// 
        /// </summary>
        CenterStationRemove,
        /// <summary>
        /// 站点动态
        /// </summary>
        CenterStationTrends,
        /// <summary>
        /// 
        /// </summary>
        CenterStationUpdate,
        /// <summary>
        /// 
        /// </summary>
        CenterStationDocument,

        /// <summary>
        ///  客户端加入
        /// </summary>
        CenterClientJoin,

        /// <summary>
        ///  客户端退出
        /// </summary>
        CenterClientLeft,

        /// <summary>
        /// 
        /// </summary>
        PlanAdd = 0x1,

        /// <summary>
        /// 
        /// </summary>
        PlanUpdate,
        /// <summary>
        /// 
        /// </summary>
        PlanQueue,
        /// <summary>
        /// 
        /// </summary>
        PlanExec,
        /// <summary>
        /// 
        /// </summary>
        PlanResult,
        /// <summary>
        /// 
        /// </summary>
        PlanPause,
        /// <summary>
        /// 
        /// </summary>
        PlanEnd,
        /// <summary>
        /// 
        /// </summary>
        PlanRemove,

        /// <summary>
        /// 
        /// </summary>
        AppRun = 0xA0,

        /// <summary>
        /// 
        /// </summary>
        ConfigUpdate,
        /// <summary>
        /// 
        /// </summary>
        AppStop,
        /// <summary>
        /// 
        /// </summary>
        AppEnd,
        /// <summary>
        /// 
        /// </summary>
        StationRun,
        /// <summary>
        /// 
        /// </summary>
        StationEnd
    }
}
