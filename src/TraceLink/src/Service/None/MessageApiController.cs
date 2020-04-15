/*design by:agebull designer date:2020/4/15 16:40:56*/
#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

using Agebull.Common;
using Agebull.Common.Ioc;

using Agebull.EntityModel.Common;
using Agebull.EntityModel.EasyUI;
using ZeroTeam.MessageMVC.ZeroApis;
using Agebull.MicroZero.ZeroApis;



using ZeroTeam.MessageMVC.MessageTraceLink;
using ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic;
using System.Threading.Tasks;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.WebApi.Entity
{
    /// <summary>
    ///  消息存储
    /// </summary>
    [Service("trace_link")]
    [Route("message/v1")]
    [ApiPage("/trace_link/None/Message/index.htm")]
    public partial class MessageApiController
         : ApiController<MessageData, MessageBusinessLogic>
    {
        /// <summary>
        /// 生成流程图文本
        /// </summary>
        /// <returns>流程图文本</returns>
        [Route("flow")]
        public async Task<IApiResult<string>> ToFlow(string traceId)
        {
            var flow = await Business.ToFlow(traceId);

            return ApiResultHelper.Succees(flow);
        }

        #region 基本扩展

        /*// <summary>
        ///     取得列表数据
        /// </summary>
        protected override ApiPageData<MessageData> GetListData()
        {
            var filter = new LambdaItem<MessageData>();
            ReadQueryFilter(filter);
            return base.GetListData(filter);
        }*/

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected override void ReadFormData(MessageData data, FormConvert convert)
        {
            DefaultReadFormData(data, convert);
        }

        #endregion
    }
}