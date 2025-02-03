using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        public class DivisionsResponse
        {
            public List<User> unnasignedUsers { get; set; } = new List<User>();
            public List<Division> divisions { get; set; } = new List<Division>();
        }

        [HttpGet("divAndUsers")]
        public ActionResult GetDivisions([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                var Divs = SQL.GetDivisionList();
                var Users = SQL.GetUsersList();

                var Map = new Dictionary<string, Division>(); 
                foreach (var Div in Divs) { Map.Add(Div.guid, Div); }


                var res = new DivisionsResponse();

                foreach (var u in Users)
                {
                    if (!string.IsNullOrEmpty(u.Division_guid))
                    {
                        if(Map.TryGetValue(u.Division_guid,out var Div))
                        {
                            Div.users.Add(u);
                            continue;
                        }
                    }

                    res.unnasignedUsers.Add(u);  

                }
                res.divisions = Divs;
                return Ok(res);
            }
            return Unauthorized();

        }

        [HttpGet("userDivUsers")]
        public ActionResult GetUserDivisionUsers([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                var res = SQL.GetUserDivisionUsers(UserGuid);
                return Ok(res);
            }
            return Unauthorized();

        }
       
    }
}
