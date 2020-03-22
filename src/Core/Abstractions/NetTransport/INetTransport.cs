using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// 表示一个网络传输对象
    /// </summary>
    public interface INetTransport : IDisposable
    {
        /// <summary>
        /// 服务
        /// </summary>
        IService Service { get; set; }


        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize()
        {
        }

        /// <summary>
        /// 将要开始
        /// </summary>
        bool Prepare()
        {
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        void Close()
        {
        }

        /// <summary>
        /// 开始轮询前的工作
        /// </summary>
        /// <returns></returns>
        void LoopBegin()
        {
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        Task<bool> Loop(CancellationToken token);

        /// <summary>
        /// 结束轮询前的工作
        /// </summary>
        /// <returns></returns>
        void LoopComplete()
        {
        }

        /// <summary>
        /// 表示已成功接收 
        /// </summary>
        /// <returns></returns>
        void Commit()
        {
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <returns></returns>
        void OnResult(IMessageItem message, object tag)
        {
        }

        /// <summary>
        /// 错误 
        /// </summary>
        /// <returns></returns>
        void OnError(Exception exception, IMessageItem message, object tag)
        {
        }
    }
}