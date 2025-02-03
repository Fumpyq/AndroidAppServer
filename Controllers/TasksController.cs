
using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static AndroidAppServer.Controllers.GeozonesController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        // GET: api/<TasksController>
        [HttpGet]
        public ActionResult GetUserTasks([FromQuery] string token)
        {
            if (Login.ValidateToken(token,out _,out string UserGuid))
            {

                var res = SQL.GetUserTasks(UserGuid);
                Log.Json(res);
                return Ok(res);
            }
            return Unauthorized();
        }        // GET: api/<TasksController>
        [HttpGet("statuses")]
        public ActionResult GetAllTasksStatuses([FromQuery] string token)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {

                var res = SQL.GetPossibleTaskStatuses();
                //  Log.Json(res);
                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpGet("geo")]
        public ActionResult GetGeozoneTasks([FromQuery] string token, [FromQuery] string id_geo, [FromQuery] bool? withHistory)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {

                var res = SQL.GetSourceTasks_Deprecated(id_geo, UserGuid, withHistory,"Geozone");
              //  Log.Json(res);
                return Ok(res);
            }
            return Unauthorized();
        }
        public enum TaskSourceType
        {
            Object, Geozone, Division, User
        }
        public class TaskFilters
        {
            public DateTime? planDateFrom { get; set; }
            public DateTime? planDateTo { get; set; }
            public bool onlyNullDatePlan { get; set; }
            public List<string> statusGuids { get; set; }
        }
        [HttpPost("any")]
        public ActionResult GetSourceTasks([FromQuery] string token,[FromQuery] string? sourceGuid, [FromQuery] string sourceType, [FromBody] TaskFilters filters,string? filterPreset)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {

                var res = SQL.GetSourceTasks(sourceGuid, UserGuid, filters, sourceType,filterPreset);
                //  Log.Json(res);
                return Ok(res);
            }
            return Unauthorized();
        }
        public class SetTaskExecutorRequest
        {
            public string executorGuid { get; set; }
            public List<string> taskGuid { get; set; }
            public DateTime? datePlan { get; set; }
        }
        [HttpPost("setExecutor")]
        public ActionResult SetTaskExecutor([FromQuery] string token, [FromBody] SetTaskExecutorRequest req)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                
                SQL.SetTaskExecutor(UserGuid,req);
                //  Log.Json(res);
                return Ok();
            }
            return Unauthorized();
        }
        // GET: api/<TasksController>
        [HttpPost] //Вот это - выполнение задачи
        public ActionResult Cleaning([FromQuery] string token, [FromQuery] string geozoneGuid, [FromQuery] int type, [FromQuery] string taskGuid)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                if (!Request.Form.TryGetValue("userPosition", out var res)) return BadRequest("\'userPosition\' part required");

                GeoPoint gp = JsonConvert.DeserializeObject<GeoPoint>(res[0]);
#if !SQL_READ_ONLY
                SQL.RegisterClearEvents(type,UserGuid,gp,geozoneGuid,taskGuid,Request.Form.Files);
#else
                Log.System("SQL_READ_ONLY SQL.RegisterClearEvents Prevented");
#endif

                //  Log.Json(res);
                return Ok();
            }
            return Unauthorized();
        }
        [HttpPost("multievent")]
        public ActionResult FinishMultiEventTask([FromQuery] string token, [FromQuery] string taskGuid, [FromQuery] string geozoneGuid)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                if (!Request.TryGetFromMultipart<GeoPoint>("userPosition", out var UserPosition)) return BadRequest("\'userPosition\' part required");
                if (!Request.TryGetFromMultipart<UniversalEvent[]>("events", out var Events)) return BadRequest("\'events\' part required");
                if (!Request.TryGetFromMultipart<int[]>("imagesMap", out var EventImagesMap)) return BadRequest("\'imagesMap\' part required");

                IFormFileCollection allFiles = Request.Form.Files;

                IFormFile[] files =  allFiles.ToArray();

                int j = 0; 
                int i= 0;
                foreach (var v in Events)
                {
                    string GUid  = SQL.InsertNewGeozoneEvents(geozoneGuid, UserGuid, v, UserPosition,null);
                    if(files.Length>0)
                    SQL.ParseFilesToSomething(GUid, new Span<IFormFile>(files, j, EventImagesMap[i]).ToArray(),UserGuid);
                    
                    j += EventImagesMap[i];
                    i++;
                }

                SQL.FinishAnyTask(taskGuid, UserGuid);


                return Ok();
            }
            return Unauthorized();
        }


    }
    public static class RequestExt
    {
        public static bool TryGetFromMultipart<T>(this HttpRequest req, string name, out T res)
        {
            if (!req.Form.TryGetValue(name, out var r))
            {
                res = default(T);
                return false;
            }
            else
            {
                res = JsonConvert.DeserializeObject<T>(r);
                return true;
            }
        }
    }
}
