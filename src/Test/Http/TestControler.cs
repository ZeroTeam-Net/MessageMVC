using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{

    [Service("api")]
    public class TestControler : IApiControler
    {
        [Route("v1/test")]
        public string Test()
        {
            return "Hello world";
        }
    }
}
