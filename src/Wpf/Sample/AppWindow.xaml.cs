using MessageMVC.Wpf.Sample.Model;
using System;
using System.ComponentModel;
using System.Windows;

namespace MessageMVC.Wpf.Sample
{
    /// <summary>
    /// AppWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppWindow : Window
    {
        public AppWindow()
        {
            InitializeComponent();
        }

        private bool isFirst = true;
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (isFirst)
            {
                isFirst = false;
                CardSimulation.Run(this);
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            CardSimulation.Close();
            base.OnClosing(e);
        }
    }
}
