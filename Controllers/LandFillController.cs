using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using Microsoft.AspNetCore.Mvc;

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandFillController : ControllerBase
    {
        [HttpGet("all")]
        public ActionResult GetAllLandFields([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                return Ok( SQL.GetAllLandFillds());
            }
            return Unauthorized();
        }
    }
}
