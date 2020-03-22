using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ZeroMQ.Monitoring
{

    /// <summary>
    /// Monitors state change events on another socket within the same context.
    /// </summary>
    public class ZMonitor : ZThread
	{
		/// <summary>
		/// The polling interval in milliseconds.
		/// </summary>
		public static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(64);

		private ZSocket _socket;

		private readonly string _endpoint;

		private readonly Dictionary<ZMonitorEvents, Action<ZMonitorEventData>> _eventHandler;

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		protected ZMonitor(ZSocket socket, string endpoint)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
			: this(ZContext.Current, socket, endpoint) 
		{ }

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

		protected ZMonitor(ZContext context, ZSocket socket, string endpoint)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
            // TODO: remove socket argument and create socket within Run?
			_socket = socket;
			_endpoint = endpoint;
			_eventHandler = new Dictionary<ZMonitorEvents, Action<ZMonitorEventData>>
			{
				{ ZMonitorEvents.AllEvents, data => InvokeEvent(AllEvents, () => new ZMonitorEventArgs(this, data)) },
				{ ZMonitorEvents.Connected, data => InvokeEvent(Connected, () => new ZMonitorFileDescriptorEventArgs(this, data)) },
				{ ZMonitorEvents.ConnectDelayed, data => InvokeEvent(ConnectDelayed, () => new ZMonitorEventArgs(this, data)) },
				{ ZMonitorEvents.ConnectRetried, data => InvokeEvent(ConnectRetried, () => new ZMonitorIntervalEventArgs(this, data)) },
				{ ZMonitorEvents.Listening, data => InvokeEvent(Listening, () => new ZMonitorFileDescriptorEventArgs(this, data)) },
				{ ZMonitorEvents.BindFailed, data => InvokeEvent(BindFailed, () => new ZMonitorEventArgs(this, data)) },
				{ ZMonitorEvents.Accepted, data => InvokeEvent(Accepted, () => new ZMonitorFileDescriptorEventArgs(this, data)) },
				{ ZMonitorEvents.AcceptFailed, data => InvokeEvent(AcceptFailed, () => new ZMonitorEventArgs(this, data)) },
				{ ZMonitorEvents.Closed, data => InvokeEvent(Closed, () => new ZMonitorFileDescriptorEventArgs(this, data)) },
				{ ZMonitorEvents.CloseFailed, data => InvokeEvent(CloseFailed, () => new ZMonitorEventArgs(this, data)) },
				{ ZMonitorEvents.Disconnected, data => InvokeEvent(Disconnected, () => new ZMonitorFileDescriptorEventArgs(this, data)) },
				{ ZMonitorEvents.Stopped, data => InvokeEvent(Stopped, () => new ZMonitorEventArgs(this, data)) },
			};
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZMonitor Create(string endpoint)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return Create(ZContext.Current, endpoint);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZMonitor Create(ZContext context, string endpoint)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
            ZMonitor monitor;
            if (null == (monitor = Create(context, endpoint, out var error)))
			{
				throw new ZException(error);
			}
			return monitor;
		}

		/// <summary>
		/// Create a socket with the current context and the specified socket type.
		/// </summary>
		public static ZMonitor Create(string endpoint, out ZError error)
		{
			return Create(ZContext.Current, endpoint, out error);
		}

		/// <summary>
		/// Create a socket with the current context and the specified socket type.
		/// </summary>
		public static ZMonitor Create(ZContext context, string endpoint, out ZError error)
		{
			ZSocket socket;
			if (null == (socket = ZSocket.Create(context, ZSocketType.PAIR, out error)))
			{
				return default;
			}

			return new ZMonitor(context, socket, endpoint);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public event EventHandler<ZMonitorEventArgs> AllEvents;
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

		/// <summary>
		/// Occurs when a new connection is established.
		/// </summary>
		public event EventHandler<ZMonitorFileDescriptorEventArgs> Connected;

		/// <summary>
		/// Occurs when a synchronous connection attempt failed, and its completion is being polled for.
		/// </summary>
		public event EventHandler<ZMonitorEventArgs> ConnectDelayed;

		/// <summary>
		/// Occurs when an asynchronous connect / reconnection attempt is being handled by a reconnect timer.
		/// </summary>
		public event EventHandler<ZMonitorIntervalEventArgs> ConnectRetried;

		/// <summary>
		/// Occurs when a socket is bound to an address and is ready to accept connections.
		/// </summary>
		public event EventHandler<ZMonitorFileDescriptorEventArgs> Listening;

		/// <summary>
		/// Occurs when a socket could not bind to an address.
		/// </summary>
		public event EventHandler<ZMonitorEventArgs> BindFailed;

		/// <summary>
		/// Occurs when a connection from a remote peer has been established with a socket's listen address.
		/// </summary>
		public event EventHandler<ZMonitorFileDescriptorEventArgs> Accepted;

		/// <summary>
		/// Occurs when a connection attempt to a socket's bound address fails.
		/// </summary>
		public event EventHandler<ZMonitorEventArgs> AcceptFailed;

		/// <summary>
		/// Occurs when a connection was closed.
		/// </summary>
		public event EventHandler<ZMonitorFileDescriptorEventArgs> Closed;

		/// <summary>
		/// Occurs when a connection couldn't be closed.
		/// </summary>
		public event EventHandler<ZMonitorEventArgs> CloseFailed;

		/// <summary>
		/// Occurs when the stream engine (tcp and ipc specific) detects a corrupted / broken session.
		/// </summary>
		public event EventHandler<ZMonitorFileDescriptorEventArgs> Disconnected;

		/// <summary>
		/// Monitoring on this socket ended.
		/// </summary>
		public event EventHandler<ZMonitorEventArgs> Stopped;

		/// <summary>
		/// Gets the endpoint to which the monitor socket is connected.
		/// </summary>
		public string Endpoint => _endpoint;

	    // private static readonly int sizeof_MonitorEventData = Marshal.SizeOf(typeof(ZMonitorEventData));

	    /// <summary>
	    /// Begins monitoring for state changes, raising the appropriate events as they arrive.
	    /// </summary>
	    /// <remarks>NOTE: This is a blocking method and should be run from another thread.</remarks>
	    protected override void Run()
	    {
	        using (_socket)
	        {
                if (!_socket.Connect(_endpoint, out var error))
                {
                    LogError(error, "connect");
                    return;
                }

                var poller = ZPollItem.CreateReceiver();

	            while (!Cancellor.IsCancellationRequested)
	            {
                    if (!_socket.PollIn(poller, out var incoming, out error, PollingInterval))
                    {
                        if (error.IsError(ZError.Code.EAGAIN))
                        {
                            // TODO: why sleep here? the loop frequency is already controlled by PollingInterval
                            Thread.Sleep(1);
                            continue;
                        }

                        LogError(error, "poll");
                    }

                    var eventValue = new ZMonitorEventData();

	                using (incoming)
	                {
	                    if (incoming.Count > 0)
	                    {
	                        eventValue.Event = (ZMonitorEvents)incoming[0].ReadInt16();
	                        eventValue.EventValue = incoming[0].ReadInt32();
	                    }

	                    if (incoming.Count > 1)
	                    {
	                        eventValue.Address = incoming[1].ReadString();
	                    }
	                }

	                OnMonitor(eventValue);
	            }

	            if (!_socket.Disconnect(_endpoint, out error))
	            {
	                LogError(error, "disconnect");
	            }
	        }
	    }

	    private void LogError(ZError error, string context)
	    {
	        // TODO: this error handling is somewhat too subtle; the client should be able to retrieve it
	        if (!error.IsError(ZError.Code.ETERM))
	        {
	            Trace.TraceError($"error on {context}: {error}");
	        }
	    }

	    internal void OnMonitor(ZMonitorEventData data)
		{
			if (_eventHandler.ContainsKey(ZMonitorEvents.AllEvents))
			{
				_eventHandler[ZMonitorEvents.AllEvents](data);
			}
			if (_eventHandler.ContainsKey(data.Event))
			{
				_eventHandler[data.Event](data);
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ZMonitor"/>, and optionally disposes of the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_socket != null)
					{
						_socket.Dispose();
						_socket = null;
					}
				}
			}
			base.Dispose(disposing);
		}

		private void InvokeEvent<T>(EventHandler<T> handler, Func<T> createEventArgs) where T : EventArgs
		{
            handler?.Invoke(this, createEventArgs());
        }
	}
}