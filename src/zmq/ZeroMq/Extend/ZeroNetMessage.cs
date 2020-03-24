using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agebull.Common.Logging;

using Newtonsoft.Json;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.ZeroMQ.ZeroRPC
{
    /// <summary>
    /// 一次传输的消息
    /// </summary>
    public class ZeroNetMessage
    {
        /// <summary>
        /// 第二帧(请求为命令类型,返回为状态)
        /// </summary>
        public byte ZeroState { get; set; }

        /// <summary>
        ///     头帧
        /// </summary>
        [JsonProperty]
        public byte[] Head { get; set; }

        /// <summary>
        ///     帧说明
        /// </summary>
        [JsonProperty]
        public byte[] Description { get; set; }

        /// <summary>
        /// 内部简化命令
        /// </summary>
        [JsonIgnore]
        public ZeroByteCommand InnerCommand
        {
            get => (ZeroByteCommand)ZeroState;
            set => ZeroState = (byte)value;
        }

        /// <summary>
        /// 结束标识
        /// </summary>
        public byte Tag { get; set; }

        /// <summary>
        /// 请求还是返回
        /// </summary>
        [JsonIgnore]
        protected bool RequestOrResult { get; set; }

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
        public string CommandOrSubTitle { get; set; }

        /// <summary>
        ///  请求者
        /// </summary>
        [JsonProperty]
        public string Requester { get; set; }


        /// <summary>
        ///  站点
        /// </summary>
        [JsonProperty]
        public string Station { get; set; }

        /// <summary>
        /// 调用方的站点类型
        /// </summary>
        [JsonProperty]
        public string StationType { get; set; }

        private byte[][] messages;

        /// <summary>
        /// 帧内容
        /// </summary>
        [JsonIgnore]
        public byte[][] Messages => messages;

        /// <summary>
        ///     内容
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> Values => _frames?.Values.Select(p => Encoding.UTF8.GetString(p.Data));


        /// <summary>
        ///     内容
        /// </summary>
        [JsonIgnore] protected Dictionary<int, ZeroFrameItem> _frames;

        /// <summary>
        /// 帧内容
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, ZeroFrameItem> Frames
        {
            set => _frames = value;
            get => _frames ?? (_frames = new Dictionary<int, ZeroFrameItem>());
        }

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
            if (_frames == null || Frames.Count <= 0) return text.ToString();
            foreach (var data in Frames.Values)
            {
                text.Append($" , [{ZeroFrameType.FrameName(data.Type)}] {data.Data}");
            }
            return text.ToString();
        }

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="isResult"></param>
        /// <param name="messages"></param>
        /// <param name="message"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool Unpack<TZeroMessage>(bool isResult, byte[][] messages, out TZeroMessage message, Func<TZeroMessage, byte, byte[], bool> action)
            where TZeroMessage : ZeroNetMessage, new()
        {
            message = new TZeroMessage
            {
                messages = messages
            };
            if (messages.Length == 0)
                return false;
            int move = isResult ? 1 : 0;
            byte[] description;
            if (isResult)
            {
                if (messages.Length == 0)
                {
                    message.ZeroState = (byte)ZeroOperatorStateType.FrameInvalid;
                    return false;
                }
                description = messages[0];
            }
            else
            {
                if (messages.Length == 1)
                {
                    description = messages[1];
                }
                else
                {
                    if (messages.Length < 2)
                    {
                        message.ZeroState = (byte)ZeroOperatorStateType.FrameInvalid;
                        return false;
                    }
                    description = messages[1];
                    message.Head = messages[0];
                }
            }

            message.Description = description;
            if (description.Length < 2)
            {
                message.ZeroState = (byte)ZeroOperatorStateType.FrameInvalid;
                return false;
            }
            int end = description[0] + 2;
            message.ZeroState = description[1];
            int size = description[0] + 2;
            if (size < description.Length)
                message.Tag = description[size];

            message._frames = new Dictionary<int, ZeroFrameItem>();

            for (int idx = 2; idx < end && messages.Length > idx - move; idx++)
            {
                if (idx >= description.Length)
                {
                    LogRecorder.Trace("RPC Frame Failed");
                    foreach (var line in messages)
                    {
                        LogRecorder.Trace(Encoding.UTF8.GetString(line));
                    }
                    break;
                }
                var bytes = messages[idx - move];
                message._frames.Add(idx - 2, new ZeroFrameItem
                {
                    Type = description[idx],
                    Data = bytes
                });
                if (bytes.Length == 0)
                    continue;

                switch (description[idx])
                {
                    case ZeroFrameType.Requester:
                        message.Requester = GetString(bytes);
                        break;
                    case ZeroFrameType.Station:
                        message.Station = GetString(bytes);
                        break;
                    case ZeroFrameType.StationType:
                        message.StationType = GetString(bytes);
                        break;
                    case ZeroFrameType.RequestId:
                        message.RequestId = GetString(bytes);
                        break;
                    case ZeroFrameType.CallId:
                        message.CallId = GetString(bytes);
                        break;
                    case ZeroFrameType.GlobalId:
                        message.GlobalId = GetString(bytes);
                        break;
                    case ZeroFrameType.Command:
                        message.CommandOrSubTitle = GetString(bytes);
                        break;
                }
                if (action != null && action.Invoke(message, description[idx], bytes))
                    continue;
                switch (description[idx])
                {
                    case ZeroFrameType.Context:
                        message.Context = GetString(bytes);
                        break;
                    case ZeroFrameType.TextContent:
                        message.Content = GetString(bytes);
                        break;
                    case ZeroFrameType.BinaryContent:
                        message.Content = GetString(bytes);
                        break;
                }
            }

            return true;
        }
        /// <summary>
        /// 取文本
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(byte[] bytes)
        {
            return bytes == null || bytes.Length == 0 ? null : Encoding.UTF8.GetString(bytes).Trim('\0');
        }
        /// <summary>
        /// 取文本
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected static long GetLong(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || !long.TryParse(Encoding.ASCII.GetString(bytes).Trim('\0'), out var l))
                return 0;
            return l;
        }

        /// <summary>
        /// 加入数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(byte type, byte[] value)
        {
            if (Frames.Count == 0)
            {
                Frames.Add(2, new ZeroFrameItem
                {
                    Type = type,
                    Data = value
                });
            }
            Frames.Add(Frames.Keys.Max() + 3, new ZeroFrameItem
            {
                Type = type,
                Data = value
            });
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">返回值</param>
        /// <returns>是否存在</returns>
        public bool TryGetValue(byte name, out byte[] value)
        {
            if (_frames == null || _frames.Count == 0)
            {
                value = null;
                return false;
            }

            var vl = _frames.Values.FirstOrDefault(p => p.Type == name);
            value = vl?.Data;
            return vl != null;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="type">名称</param>
        /// <returns>存在返回值，不存在返回空对象</returns>
        public byte[] GetValue(byte type)
        {
            if (_frames == null || _frames.Count == 0)
                return null;
            var vl = _frames.Values.FirstOrDefault(p => p.Type == type);
            return vl?.Data;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">返回值</param>
        /// <param name="parse"></param>
        /// <returns>是否存在</returns>
        public bool TryGetValue<TValue>(byte name, out TValue value, Func<byte[], TValue> parse)
        {
            if (_frames == null || _frames.Count == 0)
            {
                value = default;
                return false;
            }
            var vl = _frames.Values.FirstOrDefault(p => p.Type == name);
            value = vl == null || vl.Data.Length == 0 ? default : parse(vl.Data);
            return vl != null;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="parse"></param>
        /// <returns>存在返回值，不存在返回空对象</returns>
        public TValue GetValue<TValue>(byte name, Func<byte[], TValue> parse)
        {
            if (_frames == null || _frames.Count == 0)
                return default;
            var vl = _frames.Values.FirstOrDefault(p => p.Type == name);
            return vl == null || vl.Data.Length == 0 ? default : parse(vl.Data);
        }
    }

    /// <summary>
    /// 帧节点
    /// </summary>
    public class ZeroFrameItem
    {
        /// <summary>
        /// 帧数据类型
        /// </summary>
        public byte Type { get; set; }
        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data { get; set; }
    }
}