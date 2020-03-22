namespace ZeroMQ
{
    /// <summary>
    /// pool�ڵ�
    /// </summary>
    public class ZPollItem
	{
#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public ZPollEvent Events;
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public ZPollEvent ReadyEvents;
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public delegate bool ReceiveDelegate(ZSocket socket, out ZMessage message, out ZError error);
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public ReceiveDelegate ReceiveMessage;
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static bool DefaultReceiveMessage(ZSocket socket, out ZMessage message, out ZError error)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			message = null;
			return socket.ReceiveMessage(ref message, out error);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public delegate bool SendDelegate(ZSocket socket, ZMessage message, out ZError error);
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public SendDelegate SendMessage;
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static bool DefaultSendMessage(ZSocket socket, ZMessage message, out ZError error)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return socket.Send(message, out error);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
	    public ZPollItem(ZPollEvent events)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			Events = events;
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZPollItem Create(ReceiveDelegate receiveMessage)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return Create(receiveMessage, null);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZPollItem CreateSender(SendDelegate sendMessage)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return Create(null, sendMessage);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZPollItem Create(ReceiveDelegate receiveMessage, SendDelegate sendMessage)
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			var pollItem = new ZPollItem((receiveMessage != null ? ZPollEvent.In : ZPollEvent.None) | (sendMessage != null ? ZPollEvent.Out : ZPollEvent.None));
			pollItem.ReceiveMessage = receiveMessage;
			pollItem.SendMessage = sendMessage;
			return pollItem;
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZPollItem CreateReceiver()
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return Create(DefaultReceiveMessage, null);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZPollItem CreateSender()
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return Create(null, DefaultSendMessage);
		}

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		public static ZPollItem CreateReceiverSender()
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
		{
			return Create(DefaultReceiveMessage, DefaultSendMessage);
		}
	}
}