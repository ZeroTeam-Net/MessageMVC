using System;
using System.Threading;

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
        /// 同步运行状态
        /// </summary>
        /// <returns></returns>
        void LoopBegin()
        {
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        bool Loop(CancellationToken token);

        /// <summary>
        /// 同步关闭状态
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
        void OnResult(bool success, string message)
        {
        }

        /// <summary>
        /// 错误 
        /// </summary>
        /// <returns></returns>
        void OnError(Exception exception, string message)
        {
        }
    }
}