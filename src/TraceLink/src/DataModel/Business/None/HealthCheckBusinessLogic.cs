/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 14:02:36*/
#region
using Agebull.EntityModel.BusinessLogic;



using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic
{
    /// <summary>
    /// 健康检查结果
    /// </summary>
    public partial class HealthCheckBusinessLogic : UiBusinessLogicBase<HealthCheckData, HealthCheckDataAccess>
    {

        #region 设计器命令

        #endregion


        #region 树形数据

        #endregion
        #region CURD扩展
        /*// <summary>
        ///     保存前的操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool OnSaving(HealthCheckData data, bool isAdd)
        {
             return base.OnSaving(data, isAdd);
        }

        /// <summary>
        ///     保存完成后的操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool OnSaved(HealthCheckData data, bool isAdd)
        {
             return base.OnSaved(data, isAdd);
        }
        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool LastSavedByUser(HealthCheckData data, bool isAdd)
        {
            return base.LastSavedByUser(data, isAdd);
        }

        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool PrepareSaveByUser(HealthCheckData data, bool isAdd)
        {
            return base.PrepareSaveByUser(data, isAdd);
        }*/
        #endregion
    }
}