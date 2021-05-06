﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using BeetleX.Dispatchs;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime;

namespace BeetleX.FastHttpApi
{
    public class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler, WebSockets.IDataFrameSerializer, IWebSocketServer, IDisposable
    {

        public const int WEBSOCKET_SUCCESS = 250;

        public const int WEBSOCKET_ERROR = 550;

        public HttpApiServer() : this(null)
        {

        }

        public HttpApiServer(HttpOptions options)
        {
            mFileLog = new FileLogWriter("BEETLEX_HTTP_SERVER");
            FrameSerializer = this;
            if (options != null)
            {
                Options = options;
            }
            else
            {
                Options = LoadOptions();
            }
            mActionSettings.Load();
            mIPv4Tables.Load();
            mActionFactory = new ActionHandlerFactory(this);
            mResourceCenter = new StaticResurce.ResourceCenter(this);
            mUrlRewrite = new RouteRewrite(this);
            mModuleManager = new ModuleManager(this);
        }

        const string mConfigFile = "HttpConfig.json";

        private readonly RouteRewrite mUrlRewrite;

        private readonly IPv4Tables mIPv4Tables = new();

        private readonly ModuleManager mModuleManager;

        private readonly StaticResurce.ResourceCenter mResourceCenter;

        private IServer mServer;

        private ServerCounter mServerCounter;

        public RouteRewrite UrlRewrite => mUrlRewrite;

        public ServerCounter ServerCounter => mServerCounter;

        private readonly FileLogWriter mFileLog;

        private IPLimit mIPLimit;

        private long mRequestErrors;

        private long mTotalRequests;

        private long mTotalConnections;

        private readonly ActionSettings mActionSettings = new();

        private readonly ActionHandlerFactory mActionFactory;

        private readonly ConcurrentQueue<LogRecord> mCacheLogQueue = new();

        public LogRecord[] GetCacheLog()
        {
            return mCacheLogQueue.ToArray();
        }

        private int mCacheLogLength = 0;

        private readonly ConcurrentDictionary<string, object> mProperties = new();

        public long RequestErrors => mRequestErrors;

        public IPv4Tables IPv4Tables => mIPv4Tables;

        public ModuleManager ModuleManager => mModuleManager;

        public StaticResurce.ResourceCenter ResourceCenter => mResourceCenter;

        public event EventHttpServerLog ServerLog;

        public IServer BaseServer => mServer;

        public ActionHandlerFactory ActionFactory => mActionFactory;

        public HttpOptions Options { get; set; }

        public IDataFrameSerializer FrameSerializer { get; set; }

        private readonly ObjectPoolGroup<HttpRequest> mRequestPool = new();

        internal HttpRequest CreateRequest(ISession session)
        {
            HttpToken token = (HttpToken)session.Tag;
            token.Request.RequestTime = TimeWatch.GetTotalMilliseconds();
            return token.Request;
        }

        internal void Recovery(HttpRequest request)
        {

            if (!mRequestPool.Push(request))
            {

                request.Response = null;
            }
        }

        public void SaveOptions()
        {
            string file = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + mConfigFile;
            using (System.IO.StreamWriter writer = new(file))
            {
                System.Collections.Generic.Dictionary<string, object> config = new();
                config["HttpConfig"] = this.Options;
                string strConfig = Newtonsoft.Json.JsonConvert.SerializeObject(config);
                writer.Write(strConfig);
                writer.Flush();
                OnOptionLoad(new EventOptionsReloadArgs { HttpApiServer = this, HttpOptions = this.Options });
            }
        }

        public static HttpOptions LoadOptions()
        {
            string file = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + mConfigFile;
            if (System.IO.File.Exists(file))
            {
                using (System.IO.StreamReader reader = new(mConfigFile, Encoding.UTF8))
                {
                    string json = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(json))
                        return new HttpOptions();
                    Newtonsoft.Json.Linq.JToken toke = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    if (toke["HttpConfig"] != null && toke["HttpConfig"].Type == JTokenType.Object)
                    {
                        return toke["HttpConfig"].ToObject<HttpOptions>();
                    }
                    return new HttpOptions();
                }
            }
            else
            {
                return new HttpOptions();
            }
        }

