#define SW_LOG

using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
#if SW_LOG
using System.Diagnostics;
#endif
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectsController : ControllerBase
    {
        [HttpPost()]
        public ActionResult GetNearestObjects([FromQuery] string token, [FromBody] GeoPoint g)
        {

            if (Login.ValidateToken(token, true))
            {

                Log.Text(g.ToString());
                Stopwatch sw = Stopwatch.StartNew();
                var res = SQL.GetClosestObjects(g);
                sw.Stop();
                Log.Text($"GetNearestObjects: Total:{sw.Elapsed.TotalMilliseconds.ToString("F3").Replace(",", ".")}");
                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpPost("onClick")]
        public ActionResult OnObjectClick([FromQuery] string token, [FromQuery] string objectGuid)
        {

            if (Login.ValidateToken(token, true))
            {

                Log.Text($"OnObjectClick {objectGuid}");

                var res = SQL.GetGeoObject(objectGuid);

                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpPost("create")]
        public ActionResult CreateNewObject([FromQuery] string token)
        {

            if (Login.ValidateToken(token,out string login,out string userGuid))
            {
               
                if (!Request.TryGetFromMultipart<GeoObjectCreateRequest>("request", out var req)) return BadRequest("request part required");
                Log.Json(req);
                if (!SQL.CreateNewObject(req,userGuid, out string guid, Request.Form.Files))
                    return BadRequest();
                else
                {
                    Log.Action(login, $"New object Created ({guid})");
                    return Ok();
                }
            }
            return Unauthorized();
        }
        [HttpPost("event")]
        public ActionResult AddObjectEvent([FromQuery] string token, [FromQuery] string? objectGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                if (!Request.Form.TryGetValue("event", out var res)) return BadRequest("\'event\' part required");
                if (!Request.Form.TryGetValue("userPosition", out var res1)) return BadRequest("\'userPosition\' part required");
                var objectEvent = JsonConvert.DeserializeObject<UniversalEvent>(res[0]);
                Log.Json(objectEvent);
                GeoPoint? pos = null;

                Request.TryGetFromMultipart("initPosition", out pos);







                var position = JsonConvert.DeserializeObject<GeoPoint>(res1[0]);
                objectEvent.guid = objectGuid;
                Log.System("AddGeozoneEvent");
                Log.Text($"Files: {Request.Form.Files.Count}");
                Log.Json(objectEvent);
                Log.Json(position);

#if !SQL_READ_ONLY


                var EventGuid = SQL.InsertNewObjectEvent(objectEvent.guid, UserGuid, objectEvent, position, pos, Request.Form.Files);

#else
                Log.System("SQL_READ_ONLY SQL.InsertNewEvent Prevented");
#endif
                return Ok();
            }
            return Unauthorized();
        }
        [HttpPost("event/get")]
        public ActionResult GetObjectEvents([FromQuery] string token, [FromQuery] string objectGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                var res = SQL.GetObjectEvents( objectGuid, UserGuid );
                return Ok(res);
            }
            return Unauthorized();
        }




        [HttpGet("types")]
        public ActionResult GetObjectBaseData([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid,UseCash:true))
            {
                var res = SQL.GetObjectTypes();
                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpPost("edit")]
        public ActionResult EditGeoObject([FromQuery] string token, [FromBody] GeoObjectEditRequest req)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                SQL.EditGeoObject(req,UserGuid);
                return Ok();
            }
            return Unauthorized();
        }



        [HttpPost("geozones")]
        public ActionResult EditGeoObject([FromQuery] string token, [FromQuery] string objGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                var res = SQL.GetObjectGeozones(objGuid);
                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpPost("massgeozonelink")]
        public ActionResult LinkObjectGeozone([FromQuery] string token, [FromBody] MassiveUnlinkRequest data)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                foreach (var geo in data.geoGuids)
                {
                    foreach (var obj in data.objGuids)
                    {
                        if (SQL.LinkGeozone2Object(UserGuid, obj, geo, data.comment)) ;
                    }
                }
               
               return Ok();
               // return BadRequest();
            }
            return Unauthorized();
        }
        //public class LinkGeozoneRequest
        //{
        //    public string geozoneGuid { get; set; }
        //    public string objGuid { get; set; }
        //    public string comment { get; set; }
        //}
        //[HttpPost("geozonelink2")]
        //public ActionResult LinkObjectGeozone([FromQuery] string token, [FromBody] LinkGeozoneRequest data )
        //{
        //    if (Login.ValidateToken(token, out string _, out string UserGuid))
        //    {
        //        if (SQL.LinkGeozone2Object(UserGuid, data.objGuid, data.geozoneGuid,data.comment))
        //            return Ok();
        //        return BadRequest();
        //    }
        //    return Unauthorized();
        //}
        [HttpPost("geozonelink")]
        public ActionResult LinkObjectGeozone([FromQuery] string token, [FromQuery] string oGuid, [FromQuery] string gGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                if(SQL.LinkGeozone2Object(UserGuid,oGuid,gGuid))
                return Ok();
                return BadRequest();
            }
            return Unauthorized();
        }
        [HttpPost("search")]
        public ActionResult SearchPrompt([FromQuery] string token, [FromQuery] string prompt, [FromBody] GeoPoint UserPos)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                Log.Message($"Search Objects: {prompt} , {UserPos.ToString()}");
                var data = SQL.GetObjectsFromSearchPrompt(prompt, UserPos);
                var res = new SearchResults<GeoObjectMarker>();
                if (data.Count <=0)
                {
                   if(DadataApi.TryFindPoint(prompt,out GeoPoint pos))
                    {
                        res.point = pos;
                        res.isDataFinded=true;
                    }
                    else
                    {
                        res.isDataFinded = false;
                    }
                }
                else
                {
                    res.results = data;
                    res.isDataFinded = true;
                }
                

                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpDelete("geozoneunlink")]
        public ActionResult UnlinkObjectGeozone([FromQuery] string token, [FromQuery] string oGuid, [FromQuery] string gGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                SQL.UnlinkGeozoneFromObject(gGuid,oGuid,UserGuid);
                return Ok();
            }
            return Unauthorized();
        }
        public class MassiveTransferRequest
        {
            public List<string> objGuids { get; set; }
            public string fromGeoGuid { get; set; }
            public string toGeoGuid { get; set; }
            public string? comment { get; set; }
        }

        [HttpPost("geozoneTransfer_massive")]
        public ActionResult TransferObjectGeozone_Massive([FromQuery] string token, [FromBody] MassiveTransferRequest data)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                
                    foreach (var obj in data.objGuids)
                    {
                        SQL.LinkGeozone2Object(UserGuid, obj, data.toGeoGuid, data.comment);
                        SQL.UnlinkGeozoneFromObject(data.fromGeoGuid, obj, UserGuid, data.comment);
                       
                     }
                
                return Ok();
            }
            return Unauthorized();
        }


        public class MassiveUnlinkRequest
        {
            public List<string> objGuids { get; set; }
            public List<string> geoGuids { get; set; }
            public string? comment { get; set; }
        }

        [HttpPost("geozoneunlink_massive")]
        public ActionResult UnlinkObjectGeozone_Massive([FromQuery] string token, [FromBody] MassiveUnlinkRequest data)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                foreach (var geo in data.geoGuids)
                {
                    foreach (var obj in data.objGuids)
                    {
                        SQL.UnlinkGeozoneFromObject(geo, obj, UserGuid,data.comment);
                    }
                }
                return Ok();
            }
            return Unauthorized();
        }

        public record struct BaseObjectsEventData(List<ObjectEventType> eventTypes);

        [HttpGet("event/baseinfo")]
        public ActionResult GetObjectEventsTypes([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                var et = SQL.GetObjectEventsInfos();

                var res = new BaseObjectsEventData(
                    et
                    );

                return Ok(res);
            }
            return Unauthorized();
        }
    }
}
