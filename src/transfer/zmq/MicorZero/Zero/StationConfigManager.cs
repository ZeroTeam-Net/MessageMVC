using Agebull.Common;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Documents;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    internal class StationConfigManager : ZSimpleCommand
    {
        #region 实例

        /// <summary>
        /// 构造
        /// </summary>
        public StationConfigManager(ZeroItem center)
        {
            Center = center;
            ManageAddress = center.ManageAddress;
            ServiceKey = center.ServiceKey.ToBytes();
        }

        /// <summary>
        ///   服务中心
        /// </summary>
        public ZeroItem Center { get; }

        #endregion

        #region 系统支持


        /// <summary>
        /// 尝试安装站点
        /// </summary>
        internal bool TryInstall(ILogger logger, string station, string type)
        {
            if (ZeroRpcFlow.Config.TryGetConfig(station, out _))
            {
                return true;
            }

            logger.LogInformation("No find,try install ...");
            var r = CallCommand("install", type, station, station, station);
            if (!r.InteractiveSuccess)
            {
                logger.LogInformation("Install failed.");
                return false;
            }

            if (r.State != ZeroOperatorStateType.Ok && r.State != ZeroOperatorStateType.Failed)
            {
                logger.LogInformation("Install failed.please check name or type.");
                return false;
            }
            logger.LogInformation("Install successfully,try start it ...");
            r = CallCommand("start", station);
            if (!r.InteractiveSuccess && r.State != ZeroOperatorStateType.Ok && r.State != ZeroOperatorStateType.Runing)
            {
                logger.LogInformation("Can't start station");
                return false;
            }
            logger.LogInformation("Station runing");
            return true;
        }

        /// <summary>
        ///     上传文档
        /// </summary>
        /// <returns></returns>
        public bool UploadDocument()
        {
            bool success = true;
            foreach (var doc in ZeroRpcFlow.Config.Documents.Values)
            {
                if (!doc.IsLocal)
                {
                    continue;
                }

                var result = CallCommand("doc", doc.Name, JsonHelper.SerializeObject(doc));
                if (result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok)
                {
                    continue;
                }

                ZeroRpcFlow.Logger.Error($"UploadDocument error.{result.State}");
                success = false;
            }
            return success;
        }

        /// <summary>
        ///     下载文档
        /// </summary>
        /// <returns></returns>
        public ServiceDocument LoadDocument(string name)
        {
            ZeroResult result;
            try
            {
                result = CallCommand("doc", name);
            }
            catch (Exception e)
            {
                ZeroRpcFlow.Logger.Error($"LoadDocument({name}) exception.{e.Message}");
                return null;
            }
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroRpcFlow.Logger.Error($"LoadDocument({name}) err.{result.State}");
                return null;
            }
            if (!result.TryGetString(ZeroFrameType.Status, out var json))
            {
                ZeroRpcFlow.Logger.Error($"LoadDocument({name}) err.empty value");
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<ServiceDocument>(json);
            }
            catch
            {
                ZeroRpcFlow.Logger.Error($"LoadDocument({name}) err.Deserialize error");
                return null;
            }
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static bool LoadAllConfig()
        {
            var item = ZeroRpcFlow.Config.Master;
            var cm = new StationConfigManager(item);
            var json = cm.LoadGroupConfig();
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }
            if (!ZeroRpcFlow.Config.FlushConfigs(ZeroRpcFlow.Config.Master, json, false))
            {
                return false;
            }
            for (int i = 1; i < ZeroRpcFlow.Config.ZeroCenter.Count; i++)
            {
                item = ZeroRpcFlow.Config.ZeroCenter[i];
                cm = new StationConfigManager(item);
                json = cm.LoadGroupConfig();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    ZeroRpcFlow.Config.FlushConfigs(item, json, false);
                }
            }
            return true;
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public string LoadGroupConfig()
        {
            var result = CallCommand("host", "*");
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroRpcFlow.Logger.Error($"Load master config error.{result.State}");
                return null;
            }
            if (!result.TryGetString(ZeroFrameType.Status, out var json))
            {
                ZeroRpcFlow.Logger.Error("Load master config error. empty");
                return null;
            }
            ZeroRpcFlow.Logger.Information($"Load master{ManageAddress} config.\r\n{json}");
            return json;
        }


        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public StationConfig LoadConfig(string stationName)
        {
            if (!ZeroRpcFlow.ZerCenterIsRun)
            {
                ZeroRpcFlow.Logger.Error("LoadConfig : No ready");
                return null;
            }
            var result = CallCommand("host", stationName);
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroRpcFlow.Logger.Error($"LoadConfig error. {result.State}");
                return null;
            }

            if (result.TryGetString(ZeroFrameType.Status, out var json) || json[0] != '{')
            {
                ZeroRpcFlow.Logger.Error($"LoadConfig error. json empty or faid.");
                return null;
            }

            return !ZeroRpcFlow.Config.UpdateConfig(Center, stationName, json, out var config) ? null : config;
        }


        #endregion
    }
}