        public object this[string name]
        {
            get
            {
                object result;
                mProperties.TryGetValue(name, out result);
                return result;
            }
            set
            {
                mProperties[name] = value;
            }
        }

        public DateTime StartTime { get; set; }

        public long TotalRequest => mTotalRequests;

        public long TotalConnections => mTotalConnections;

        public EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        public event EventHandler<ConnectingEventArgs> HttpConnecting;

        public event EventHandler<EventActionRegistingArgs> ActionRegisting;

        public event EventHandler<ConnectedEventArgs> HttpConnected;

        public event EventHandler<EventHttpRequestArgs> HttpRequesting;

        public event EventHandler<EventHttpRequestArgs> HttpRequestNotfound;

        public event EventHandler<SessionEventArgs> HttpDisconnect;

        public event EventHandler<EventHttpServerStartedArgs> Started;

        public event EventHandler<EventActionExecutingArgs> ActionExecuting;

        public event EventHandler<EventOptionsReloadArgs> OptionLoad;

        public event EventHandler<EventHttpResponsedArgs> HttpResponsed;

        public EventHandler<WebSocketConnectArgs> WebSocketConnect { get; set; }

        private readonly List<System.Reflection.Assembly> mAssemblies = new();

        public List<System.Reflection.Assembly> Assemblies => mAssemblies;

        private DispatchCenter<IOQueueProcessArgs> mRequestIOQueues;

