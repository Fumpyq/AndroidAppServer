using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IllegalPileController : ControllerBase
    {
        [HttpPost("create")]
        public async Task<ActionResult> CreateNewIllegalPile([FromQuery] string token/*, [FromBody] GeozoneCreate geozEvent*/)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                // Log.Text($"user GUid: {UserGuid}");
                if (!Request.Form.TryGetValue("req", out var res)) return BadRequest("req part required");
                //  Log.Text("=-=-=-=-=-=-=-RAW-=-=-=-=-=-=-= \n" + res[0]);
                //Log.Text(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(res[0]), Formatting.Indented));
                var req = JsonConvert.DeserializeObject<IllegalTrashPileCreate>(res[0]);
                Log.Json(req);

                await SQL.CreateIllegalTrashPile(req,UserGuid,Request.Form.Files);


                return Ok();
            }
            return Unauthorized();
        }            
        [HttpPost("open")]
        public async Task<ActionResult> ReopenIllegalPile([FromQuery] string token/*, [FromBody] GeozoneCreate geozEvent*/)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                // Log.Text($"user GUid: {UserGuid}");
                if (!Request.Form.TryGetValue("req", out var res)) return BadRequest("req part required");
                //  Log.Text("=-=-=-=-=-=-=-RAW-=-=-=-=-=-=-= \n" + res[0]);
                //Log.Text(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(res[0]), Formatting.Indented));
                var req = JsonConvert.DeserializeObject<CloseTrashPileRequest>(res[0]);
                Log.Json(req);

                if( SQL.ReopenIllegal(req,UserGuid,Request.Form.Files)) return Ok();



                return BadRequest();
            }
            return Unauthorized();
        }        
        [HttpPost("close")]
        public async Task<ActionResult> CloseIllegalPile([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                if (!Request.TryGetFromMultipart<CloseTrashPileRequest>("req", out var req)) return BadRequest("req part required");

                if (!SQL.CloseIllegalTrashPile(req, UserGuid,Request.Form.Files))
                    return BadRequest();


                return Ok();
            }
            return Unauthorized();
        }        
        [HttpPost("edit")]
        public async Task<ActionResult> UpdateIllegalPile([FromQuery] string token, [FromBody] TrashPileUpdateRequest request)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {

                if (!SQL.Update_TrashHeap(request,UserGuid))
                    return BadRequest();
                return Ok();
            }
            return Unauthorized();
        }
        [HttpGet("all")]
        public ActionResult GetAllPiles([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                return Ok(SQL.GetAllIllegalTrashPiles());
            }
            return Unauthorized();
           
        }

        [HttpGet("history")]
        public ActionResult GetPileHistory([FromQuery] string token, [FromQuery] string guid)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                return Ok(SQL.GetIllegalHistory(guid));
            }
            return Unauthorized();

        }
    }
}
