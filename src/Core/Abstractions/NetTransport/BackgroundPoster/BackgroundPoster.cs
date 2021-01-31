using Agebull.Common;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
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
        /// 是否异步投递
        /// </summary>
        public bool AsyncPost { get; protected set; }

        /// <summary>
        /// 取消凭据
        /// </summary>
        protected CancellationTokenSource cancellation;

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="log"></param>
        protected virtual void RecordLog(LogLevel level, string log)
        {
            if (Logger.IsEnabled(level))
                Logger.Log(level, log);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="log"></param>
        protected virtual void RecordLog(LogLevel level, Func<string> log)
        {
            if (Logger.IsEnabled(level))
                Logger.Log(level, log());
        }

        /// <summary>
        /// 开启
        /// </summary>
        Task ILifeFlow.Open()
        {
            if (AsyncPost)
                AsyncPostQueue();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        Task ILifeFlow.Destory()
        {
            cancellation?.Cancel();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract Task<bool> DoPost(TQueueItem item);

        #endregion

        #region 消息生产

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        async Task<IMessageResult> IMessagePoster.Post(IInlineMessage message)
        {
            if (!ZeroAppOption.Instance.IsAlive)
            {
                message.State = MessageState.Cancel;
                return null;
            }
            message.Offline();
            message.RealState = MessageState.AsyncQueue;
            if (AsyncPost)
            {
                var item = new TQueueItem
                {
                    ID = message.ID,
                    Topic = message.Service,
                    Time = DateTime.Now,
                    Message = message
                };
                queues.Enqueue(item);
                semaphore.Release();
                FlowTracer.MonitorDetails(() => $"[异步消息投递]  {item.ID} 消息已投入发送队列,将在后台静默发送直到成功");

                return new MessageResult
                {
                    ID = message.ID,
                    State = MessageState.AsyncQueue
                };
            }
            try
            {
                ZeroAppOption.Instance.BeginRequest();
                var success = await DoPost(new TQueueItem
                {
                    ID = message.ID,
                    Topic = message.Service,
                    Time = DateTime.Now,
                    Message = message
                });
                return new MessageResult
                {
                    ID = message.ID,
                    State = success ? MessageState.Accept : MessageState.NetworkError
                };
            }
            finally
            {
                ZeroAppOption.Instance.EndRequest();
            }
        }

        #endregion

        #region 消息可靠性

        private static readonly ConcurrentQueue<TQueueItem> queues = new ConcurrentQueue<TQueueItem>();

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(0);


        private async void AsyncPostQueue()
        {
            try
            {
                await Task.Yield();

                State = StationStateType.Run;

                cancellation = new CancellationTokenSource();
                //还原发送异常文件
                BackupFolder = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, Name);
                await ReQueueErrorMessage();

                RecordLog(LogLevel.Information, "异步消息投递已启动");
                bool isFailed = false;
                while (!cancellation.IsCancellationRequested)
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
                            await semaphore.WaitAsync(1000, cancellation.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            RecordLog(LogLevel.Information, "收到系统退出消息,正在退出...");
                            break;
                        }
                        catch (Exception ex)
                        {
                            RecordLog(LogLevel.Error, () => $"错误信号.{ex.Message}");
                            isFailed = true;
                            continue;
                        }
                    }

                    while (queues.TryDequeue(out TQueueItem item))
                    {
                        isFailed = ZeroAppOption.Instance.IsDestroy;
                        try
                        {
                            if (!isFailed && !await DoPost(item))
                                isFailed = true;
                        }
                        catch (Exception ex)
                        {
                            RecordLog(LogLevel.Error, () => $"[异步消息投递] {item.ID} 发送异常.{ex.Message}");
                            isFailed = true;
                        }
                        if (!isFailed)
                        {
                            RemoveBackup(item);
                        }
                        else
                        {
                            await Backup(item);
                        }
                    }
                }
                RecordLog(LogLevel.Information, "异步消息投递已关闭，正在备份未发送内容");
                State = StationStateType.Closed;
                await Backup(queues.ToArray());
                RecordLog(LogLevel.Information, "备份未发送内容完成");
            }
            catch (Exception ex)
            {
                RecordLog(LogLevel.Error, () => $"AsyncPostQueue\r\n{ex}");
            }
        }

        /// <summary>
        /// 备份目录
        /// </summary>
        private string BackupFolder;

        /// <summary>
        /// 还原发送异常文件
        /// </summary>
        /// <returns></returns>
        private async Task ReQueueErrorMessage()
        {
            var file = Path.Combine(BackupFolder, $"{Name}_Backup.msg");
            if (!File.Exists(file))
                return;
            try
            {
                var json = await File.ReadAllTextAsync(file);
                if (string.IsNullOrEmpty(json))
                    return;
                var items = SmartSerializer.ToObject<TQueueItem[]>(json);
                if (items == null || items.Length == 0)
                    return;

                RecordLog(LogLevel.Information, () => $"载入发送错误消息,总数{items.Length}");
                foreach (var item in items)
                    queues.Enqueue(item);
            }
            catch (Exception ex)
            {
                RecordLog(LogLevel.Error, () => $"{file} : 消息载入错误.{ex.Message}");
            }
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
                    RecordLog(LogLevel.Error, () => $"[异步消息投递] {item.ID} 发送成功,删除备份文件失败,{item.FileName}");
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
        protected async Task Backup(TQueueItem[] item)
        {
            try
            {
                Directory.Delete(BackupFolder);
                BackupFolder = IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, Name);
            }
            catch (Exception ex)
            {
                RecordLog(LogLevel.Error, () => $"[异步消息投递] 删除碎片文件失败.错误:{ex.Message}.文件名:{BackupFolder}");
            }
            var file = Path.Combine(BackupFolder, $"{Name}_Backup.msg");
            try
            {
                await File.WriteAllTextAsync(file, SmartSerializer.ToString(item));
            }
            catch (Exception ex)
            {
                RecordLog(LogLevel.Error, () => $"[异步消息投递] 记录异常备份文件失败.错误:{ex.Message}.文件名:{file}");
            }
        }
        /// <summary>
        /// 备份消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected async Task Backup(TQueueItem item)
        {
            //写入异常文件
            ++item.Try;
            if (item.FileName == null)
            {
                item.FileName = Path.Combine(BackupFolder, $"{item.ID}.msg");
                RecordLog(LogLevel.Error, () => $"[异步消息投递] {item.ID} 发送失败,记录异常备份文件,{item.FileName}");
                try
                {
                    await File.WriteAllTextAsync(item.FileName, SmartSerializer.ToString(item));
                }
                catch (Exception ex)
                {
                    item.FileName = null;
                    RecordLog(LogLevel.Error, () => $"[异步消息投递] {item.ID} 发送失败,记录异常备份文件失败.错误:{ex.Message}.文件名:{item.FileName}");
                }
            }
            if (!ZeroAppOption.Instance.IsClosed)
                queues.Enqueue(item);
        }

        #endregion
    }
}
