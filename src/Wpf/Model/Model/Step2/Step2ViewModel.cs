using Agebull.Common.Mvvm;
using Agebull.EntityModel.Config;

namespace MessageMVC.Wpf.Sample.Model
{
    public class Step2ViewModel : FlowViewModel<Step2DataModel>
    {
        public DelegateCommand NextCommand => new DelegateCommand(() => Model.Next());

    }
}
