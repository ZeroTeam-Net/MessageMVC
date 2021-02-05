using Agebull.Common;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
        /// 取得生命周期对象
        /// </summary>
        /// <returns></returns>
        ILifeFlow IMessagePoster.GetLife() => LifeFlow;

        /// <summary>
        /// 征集周期管理器
        /// </summary>
        protected abstract ILifeFlow LifeFlow { get; }//AsyncPost ? this as ILifeFlow : null;

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

        #endregion

        #region 流程支持

        TaskCompletionSource<bool> AsyncPostQueueTask;

        /// <summary>
        /// 开启
        /// </summary>
        protected void DoStart()
        {
            if (AsyncPost)
                AsyncPostQueue();
            CheckTimeOut();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        protected async Task Destroy()
        {
            cancellation?.Cancel();
            if (AsyncPostQueueTask != null)
                await AsyncPostQueueTask.Task;
        }

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
                return await PostMessage(new TQueueItem
                {
                    ID = message.ID,
                    Topic = message.Service,
                    Time = DateTime.Now,
                    Message = message
                });
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
            AsyncPostQueueTask = new TaskCompletionSource<bool>();
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
                            if (!isFailed)
                            {
                                var result = await PostMessage(item);
                                if (!result.State.IsSuccess())
                                    isFailed = true;
                            }
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
                            break;
                        }
                    }
                }
                RecordLog(LogLevel.Information, "异步消息投递已关闭，正在备份未发送内容");
                State = StationStateType.Closed;
                await Backup(queues.ToArray());
                RecordLog(LogLevel.Information, "备份未发送内容完成");
                AsyncPostQueueTask.SetResult(true);
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
                    RecordLog(LogLevel.Trace, () => $"[异步消息投递] {item.ID} 发送成功,删除备份文件,{item.FileName}");
                    File.Delete(item.FileName);
                }
                catch
                {
                    RecordLog(LogLevel.Error, () => $"[异步消息投递] {item.ID} 发送成功,删除备份文件失败,{item.FileName}");
                }
            }
            else
            {
                RecordLog(LogLevel.Trace, () => $"[异步消息投递] {item.ID} 发送成功");
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
                Directory.Delete(BackupFolder, true);
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

        #region 发送可靠性

        /// <summary>
        /// 发送中的队列
        /// </summary>
        public class PostTask
        {
            /// <summary>
            /// 开始时间
            /// </summary>
            internal int Start { get; set; }

            /// <summary>
            /// 原始内容
            /// </summary>
            public MessageResult Result { get; set; }

            /// <summary>
            /// 原始内容
            /// </summary>
            public IMessageItem Message { get; set; }

            /// <summary>
            /// 当前任务
            /// </summary>
            public TaskCompletionSource<IMessageResult> WaitingTask { get; set; }
        }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract TaskCompletionSource<IMessageResult> DoPost(TQueueItem item);

        /// <summary>
        /// 当前任务
        /// </summary>
        protected PostTask CurrentTask { get; set; }

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        async Task<IMessageResult> PostMessage(TQueueItem item)
        {
            var task = new PostTask
            {
                Start = DateTime.Now.ToTimestamp(),
                Message = item.Message,
                Result = new MessageResult
                {
                    ID = item.ID,
                    Trace = item.Message.TraceInfo,
                    State = MessageState.None
                }
            };
            PostTasks[item.Message.ID] = task;
            task.WaitingTask = DoPost(item);
            CurrentTask = task;
            try
            {
                var resut = await task.WaitingTask.Task;
                PostTasks.TryRemove(item.ID, out _);
                return resut;
            }
            catch
            {
                task.Result.State = MessageState.NetworkError;
                return task.Result;
            }
            finally
            {
                CurrentTask = null;
            }
        }

        /// <summary>
        /// 当前等待队列
        /// </summary>
        protected static readonly ConcurrentDictionary<string, PostTask> PostTasks = new ConcurrentDictionary<string, PostTask>();

        static int CheckTimeOutRun;
        /// <summary>
        /// 检查超时
        /// </summary>
        async void CheckTimeOut()
        {
            if (Interlocked.Increment(ref CheckTimeOutRun) > 1)
                return;
            await Task.Yield();
            while (!ZeroAppOption.Instance.IsDestroy)
            {
                if (PostTasks.Count == 0)
                {
                    await Task.Delay(1000);
                    continue;
                }
                var items = PostTasks.Values.ToArray();
                var now = DateTime.Now.ToTimestamp();
                foreach (var item in items)
                {
                    if (now - item.Start > 3)
                    {
                        try
                        {
                            item.Result.State = MessageState.NetworkError;
                            var task = item.WaitingTask;
                            item.WaitingTask = null;
                            task.TrySetResult(item.Result);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    if (now - item.Start > 180)
                    {
                        try
                        {
                            PostTasks.TryRemove(item.Result.ID, out _);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                await Task.Delay(1000);
            }
        }

        #endregion
    }
}
