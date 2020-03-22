namespace Agebull.MicroZero
{
    /// <summary>
    /// Zmq������
    /// </summary>
    public enum ZeroByteCommand : sbyte
    {
        /// <summary>
        /// ��׼����
        /// </summary>
        General = (sbyte)1,

        /// <summary>
        /// �ƻ�����
        /// </summary>
        Plan = (sbyte)2,

        /// <summary>
        /// ����ִ��
        /// </summary>
        Proxy = (sbyte)3,

        /// <summary>
        /// ����
        /// </summary>
        Restart = (sbyte)4,

        /// <summary>
        /// ȫ�ֱ�ʶ
        /// </summary>
        GetGlobalId = (sbyte)'>',

        /// <summary>
        /// �ȴ����
        /// </summary>
        Waiting = (sbyte)'#',

        /// <summary>
        /// ���ҽ��
        /// </summary>
        Find = (sbyte)'%',

        /// <summary>
        /// �رս��
        /// </summary>
        Close = (sbyte)'-',

        /// <summary>
        /// Ping
        /// </summary>
        Ping = (sbyte)'*',

        /// <summary>
        /// ��������
        /// </summary>
        HeartJoin = (sbyte)'J',

        /// <summary>
        /// ��������
        /// </summary>
        HeartReady = (sbyte)'R',

        /// <summary>
        /// ��������
        /// </summary>
        HeartPitpat = (sbyte)'P',

        /// <summary>
        /// �����˳�
        /// </summary>
        HeartLeft = (sbyte)'L',
    }
}