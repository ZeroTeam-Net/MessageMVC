using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Agebull.Common.Logging
{

    /// <summary>
    ///     跟踪信息
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TraceStep : TraceItem
    {
        /// <summary>
        /// 子级
        /// </summary>
        public List<TraceItem> Children { get; } = new List<TraceItem>();

        /// <summary>
        ///     起止时间
        /// </summary>
        public DateTime Start, End;
    }
}