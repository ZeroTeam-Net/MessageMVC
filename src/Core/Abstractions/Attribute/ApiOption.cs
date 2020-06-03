using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API��������
    /// </summary>
    [Flags]
    public enum ApiOption
    {
        /// <summary>
        ///     ���ɷ���
        /// </summary>
        None,

        /// <summary>
        ///     ��������(�������)
        /// </summary>
        Public = 0x1,

        /// <summary>
        ///     ��������
        /// </summary>
        Anymouse = 0x2,

        /// <summary>
        ///     ֻ������
        /// </summary>
        Readonly = 0x4,

        /// <summary>
        ///     ��������Ϊnull
        /// </summary>
        ArgumentCanNil = 0x10,

        /// <summary>
        ///     ������Ϊ����,�ڲ�����ͨ���ֵ��ȡ
        /// </summary>
        DictionaryArgument = 0x20,

        /// <summary>
        ///     �����Զ������
        /// </summary>
        CustomContent = 0x40
    }
}