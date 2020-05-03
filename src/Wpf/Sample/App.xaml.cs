using Agebull.Common.Ioc;
using System.Windows;
using ZeroTeam.MessageMVC;

namespace MessageMVC.Wpf.Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DependencyHelper.ServiceCollection.UseSampleApp();
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            ZeroFlowControl.Shutdown();
            base.OnExit(e);
        }
    }
}