        public ISession LogOutput { get; set; }

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            //mUrlRewrite.UrlIgnoreCase = Options.UrlIgnoreCase;
            mAssemblies.AddRange(assemblies);
            try
            {
                mActionFactory.Register(assemblies);
            }
            catch (Exception e_)
            {
                Log(LogType.Error, "http api server load controller error " + e_.Message + "@" + e_.StackTrace);
            }
        }

        public virtual void IncrementResponsed(HttpRequest request, HttpResponse response, double time, int code, string msg)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref mTotalRequests);
                if (code >= 500)
                    System.Threading.Interlocked.Increment(ref mRequestErrors);
                if (HttpResponsed != null)
                {
                    var e = new EventHttpResponsedArgs(request, response, time, code, msg);
                    HttpResponsed(this, e);
                }
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                    Log(LogType.Error, $"{request.Session.RemoteEndPoint} {request.Method} {request.Url} responsed event error {e_.Message}@{e_.StackTrace}");
            }
        }

        private void InitFromCommandLineArgs(IServer server)
        {
            string[] lines = System.Environment.GetCommandLineArgs();
            if (lines != null)
            {
                foreach (var item in lines)
                {
                    var values = item.Split('=');
                    if (values.Length == 2)
                    {
                        if (string.Compare(values[0], "host", true) == 0)
                        {
                            server.Options.DefaultListen.Host = values[1];
                        }
                        if (string.Compare(values[0], "port", true) == 0)
                        {
                            if (int.TryParse(values[1], out int port))
                            {
                                server.Options.DefaultListen.Port = port
;
                            }
                        }
                    }
                }
            }
        }
        public void Open()
        {
            var date = GMTDate.Default.DATE;
            var ct = ContentTypes.TEXT_UTF8;
            var a = HeaderTypeFactory.Find("Content-Length");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
            HttpPacket hp = new(this, this.FrameSerializer);
            var gtmdate = GMTDate.Default;
            string serverInfo = $"Server: beetlex.io\r\n";
            HeaderTypeFactory.SERVAR_HEADER_BYTES = Encoding.ASCII.GetBytes(serverInfo);
            mServer = SocketFactory.CreateTcpServer(this, hp)
                .Setting(o =>
                {
                    o.SyncAccept = Options.SyncAccept;
                    // o.IOQueues = Options.IOQueues;
                    o.DefaultListen.Host = Options.Host;
                    // o.IOQueueEnabled = Options.IOQueueEnabled;
                    o.DefaultListen.Port = Options.Port;
                    o.BufferSize = Options.BufferSize;
                    o.LogLevel = Options.LogLevel;
                    o.Combined = Options.PacketCombined;
                    o.SessionTimeOut = Options.SessionTimeOut;
                    o.UseIPv6 = Options.UseIPv6;
                    o.BufferPoolMaxMemory = Options.BufferPoolMaxMemory;
                    o.LittleEndian = false;
                    o.Statistical = Options.Statistical;
                    o.BufferPoolGroups = Options.BufferPoolGroups;
                    o.BufferPoolSize = Options.BufferPoolSize;
                    o.PrivateBufferPoolSize = Options.MaxBodyLength;
                    o.MaxWaitMessages = Options.MaxWaitQueue;
                });
            if (Options.IOQueueEnabled)
            {
                mRequestIOQueues = new DispatchCenter<IOQueueProcessArgs>(OnIOQueueProcess, Options.IOQueues);
            }
            if (Options.SSL)
            {
                mServer.Setting(o =>
                {
                    o.AddListenSSL(Options.CertificateFile, Options.CertificatePassword, o.DefaultListen.Host, Options.SSLPort);
                });
            }
            Name = "BeetleX Http Server";
            if (mAssemblies != null)
            {
                foreach (System.Reflection.Assembly assembly in mAssemblies)
                {
                    mResourceCenter.LoadManifestResource(assembly);
                }
            }
            mResourceCenter.LoadManifestResource(typeof(HttpApiServer).Assembly);
            mResourceCenter.Path = Options.StaticResourcePath;
            mResourceCenter.Debug = Options.Debug;
            mResourceCenter.Load();
            ModuleManager.Load();
            if (Options.ManageApiEnabled)
            {
                ServerController serverStatusController = new();
                mActionFactory.Register(serverStatusController);
            }
            StartTime = DateTime.Now;
            mServer.WriteLogo = WriteLogo ?? OutputLogo;
            InitFromCommandLineArgs(mServer);
            mServer.Open();
            mServerCounter = new ServerCounter(this);
            // mUrlRewrite.UrlIgnoreCase = Options.UrlIgnoreCase;
            mUrlRewrite.Load();
            //mUrlRewrite.AddRegion(this.Options.Routes);
            HeaderTypeFactory.Find(HeaderTypeFactory.HOST);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                using (System.IO.StreamWriter writer = new("__UnhandledException.txt"))
                {
                    Exception error = e.ExceptionObject as Exception;
                    writer.WriteLine(DateTime.Now);
                    if (error != null)
                    {
                        writer.WriteLine(error.Message);
                        writer.WriteLine(error.StackTrace);
                        if (error.InnerException != null)
                        {
                            writer.WriteLine(error.InnerException.Message);
                            writer.WriteLine(error.InnerException.StackTrace);
                        }
                    }
                    else
                    {
                        writer.WriteLine("Unhandled Exception:" + e.ExceptionObject.ToString());

                    }
                    writer.Flush();
                }
            };
            mIPLimit = new IPLimit(this);
            OnOptionLoad(new EventOptionsReloadArgs { HttpApiServer = this, HttpOptions = this.Options });
            OnStrated(new EventHttpServerStartedArgs { HttpApiServer = this });
            if (Options.Virtuals != null)
            {
                foreach (var item in Options.Virtuals)
                {
                    item.Verify();
                    if (EnableLog(LogType.Info))
                        Log(LogType.Info, $"Set virtual folder {item.Folder} to {item.Path}");
                }
            }

        }

        public void AddExts(string exts)
        {
            exts = exts.ToLower();
            ResourceCenter.SetFileExts(exts);
        }

        public void AddVirtualFolder(string folder, string path)
        {
            if (Options.Virtuals == null)
                Options.Virtuals = new List<VirtualFolder>();
            VirtualFolder vf = new() { Folder = folder, Path = path };
            vf.Verify();
            var has = Options.Virtuals.FirstOrDefault(p => string.Compare(p.Folder, vf.Folder, true) == 0);
            if (has == null)
            {
                Options.Virtuals.Add(vf);
            }
            else
            {
                has.Path = vf.Path;
            }
            SaveOptions();
        }

        public void ChangeExtContentType(string ext, string contentType)
        {
            ext = ext.ToLower();
            AddExts(ext);
            ResourceCenter.Exts[ext].ContentType = contentType;
        }

        public Action WriteLogo { get; set; }

        private void OutputLogo()
        {
            AssemblyCopyrightAttribute productAttr = typeof(BeetleX.FastHttpApi.HttpApiServer).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var logo = "\r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo +=
@"          ____                  _     _         __   __
         |  _ \                | |   | |        \ \ / /
         | |_) |   ___    ___  | |_  | |   ___   \ V / 
         |  _ <   / _ \  / _ \ | __| | |  / _ \   > <  
         | |_) | |  __/ |  __/ | |_  | | |  __/  / . \ 
         |____/   \___|  \___|  \__| |_|  \___| /_/ \_\ 

                            http and websocket framework   

