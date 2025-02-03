using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using AndroidAppServer.Libs.BinMan;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BinManParser.Api;
using Prometheus;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {

        private static readonly Counter GeopositionsCounter = Metrics
.CreateCounter("user_gps", "Частота обновления позиции", labelNames: new[] { "usr" });
        /// <summary>
        /// Token/Guid
        /// </summary>
        private static Dictionary<string,string> LogginedTokens = new Dictionary<string, string>(5);
        // GET: api/<ServiceController>
        [HttpPost()]
        public ActionResult PostPosition([FromQuery] string token,[FromBody] GeoPoint position)
        {
        //    if (!LogginedTokens.TryGetValue(token,out string UserGuid))
      //      {
                if (! Login.ValidateToken(token, out string login, out string UserGuid,IgnoreVersion:true,UseCash:true)) return Unauthorized();
            
         //   }

            // LogginedTokens.Add(token);
            try
            {
                GeopositionsCounter.WithLabels(login).Inc();
            }
            catch (Exception ex) { Log.Error(ex); }


            Log.System($"[{UserGuid}] New point La:{position.mLatitude}, Lo:{position.mLongitude}");
                //Log.Json(position);
                SQL.InsertUserTrackingPoint(position,UserGuid);
                return Ok();
        }
    
        [HttpPost("users")]
        public ActionResult GetUsers([FromQuery] string token, [FromBody] DateTime date)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetLastUsersPoint4Date(date,UserGuid);
                return Ok(res);
              //  var res = SQL.GetOnClickData(geozoneGuid);
                //var res = new List<GeoObject>(){
                //    new GeoObject() { name = "test", address = "test", position = new GeoPoint(86.138+0.0003f,55.3342+0.0003f ),client = new Client(){name="client name",inn=183_582_946_1,ogrn=183_759_274_835 } }
                //};

               // return Ok(res);
            }
            return Unauthorized();
        }        
        [HttpPost("cars")]
        public ActionResult GetCars([FromQuery] string token, [FromBody] DateTime date)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetLastCarsPoints4Day(date);
                return Ok(res);
              //  var res = SQL.GetOnClickData(geozoneGuid);
                //var res = new List<GeoObject>(){
                //    new GeoObject() { name = "test", address = "test", position = new GeoPoint(86.138+0.0003f,55.3342+0.0003f ),client = new Client(){name="client name",inn=183_582_946_1,ogrn=183_759_274_835 } }
                //};

               // return Ok(res);
            }
            return Unauthorized();
        }


        [HttpPost("vis/geo")]
        public ActionResult GetGeozoneVisits([FromQuery] string token, [FromQuery] string id_geo)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, false))
            {
                var res = SQL.GetGeozoneVisits(id_geo);
                return Ok(res);
                
            }
            return Unauthorized();
        }
        [HttpPost("vis/geo/mig")]
        public ActionResult GetMigGeozoneVisits([FromQuery] string token, [FromQuery] string id_geo)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, false))
            {
                var res = SQL.GetMigGeozoneVisits(id_geo);
                return Ok(res);

            }
            return Unauthorized();

        }
        [HttpGet("pref/coments")]
        public ActionResult GetUserCustomComments([FromQuery] string token, [FromQuery] string form)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetUserCustomComments(UserGuid,form);
                return Ok(res);

            }
            return Unauthorized();

        }
        public class CreateCustomCommentRequest
        {
            public string? guid { get; set; }
            public string form { get; set; }
            public string comment { get; set; }
        }
        [HttpPost("pref/coments/add")]
        public ActionResult AddUserCustomComments([FromQuery] string token, [FromBody] CreateCustomCommentRequest comment)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                SQL.SaveUserCustomCommentary(comment, UserGuid);
                return Ok();

            }
            return Unauthorized();

        }
        public class UpdateCustomCommentsOrderRequest
        {
            public List<string> orderGuids { get; set; }
        }
        [HttpPost("pref/coments/order")]
        public ActionResult UpdateUserCustomCommentsOrder([FromQuery] string token, [FromBody] UpdateCustomCommentsOrderRequest comment)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                SQL.UpdateOrderUserCustomCommentary(comment.orderGuids);
                return Ok();

            }
            return Unauthorized();

        }

        [HttpPost("pref/coments/remove")]
        public ActionResult RemoveUserCustomComments([FromQuery] string token, [FromQuery] string commentGuid)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                SQL.RemoveUserCustomCommentary(commentGuid);
                return Ok();

            }
            return Unauthorized();

        }

        [HttpPost("car/track")]
        public ActionResult GetCarTrack([FromQuery] string token, [FromQuery] string user, [FromBody] DateTime date)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res = SQL.GetCarTrack(user, date);
                return Ok(res);

            }
            return Unauthorized();

        }
        [HttpPost("track")]
        public ActionResult GetUserTrack([FromQuery] string token, [FromQuery] string user, [FromBody] DateTime date)
        {
            if (Login.ValidateToken(token, out string _, out string UserGuid, true))
            {
                var res =SQL.GetUserTrack(user,date);
                return Ok(res);

            }
            return Unauthorized();
           
        }
        [HttpPost("fdbck")]
        public ActionResult ReceiveFeedBack([FromQuery] string token, [FromBody] FeedBackMessage mesg)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid, true))
            {
                var msg= MailService.FormatFeedBackMessage(mesg,login,UserGuid);
                Task.Run(() => MailService.SendMail(msg,!mesg.title.ToLower().Contains("тест"))); ;
                return Ok();

            }
            return Unauthorized();
        }
        public class DebugData
        {
            public string log { get; set; }
        }
        [HttpPost("dinfo")]
        public ActionResult ReceiveDinfo([FromQuery] string token)
        {
            Log.Text("DINfO !");
            if (Login.ValidateToken(token, out string login, out string UserGuid, true))
            {

                if (!Request.TryGetFromMultipart("log", out DebugData dd)) { Log.Error("ReceiveDinfo log is null ?!"); return BadRequest("\'log\' part required"); }
               

                var log = dd.log;
               // log = log.Substring(1, log.Length - 1);
                MemoryStream mstr =  new MemoryStream();    
                Request.Form.Files.First().CopyTo(mstr);
                var bytes = mstr.ToArray();
              //  System.IO.File.WriteAllBytes(Log.AppDirrectory+"//test.png", bytes);
                Task tt =  Task.Run(()=> { DebugGatherHandler.WriteDebug(UserGuid, log,bytes); });
                return Ok();

            }
            return Unauthorized();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="docId">BinMan id</param>
        /// <returns></returns>
        [HttpPost("requestdocUpdate")]
        public ActionResult RequestUpdateByDog([FromQuery] string token, [FromQuery] string docId)
        {


            string UserGuid = "";
            if (token == Secret ||Login.ValidateToken(token, out string login, out UserGuid, true))
            {
                if (token == Secret) UserGuid = SystemUser;
                string DocGuid = Guid.NewGuid().ToString();
                var dtmp= SQL.GetDocumentBinId(docId);
                if(string.IsNullOrEmpty( dtmp)) return NotFound("Договор не найден в БД");
              
                docId = dtmp;

                SQL.DocParseRequestJ(docId, UserGuid, DocGuid);

                var Status = BinManDocumentParser.TryParseDocumentInfo(BinManApi.GetNextAccount(), docId, out var data);
                if (Status==BinManDocumentParser.DocumentParseResult.OK)
                {

                    BinManHelper.LoadFullDocumentParse(data,DocGuid); 
                    return Ok(DocGuid);
                }
                if(Status== BinManDocumentParser.DocumentParseResult.ParseError) return BadRequest("Не удалось считать информацию");
                if(Status== BinManDocumentParser.DocumentParseResult.NotFound) return NotFound("Договор не найден в BinMan");
                if (Status == BinManDocumentParser.DocumentParseResult.FatalException) return StatusCode(500,"Что-то пошло совсем не так xd");
                return StatusCode(204,"Не известный результат");
            }
            return Unauthorized();
        }



        private static bool EventsEnabled=true;
        public object AddLock = new object();
        private static Dictionary<string, List<ClientEventRequest>> Events = new Dictionary<string, List<ClientEventRequest>>();
        private static Dictionary<string, Dictionary<string, ClientEventRequest>> SendedEvents = new Dictionary<string, Dictionary<string, ClientEventRequest>>();

        [HttpPost("events")]
        public ActionResult<IEnumerable<ClientEventRequest>> ProceedEvents([FromQuery] string token, [FromBody] IClientEventResponse[] events)
        {
            if (!EventsEnabled) return NotFound();
            //Log.Json(Events);
            //Log.Json(SendedEvents);
            Login.DecodeToken(token,out var l,out _ ,out _);

            if(events !=null && events.Length > 0)
            {
                foreach(var e in events)
                {
                    lock (AddLock)
                    {
                        if (SendedEvents.TryGetValue(l,out var se))
                        {
                            if(se.TryGetValue(e.eventGuid,out var evv))
                            {
                                evv.response = e;
                                Log.Text("NormalRelize");
                                evv.Release();
                                se.Remove(e.eventGuid);
                            }
                            else
                            {
                                Log.Warning("Arrived some old event after server restart");
                                Log.Json(e);
                            }
                            
                        }
                       
                    }
                }
            }
            lock (AddLock)
            {
                if (Events.TryGetValue(l, out var ev))
                {
                    foreach (var v in ev)
                    {
                        v.eventGuid = Guid.NewGuid().ToString();
                        if (v.SendExpireTime < DateTime.Now) continue;
                        if (SendedEvents.TryGetValue(l, out var se))
                        {
                            se.Add(v.eventGuid, v);
                        }
                        else {
                            SendedEvents.Add(l, new Dictionary<string, ClientEventRequest>() { { v.eventGuid, v } });
                        }
                    }
                    var res = new object[ev.Count];
                    for (int j = 0; j < res.Length; j++) res[j] = ev[j];
                    ev.Clear();
                    return Ok(res);
                }
                else return NotFound();
            }
            
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        /// <summary>
        /// Long Wait until response arrive
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="ReceiverUserLogin"></param>
        /// <returns></returns>
        public static IClientEventResponse ProceedEvent(ClientEventRequest ev, string ReceiverUserLogin)
        {
            AddEvent(ev, ReceiverUserLogin);
           // return null;
            return ev.WaitUntilResponse();
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        protected static void AddEvent(ClientEventRequest ev,string ReceiverUserLogin)
        {
            if (Events.TryGetValue(ReceiverUserLogin, out var e))
            {
                e.Add(ev);
            }
            else
            {
                Events.Add(ReceiverUserLogin, new List<ClientEventRequest>() { ev });
            }
               
        }
    }
    

    public enum ClientEventStatus
    {
        ok,
        failed
    }
    public enum ClientEventType
    {
        simpleTextNotification
    }
    public class ClientEventRequest
    {
        public string eventGuid { get; set; }
        public ClientEventType type { get; }

        public Semaphore sem = new Semaphore(0,1);
        public IClientEventResponse response;
        public DateTime SendExpireTime;
        //public DateTime ReceiveExpireTime;
        public IClientEventResponse WaitUntilResponse()
        {
            SendExpireTime = DateTime.Now.AddMinutes(4);
            Task.Run(() => { Thread.Sleep(4 * 60*1000); if (response == null) { Release(); Log.Text("Timeout release"); } });
            Log.Text("Before WaitOne");
            sem.WaitOne();
            Log.Text("WaitOne");
            return response;
        }
        public void Release()
        {
            Log.Text("Release");
            sem.Release();
        }
    }
    public class IClientEventResponse
    {
        public ClientEventStatus status { get; set; }
        public string eventGuid { get; set; }
    }


    public class CER_SimpleTextNotification : ClientEventRequest
    {
        public CER_SimpleTextNotification(string headerText, string messagetext)
        {
            this.headerText = headerText;
            this.messagetext = messagetext;
        }
        public ClientEventType type { get => ClientEventType.simpleTextNotification; }

        public string headerText { get; set; }
        public string messagetext { get; set; }
    }





}
