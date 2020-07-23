using Agebull.Common;
using Agebull.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.MessageQueue
{
    /// <summary>
    ///    后台消息发布
    /// </summary>
    public abstract class BackgroundPoster<TQueueItem> : MessagePostBase, IMessagePoster
        where TQueueItem : QueueItem, new()
    {
        #region IMessagePoster 

        /// <summary>
        /// 实例名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 取消凭据
        /// </summary>
        protected CancellationTokenSource cancellation;

        /// <summary>
        /// 开启
        /// </summary>
        Task IMessagePoster.Open()
        {
            return AsyncPostQueue();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task IMessagePoster.Close()
        {
            cancellation.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract Task<bool> DoPost(TQueueItem item);

        /// <summary>
        /// 关闭处理
        /// </summary>
        protected virtual Task OnOpen()
        {
            State = StationStateType.Run;
            cancellation = new CancellationTokenSource();
            //还原发送异常文件
            BackupFolder = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "rebbit");
            ReQueueErrorMessage();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭处理
        /// </summary>
        protected virtual Task OnClose()
        {
            cancellation.Dispose();
            cancellation = null;
            State = StationStateType.Closed;

            return Task.CompletedTask;
        }

        #endregion

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            message.Offline();
            message.RealState = MessageState.AsyncQueue;
            return Post(new TQueueItem
            {
                ID = message.ID,
                Topic = message.Topic,
                Message = SmartSerializer.SerializeMessage(message)
            });//直接使用状态
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="item">消息</param>
        /// <returns></returns>
        protected Task<IMessageResult> Post(TQueueItem item)
        {
            queues.Enqueue(item);
            semaphore.Release();
            Logger.Trace(() => $"[异步消息投递]  {item.ID} 消息已投入发送队列,将在后台静默发送直到成功");
            return Task.FromResult<IMessageResult>(null);//直接使用状态
        }
        #endregion

        #region 消息可靠性

        private static readonly ConcurrentQueue<TQueueItem> queues = new ConcurrentQueue<TQueueItem>();

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);


        private async Task AsyncPostQueue()
        {
            await Task.Yield();
            await OnOpen();

            Logger.Information("异步消息投递已启动");
            bool isFailed = false;
            while (ZeroAppOption.Instance.IsAlive)
            {
                if (isFailed)
                {
                    await Task.Delay(1000);
                    isFailed = false;
                }
                if (queues.Count == 0)
                {
                    try
                    {
                        await semaphore.WaitAsync(60000, cancellation.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Information("收到系统退出消息,正在退出...");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(() => $"错误信号.{ex.Message}");
                        isFailed = true;
                        continue;
                    }
                }

                while (queues.TryDequeue(out TQueueItem item))
                {
                    isFailed = false;
                    try
                    {
                        if (!await DoPost(item))
                            isFailed = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(() => $"[异步消息投递] {item.ID} 发送异常.{ex.Message}");
                        isFailed = true;
                    }
                    if (!isFailed)
                    {
                        RemoveBackup(item);
                    }
                    else
                    {
                        await Backup(item);
                        break;
                    }
                }
            }
            Logger.Information("异步消息投递已关闭");
            await OnClose();
        }

        /// <summary>
        /// 备份目录
        /// </summary>
        private string BackupFolder;

        /// <summary>
        /// 还原发送异常文件
        /// </summary>
        /// <returns></returns>
        private void ReQueueErrorMessage()
        {
            var files = IOHelper.GetAllFiles(BackupFolder, "*.msg");
            if (files.Count <= 0)
            {
                return;
            }
            Logger.Information(() => $"载入发送错误消息,总数{files.Count}");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrEmpty(json))
                        continue;
                    var item = SmartSerializer.ToObject<TQueueItem>(json);
                    item.FileName = file;
                    queues.Enqueue(item);
                }
                catch (Exception ex)
                {
                    Logger.Warning(() => $"{file} : 消息载入错误.{ex.Message}");
                }
            }
            return;
        }

        /// <summary>
        /// 删除备份
        /// </summary>
        /// <param name="item"></param>
        protected void RemoveBackup(TQueueItem item)
        {
            if (item.FileName != null)
            {
                try
                {
                    Logger.Trace(() => $"[异步消息投递] {item.ID} 发送成功,删除备份文件,{item.FileName}");
                    File.Delete(item.FileName);
                }
                catch
                {
                    Logger.Warning(() => $"[异步消息投递] {item.ID} 发送成功,删除备份文件失败,{item.FileName}");
                }
            }
            else
            {
                Logger.Trace(() => $"[异步消息投递] {item.ID} 发送成功");
            }
        }

        /// <summary>
        /// 备份消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected async Task Backup(TQueueItem item)
        {
            queues.Enqueue(item);
            //写入异常文件
            ++item.Try;
            if (item.FileName != null)
            {
                return;
            }

            item.FileName = Path.Combine(BackupFolder, $"{item.ID}.msg");
            Logger.Warning(() => $"[异步消息投递] {item.ID} 发送失败,记录异常备份文件,{item.FileName}");
            try
            {
                await File.WriteAllTextAsync(item.FileName, SmartSerializer.ToString(item));
            }
            catch (Exception ex)
            {
                item.FileName = null;
                Logger.Error(() => $"[异步消息投递] {item.ID} 发送失败,记录异常备份文件失败.错误:{ex.Message}.文件名:{item.FileName}");
            }
        }

        #endregion
    }
}
