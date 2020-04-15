// 所在工程：Agebull.EntityModel
// 整理用户：bull2
// 建立时间：2012-08-13 5:35
// 整理时间：2012-08-30 3:12

#region

using Agebull.Common.Frame;
using System;
using System.Collections.Generic;

#endregion

namespace Agebull.Common.Logging
{

    /// <summary>
    ///   表示一个栈底为固定值的栈
    /// </summary>
    internal sealed class LogStack
    {
        private readonly Stack<TraceStep> _value = new Stack<TraceStep>();

        /// <summary>
        /// 栈深
        /// </summary>
        public int StackCount => _value.Count;

        /// <summary>
        ///   当前
        /// </summary>
        public TraceStep Current { get; private set; }

        /// <summary>
        ///  栈是否为空
        /// </summary>
        public bool IsEmpty => _value.Count == 0;

        /// <summary>
        ///   固定
        /// </summary>
        public TraceStep FixValue { get; private set; }

        /// <summary>
        ///   配置固定值(只第一次调用有效果)
        /// </summary>
        /// <param name="value"> </param>
        public void SetFix(TraceStep value)
        {
            value.Start = DateTime.Now;
            using (ThreadLockScope.Scope(this))
                _value.Push(FixValue = Current = value);
        }

        /// <summary>
        ///   入栈
        /// </summary>
        /// <param name="value"> </param>
        public void Push(TraceStep value)
        {
            value.Start = DateTime.Now;
            using (ThreadLockScope.Scope(this))
            {
                Current.Children.Add(value);
                _value.Push(Current = value);
            }
        }

        /// <summary>
        ///   出栈
        /// </summary>
        public TraceStep Pop()
        {
            using (ThreadLockScope.Scope(this))
            {
                if (_value.Count == 0)
                {
                    return FixValue;
                }
                Current.End = DateTime.Now;
                _value.Pop();
                return Current = _value.Count == 0 ? FixValue : _value.Peek();
            }
        }
    }
}
