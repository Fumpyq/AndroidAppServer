using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using Microsoft.AspNetCore.Mvc;
using static CHGKManager.Libs.ActiveDirectory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        // GET: api/<FilesController>


        // GET api/<FilesController>/5
        [HttpGet("file")]
        public ActionResult GetFileData([FromQuery] string token,[FromQuery] string fileGuid, [FromQuery] FileQuality? Qul)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                Log.Text($"FileRequest ({fileGuid})");
                if(!SQL.TryGetFileData(fileGuid,out var f,Qul))
                {
                    return this.NoContent();
                }
                else
                return File(f.data,f.FileType);
            }



            return Unauthorized();
        }
        [HttpGet("anyfile")]
        public ActionResult GetAnyFileData([FromQuery] string token, [FromQuery] string fileGuid)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                Log.Text($"FileRequest ({fileGuid})");
                if (!SQL.TryGetRawFileData(fileGuid, out var f))
                {
                    return this.NoContent();
                }
                else
                    return File(f.data, f.FileType);
            }
            return Unauthorized();
        }
        [HttpPost("add")]
        public ActionResult UploadFiles([FromQuery] string token, [FromQuery] string guid)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                
                SQL.ParseFilesToSomething(guid,Request.Form.Files,UserGuid,$"Просто доп. фото {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}", $"Просто доп. фото {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")}");
                return Ok();
            }
            return Unauthorized();
        }
        [HttpDelete("delete")]
        public ActionResult DeleteFile([FromQuery] string token, [FromQuery] string guid)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {

                SQL.DeleteFile(guid);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpGet("info")]
        public ActionResult GetFileInfo([FromQuery] string token, [FromQuery] string guid)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                var res =SQL.GetSomethingFilesInfo(guid,UserGuid);
                return Ok(res);
            }
            return Unauthorized();
        }
        [HttpGet("infoExt")]
        public ActionResult GetExtendedFileInfo([FromQuery] string token, [FromQuery] string guid)
        {
            if (Login.ValidateToken(token, out string login, out string UserGuid))
            {
                var res = SQL.GetSomethingFilesInfo_Ext(guid, UserGuid);
                return Ok(res);
            }
            return Unauthorized();
        }
    }
}
