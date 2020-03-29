using System;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [NetEvent("OrderEvent")]
    public class OrderEventControler : IApiControler
    {
        [Route("offline/v1/new")]
        public void OnOrderNew(UnionOrder order)
        {
            throw new Exception("故意的");
            Console.WriteLine(JsonHelper.SerializeObject(order));
        }
    }
}