";
            logo += " -----------------------------------------------------------------------------\r\n";
            //logo += $" {productAttr.Copyright}\r\n";
            logo += $" ServerGC    [{GCSettings.IsServerGC}]\r\n";
            logo += $" BeetleX     Version [{typeof(BeetleX.BXException).Assembly.GetName().Version}]\r\n";
            logo += $" FastHttpApi Version [{ typeof(HttpApiServer).Assembly.GetName().Version}] \r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            foreach (var item in mServer.Options.Listens)
            {
                logo += $" {item}\r\n";
            }
            logo += " -----------------------------------------------------------------------------\r\n";

            Log(LogType.Info, logo);


        }

        public void ActionSettings(ActionHandler handler)
        {
            mActionSettings.SetAction(handler);
        }

        public void SaveActionSettings()
        {
            try
            {
                mActionSettings.Save(ActionFactory.Handlers);
                if (EnableLog(LogType.Info))
                    Log(LogType.Info, $"HTTP save actions settings success");
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                    Log(LogType.Error, $"HTTP save actions settings error {e_.Message}@{e_.StackTrace}");
            }
        }

        public Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            try
            {
                //  Log(LogType.Info, $"{args.RequestingAssembly.FullName} load assembly {args.Name}");
                string path = System.IO.Path.GetDirectoryName(args.RequestingAssembly.Location) + System.IO.Path.DirectorySeparatorChar;
                string name = args.Name.Substring(0, args.Name.IndexOf(','));
                string file = path + name + ".dll";
                Assembly result = Assembly.LoadFile(file);
                return result;
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Warring))
                    Log(LogType.Warring, $"{args.RequestingAssembly.FullName} load assembly {args.Name} error {e_.Message}");
            }
            return null;
        }

        public string Name { get { return mServer.Name; } set { mServer.Name = value; } }


        public void SendToWebSocket(DataFrame data, Func<ISession, HttpRequest, bool> filter = null)
        {
            IList<HttpRequest> items = GetWebSockets();

            if (items.Count > 0)
            {
                List<ISession> receiveRequest = new();
                IServer server = items[0].Server.BaseServer;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if ((filter == null || filter(item.Session, item)))
                    {
                        receiveRequest.Add(item.Session);
                    }

                }
                server.Send(data, receiveRequest.ToArray());
            }
        }

        public void SendToWebSocket(DataFrame data, params HttpRequest[] request)
        {
            if (request != null)
            {
                IServer server = request[0].Server.BaseServer;
                ISession[] sessions = new ISession[request.Length];
                for (int i = 0; i < request.Length; i++)
                {
                    sessions[i] = request[i].Session;
                }
                server.Send(data, sessions);

            }
        }

        private void OnSendToWebSocket(DataFrame data, HttpRequest request)
        {
            data.Send(request.Session);

        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            System.Threading.Interlocked.Increment(ref mTotalConnections);
            HttpToken token = new();
            token.Request = new HttpRequest();
            token.Request.Init(e.Session, this);
            e.Session.Tag = token;
            e.Session.SocketProcessHandler = this;
            if (Options.IOQueueEnabled)
                token.IOQueue = mRequestIOQueues.Next();
            HttpConnected?.Invoke(server, e);
            base.Connected(server, e);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            try
            {
                HttpToken token = (HttpToken)e.Session.Tag;
                if (token != null)
                {
                    if (token.Request != null)
                        token.Request.Response = null;
                    token.Request = null;
                }

                if (LogOutput == e.Session)
                    LogOutput = null;
                HttpDisconnect?.Invoke(server, e);
                SessionControllerFactory.DisposedFactory(e.Session);
                base.Disconnect(server, e);
            }
            finally
            {
                e.Session.Tag = null;
            }
        }

        //public void Log(LogType type, string message, params object[] parameters)
        //{
        //    Log(type, null, string.Format(message, parameters));
        //}

        //public void Log(LogType type, string tag, string message, params object[] parameters)
        //{
        //    Log(type, tag, string.Format(message, parameters));
        //}

        public void Log(LogType type, object tag, string message)
        {
            try
            {
                Log(null, new HttpServerLogEventArgs(tag, message, type));
            }
            catch { }
        }

        public void Log(LogType type, string message)
        {
            Log(type, null, message);
        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {
            if (server.Count > Options.MaxConnections)
            {
                e.Cancel = true;
                if (EnableLog(LogType.Warring))
                {
                    Log(LogType.Warring, $"HTTP ${e.Socket.RemoteEndPoint} out of max connections!");
                }
            }
            IPEndPoint ipPoint = (IPEndPoint)e.Socket.RemoteEndPoint;
            if (!IPv4Tables.Verify(ipPoint.Address))
            {
                e.Cancel = true;
                if (EnableLog(LogType.Warring))
                {
                    Log(LogType.Warring, $"HTTP ${e.Socket.RemoteEndPoint} IPv4 tables verify no permission!");
                }
            }
            if (Options.IPRpsLimit > 0)
            {
                string ip = ((IPEndPoint)e.Socket.RemoteEndPoint).Address.ToString();
                var item = mIPLimit.GetItem(ip);
                if (item != null)
                {
                    if (!item.Enabled)
                        e.Cancel = true;
                }
            }
            HttpConnecting?.Invoke(this, e);
            e.Socket.NoDelay = true;
        }

        public override void Error(IServer server, ServerErrorEventArgs e)
        {
            base.Error(server, e);
        }


        #region websocket
        public virtual object FrameDeserialize(DataFrame data, PipeStream stream)
        {
            return stream.ReadString((int)data.Length);
        }

        private readonly System.Collections.Concurrent.ConcurrentQueue<byte[]> mBuffers = new();

        public virtual ArraySegment<byte> FrameSerialize(DataFrame data, object body)
        {
            byte[] result;
            if (!mBuffers.TryDequeue(out result))
            {
                result = new byte[this.Options.MaxBodyLength];
            }
            string value;
            if (body is string)
            {
                value = (string)body;
                int length = Options.Encoding.GetBytes(value, 0, value.Length, result, 0);
                return new ArraySegment<byte>(result, 0, length);
            }
            else
            {
                value = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                int length = Options.Encoding.GetBytes(value, 0, value.Length, result, 0);
                return new ArraySegment<byte>(result, 0, length);
            }
        }

        public virtual void FrameRecovery(byte[] buffer)
        {
            mBuffers.Enqueue(buffer);
        }

        private void OnWebSocketConnect(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.KeepAlive = true;
            token.WebSocket = true;
            request.WebSocket = true;
            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, request.Session, $"HTTP {request.ID} {request.Session.RemoteEndPoint} upgrade to websocket");
            }
            ConnectionUpgradeWebsocket(request, response);


        }

        protected virtual void ConnectionUpgradeWebsocket(HttpRequest request, HttpResponse response)
        {
            WebSocketConnectArgs wsca = new(request);
            wsca.Request = request;
            WebSocketConnect?.Invoke(this, wsca);
            if (wsca.Cancel)
            {
                if (EnableLog(LogType.Warring))
                {
                    mServer.Log(LogType.Warring, request.Session, $"HTTP {request.ID} {request.Session.RemoteEndPoint} cancel upgrade to websocket");
                }
                response.Session.Dispose();
            }
            else
            {
                UpgradeWebsocketResult upgradeWebsocket = new(request.Header[HeaderTypeFactory.SEC_WEBSOCKET_KEY]);
                response.Result(upgradeWebsocket);
            }
        }

        public void ExecuteWS(HttpRequest request, DataFrame dataFrame)
        {
            JToken dataToken = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject((string)dataFrame.Body);
            this.mActionFactory.ExecuteWithWS(request, this, dataToken);
        }


        public DataFrame CreateDataFrame(object body = null)
        {
            DataFrame dp = new(this);
            dp.DataPacketSerializer = this.FrameSerializer;
            dp.Body = body;
            return dp;
        }

        protected virtual void OnWebSocketRequest(HttpRequest request, ISession session, DataFrame data)
        {

            if (Options.WebSocketMaxRPS > 0 && session.Count > Options.WebSocketMaxRPS)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Log(LogType.Error, session, $"Websocket {request.ID} {request.RemoteIPAddress} session message queuing exceeds maximum rps!");
                }
                session.Dispose();
                return;
            }

            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, session, $"Websocket {request.ID} {request.RemoteIPAddress} receive {data.Type.ToString()}");
            }
            HttpToken token = (HttpToken)session.Tag;
            if (data.Type == DataPacketType.ping)
            {
                DataFrame pong = CreateDataFrame();
                pong.Type = DataPacketType.pong;
                pong.FIN = true;
                session.Send(pong);
            }
            else if (data.Type == DataPacketType.connectionClose)
            {
                session.Dispose();
            }
            else
            {
                if (WebSocketReceive == null)
                {
                    if (data.Type == DataPacketType.text)
                    {
                        ExecuteWS(token.Request, data);
                    }
                }
                else
                {

                    try
                    {
                        var args = new WebSocketReceiveArgs();
                        args.Frame = data;
                        args.Sesson = session;
                        args.Server = this;
                        args.Request = token.Request;
                        WebSocketReceive?.Invoke(this, args);
                    }
                    finally
                    {

                    }
                }

            }

        }

        #endregion

        private void CacheLog(ServerLogEventArgs e)
        {
            if (Options.CacheLogMaxSize > 0)
            {
                LogRecord record = new();
                record.Type = e.Type.ToString();
                record.Message = e.Message;
                record.Time = DateTime.Now.ToString("H:mm:ss");
                System.Threading.Interlocked.Increment(ref mCacheLogLength);
                if (mCacheLogLength > Options.CacheLogMaxSize)
                {
                    mCacheLogQueue.TryDequeue(out LogRecord log);
                    System.Threading.Interlocked.Decrement(ref mCacheLogLength);
                }
                mCacheLogQueue.Enqueue(record);
            }
        }

        public override void Log(IServer server, ServerLogEventArgs e)
        {
            var httpLog = e as HttpServerLogEventArgs;
            CacheLog(e);
            ServerLog?.Invoke(server, e);
            if (Options.LogToConsole && (httpLog == null || httpLog.OutputConsole))
                base.Log(server, e);
            if (Options.WriteLog && (httpLog == null || httpLog.OutputFile))
                mFileLog.Add(e.Type, e.Message);
            ISession output = LogOutput;
            if (output != null && e.Session != output)
            {
                ActionResult log = new();
                log.Data = new { LogType = e.Type.ToString(), Time = DateTime.Now.ToString("H:mm:ss"), Message = e.Message };
                CreateDataFrame(log).Send(output);
            }
        }

        protected virtual void OnOptionLoad(EventOptionsReloadArgs e)
        {
            if (EnableLog(LogType.Debug))
                Log(LogType.Debug, "HTTP server options reload event!");
            OptionLoad?.Invoke(this, e);
        }

        protected virtual void OnStrated(EventHttpServerStartedArgs e)
        {
            if (EnableLog(LogType.Debug))
                Log(LogType.Debug, "HTTP server started event!");
            Started?.Invoke(this, e);
        }

        protected virtual void OnProcessResource(HttpRequest request, HttpResponse response)
        {

            try
            {
                if (request.Method == HttpParse.GET_TAG)
                {
                    try
                    {
                        mResourceCenter.ProcessFile(request, response);
                    }
                    catch (Exception e_)
                    {
                        if (EnableLog(LogType.Error))
                        {
                            BaseServer.Error(e_, request.Session, $"{request.RemoteIPAddress} {request.Method} {request.BaseUrl} file error {e_.Message}");
                        }
                        InnerErrorResult result = new($"response file error ", e_, Options.OutputStackTrace);
                        response.Result(result);
                    }
                }
                else
                {
                    if (EnableLog(LogType.Info))
                        Log(LogType.Info, $"{request.RemoteIPAddress}{request.Method} {request.Url} not support");
                    NotSupportResult notSupport = new($"{request.Method} {request.Url} not support");
                    response.Result(notSupport);

                }
            }
            finally
            {
            }
        }

        internal class IOQueueProcessArgs
        {

            public HttpRequest Request;

            public HttpResponse Response;

        }

        private void OnIOQueueProcess(IOQueueProcessArgs e)
        {
            try
            {
                if (!e.Request.Session.IsDisposed)
                    OnHttpRequest(e.Request, e.Response);
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Error(e_, e.Request.Session, $"HTTP {e.Request.ID} {e.Request.RemoteIPAddress} on queue process error {e_.Message}@{e_.StackTrace}");
                }
            }
        }

        private void OnRequestHandler(PacketDecodeCompletedEventArgs e)
        {
            try
            {
                HttpToken token = (HttpToken)e.Session.Tag;
                if (token.WebSocket)
                {
                    OnWebSocketRequest(token.Request, e.Session, (WebSockets.DataFrame)e.Message);
                }
                else
                {
                    HttpRequest request = (HttpRequest)e.Message;

                    if (EnableLog(LogType.Info))
                    {
                        mServer.Log(LogType.Info, null, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {request.Url}");
                    }
                    if (EnableLog(LogType.Debug))
                    {
                        mServer.Log(LogType.Debug, e.Session, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {request.Url} detail {request.ToString()}");
                    }
                    request.Server = this;
                    HttpResponse response = request.CreateResponse();
                    token.KeepAlive = request.KeepAlive;
                    if (!mIPLimit.ValidateRPS(request))
                    {
                        token.KeepAlive = false;
                        InnerErrorResult innerErrorResult = new("400", $"{request.RemoteIPAddress} request limit!");
                        response.Result(innerErrorResult);
                        return;
                    }
                    if (token.FirstRequest && string.Compare(request.Header[HeaderTypeFactory.UPGRADE], "websocket", true) == 0)
                    {
                        token.FirstRequest = false;
                        OnWebSocketConnect(request, response);
                    }
                    else
                    {
                        token.FirstRequest = false;
                        if (!Options.IOQueueEnabled)
                        {
                            OnHttpRequest(request, response);
                        }
                        else
                        {
                            IOQueueProcessArgs args = new()
                            {
                                Request = request,
                                Response = response
                            };
                            token.IOQueue.Enqueue(args);
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Error(e_, e.Session, $"HTTP {e.Session.RemoteEndPoint} {0} OnRequestHandler error {e_.Message}@{e_.StackTrace}");
                }
            }
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            if (Options.SessionTimeOut > 0)
            {
                BaseServer.UpdateSession(e.Session);
            }
            OnRequestHandler(e);
        }

        internal void OnActionRegisting(EventActionRegistingArgs e)
        {
            ActionRegisting?.Invoke(this, e);
        }

        internal bool OnActionExecuting(IHttpContext context,ActionHandler handler)
        {
            if (ActionExecuting != null)
            {
                EventActionExecutingArgs e = new();
                e.HttpContext = context;
                e.Handler = handler;
                ActionExecuting(this, e);
                return !e.Cancel;
            }
            return true;
        }

        internal EventHttpRequestArgs OnHttpRequesNotfound(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.RequestArgs.Request = request;
            token.RequestArgs.Response = response;
            token.RequestArgs.Cancel = false;
            HttpRequestNotfound?.Invoke(this, token.RequestArgs);
            return token.RequestArgs;
        }


        internal EventHttpRequestArgs OnHttpRequesting(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.RequestArgs.Request = request;
            token.RequestArgs.Response = response;
            token.RequestArgs.Cancel = false;
            HttpRequesting?.Invoke(this, token.RequestArgs);
            return token.RequestArgs;
        }

        public ServerCounter.ServerStatus GetServerInfo()
        {
            if (ServerCounter != null)
            {
                return ServerCounter.Next();
            }
            return new ServerCounter.ServerStatus();
        }

        protected virtual void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            if (!OnHttpRequesting(request, response).Cancel)
            {
                string baseUrl = request.BaseUrl;
                if (string.IsNullOrEmpty(request.Ext) && baseUrl[baseUrl.Length - 1] != '/')
                {
                    mActionFactory.Execute(request, response, this);
                }
                else
                {
                    OnProcessResource(request, response);
                }
            }
        }


        public virtual void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {

        }



        public virtual void SendCompleted(ISession session, SocketAsyncEventArgs e, bool end)
        {
            HttpToken token = (HttpToken)session.Tag;
            if (token.File != null)
            {
                token.File = token.File.Next();
                if (token.File != null)
                {
                    session.Send(token.File);
                    return;
                }
            }
            if (session.Count == 0 && !token.KeepAlive)
            {
                session.Dispose();
            }
        }

        private long mVersion;

        private IList<HttpRequest> mOnlines = new List<HttpRequest>();

        private int mGetWebsocketStatus = 0;

        public IList<HttpRequest> GetWebSockets()
        {
            if (mVersion != BaseServer.Version)
            {
                if (System.Threading.Interlocked.CompareExchange(ref mGetWebsocketStatus, 1, 0) == 0)
                {
                    try
                    {
                        if (mVersion != BaseServer.Version)
                        {
                            ISession[] items = BaseServer.GetOnlines();
                            List<HttpRequest> lst = new();
                            for (int i = 0; i < items.Length; i++)
                            {
                                HttpToken token = (HttpToken)items[i].Tag;
                                if (token != null && token.WebSocket)
                                    lst.Add(token.Request);
                            }
                            mOnlines = lst;
                            mVersion = BaseServer.Version;
                        }
                    }
                    finally
                    {
                        System.Threading.Interlocked.Exchange(ref mGetWebsocketStatus, 0);
                    }

                }
            }
            return mOnlines;
        }

        public bool EnableLog(LogType logType)
        {
            return (int)(this.Options.LogLevel) <= (int)logType;
        }

        public HttpApiServer GetLog(LogType logType)
        {
            if (EnableLog(logType))
                return this;
            return null;
        }

        public Validations.IValidationOutputHandler ValidationOutputHandler { get; set; } = new Validations.ValidationOutputHandler();

        public void Dispose()
        {
            if (BaseServer != null)
                BaseServer.Dispose();
        }

    }
}
