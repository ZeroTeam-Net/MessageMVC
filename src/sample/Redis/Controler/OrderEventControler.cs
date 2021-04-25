using System;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [NetEvent("DataEvent")]
    public class OrderEventControler : IApiController
    {
        [Route("*")]
        public void OnOrderNew()
        {
            //Console.WriteLine(JsonHelper.SerializeObject(order));
        }
    }
}
