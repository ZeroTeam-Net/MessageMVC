using Agebull.Common.Ioc;
using Newtonsoft.Json;
using System.Text;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Context
{

    /// <summary>
    /// 保存于上下文的状态
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ContextStatus
    {
        /// <summary>
        ///     是否工作在系统内核模式下
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsKernelMode { get; set; }

        /// <summary>
        ///     其它特性
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Feature { get; set; }

        private IOperatorStatus _status;

        /// <summary>
        ///     最后状态(当前时间)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IOperatorStatus LastStatus
        {
            get
            {
                if (_status == null)
                {
                    _status = DependencyHelper.GetService<IOperatorStatus>();
                    _status.Success = true;
                }
                return _status;
            }

            set => _status = value;
        }

        #region 方便使用

        /// <summary>
        ///     最后操作的操作状态
        /// </summary>
        public int LastState
        {
            get => LastStatus.Code;
            set => LastStatus.Code = value;
        }

        private bool _messageChanged;

        private StringBuilder _messageBuilder;

        /// <summary>
        ///     加入消息
        /// </summary>
        /// <param name="msg"></param>
        public void AppendMessage(string msg)
        {
            _messageBuilder ??= new StringBuilder(_status.Message);
            _messageBuilder.AppendLine(msg);
            _messageChanged = true;
        }

        /// <summary>
        ///     重置状态
        /// </summary>
        public void ResetStatus()
        {
            _status.Success = true;
            _status.Code = OperatorStatusCode.Success;
            _messageBuilder = null;
            _messageChanged = false;
            _status.Message = null;
        }

        /// <summary>
        ///     最后一个的操作消息
        /// </summary>
        public string LastMessage
        {
            get
            {
                if (_messageChanged)
                {
                    _messageChanged = false;
                    LastStatus.Message = _messageBuilder?.ToString();
                    _messageBuilder = null;
                }
                return LastStatus.Message;
            }
            set => LastStatus.Message = value;
        }

        #endregion
        /*
        /// <summary>
        ///     最后状态(当前时间)
        /// </summary>
        IOperatorStatus IGlobalContext.LastStatus => LastStatus;*/

    }
}
