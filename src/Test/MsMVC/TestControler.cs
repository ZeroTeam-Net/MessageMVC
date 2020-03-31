using Microsoft.AspNetCore.Mvc;

namespace MsMVC.Controllers
{
    [ApiController]
    [Route("api")]
    public class TestControler : ControllerBase
    {
        [Route("v1/test")]
        public string Test()
        {
            return "Hello world";
        }
    }
}
