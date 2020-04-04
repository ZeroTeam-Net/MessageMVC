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
        ///     是否工作在管理模式下(数据全看模式)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsManageMode { get; set; }

        /// <summary>
        ///     是否工作在系统模式下
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsSystemMode { get; set; }


        /// <summary>
        ///     其它特性
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Feature { get; set; }

        private IOperatorStatus _status = new OperatorStatus();

        /// <summary>
        ///     最后状态(当前时间)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public IOperatorStatus LastStatus
        {
            get
            {
                if (_messageChanged)
                {
                    _status.Message = _messageBuilder.ToString();
                }
                return _status;
            }
            set
            {
                _status = value ?? new OperatorStatus();
                if (_status.Message == null)
                    return;
                _messageBuilder = new StringBuilder();
                _messageBuilder.Append(_status.Message);
            }
        }

        #region 方便使用


        /// <summary>
        ///     最后操作的操作状态
        /// </summary>
        public int LastState
        {
            get => _status.Code;
            set => _status.Code = value;
        }

        private bool _messageChanged;

        private StringBuilder _messageBuilder;

        /// <summary>
        ///     加入消息
        /// </summary>
        /// <param name="msg"></param>
        public void AppendMessage(string msg)
        {
            if (_messageBuilder == null)
            {
                _messageBuilder = new StringBuilder(_status.Message);
            }

            _messageBuilder.AppendLine(msg);
            _messageChanged = true;
        }

        /// <summary>
        ///     重置状态
        /// </summary>
        public void ResetStatus()
        {
            _status.Code = DefaultErrorCode.Success;
            ClearMessage();
        }

        /// <summary>
        ///     清除消息
        /// </summary>
        public void ClearMessage()
        {
            _messageBuilder = null;
            _messageChanged = false;
            _status.Message = null;
        }

        /// <summary>
        ///     最后一个的操作消息
        /// </summary>
        public string LastMessage
        {
            get => _status.Message;
            set => _status.Message = value;
        }

        #endregion
        /*
        /// <summary>
        ///     最后状态(当前时间)
        /// </summary>
        IOperatorStatus IGlobalContext.LastStatus => LastStatus;*/

    }
}
