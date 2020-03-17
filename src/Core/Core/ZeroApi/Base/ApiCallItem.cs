using System;
using System.Collections.Generic;
using System.Text;
using ZeroTeam.MessageMVC.Context;
using Agebull.Common.Logging;
using Newtonsoft.Json;


namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// Api调用节点
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ApiCallItem
    {
        /// <summary>
        /// 全局ID
        /// </summary>
        internal List<IApiHandler> Handlers { get; set; }

        /// <summary>
        /// 站点请求ID(队列使用)
        /// </summary>
        public string LocalId { get; set; }

        /// <summary>
        /// 全局ID(本次)
        /// </summary>
        [JsonProperty]
        public string GlobalId { get; set; }

        /// <summary>
        /// 全局ID(调用方)
        /// </summary>
        [JsonProperty]
        public string CallId { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        [JsonProperty]
        public string RequestId { get; set; }

        /// <summary>
        ///  原始上下文的JSO内容
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///  调用的命令或广播子标题
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        ///  请求者
        /// </summary>
        [JsonProperty]
        public string Requester { get; set; }


        /// <summary>
        ///  服务
        /// </summary>
        [JsonProperty]
        public string Service { get; set; }


        /// <summary>
        /// 请求参数
        /// </summary>
        public string Argument { get; set; }


        /// <summary>
        /// 请求参数
        /// </summary>
        public string Extend { get; set; }

        /// <summary>
        /// 返回
        /// </summary>
        public string Result { get; set; }


        /// <summary>
        /// 执行状态
        /// </summary>
        public UserOperatorStateType Status { get; set; }

        /// <summary>
        /// 还原调用上下文
        /// </summary>
        /// <returns></returns>
        public void RestoryContext(string station)
        {
            try
            {
                GlobalContext.SetContext(!string.IsNullOrWhiteSpace(Context)
                    ? JsonConvert.DeserializeObject<GlobalContext>(Context)
                    : new GlobalContext());
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace(() => $"Restory context exception:{e.Message}");
                ZeroTrace.WriteException(station, e, "restory context", Context);
                GlobalContext.SetContext(new GlobalContext());
            }
            GlobalContext.Current.Request.CallGlobalId = CallId;
            GlobalContext.Current.Request.LocalGlobalId = GlobalId;
            GlobalContext.Current.Request.RequestId = RequestId;
        }

        /// <summary>
        /// 显示到文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();

            return text.ToString();
        }
    }
}