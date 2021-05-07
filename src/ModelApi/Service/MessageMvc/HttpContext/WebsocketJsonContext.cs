using BeetleX.FastHttpApi.Data;
using BeetleX.FastHttpApi.WebSockets;
using System;

namespace BeetleX.FastHttpApi
{
    public class WebsocketJsonContext : IHttpContext
    {
        public WebsocketJsonContext(HttpApiServer server, HttpRequest request, IDataContext dataContext)
        {
            Server = server;
            Request = request;
            AsyncResult = false;
            mDataContext = dataContext;
        }

        private IDataContext mDataContext;

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpApiServer Server { get; set; }

        public object Tag { get; set; }

        public NextQueue Queue { get; set; }

        public string RequestID { get; set; }

        public void Result(object data)
        {
            DataFrame frame = data as DataFrame;
            if (frame == null)
            {
                ActionResult result = data as ActionResult;
                if (result == null)
                {
                    result = new ActionResult();
                    result.Data = data;
                }
                result.ID = RequestID;
                if (result.Url == null)
                    result.Url = this.ActionUrl;
                frame = Server.CreateDataFrame(result);
            }
            frame.Send(Request.Session);
        }

        internal bool AsyncResult { get; set; }

        public bool WebSocket => true;

        public IDataContext Data => mDataContext;

        public ISession Session => Request.Session;

        public string ActionUrl { get; internal set; }

        public object this[string name] { get => Session[name]; set => Session[name] = value; }

        public void Async()
        {
            AsyncResult = true;
        }


        public void SendToWebSocket(DataFrame data, HttpRequest request)
        {
            Server.SendToWebSocket(data, request);
        }

        public void SendToWebSocket(DataFrame data, Func<ISession, HttpRequest, bool> filter = null)
        {
            Server.SendToWebSocket(data, filter);
        }


        public void SendToWebSocket(ActionResult data, HttpRequest request)
        {

            if (data.Url == null)
                data.Url = this.ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, request);

        }

        public void SendToWebSocket(ActionResult data, Func<ISession, HttpRequest, bool> filter = null)
        {
            if (data.Url == null)
                data.Url = this.ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, filter);
        }


    }
}
