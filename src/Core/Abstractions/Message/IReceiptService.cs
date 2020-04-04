using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Messages
{
    /// <summary>
    /// 表示一个回执服务
    /// </summary>
    public interface IReceiptService
    {
        /// <summary>
        /// 保存回执
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<MessageState> Save(string message);

        /// <summary>
        /// 读取回执
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<string> Load(string id);

        /// <summary>
        /// 删除回执
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<MessageState> Remove(string id);

    }
}
