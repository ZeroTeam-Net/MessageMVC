using Agebull.Common.Mvvm;
using Agebull.EntityModel.Config;

namespace MessageMVC.Wpf.Sample.Model
{
    public class Step3ViewModel : FlowViewModel<Step3DataModel>
    {
        public DelegateCommand NextCommand => new DelegateCommand(() => Model.Next());

    }
}
