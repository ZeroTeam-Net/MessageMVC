using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.WechatEx
{
    /// <summary>
    /// 微信登录Api简单封装
    /// </summary>
    public static class WechatApi
    {
        const string testCode = "_^_SupperCode_^_";
        const string testOpenId = "oQKU65Mzque7xhph6xHVtKFA2ZxA";
        const string testSession = "9Xcft8ToqS1QYKgoMnfuHw==";

        /// <summary>
        /// 向微信接口请求openid，unionid，session_key     
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<(bool, WechatSession)> GetSession(string code)
        {
            if (code == testCode)
            {
                return (true, new WechatSession
                {
                    OpenID = testOpenId,
                    SessionKey = testSession
                });
            }
            try
            {
                var arg = $"?appid={WechatOption.Instance.AppID}&secret={WechatOption.Instance.AppSecret}&js_code={code}&grant_type=authorization_code";
                var (state, json) = await HttpPoster.OutGet("WeixinCode2session", arg);
                var result = SmartSerializer.FromInnerString<WechatSession>(json);
                if (state != MessageState.Success || result == null || result.ErrCode != null)
                {
                    return (false, result);
                }
                return (true, result);
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                FlowTracer.MonitorDetails(e.Message);
                GlobalContext.Current.Status.LastMessage = "微信接口获取openid，sessionkey等信息发生异常！";
                GlobalContext.Current.Status.LastState = OperatorStatusCode.NetworkError;
                return (false, null);
            }
        }


        /// <summary>
        /// 根据OpenID和全局AccessToken获得微信用户基本信息
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static async Task<(bool, WechatUserInfo)> GetUserInfo(string openid)
        {
            if (openid == testOpenId)
            {
                return (true, new WechatUserInfo
                {
                    OpenID = openid,
                    NickName = "test"
                });
            }
            try
            {
                var (success, at) = await GetAccessToken();
                if (!success)
                {
                    return (false, new WechatUserInfo());
                }
                var arg = $"?access_token={at}&openid={openid}&lang=zh_CN";
                var (state, json) = await HttpPoster.OutGet("WeixinUserInfo", arg);
                var result = SmartSerializer.FromInnerString<WechatUserInfo>(json);
                if (state != MessageState.Success || result == null || result.ErrCode != null)
                {
                    return (false, result);
                }
                return (true, result);
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                FlowTracer.MonitorDetails(e.Message);
                GlobalContext.Current.Status.LastMessage = "微信接口根据OpenID和全局AccessToken获得微信用户基本信息发生异常！";
                GlobalContext.Current.Status.LastState = OperatorStatusCode.NetworkError;
                return (false, null);
            }
        }

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="arg">模板参数</param>
        /// <returns></returns>
        public static async Task<bool> SendTemplate(string arg)
        {
            try
            {
                var (success, at) = await GetAccessToken();
                if (!success)
                {
                    return false;
                }
                var api = $"cgi-bin/message/template/send?access_token={at}";
                var (state, json) = await HttpPoster.OutPost("WeixinApi", api, arg);
                var result = SmartSerializer.FromInnerString<WechatUserInfo>(json);
                if (state != MessageState.Success || result == null || result.ErrCode != null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                FlowTracer.MonitorDetails(e.Message);
                GlobalContext.Current.Status.LastMessage = "微信接口发送模板消息发生异常！";
                GlobalContext.Current.Status.LastState = OperatorStatusCode.NetworkError;
                return false;
            }
        }


        static int isBusy = 0;

        static readonly List<TaskCompletionSource<(bool, string)>> waitList = new List<TaskCompletionSource<(bool, string)>>();

        /// <summary>
        /// 根据appid获得最新的AccessToken值
        /// </summary>
        /// <returns></returns>
        public static async Task InitAccessToken(CancellationToken token)
        {
            ScopeRuner.ScopeLogger.Information(SmartSerializer.ToInnerString(WechatOption.Instance.Templates));
            await GetAccessToken();
        }
        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="args">模板参数</param>
        /// <returns></returns>
        public static Task<bool> SendTemplate(WechatTemplateValue args)
        {
            if (!WechatOption.Instance.Templates.TryGetValue(args.TemplateId, out var template))
            {
                return Task.FromResult(false);
            }
            var nt = new WechatTemplate
            {
                TemplateId = args.TemplateId,
                TopColor = template.TopColor,
                OpenId = args.OpenId,
                Url = args.Url
            };

            foreach (var item in template.Data)
            {
                if (args.Data.TryGetValue(item.Key, out var value))
                {
                    nt.Data.Add(item.Key, new WechatTemplateItem
                    {
                        Value = value,
                        Color = item.Value.Color
                    });
                }
                else
                {
                    nt.Data.Add(item.Key, item.Value);
                }
            }
            return SendTemplate(SmartSerializer.ToInnerString(args));
        }

        /// <summary>
        /// 根据appid获得最新的AccessToken值
        /// </summary>
        /// <returns></returns>
        public static async Task<(bool, string)> GetAccessToken()
        {
            if (WechatOption.Instance.Last > DateTime.Now)
                return (true, WechatOption.Instance.AccessToken);
            try
            {
                if (Interlocked.Increment(ref isBusy) > 1)
                {
                    if (WechatOption.Instance.AccessToken.IsPresent())
                        return (true, WechatOption.Instance.AccessToken);
                    var task = new TaskCompletionSource<(bool, string)>();

                    waitList.Add(task);
                    return await task.Task;
                }

                var arg = $"?grant_type=client_credential&appid={WechatOption.Instance.AppID}&secret={WechatOption.Instance.AppSecret}";
                var (state, json) = await HttpPoster.OutGet("WeixinToken", arg);
                var token = SmartSerializer.FromInnerString<WechatToken>(json);
                if (state == MessageState.Success && token != null && token.ErrCode == null)
                {
                    WechatOption.Instance.AccessToken = token.AccessToken;

                    ScopeRuner.ScopeLogger.Information(WechatOption.Instance.AccessToken);
                    //过期时间比原计划小120秒，避免请求时间导致的错误
                    WechatOption.Instance.Last = DateTime.Now.AddSeconds(token.ExpiresIn - 120);
                }
            }
            catch (Exception e)
            {
                ScopeRuner.ScopeLogger.Exception(e);
                FlowTracer.MonitorDetails(e.Message);
            }
            Interlocked.Exchange(ref isBusy, 0);
            await Task.Delay(5);
            var res = (WechatOption.Instance.AccessToken.IsPresent(), WechatOption.Instance.AccessToken);
            var tasks = waitList.ToArray();
            waitList.Clear();
            foreach (var wait in tasks)
            {
                wait.TrySetResult(res);
            }
            return res;
        }
    }
}