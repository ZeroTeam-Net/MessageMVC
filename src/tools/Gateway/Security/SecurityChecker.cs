using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Services;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    ///     安全检查员
    /// </summary>
    internal class SecurityChecker : IMessageMiddleware
    {
        #region IMessageMiddleware

        /// <summary>
        /// 层级
        /// </summary>
        int IMessageMiddleware.Level => short.MinValue;


        /// <summary>
        /// 消息中间件的处理范围
        /// </summary>
        MessageHandleScope IMessageMiddleware.Scope => MessageHandleScope.Prepare;

        /// <summary>
        /// 当前处理器
        /// </summary>
        MessageProcessor IMessageMiddleware.Processor { get; set; }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="service">当前服务</param>
        /// <param name="message">当前消息</param>
        /// <param name="tag">扩展信息</param>
        /// <param name="next">下一个处理方法</param>
        /// <returns></returns>
        Task<bool> IMessageMiddleware.Prepare(IService service, IMessageItem message, object tag)
        {
            return CheckToken2(message);
        }
        #endregion

        #region 变量

        static SecurityChecker()
        {
            ConfigurationManager.RegistOnChange(LoadOption, true);
            
        }
        static void LoadOption()
        {
            Option = ConfigurationManager.Option<SecurityConfig>("Gateway:Security") ?? new SecurityConfig();

            Option.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Option.denyTokens != null)
            {
                foreach (var apiItem in Option.denyTokens)
                    Option.DenyTokens.Add(apiItem, apiItem);
            }
        }

        /// <summary>
        /// 配置
        /// </summary>
        public static SecurityConfig Option = new SecurityConfig();

        #endregion

        #region 预检

        /// <summary>
        ///     数据
        /// </summary>
        private HttpMessage Message;

        /// <summary>
        ///     预检
        /// </summary>
        /// <returns></returns>
        public bool PreCheck()
        {
            if (CheckSign() && KillDenyHttpHeaders())
            {
                return true;
            }

            Message.Result = ApiResultHelper.DenyAccessJson;
            return false;

        }

        /// <summary>
        ///     检查特定操作系统与App的适用性
        /// </summary>
        /// <returns></returns>
        private bool CheckApisInner()
        {
            //var header = Request.Headers.Values.LinkToString(" ");
            //if (string.IsNullOrWhiteSpace(header) || header.Contains("iToolsVM"))
            //    return false;
            if (!Option.CheckApiItem)//|| Data.ApiItem == null
            {
                return true;
            }
            ////OS匹配
            //if (!string.IsNullOrWhiteSpace(Data.ApiItem.Os) && Data.Trace.Token.IndexOf(Data.ApiItem.Os, StringComparison.OrdinalIgnoreCase) < 0)
            //    return false;
            ////APP匹配
            //if (!string.IsNullOrWhiteSpace(Data.ApiItem.App) && Data.Trace.Token.IndexOf(Data.ApiItem.App, StringComparison.OrdinalIgnoreCase) < 0)
            //    return false;
            return true;
        }

        /// <summary>
        ///     验签
        /// </summary>
        /// <returns></returns>
        internal bool CheckSign()
        {
            return true;
        }

        /// <summary>
        ///     针对HttpHeader特征阻止不安全访问
        /// </summary>
        /// <returns></returns>
        internal bool KillDenyHttpHeaders()
        {
            try
            {
                foreach (var head in Option.DenyHttpHeaders)
                {
                    if (!Message.Trace.Headers.ContainsKey(head.Head))
                    {
                        continue;
                    }

                    switch (head.DenyType)
                    {
                        case DenyType.Hase:
                            if (Message.Trace.Headers.ContainsKey(head.Head))
                            {
                                return false;
                            }

                            break;
                        case DenyType.NonHase:
                            if (!Message.Trace.Headers.ContainsKey(head.Head))
                            {
                                return false;
                            }

                            break;
                        case DenyType.Count:
                            if (!Message.Trace.Headers.ContainsKey(head.Head))
                            {
                                break;
                            }

                            if (Message.Trace.Headers[head.Head].Count == int.Parse(head.Value))
                            {
                                return false;
                            }

                            break;
                        case DenyType.Equals:
                            if (!Message.Trace.Headers.ContainsKey(head.Head))
                            {
                                break;
                            }

                            if (string.Equals(Message.Trace.Headers[head.Head].ToString(), head.Value,
                                StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }

                            break;
                        case DenyType.Like:
                            if (!Message.Trace.Headers.ContainsKey(head.Head))
                            {
                                break;
                            }

                            if (Message.Trace.Headers[head.Head].ToString().Contains(head.Value))
                            {
                                return false;
                            }

                            break;
                        case DenyType.Regex:
                            if (!Message.Trace.Headers.ContainsKey(head.Head))
                            {
                                break;
                            }

                            var regx = new Regex(head.Value,
                                RegexOptions.IgnoreCase | RegexOptions.ECMAScript | RegexOptions.Multiline);
                            if (regx.IsMatch(Message.Trace.Headers[head.Head].ToString()))
                            {
                                return false;
                            }

                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return true;
            }
        }

        /// <summary>
        ///     执行检查
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空或不合格
        ///     2：令牌是伪造的
        /// </returns>
        private async Task<bool> CheckToken2(IMessageItem message)
        {
            Message = message as HttpMessage;
            try
            {
                if (!Option.FireBearer)
                {
                    return true;
                }
                if (Message.Arguments.ContainsKey("token"))
                {
                    Message.Arguments.Remove("token");
                }

                if (!CheckApisInner())
                {
                    Message.State = MessageState.NoSupper;
                    Message.Result = ApiResultHelper.DenyAccessJson;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Message.Trace.Token))
                {
                    //if (Data.ApiItem != null && Data.ApiItem.NeedLogin)
                    //{
                    //    Data.Result = ApiResultHelper.DenyAccessJson;
                    //    return false;
                    //}
                    return true;
                }
                var isAccessToken = TokenFormalCheck();
                if (isAccessToken == null)
                {
                    return false;
                }

                var (state, json) = await CheckToken();
                if (state != MessageState.Success)
                {
                    Message.State = MessageState.NetError;
                    Message.Result = ApiResultHelper.UnavailableJson;
                    return false;
                }
                var result = JsonHelper.DeserializeObject<ApiResult<UserInfo>>(json);

                if (result == null || !result.Success)
                {
                    Message.State = MessageState.NoSupper;
                    Message.Result = json;
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                Message.Exception = e;
                Message.State = MessageState.NetError;
                return false;
            }
        }

        /// <summary>
        /// 令牌格式检查
        /// </summary>
        /// <returns></returns>
        private bool? TokenFormalCheck()
        {
            var token1 = Message.Trace.Token[0];
            bool isAccessToken = false;
            switch (token1)
            {
                default:
                    Message.State = MessageState.FormalError;
                    Message.Result = ApiResultHelper.DenyAccessJson;
                    return null;
                case '*':
                    //if (Data.ApiItem != null && Data.ApiItem.NeedLogin)
                    //{
                    //    Data.Result = ApiResultHelper.DenyAccessJson;
                    //    return false;
                    //}
                    break;
                case '#':
                    isAccessToken = true;
                    break;
            }
            if (Option.DenyTokens.ContainsKey(Message.Trace.Token))
            {
                Message.State = MessageState.NoSupper;
                Message.Result = ApiResultHelper.DenyAccessJson;
                return null;
            }
            for (var index = 1; index < Message.Trace.Token.Length; index++)
            {
                var ch = Message.Trace.Token[index];
                if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z' || ch == '_')
                {
                    continue;
                }

                LogRecorder.MonitorTrace("Token Layout Error");
                Message.State = MessageState.FormalError;
                Message.Result = ApiResultHelper.DenyAccessJson;
                return null;
            }
            return isAccessToken;
        }

        private Task<(MessageState state, string result)> CheckToken()
        {
            var message = new MessageItem
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Topic = Option.AuthStation,
                Title = Option.TokenCheckApi,
                Content = JsonHelper.SerializeObject(new
                {
                    AuthToken = Message.Trace.Token,
                    SceneToken = Message["__scene"],
                    Page = Message["__page"],
                    API = $"{Message.ApiHost}/{Message.ApiName}"
                })
            };
            return MessagePoster.Post(message);
        }

        #endregion

    }
}

/*
    /// <summary>
    /// AT校验请求参数
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class VerifyTokenArgument
    {
        /// <summary>
        /// 身份令牌
        /// </summary>
        [JsonProperty]
        public string AuthToken { get; set; }

        /// <summary>
        /// 场景令牌
        /// </summary>
        [JsonProperty]
        public string SceneToken { get; set; }

        /// <summary>
        /// 场景令牌
        /// </summary>
        [JsonProperty]
        public string Page { get; set; }

        /// <summary>
        /// 当前API
        /// </summary>
        [JsonProperty]
        public string API { get; set; }
    }
*/
