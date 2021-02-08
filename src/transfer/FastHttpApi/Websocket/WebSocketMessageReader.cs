using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroTeam.MessageMVC.Messages;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    /// Http消息体读取器
    /// </summary>
    public class WebSocketMessageReader
    {
        /// <summary>
        /// 消息体
        /// </summary>
        public HttpMessage Message;

        /// <summary>
        ///     调用检查
        /// </summary>
        public (bool success, HttpMessage message) CheckRequest(HttpApiServer server, HttpRequest request, DataFrame dataFrame)
        {
            string json;
            //if (dataFrame.Body is DataBuffer<byte> buffer)
            //{
            //    json = Encoding.UTF8.GetString(buffer.Data);
            //}
            //else 
            
            if (dataFrame.Body is byte[] bytes)
            {
                json = Encoding.UTF8.GetString(bytes);
            }
            else if (dataFrame.Body is string text)
            {
                json = text;
            }
            else
            {
                return (false, null);
            }

            Message = new HttpMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Request = request,
                HttpContent = json,
                ExtensionDictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase),
                DataState = MessageDataState.ArgumentOffline
            };
            var token = (JToken)JsonConvert.DeserializeObject(json);
            JToken url = token["url"];
            if (url == null)
                return (false, null);
            Message.Url = url.Value<string>().Split('?')[0];
            var words = Message.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                return (false, null);
            }
            Message.Service = words[0].ToLower();
            Message.Method = string.Join('/', words.Skip(1).Select(p => p.ToLower()));



            Message.ContentObject = token["params"];
            if (Message.ContentObject == null)
                Message.ContentObject = (JToken)JsonConvert.DeserializeObject("{}");

            JToken requestid = Message.ContentObject["_requestid"];
            Message.RequestId = requestid != null ? requestid.Value<string>() : Message.ID;
            Message.HttpContext = new WebsocketContext(server, request, Message)
            {
                ActionUrl = Message.Url,
                RequestID = Message.RequestId
            };

            return (true, Message);
        }
    }
}
