using System.Collections.Generic;

namespace ZeroTeam.MessageMVC.Context
{
    /// <summary>
    /// 可通过字典传输的对象
    /// </summary>
    public interface IDictionaryTransfer
    {

        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        Dictionary<string, string> ToDictionary();

        /// <summary>
        /// 转为可传输的对象
        /// </summary>
        void Reset(Dictionary<string, string> dict);
    }
}