using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MicroZero.Http.Gateway
{

    /// <summary>
    ///     ��������
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheOption
    {
        /// <summary>
        ///     ����API����
        /// </summary>
        [JsonProperty("api")]
        public List<ApiCacheOption> Api { get; set; }


        /// <summary>
        ///     ˢ�´���API����
        /// </summary>
        [JsonProperty("trigger")]
        public List<CacheFlushOption> Trigger { get; set; }

        /// <summary>
        ///     ��ʼ��
        /// </summary>
        public void Initialize()
        {
            Api?.ForEach(p => p.Initialize());
        }
    }


    /// <summary>
    ///     ��������
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiCacheOption
    {
        /// <summary>
        ///     API����
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string Api { get; set; }


        /// <summary>
        ///     ����У������ͷ
        /// </summary>
        [DataMember]
        [JsonProperty]
        public List<string> Keys { get; set; }

        /// <summary>
        ///     ����У������ͷ
        /// </summary>
        [DataMember]
        [JsonProperty]
        public bool Bear { get; set; }

        /// <summary>
        ///     ������µ�����
        /// </summary>
        [DataMember]
        [JsonProperty]
        public int FlushSecond { get; set; }

        /// <summary>
        ///     ����ʱ��ʹ�����ƣ����������ѯ�ַ�����
        /// </summary>
        [DataMember]
        [JsonProperty]
        public bool OnlyName { get; set; }

        /// <summary>
        ///     �����������ʱ����
        /// </summary>
        [DataMember]
        [JsonProperty]
        public bool ByNetError { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore] public CacheFeature Feature { get; private set; }

        /// <summary>
        ///     ��ʼ��
        /// </summary>
        public void Initialize()
        {
            //Ĭ��5����
            if (FlushSecond <= 0)
            {
                FlushSecond = 300;
            }
            else if (FlushSecond > 3600)
            {
                FlushSecond = 3600;
            }

            if (Keys != null && Keys.Count > 0)
            {
                Feature = CacheFeature.Keys;
                return;
            }
            if (Bear)
            {
                Feature |= CacheFeature.Bear;
            }

            if (!OnlyName)
            {
                Feature |= CacheFeature.QueryString;
            }

            if (ByNetError)
            {
                Feature |= CacheFeature.NetError;
            }
        }
    }


    /// <summary>
    ///     ���津��ˢ������
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheFlushOption
    {
        /// <summary>
        ///     �������µ�API����
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string TriggerApi { get; set; }

        /// <summary>
        ///     ��Ҫ�����API����
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string CacheApi { get; set; }

        /// <summary>
        ///  �ֶ�ӳ��
        /// </summary>
        [DataMember]
        [JsonProperty]
        public Dictionary<string, string> Map { get; set; }

    }
}