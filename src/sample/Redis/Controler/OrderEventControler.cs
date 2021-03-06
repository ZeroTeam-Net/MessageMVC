﻿using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [NetEvent("OrderEvent")]
    public class OrderEventControler : IApiController
    {
        [Route("offline/v1/new")]
        public void OnOrderNew(UnionOrder order)
        {
            Console.WriteLine(JsonHelper.SerializeObject(order));
        }
    }
}
