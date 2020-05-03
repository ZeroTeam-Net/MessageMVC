using Agebull.Common.Ioc;
using Microsoft.Extensions.Logging;
using System;

namespace Agebull.EntityModel.Config
{
    public class FlowDataModel : DataModelBase
    {

        protected ILogger Logger;

        public FlowDataModel()
        {
            Logger = DependencyHelper.LoggerFactory.CreateLogger(GetType().GetTypeName());
        }

        private bool isBusy;
        /// <summary>
        /// 是否正忙
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value)
                {
                    return;
                }

                isBusy = value;
                RaisePropertyChanged(nameof(IsBusy));
                RaisePropertyChanged(nameof(IsEnable));
            }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsEnable
        {
            get => !isBusy;
        }
    }
}