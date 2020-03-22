namespace ZeroMQ.Monitoring
{
	using System;

	/// <summary>
	/// A base class for the all ZmqMonitor events.
	/// </summary>
	public class ZMonitorEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ZMonitorEventArgs"/> class.
		/// </summary>
		/// <param name="monitor">The <see cref="ZMonitor"/> that triggered the event.</param>
		/// <param name="ed">The peer address.</param>
		public ZMonitorEventArgs(ZMonitor monitor, ZMonitorEventData ed)
		{
			Monitor = monitor;
			Event = ed;
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public ZMonitorEventData Event { get; private set; }
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

		/// <summary>
		/// Gets the monitor that triggered the event.
		/// </summary>
		public ZMonitor Monitor { get; private set; }
	}
}