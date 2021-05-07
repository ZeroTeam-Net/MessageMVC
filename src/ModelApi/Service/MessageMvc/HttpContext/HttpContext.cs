using BeetleX.FastHttpApi.Data;
using BeetleX.FastHttpApi.WebSockets;
using System;

namespace BeetleX.FastHttpApi
{

    public class HttpContext : IHttpContext
    {

        public HttpContext(HttpApiServer server, HttpRequest request, HttpResponse response, IDataContext dataContext)
        {
            Request = request;
            Response = response;
            Server = server;
            mDataContext = dataContext;
        }

        private IDataContext mDataContext;

        public NextQueue Queue { get; set; }

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpApiServer Server { get; set; }

        public object Tag { get; set; }

        public bool WebSocket => false;

        public IDataContext Data => mDataContext;

        public ISession Session => Request.Session;

        public string ActionUrl { get; internal set; }

        public object this[string name] { get => Session[name]; set => Session[name] = value; }

        public void Result(object data)
        {
            Response.Result(data);
        }

        public void Async()
        {
            Response.Async();
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
