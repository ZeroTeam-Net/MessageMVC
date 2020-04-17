/*design by:agebull designer date:2020/4/15 16:40:56*/
#region
using Agebull.MicroZero.ZeroApis;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic;
using ZeroTeam.MessageMVC.ZeroApis;

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