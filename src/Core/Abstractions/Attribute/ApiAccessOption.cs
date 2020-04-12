using System;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    ///     API��������
    /// </summary>
    [Flags]
    public enum ApiAccessOption
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
        ///     �ڲ�����(�ڲ�Poster)
        /// </summary>
        Internal = 0x2,

        /// <summary>
        ///     ��������
        /// </summary>
        Anymouse = 0x4,

        /// <summary>
        ///     ��Ҫ�����֤
        /// </summary>
        Authority = 0x8,

        /// <summary>
        ///     ��������Ϊnull
        /// </summary>
        ArgumentCanNil = 0x10,

        /// <summary>
        ///     ������Ϊ����,�ڲ�����ͨ���ֵ��ȡ
        /// </summary>
        DictionaryArgument = 0x20,

        /// <summary>
        /// ���ŷ���,����������������
        /// </summary>
        OpenAccess = Public | Anymouse,

        /// <summary>
        /// �û�����,����������Ҫ�����֤
        /// </summary>
        UserAccess = Public | Authority,

        /// <summary>
        ///     ������Ϊ����,�ڲ�����ͨ���ֵ��ȡ
        /// </summary>
        ArgumentIsDefault = DictionaryArgument
    }
}