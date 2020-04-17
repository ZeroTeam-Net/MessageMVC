/*design by:agebull designer date:2020/4/15 14:32:57*/
#region
using Agebull.EntityModel.BusinessLogic;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink.BusinessLogic
{
    /// <summary>
    /// 消息存储
    /// </summary>
    public partial class MessageBusinessLogic : UiBusinessLogicBase<MessageData, MessageDataAccess>
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
        protected override bool OnSaving(MessageData data, bool isAdd)
        {
             return base.OnSaving(data, isAdd);
        }

        /// <summary>
        ///     保存完成后的操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool OnSaved(MessageData data, bool isAdd)
        {
             return base.OnSaved(data, isAdd);
        }
        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool LastSavedByUser(MessageData data, bool isAdd)
        {
            return base.LastSavedByUser(data, isAdd);
        }

        /// <summary>
        ///     被用户编辑的数据的保存前操作
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="isAdd">是否为新增</param>
        /// <returns>如果为否将阻止后续操作</returns>
        protected override bool PrepareSaveByUser(MessageData data, bool isAdd)
        {
            return base.PrepareSaveByUser(data, isAdd);
        }*/
        #endregion

        #region 流程图还原

        public async Task<string> ToFlow(string id)
        {
            var messages = await Access.AllAsync(p => p.TraceId == id);
            if (messages.Count == 0)
            {
                return null;
            }

            var root = messages.FirstOrDefault(p => p.Level == 0);
            if (root == null)
            {
                return null;
            }

            var services = messages.Select(p => p.LocalApp).Concat(messages.Select(p => p.CallApp)).Distinct();
            StringBuilder mark = new StringBuilder();
            mark.AppendLine("```sequence");
            foreach (var service in services)
            {
                mark.AppendLine($"participant {service}");
            }
            mark.AppendLine();
            ToFlow(mark, root, messages);
            mark.AppendLine("```");
            return mark.ToString();
        }

        public void ToFlow(StringBuilder mark, MessageData data, List<MessageData> messages)
        {
            var message = JsonHelper.DeserializeObject<InlineMessage>(data.Message);
            mark.AppendLine($"{data.CallApp} -> {data.LocalApp} : {message.ServiceName}/{message.ApiName}");

            var childs = messages.Where(p => p.CallId == data.LocalId && p.Level == data.Level + 1);
            if (childs.Any())
            {
                var array = childs.OrderBy(p => p.Level);
                foreach (var item in array)
                {
                    ToFlow(mark, item, messages);
                }
            }
            mark.AppendLine($"{data.LocalApp} -->> {data.CallApp} : {message.State}");
        }

        #endregion
    }
}