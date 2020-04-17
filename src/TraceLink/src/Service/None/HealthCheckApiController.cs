/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 14:29:42*/
#region
using Agebull.MicroZero.ZeroApis;
using ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic;
using ZeroTeam.MessageMVC.ZeroApis;

#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.WebApi.Entity
{
    /// <summary>
    ///  健康检查
    /// </summary>
    [Service("trace_link")]
    [Route("healthCheck/v1")]
    [ApiPage("/Trace/None/HealthCheck/index.htm")]
    public partial class HealthCheckApiController
         : ApiController<HealthCheckData, HealthCheckBusinessLogic>
    {
        #region 基本扩展

        /*// <summary>
        ///     取得列表数据
        /// </summary>
        protected override ApiPageData<HealthCheckData> GetListData()
        {
            var filter = new LambdaItem<HealthCheckData>();
            ReadQueryFilter(filter);
            return base.GetListData(filter);
        }*/

        /// <summary>
        /// 读取Form传过来的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="convert">转化器</param>
        protected override void ReadFormData(HealthCheckData data, FormConvert convert)
        {
            DefaultReadFormData(data, convert);
        }

        #endregion
    }
}