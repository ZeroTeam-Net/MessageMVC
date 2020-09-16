﻿using System.Threading;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Service("test2")]
    public class HttpTest3Controler : IApiController
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("hello"), ApiOption(ApiOption.CustomContent)]
        public IApiResult<string> Hello(string abc)
        {
            Thread.Sleep(10);
            return ApiResultHelper.Helper.Succees($"hello1:{abc}");
        }
        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("delay"), ApiOption(ApiOption.CustomContent)]
        public IApiResult<string> Delay(string abc)
        {
            GlobalContext.Current.Task.SetResult((Messages.MessageState.Success, $"hello1:{abc}"));
            Thread.Sleep(10);
            return ApiResultHelper.Helper.Succees($"hello1:{abc}");
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("error"), ApiOption(ApiOption.CustomContent)]
        public IApiResult<string> Error(string abc)
        {
            throw new System.Exception("Error");
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("cancel"), ApiOption(ApiOption.CustomContent)]
        public void Cancel(string abc)
        {
            GlobalContext.Current.Task.SetCanceled();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Route("exception"), ApiOption(ApiOption.CustomContent)]
        public void Exception(string abc)
        {
            GlobalContext.Current.Task.SetException(new System.Exception("Exception"));
        }
    }
}
