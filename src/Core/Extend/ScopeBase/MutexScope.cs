using Agebull.Common.Base;

namespace System.Threading
{
    /// <summary>
    /// 互斥锁范围
    /// </summary>
    public class ManualResetEventSlimScope : ScopeBase
    {
        /// <summary>
        /// 锁定对象
        /// </summary>
        private readonly ManualResetEventSlim mutex;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="m">要锁定的一或多个对象</param>
        public static ManualResetEventSlimScope Scope(ManualResetEventSlim m)
        {
            return new ManualResetEventSlimScope(m);
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="m">要锁定的一或多个对象</param>
        public ManualResetEventSlimScope(ManualResetEventSlim m)
        {
            mutex = m;
            mutex.Wait();//阻止当前线程，直到设置了当前
            //mutex.Reset();//将事件状态设置为非终止，从而导致线程受阻。
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnDispose()
        {
            mutex.Set();//将事件状态设置为有信号，从而允许一个或多个等待该事件的线程继续。
        }
    }
}