using Newtonsoft.Json;

namespace ZeroTeam.MessageMVC
{

    /// <summary>
    /// �ƻ�����
    /// </summary>
    public enum plan_date_type
    {
        /// <summary>
        /// �޼ƻ�����������
        /// </summary>
        none,
        /// <summary>
        /// ��ָ����ʱ�䷢��
        /// </summary>
        time,
        /// <summary>
        /// ��������
        /// </summary>
        second,
        /// <summary>
        /// ���Ӽ������
        /// </summary>
        minute,
        /// <summary>
        /// Сʱ�������
        /// </summary>
        hour,
        /// <summary>
        /// �ռ������
        /// </summary>
        day,
        /// <summary>
        /// ÿ�ܼ�
        /// </summary>
        week,
        /// <summary>
        /// ÿ�¼���
        /// </summary>
        month
    }

    /// <summary>
    /// �ƻ�״̬
    /// </summary>
    public enum plan_message_state
    {
        /// <summary>
        /// ��״̬
        /// </summary>
        none,
        /// <summary>
        /// �Ŷ�
        /// </summary>
        queue,
        /// <summary>
        /// ����ִ��
        /// </summary>
        execute,
        /// <summary>
        /// ����ִ��
        /// </summary>
        retry,
        /// <summary>
        /// ����
        /// </summary>
        skip,
        /// <summary>
        /// ��ͣ
        /// </summary>
        pause,
        /// <summary>
        /// ����ر�
        /// </summary>
        error,
        /// <summary>
        /// �����ر�
        /// </summary>
        close,
        /// <summary>
        /// ɾ��
        /// </summary>
        remove
    }
    /// <summary>
    /// �ƻ�
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ZeroPlanInfo
    {
        /// <summary>
        /// �������ô���(1-n ��������)
        /// </summary>
        public int skip_set;

        /// <summary>
        /// �ƻ�����
        /// </summary>
        public plan_date_type plan_type;

        /// <summary>
        /// ����ֵ
        /// </summary>
        /// <remarks>
        /// none time ��Ч
        /// second minute hour day : ��ʱ����� ָ����ʱ����(��λΪ��Ӧ��plan_date_type)
        /// week : ���յ�����(0-6),ֵ��Чϵͳ�Զ�����(����ʾ)
        /// month: ����Ϊָ������(�統�²�����,��ʹ�õ������һ��) �����Ϊ��δ����(0Ϊ���һ��,����Ϊ��ȥ������,���Ľ��Ϊ0������,��Ϊ��ǰ��һ��)
        /// </remarks>
        public short plan_value;

        /// <summary>
        /// �ظ�����,0���ظ� >0�ظ�����,-1�����ظ�
        /// </summary>
        /// <remarks>
        /// none time ��Ч
        /// second minute hour day : ��ʱ�����,��ָ��ʱ��̫С,
        /// no_keep=true:
        ///     ��μ���ʱ�����С�ڵ�ǰ��,���ܼ�ִ��
        /// no_keep=false:
        ///     ÿ�ο���(ϵͳ��ִ�в���)Ҳ��һ��,��ʱ���㹻С,����ȫ�ǿ�����δ��ִ��һ��,����ָ�����������()
        /// </remarks>
        public int plan_repet;

        /// <summary>
        /// �ƻ�˵��
        /// </summary>
        public string description;

        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool no_skip;

        /// <summary>
        /// �ƻ�ʱ��
        /// </summary>
        /// <remarks>
        /// ʹ��UNIXʱ��(1970��1��1��0ʱ0��0������ܺ�����)
        /// plan_type :
        ///  none ��Ч,ϵͳ�Զ����䵱ǰʱ���Ա���һ������ִ��,plan_repet��Ч
        ///  time ָ��ʱ��,��ʱ�����,ϵͳ�Զ�����,plan_repet��Ч
        ///  second minute hour day : ��ʱ�����,��ָ��,���Դ�ʱ��Ϊ��׼,������ϵͳ����ʱ��Ϊ��׼(���ܲ������)
        ///  week month:ָ�����ڵ�,ֻʹ�ô�ʱ������ڲ���(����ʱ�������ȷָ��������ʱ��),�粻ָ��,��ʱ��Ϊ0:0:0
        /// </remarks>
        public long plan_time;


    /// <summary>
    /// �ƻ�
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ZeroPlan : ZeroPlanInfo
    {
        /// <summary>
        /// ����
        /// </summary>
        public string name;

        /// <summary>
        /// ��Ϣ��ʶ
        /// </summary>
        public long plan_id;

        /// <summary>
        /// �������ṩ�ı�ʶ
        /// </summary>
        public string request_id;

        /// <summary>
        /// վ��
        /// </summary>
        public string station;

        /// <summary>
        /// վ��
        /// </summary>
        public int station_type;

        /// <summary>
        /// ԭʼ������
        /// </summary>
        public string caller;


        /// <summary>
        /// ִ�д���
        /// </summary>
        public int real_repet;



        /// <summary>
        /// ������������,
        /// 1 ��no_skip=trueʱ,����Ҳ��������.
        /// 2 �˼�����ִ��ʱ����,
        ///     2.1 skip_set &lt; 0 ֱ�Ӽ�����һ��ִ��ʱ��,
        ///     2.2 ��skip_set &gt; 0ʱ,skip_set &lt; skip_numʱֱ�Ӽ�����һ��ִ��ʱ��,��������ִ��
        /// </summary>
        public int skip_num;

        /// <summary>
        /// ���һ��ִ��״̬
        /// </summary>
        public int exec_state;

        /// <summary>
        /// �ƻ�״̬
        /// </summary>
        public plan_message_state plan_state;

        /// <summary>
        /// ִ��ʱ��
        /// </summary>
        public long exec_time;

        /// <summary>
        /// ���õ�API
        /// </summary>
        public string command;

        /// <summary>
        /// ������
        /// </summary>
        [JsonIgnore]
        public string context;
        /// <summary>
        /// ���õĲ���
        /// </summary>
        [JsonIgnore]
        public string argument;
    }
}