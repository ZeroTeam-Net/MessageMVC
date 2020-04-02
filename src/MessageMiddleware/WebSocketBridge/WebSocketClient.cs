using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Web
{
    /// <summary>
    /// 表示一个WebSocket连接
    /// </summary>
    internal class WebSocketClient : IDisposable
    {
        internal readonly List<string> Subscriber = new List<string>();
        internal const int BufferSize = 4096;
        private readonly WebSocket _socket;
        public readonly string Classify;
        private readonly List<WebSocketClient> _clients;


        internal WebSocketClient(WebSocket socket, string path, List<WebSocketClient> clients)
        {
            Classify = path.Trim('\\', '/', ' ');
            _socket = socket;
            _clients = clients;
        }


        internal async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            while (_socket.State == WebSocketState.Open)
            {
                try
                {
                    string value;
                    using (var mem = new MemoryStream())
                    {
                        var incoming = await _socket.ReceiveAsync(seg, CancellationToken.None);
                        if (!incoming.EndOfMessage)
                        {
                            break;
                        }
                        if (incoming.Count == 0)
                            continue;
                        mem.Write(seg.Array, 0, incoming.Count);
                        mem.Flush();
                        mem.Position = 0;
                        TextReader reader = new StreamReader(mem);
                        value = reader.ReadToEnd();
                    }
                    if (string.IsNullOrEmpty(value) || value.Length <= 1)
                        continue;

                    string title = value.Length == 0 ? "" : value.Substring(1);
                    if (value[0] == '+')
                    {
                        if (!string.IsNullOrWhiteSpace(title) && !Subscriber.Contains(title))
                            Subscriber.Add(title);
                    }
                    else if (value[0] == '-')
                    {
                        if (string.IsNullOrWhiteSpace(title))
                            Subscriber.Clear();
                        else
                            Subscriber.Remove(title);
                    }
                }
                catch (WebSocketException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
            Dispose();
        }
        internal async Task Send(ArraySegment<byte> title, ArraySegment<byte> array)
        {
            if (_isDisposed)
                return;
            try
            {
                //await this.socket.SendAsync(title, WebSocketMessageType.Text, true, CancellationToken.None);
                await _socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                //Dispose();
            }
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            _clients.Remove(this);
            try
            {
                _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭", CancellationToken.None);
            }
            catch
            {
                // ignored
            }
        }
    }
}