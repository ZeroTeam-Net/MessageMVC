using Agebull.Common.Mvvm;
using Agebull.EntityModel.Config;

namespace MessageMVC.Wpf.Sample.Model
{
    public class Step1ViewModel : FlowViewModel<Step1DataModel>
    {
        public DelegateCommand NextCommand => new DelegateCommand(() => Model.Next());

    }
}
