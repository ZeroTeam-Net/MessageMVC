using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace BeetleX.FastHttpApi
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class HttpReceiver : MessageReceiverBase, IServiceReceiver
    {
        /// <summary>
        /// 构造
        /// </summary>
        public HttpReceiver() : base(nameof(HttpReceiver))
        {
        }

        /// <summary>
        /// 对应发送器名称
        /// </summary>
        string IMessagePoster.PosterName => nameof(HttpReceiver);

        private TaskCompletionSource<bool> task;

        Task<bool> IMessageReceiver.Loop(CancellationToken token)
        {
            task = new TaskCompletionSource<bool>();
            return task.Task;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        Task ILifeFlow.Closing()
        {
            task.SetResult(true);
            return Task.CompletedTask;
        }


        /// <summary>
        /// 标明调用结束
        /// </summary>
        /// <returns>是否发送成功</returns>
        Task<bool> IMessageWriter.OnResult(IInlineMessage message, object tag)
        {
            if (tag is HttpRequest request)
            {
                request.Session.Send(HttpFlow.Instance.Server.CreateDataFrame(message.ResultData));
            }
            else if (tag is HttpResponse response)
            {
                response.Result(new MessageMvcApiResult
                {
                    Message = message
                });
            }
            return Task.FromResult(true);
        }
    }
}
