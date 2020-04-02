﻿using System;
using System.Linq;
using System.Text;

namespace Agebull.Common.Logging
{
    /// <summary>
    ///     跟踪信息
    /// </summary>
    [Serializable]
    internal class MonitorItem
    {
        /// <summary>
        ///     记录堆栈
        /// </summary>
        internal LogStack<MonitorData> Stack = new LogStack<MonitorData>();

        internal readonly StringBuilder Texter = new StringBuilder();

        internal DateTime Id = DateTime.Now;

        /// <summary>
        ///     侦测开关
        /// </summary>
        internal bool InMonitor;

        /// <summary>
        ///     开始检测资源
        /// </summary>
        public void BeginMonitor(string title)
        {
            if (!InMonitor)
            {
                Begin(title);
            }
            else
            {
                BeginStep(title);
            }
        }


        private void Begin(string title)
        {
            InMonitor = true;
#if !NETCOREAPP
            if (!AppDomain.MonitoringIsEnabled)
                AppDomain.MonitoringIsEnabled = true;
#endif
            {
                var root = new MonitorData
                {
                    Title = title
                };
                //Stack.SetFix(root);
                Stack.Push(root);
                //Texter.Append("标题");
                //Texter.Append(' ', 24);
                //Texter.Append("|状态|   时间   |   用 时(ms)   |");
                //#if !NETCOREAPP
                //                Texter.Append("|  CPU(ms) |内存分配Kb| 总分配Mb |内存驻留Kb| 总驻留Mb |");
                //#endif
                Write(title, ItemType.Open);
            }
        }

        /// <summary>
        ///     刷新资源检测
        /// </summary>
        internal void BeginStep(string title)
        {
            Stack.Push(new MonitorData
            {
                Title = title
            });
            Write(title, ItemType.Begin);
        }

        /// <summary>
        ///     刷新资源检测
        /// </summary>
        public void EndStep()
        {
            if (Stack.FixValue == Stack.Current)
            {
                return;
            }
            Stack.Current.EndMessage();
            Write(Stack.Current.Title, ItemType.End);
            var pre = Stack.Current;
            Stack.Pop();
            Stack.Current.Coll(pre);
            Stack.Current.Flush();
        }
        /// <summary>
        ///     刷新资源检测
        /// </summary>
        public void Flush(string title, bool number = false)
        {
            Stack.Current.Coll();
            Stack.Current.FlushMessage();
            Write(title, ItemType.Item);
            Stack.Current.Flush();
        }

        /// <summary>
        ///     刷新资源检测
        /// </summary>
        public string End()
        {
            InMonitor = false;
            try
            {
                MonitorData pre = null;
                while (Stack.StackCount > 0)
                {
                    Stack.Current.Coll(pre);
                    Stack.Current.EndMessage();
                    Write(Stack.Current.Title, ItemType.End);
                    pre = Stack.Current;
                    Stack.Pop();
                }
                //Stack.Current.Coll(pre);
                //Stack.FixValue.EndMessage();
                //Write(Stack.FixValue.Title ?? "Monitor", ItemType.End);
                return Texter.ToString();
            }
            catch (Exception ex)
            {
                LogRecorder.Error("EndMonitor{0}", ex);
                return null;
            }
            finally
            {
                Texter.Clear();
            }
        }

        internal enum ItemType
        {
            Open,
            Begin,
            Item,
            End,
            Close
        }
        internal void Write(string title, ItemType type, bool showMonitorValue = true)
        {
            Texter.AppendLine();
            var cnt = Stack.StackCount - 1;
            switch (type)
            {
                case ItemType.Open:
                case ItemType.Begin:
                    if (cnt > 0)
                    {
                        Texter.Append('│', cnt - 1);
                        Texter.Append("├┬");
                    }
                    else
                    {
                        Texter.Append('┌');
                    }
                    break;
                case ItemType.End:
                case ItemType.Close:
                    if (cnt > 0)
                    {
                        Texter.Append('│', cnt);
                    }

                    Texter.Append('└');
                    break;
                default:
                    if (cnt > 0)
                    {
                        Texter.Append('│', cnt);
                    }

                    Texter.Append('├');
                    cnt++;
                    break;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = "*";
            }

            Texter.Append(title);
            if (!showMonitorValue)
            {
                return;
            }
            var l = cnt + title.GetLen();
            if (l < 50)
            {
                Texter.Append(' ', 50 - l);
            }
            Texter.Append(Stack.Current.Message);
        }


    }
}