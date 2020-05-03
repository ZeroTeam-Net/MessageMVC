using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ApiContract;

namespace MessageMVC.Wpf.Sample.Model
{

    ///<summary>    
    /// 卡片驱动模拟
    ///</summary>
    public class CardSimulation
    {
        #region Member
        /// <summary>
        /// 热键字典
        /// </summary>
        private static readonly Dictionary<int, HotKey> KeyPair = new Dictionary<int, HotKey>();

        /// <summary>
        /// 热键消息编号
        /// </summary>
        private const int WM_HOTKEY = 0x0312;

        #endregion

        #region Ctor

        /// <summary>
        /// 运行
        /// </summary>
        public static void Run(Window win)
        {
            var handle = new WindowInteropHelper(win).Handle;
            if (win == null || handle == IntPtr.Zero)
            {
                throw new Exception("窗口对象无效!");
            }
            RegistHotKey(handle, KeyFlags.Control, ConsoleKey.A);
            RegistHotKey(handle, KeyFlags.Control, ConsoleKey.B);
            //消息挂钩只能连接一次!!
            if (!InstallHotKeyHook(handle))
            {
                throw new Exception("消息挂钩连接失败!");
            }
        }

        ///<summary>
        /// 构造函数
        ///</summary>
        ///<param name="handle">注册窗体</param>
        ///<param name="control">控制键</param>
        ///<param name="consoleKey">键</param>
        private static void RegistHotKey(IntPtr handle, KeyFlags control, ConsoleKey consoleKey)
        {
            int key = (int)consoleKey;
            int id = (key * 10) + (int)control;
            if (KeyPair.ContainsKey(id))
            {
                return;
            }
            //注册热键
            if (!RegisterHotKey(handle, id, (uint)control, (uint)key))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 1409)
                {
                    throw new Exception($"热键({control}+{consoleKey})被占用!");
                }
                else if (errorCode != 0)
                {
                    throw new Exception($"热键({control}+{consoleKey})注册失败!{errorCode}");
                }
            }
            //添加这个热键索引
            KeyPair.Add(id, new HotKey
            {
                Key = consoleKey,
                ControlKey = control,
                Handle = handle,
                KeyId = id
            });
        }


        /// <summary>
        /// 析构函数,解除热键
        /// </summary>
        public static void Close()
        {
            foreach (var item in KeyPair.Values)
            {
                UnregisterHotKey(item.Handle, item.KeyId);
            }
        }
        #endregion

        #region core

        [DllImport("user32")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

        [DllImport("user32")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// 安装热键处理挂钩
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static bool InstallHotKeyHook(IntPtr handle)
        {
            //获得消息源
            HwndSource source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                return false;
            }
            //挂接事件            
            source.AddHook(HotKeyHook);
            return true;
        }


        #endregion

        #region HotKeyHook

        /// <summary>
        /// 热键处理过程
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private static IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_HOTKEY || !KeyPair.TryGetValue((int)wParam, out var hk))
            {
                return IntPtr.Zero;
            }
            var Logger = DependencyHelper.LoggerFactory.CreateLogger(nameof(CardSimulation));
            switch (hk.Key)
            {
                case ConsoleKey.A:
                    Task.Factory.StartNew(async () =>
                    {
                        Logger.Information("接收到模拟卡片插入,正在发送驱动事件");
                        var state = await MessagePoster.PublishAsync("Devices", "v1/card/push", new Argument
                        {
                            Value = "88888888"
                        });
                        Logger.Information($"正在发送驱动事件完成,状态为{state}");
                    });
                    break;
                case ConsoleKey.B:
                    Task.Factory.StartNew(async () =>
                    {
                        Logger.Information("接收到模拟卡片弹出,正在发送驱动事件");
                        var state = await MessagePoster.PublishAsync("Devices", "v1/card/pull", new Argument
                        {
                            Value = "88888888"
                        });
                        Logger.Information($"正在发送驱动事件完成,状态为{state}");
                    });
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}