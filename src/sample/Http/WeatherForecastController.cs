using Microsoft.AspNetCore.Mvc;

namespace MsWebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet, Route("get")]
        public string Get()
        {
            return "Hello";
        }
    }
}
