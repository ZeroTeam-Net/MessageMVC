using System;

namespace MessageMVC.Wpf.Sample.Model
{
    public class HotKey
    {
        /// <summary>
        /// 热键编号
        /// </summary>
        public int KeyId { get; internal set; }
        /// <summary>
        /// 窗体句柄
        /// </summary>
        public IntPtr Handle { get; internal set; }
        /// <summary>
        /// 热键控制键
        /// </summary>
        public KeyFlags ControlKey { get; internal set; }
        /// <summary>
        /// 热键主键
        /// </summary>
        public ConsoleKey Key { get; internal set; }

    }
}