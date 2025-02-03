#define SW_LOG
using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using CHGKManager.Libs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prometheus;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using static CHGKManager.Libs.ActiveDirectory;

namespace AndroidAppServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeozonesController : ControllerBase
    {
        //private const bool PreventSQlWrites=true;
        private static readonly Counter DoubleClickCounter = Metrics
    .CreateCounter("user_double_tap", "Колво двойных нажатий", labelNames: new[] {"usr"});

        private static readonly Counter CreateGeozoneCounter = Metrics
.CreateCounter("user_geozone_create", "Геозон создано", labelNames: new[] { "usr" });
        private static readonly Counter EditGeozoneCounter = Metrics
.CreateCounter("user_geozone_edit", "Геозон отредактировано", labelNames: new[] { "usr" });


        public const float GPS5metre=0.00005f;
        public record struct GeoPoint_short (double lat,double lon);       
        [HttpPost()]
        public ActionResult GetNearestGeozones([FromQuery] string token, [FromBody] GeoPoint g, [FromQuery] float? displayrange=650)
        {
#if SW_LOG
              Stopwatch sw = Stopwatch.StartNew();
               Stopwatch sw2 = Stopwatch.StartNew();
#endif
            if (Login.ValidateToken(token,out string login,true))
            {
#if SW_LOG
                sw2.Stop();
#endif




                try
                {
                    DoubleClickCounter.WithLabels(login).Inc();
                }
                catch (Exception ex) { Log.Error(ex); }

                Log.Action("GetRoundGeo", g.ToString() +"In range: "+displayrange, login);
                // Random ran = new Random();
                //List<Geozone> res =  new List<Geozone>();
#if SW_LOG
                Stopwatch sw1 = Stopwatch.StartNew();
#endif
                var res = SQL.GetClosestGeozones(g,(displayrange.HasValue? displayrange.Value:650));
#if SW_LOG
                sw1.Stop();
#endif
                int l = res.Count;
                for (int i = 0; i < l; i++)
                {
                    // (g.lat + GPS5metre * (float)((ran.NextDouble() - 0.5f * 2)) * 22f), g.lon + GPS5metre * (float)((((ran.NextDouble() - 0.5f * 2))) * 22f)

                   // List<GeoContainer> container = new List<GeoContainer>();
                   // container.Add(new GeoContainer() { type = "Паровозик", count = i, volume = 0.77f });
                 //   container.Add(new GeoContainer() { type = "Паровозик-2", count = i - 2, volume = 0.27f });
                 //   container.Add(new GeoContainer() { type = "Паровозик+4", count = i + 4, volume = 0.37f });
                   // res[i].containers = container;
                    //if (res[i].events.Count > 0) Log.Text($"Events ({res[i].events.Count}): "+res[i].name);
                    // new GeoPoint(g.lon + GPS5metre * (float)((((ran.NextDouble() - 0.5f * 2))) * 22f), (g.lat + GPS5metre * (float)((ran.NextDouble() - 0.5f * 2)) * 22f))

                    //res.Add(
                    //    new Geozone()
                    //    {
                    //        guid = r[i].guid,
                    //        name = r[i].name,
                    //        position = r[i].position,
                    //        color = r[i].color,

                    //        roof = true,
                    //        fence = true,
                    //        basement = BasementType.grunt,
                    //        address = r[i].address,
                    //        containers = container
                    //    }
                    //    );
                }
#if SW_LOG
                sw.Stop();
                Log.Text($"GetNearestGeozones: Total:{sw.Elapsed.TotalMilliseconds.ToString("F3").Replace(",",".")}," +
                    $" noSQl: {(sw.Elapsed.TotalMilliseconds-sw1.Elapsed.TotalMilliseconds-sw2.Elapsed.TotalMilliseconds).ToString("F3").Replace(",", ".")}," +
                    $" SQL: {sw1.Elapsed.TotalMilliseconds.ToString("F3").Replace(",", ".")}" +
                    $" Login: {sw2.Elapsed.TotalMilliseconds.ToString("F3").Replace(",", ".")} ms");
#endif
                return Ok(res);
            }
            return Unauthorized();
        }




        private record struct BaseCombinedData(
            List<ContainerType> containersMap,
            List<Districts> districtsList,
            List<GeozoneType> geoTypeMap,
            List<GeozoneTask> tasks
            );
        [HttpGet("base_info")]
        public ActionResult GetBaseInfo([FromQuery] string token)
        {
            if (Login.ValidateToken(token,out string _,out string UserGuid))
            {
               
                SQL.ReadBaseData(out var districtsList, out var containersMap, out var geoTypeMap);
                var tasks = SQL.GetUserTasks(UserGuid);
                Log.Text($"GettingBaseData (districtsList: {districtsList.Count} containersMap: {containersMap.Count} geoTypeMap: {geoTypeMap.Count})");
                var v = new BaseCombinedData(containersMap, districtsList, geoTypeMap,tasks);
                return Ok(v);
            }
            return Unauthorized();
        }
        [HttpGet("onclick")]
        public ActionResult OnGeozoneClick([FromQuery] string token, [FromQuery] string geozoneGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid,true))
            {
                var res = SQL.GetOnClickData(geozoneGuid);
                //var res = new List<GeoObject>(){
                //    new GeoObject() { name = "test", address = "test", position = new GeoPoint(86.138+0.0003f,55.3342+0.0003f ),client = new Client(){name="client name",inn=183_582_946_1,ogrn=183_759_274_835 } }
                //};

                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpGet("eventTypes")]
        public ActionResult GetGeozonesEventTypes([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetGeozoneEventTypes();
                

                return Ok(res);
            }
            return Unauthorized();
        }

        [HttpGet("linkdetail")]
        public ActionResult GetGeozonesLinkDetail([FromQuery] string token, [FromQuery] string geozoneGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetOnClickData(geozoneGuid);
                //var res = new List<GeoObject>(){
                //    new GeoObject() { name = "test", address = "test", position = new GeoPoint(86.138+0.0003f,55.3342+0.0003f ),client = new Client(){name="client name",inn=183_582_946_1,ogrn=183_759_274_835 } }
                //};

                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpGet("comments")]
        public ActionResult GetGeozonesCommentaries([FromQuery] string token, [FromQuery] string geozoneGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetGeozoneCommentaries(geozoneGuid);
                return Ok(res);
            }
            return Unauthorized();
        }


        [HttpPost("pic")]
        public async Task<ActionResult> ReceiveFileAsync([FromQuery] string token)
        {
            if (Login.ValidateToken(token))
            {
                try
                {
                    if (Request.Form.Files.Count > 0)
                    {
                        IFormFile file = Request.Form.Files[0];



                        using (var stream = System.IO.File.Create("C:\\Users\\a.m.maltsev\\source\\repos\\AndroidAppServer\\FileTest\\file.png"))
                        {
                            await file.CopyToAsync(stream);
                        }
                        return Ok();
                    }

                }
                catch ( Exception e)
                { 
                    Log.Error(e);
                    return BadRequest();
                }
            }
            return Unauthorized();
        }

        [HttpPost("search")]
        public ActionResult SearchPrompt([FromQuery] string token, [FromQuery] string prompt, [FromBody] GeoPoint UserPos)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                Log.Message($"Search Objects: {prompt} , {UserPos.ToString()}");
                var data = SQL.GetGeozonesFromSearchPrompt(prompt, UserPos);
                var res = new SearchResults<GeozoneMarker>();
                if (data.Count <= 0)
                {
                    if (DadataApi.TryFindPoint(prompt, out GeoPoint pos,UserPos))
                    {
                        res.point = pos;
                        res.isDataFinded = true;
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



        //ex login a.morozova pass: empty none null~
        [HttpPost("create")]
        public async Task<ActionResult> CreateNewGeozone([FromQuery] string token/*, [FromBody] GeozoneCreate geozEvent*/)
        {
            if (Login.ValidateToken(token,out string login,out string UserGuid))
            {
               // Log.Text($"user GUid: {UserGuid}");
                if(! Request.Form.TryGetValue("geozone", out var res))return BadRequest("geozone part required");
                try
                {
                    CreateGeozoneCounter.WithLabels(login).Inc();
                  
                }catch (Exception ex) { Log.Error(ex); }
                //  Log.Text("=-=-=-=-=-=-=-RAW-=-=-=-=-=-=-= \n" + res[0]);
                //Log.Text(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(res[0]), Formatting.Indented));
                var geoz = JsonConvert.DeserializeObject<GeozoneCreate>(res[0]);
                Log.Json(geoz);

                BinManGeozone bg = new BinManGeozone();
                geoz.AddressDetails = bg;
                bg.LAT = (float)geoz.position.mLatitude;
                bg.LON = (float)geoz.position.mLongitude;

                string Address = "";
                //  Log.Text(Address);
                if (string.IsNullOrEmpty(geoz.address))
                {
                    Address = DadataApi.GetAddress(geoz.position, bg);
                }
                else { Address = geoz.address; 
                   //if(geoz.isCustomAddress.HasValue && !geoz.isCustomAddress.Value) DadataApi.GetAddress(geoz.address, bg); else
                   // {
                    //    bg.ADDRESS = geoz.address;
                  //  }
                }
                geoz.address = Address;
#if !SQL_READ_ONLY
                Stopwatch sw = Stopwatch.StartNew();
                await SQL.FullGeozoneCreation(geoz, UserGuid, Request.Form.Files,bg);
                sw.Stop();
                Log.Text($"Total Time: {TimeOnly.FromTimeSpan( sw.Elapsed).ToString("H:mm:ss")}");

                string GeozoneGuid = geoz.guid;


               var geozone = SQL.GetGeozone(GeozoneGuid);
#else
                Log.System("SQL_READ_ONLY SQL.CreateGeozone Prevented");
#endif
                //GeozoneGuid


                if (geozone != null)
                {
                    return Ok(
                       new GeozoneCreateCallBack()
                       {
                           name = geozone.name,
                           address = geozone.address,
                           guid = geozone.guid
                       }
                        );
                }
                else
                {
                    Log.Error($"{GeozoneGuid} Была создана геозона, но при попытке её считывания она не была обнаружена в бд");
                    return BadRequest();
                }
            }
            return Unauthorized();
        }
        [HttpPost("move")]
        public ActionResult MoveGeozone([FromQuery] string token, [FromBody] GeozoneMove move)
        {
            if (Login.ValidateToken(token,out string login,out string UserGuid))
            {

                Log.Json(move);
                SQL.MoveGeozone(move, UserGuid);
                return Ok();
            }
            return Unauthorized();
        }
        [HttpPost("archive")]
        public ActionResult MoveGeozone([FromQuery] string token, [FromQuery] string commentary, [FromQuery] string geoGuid, [FromQuery] bool? inArchive)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                Log.Text($"[{UserGuid}] ArchivedGeozone {geoGuid}");
                SQL.ArchiveGeozone(geoGuid, UserGuid, commentary,(!inArchive.HasValue? false:inArchive.Value));
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost("edit")]
        public ActionResult EditGeozone([FromQuery] string token, [FromBody] GeozoneEdit edit)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                try
                {
                    EditGeozoneCounter.WithLabels(login).Inc();
                }
                catch (Exception ex) { Log.Error(ex); }
                Log.Json(edit);
                SQL.EditGeozone(edit, UserGuid);
                return Ok();
            }
            return Unauthorized();
        }


        [HttpPost("event")]
        public ActionResult AddGeozoneEvent([FromQuery] string token, [FromQuery] string? geozoneGuid)
        {
            if (Login.ValidateToken(token, out string _,out string UserGuid))
            {
                if (!Request.Form.TryGetValue("geozone", out var res)) return BadRequest("\'geozone\' part required");
                if (!Request.Form.TryGetValue("userPosition", out var res1)) return BadRequest("\'userPosition\' part required");
                var geozEvent = JsonConvert.DeserializeObject<UniversalEvent>(res[0]);
                Log.Json(geozEvent);
                GeoPoint? pos =null;
                switch (geozEvent.type)
                {
                    case UniversalEvent.GEO_EVENT_Move:
                    case UniversalEvent.GEO_EVENT_InsertRequest:
                        if (!Request.TryGetFromMultipart("geozPosition", out pos)) return BadRequest("\'geozPosition\' part required for this request type");
                        break;
                    default: break;
                }
                





                var position = JsonConvert.DeserializeObject<GeoPoint>(res1[0]);
                geozEvent.guid = geozoneGuid;
                Log.System("AddGeozoneEvent");
                Log.Text($"Files: {Request.Form.Files.Count}");
                Log.Json(geozEvent);
                Log.Json(position);

#if !SQL_READ_ONLY


                var EventGuid = SQL.InsertNewGeozoneEvent(geozEvent.guid, UserGuid, geozEvent, position,pos, Request.Form.Files);

#else
                Log.System("SQL_READ_ONLY SQL.InsertNewEvent Prevented");
#endif
                return Ok();
            }
            return Unauthorized();
        }
        [HttpPost("event/get")]
        public ActionResult GetGeozoneEvents([FromQuery] string token, [FromQuery] string geozoneGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                var res = SQL.GetGeozoneEvents(geozoneGuid, UserGuid);
                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpDelete("event/delete")]
        public ActionResult DeleteEvents([FromQuery] string token, [FromQuery] string eventGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid))
            {
                SQL.DeleteEvent(eventGuid);
                return Ok();
            }
            return Unauthorized();
        }
    }

}
