using Newtonsoft.Json;


namespace ZeroTeam.MessageMVC.Documents
{
    /// <summary>
    ///     自注释配置对象
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AnnotationsConfig
    {
        /// <summary>
        ///     名称
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        ///     标题
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        ///     说明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        ///     显示文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}({Caption})";
        }
    }
}