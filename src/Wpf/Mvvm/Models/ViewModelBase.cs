// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

#region 引用

using Agebull.Common.Ioc;
using Agebull.Common.Mvvm;
using System;
using System.Windows;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     MVVM的ViewModel的基类
    /// </summary>
    public abstract class ViewModelBase : MvvmBase
    {
        /// <summary>
        /// 对应的视图
        /// </summary>
        public FrameworkElement View { get; private set; }

        /// <summary>
        /// 绑定视图对象的行为
        /// </summary>
        public DependencyAction ViewBehavior => new DependencyAction
        {
            AttachAction = BindingView
        };

        /// <summary>
        ///  绑定视图对象
        /// </summary>
        /// <param name="obj"></param>
        private void BindingView(DependencyObject obj)
        {
            View = obj as FrameworkElement;
            if (View == null)
            {
                return;
            }

            Dispatcher = View.Dispatcher;
            Synchronous = new DispatcherSynchronousContext
            {
                Dispatcher = Dispatcher
            };
            WorkContext.SynchronousContext ??= Synchronous;

            OnViewSeted();
        }
        /// <summary>
        /// 视图绑定成功后的初始化动作
        /// </summary>
        protected abstract void OnViewSeted();
    }

    /// <summary>
    ///     MVVM的ViewModel的基类
    /// </summary>
    public abstract class ViewModelBase<TModel> : ViewModelBase
            where TModel : DataModelBase, new()
    {
        /// <summary>
        ///     模型
        /// </summary>
        public TModel Model { get; private set; }

        /// <summary>
        /// 视图绑定成功后的初始化动作
        /// </summary>
        protected override void OnViewSeted()
        {
            Model = DependencyHelper.Create<TModel>() ?? new TModel();
            Model.Dispatcher = Dispatcher;
            Model.Synchronous = Synchronous;
            Dispatcher.BeginInvoke(new Action(Model.Initialize));
            RaisePropertyChanged(nameof(Model));
        }
    }
}
