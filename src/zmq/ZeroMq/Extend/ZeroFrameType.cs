namespace Agebull.MicroZero
{
    /// <summary>
    ///     ֡����
    /// </summary>
    public class ZeroFrameType
    {
        /// <summary>
        ///     ��ֹ����
        /// </summary>
        public const byte End = 0;

        /// <summary>
        ///     ��ֹ����
        /// </summary>
        public const byte ExtendEnd = 0xFF;

        /// <summary>
        ///     ��ֹ����(һ�㷵��ֵ)
        /// </summary>
        public const byte ResultEnd = 0xFE;
        
        /// <summary>
        ///     ��ֹ����(�ļ�����ֵ)
        /// </summary>
        public const byte ResultFileEnd = 0xFD;

        /// <summary>
        ///     ȫ�ֱ�ʶ
        /// </summary>
        public const byte GlobalId = 1;

        /// <summary>
        ///     վ������֡
        /// </summary>
        public const byte Station = 2;

        /// <summary>
        ///     ״̬֡
        /// </summary>
        public const byte Status = 3;

        /// <summary>
        ///     �ڲ�����
        /// </summary>
        public const byte InnerCommand = Status;

        /// <summary>
        ///     ����ID
        /// </summary>
        public const byte RequestId = 4;

        /// <summary>
        ///     ִ�мƻ�
        /// </summary>
        public const byte Plan = 5;

        /// <summary>
        ///     �ƻ�ʱ��
        /// </summary>
        public const byte PlanTime = 6;

        /// <summary>
        ///     ������֤��ʶ
        /// </summary>
        public const byte SerivceKey = 7;

        /// <summary>
        ///     ������֤��ʶ
        /// </summary>
        public const byte LocalId = 8;

        /// <summary>
        /// ���÷���վ������
        /// </summary>
        public const byte StationType = 9;

        /// <summary>
        /// ���÷���ȫ�ֱ�ʶ
        /// </summary>
        public const byte CallId = 0xB;
        /// <summary>
        /// ���ݷ���
        /// </summary>
        public const byte DataDirection = 0xC;

        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original1 = 0x10;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original2 = 0x11;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original3 = 0x12;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original4 = 0x13;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original5 = 0x14;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original6 = 0x15;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original7 = 0x16;
        /// <summary>
        /// ԭ������
        /// </summary>
        public const byte Original8 = 0x17;
        /// <summary>
        /// ��ʼʱ��
        /// </summary>
        public const byte BeginTime = 0x18;
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public const byte EndTime = 0x19;
        /// <summary>
        ///     ����
        /// </summary>
        public const byte Argument = (byte)'%';

        /// <summary>
        ///     ������
        /// </summary>
        public const byte Requester = (byte)'>';

        /// <summary>
        ///     ������/������
        /// </summary>
        public const byte Publisher = Requester;

        /// <summary>
        ///     �ظ���
        /// </summary>
        public const byte Responser = (byte)'<';

        /// <summary>
        ///     ������/�˷���
        /// </summary>
        public const byte Subscriber = Responser;

        /// <summary>
        ///     ������������Ϣ
        /// </summary>
        public const byte Context = (byte)'#';

        /// <summary>
        ///     ��������
        /// </summary>
        public const byte Command = (byte)'$';

        /// <summary>
        ///     �㲥����
        /// </summary>
        public const byte PubTitle = (byte)'*';

        /// <summary>
        ///     �㲥����
        /// </summary>
        public const byte SubTitle = Command;

        /// <summary>
        ///     �����ı�����
        /// </summary>
        public const byte ResultText = (byte)'J';

        /// <summary>
        ///     �����ı�����
        /// </summary>
        public const byte ExtendText = (byte)'X';


        /// <summary>
        ///     һ���ı�����
        /// </summary>
        public const byte TextContent = (byte)'T';

        /// <summary>
        ///     ����������
        /// </summary>
        public const byte BinaryContent = (byte)'B';

        /// <summary>
        ///     ����������
        /// </summary>
        public const byte TsonContent = (byte)'V';

        /// <summary>
        ///     ˵��֡����
        /// </summary>
        public static string FrameName(byte value)
        {
            switch (value)
            {
                //��ֹ����
                case End: return nameof(End); 
                //��ֹ����
                case ExtendEnd: return nameof(ExtendEnd); 
                //��ֹ����
                case ResultEnd: return nameof(ResultEnd); 
                //ȫ�ֱ�ʶ
                case GlobalId: return nameof(GlobalId); 
                //վ������֡
                case Station: return nameof(Station); 
                //״̬֡
                case Status: return nameof(Status) + "/" + nameof(InnerCommand); 
                //����ID
                case RequestId: return nameof(RequestId); 
                //ִ�мƻ�
                case Plan: return nameof(Plan); 
                //�ƻ�ʱ��
                case PlanTime: return nameof(PlanTime); 
                //������֤��ʶ
                case SerivceKey: return nameof(SerivceKey); 
                //������֤��ʶ
                case LocalId: return nameof(LocalId); 
                //���÷���վ������
                case StationType: return nameof(StationType); 
                //���÷���ȫ�ֱ�ʶ
                case CallId: return nameof(CallId); 
                //���ݷ���
                case DataDirection: return nameof(DataDirection); 
                //ԭ������
                case Original1: return nameof(Original1); 
                //ԭ������
                case Original2: return nameof(Original2); 
                //ԭ������
                case Original3: return nameof(Original3); 
                //ԭ������
                case Original4: return nameof(Original4); 
                //ԭ������
                case Original5: return nameof(Original5); 
                //ԭ������
                case Original6: return nameof(Original6); 
                //ԭ������
                case Original7: return nameof(Original7); 
                //ԭ������
                case Original8: return nameof(Original8); 
                //����
                case Argument: return nameof(Argument); 
                //������
                case Requester: return nameof(Requester) + "/" + nameof(Publisher); 
                //�ظ���
                case Responser: return nameof(Responser) + "/" + nameof(Subscriber);
                //������������Ϣ
                case Context: return nameof(Context);
                //��������
                case Command: return nameof(Command) + "/" + nameof(SubTitle);
                //�㲥����
                case PubTitle: return nameof(PubTitle);
                //һ���ı�����
                case TextContent: return nameof(TextContent);
                //JSON�ı�����
                case ResultText: return nameof(ResultText);
                //����������
                case BinaryContent: return nameof(BinaryContent);
                //����������
                case TsonContent: return nameof(TsonContent);
                default: return "Error";
            }
        }

    }

}