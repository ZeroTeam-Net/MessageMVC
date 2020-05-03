﻿// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-12-09
// 修改:2014-12-09
// *****************************************************/

#region 引用

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     有属性通知的对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        #region 属性修改通知

        /// <summary>
        ///     属性修改事件
        /// </summary>
        private event PropertyChangedEventHandler PropertyChangedEvent;
        /// <summary>
        ///     属性修改事件(属性为空表示删除)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEvent -= value;
                PropertyChangedEvent += value;
            }
            remove => PropertyChangedEvent -= value;
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="action">属性字段</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> action)
        {
            OnPropertyChanged(GetPropertyName(action));
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="propertyName">属性</param>
        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="propertyName">属性</param>
        public void RaisePropertyChanged<T>(string propertyName,T old,T now)
        {
            InvokeInUiThread(RaisePropertyChangedInner,new PropertyChangingEventArgsEx<T>(propertyName,old,now));
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="propertyName">属性</param>
        public void RaisePropertyChanged(string propertyName, object old, object now)
        {
            InvokeInUiThread(RaisePropertyChangedInner, new PropertyChangingEventArgsEx(propertyName, old, now));
        }

        /// <summary>
        ///     发出属性修改事件(不受阻止模式影响)
        /// </summary>
        /// <param name="args">属性</param>
        private void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            InvokeInUiThread(RaisePropertyChangedInner, args);
        }

        private string preName;
        /// <summary>
        ///     发出属性修改事件(不受阻止模式影响)
        /// </summary>
        /// <param name="args">属性</param>
        private void RaisePropertyChangedInner(PropertyChangedEventArgs args)
        {
            if (preName == args.PropertyName)
            {
                return;
            }

            preName = args.PropertyName;
            try
            {
                PropertyChangedEvent?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, "NotificationObject.RaisePropertyChangedInner");
            }
            finally
            {
                preName = null;
            }
        }
        /// <param name="action">属性字段</param>
        protected static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            return expression.Member.Name;
        }
        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="action">属性字段</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            OnPropertyChanged(GetPropertyName(action));
        }

        /// <summary>
        ///     发出属性修改事件
        /// </summary>
        /// <param name="propertyName">属性</param>
        public void OnPropertyChanged(string propertyName)
        {
            RecordModifiedInner(propertyName);
            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
            //if (!IsModify && propertyName != nameof(IsModify))
            //{
            //    OnStatusChanged(NotificationStatusType.Modified);
            //    IsModify = true;
            //}
        }

        /// <summary>
        ///     记录属性修改
        /// </summary>
        /// <param name="propertyName">属性</param>
        protected virtual void RecordModifiedInner(string propertyName)
        {
        }

        /// <summary>
        ///     属性修改处理
        /// </summary>
        /// <param name="propertyName">属性</param>
        protected virtual void OnPropertyChangedInner(string propertyName)
        {
        }

        #endregion

        #region UI线程同步支持

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        public void InvokeInUiThread(Action action)
        {
            if (WorkContext.SynchronousContext == null)
            {
                action();
            }
            else
            {
                WorkContext.SynchronousContext.InvokeInUiThread(action);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        public void BeginInvokeInUiThread(Action action)
        {
            if (WorkContext.SynchronousContext == null)
            {
                action();
            }
            else
            {
                WorkContext.SynchronousContext.BeginInvokeInUiThread(action);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        public void BeginInvokeInUiThread<T>(Action<T> action, T args)
        {
            if (WorkContext.SynchronousContext == null)
            {
                action(args);
            }
            else
            {
                WorkContext.SynchronousContext.BeginInvokeInUiThread(action, args);
            }
        }

        /// <summary>
        ///     在UI线程中执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        public void InvokeInUiThread<T>(Action<T> action, T args)
        {
            if (WorkContext.SynchronousContext == null)
            {
                action(args);
            }
            else
            {
                WorkContext.SynchronousContext.InvokeInUiThread(action, args);
            }
        }
        #endregion

        #region 状态修改事件
        /*
        [IgnoreDataMember]
        private bool _isModify;
        /// <summary>
        ///     冻结
        /// </summary>
        [IgnoreDataMember, Category("系统"), DisplayName("已修改")]
        public bool IsModify
        {
            get => _isModify;
            set
            {
                if (_isModify == value)
                {
                    return;
                }

                _isModify = value;
                OnStatusChanged(NotificationStatusType.Modified);
            }
        }

        /// <summary>
        ///     状态变化事件
        /// </summary>
        private event PropertyChangedEventHandler StatusChangedEvent;

        /// <summary>
        ///     状态变化事件
        /// </summary>
        public event PropertyChangedEventHandler StatusChanged
        {
            add
            {
                StatusChangedEvent -= value;
                StatusChangedEvent += value;
            }
            remove => StatusChangedEvent -= value;
        }

        /// <summary>
        ///     发出状态变化事件
        /// </summary>
        /// <param name="args">属性</param>
        private void RaiseStatusChangedInner(PropertyChangedEventArgs args)
        {
            StatusChangedEvent?.Invoke(this, args);
        }


        /// <summary>
        ///     发出状态变化事件
        /// </summary>
        /// <param name="status">状态</param>
        public void OnStatusChanged(NotificationStatusType status)
        {
            OnStatusChangedInner(status);
            RaiseStatusChanged(status.ToString());
        }

        /// <summary>
        ///     发出状态变化事件
        /// </summary>
        /// <param name="status">状态</param>
        public void OnStatusChanged(string status)
        {
            OnStatusChangedInner(status);
            RaiseStatusChanged(status);
        }

        /// <summary>
        ///     发出状态变化事件
        /// </summary>
        /// <param name="statusName">状态</param>
        private void RaiseStatusChanged(string statusName)
        {
            InvokeInUiThread(RaiseStatusChangedInner, new PropertyChangedEventArgs(statusName));
        }


        /// <summary>
        ///    状态变化处理
        /// </summary>
        /// <param name="status">状态</param>
        protected virtual void OnStatusChangedInner(NotificationStatusType status)
        {

        }

        /// <summary>
        ///    状态变化处理
        /// </summary>
        /// <param name="status">状态</param>
        protected virtual void OnStatusChangedInner(string status)
        {

        }
        */
        #endregion
    }
}
