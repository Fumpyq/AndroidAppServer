using ADCHGKUser4.Controllers.Libs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientEventsController : ControllerBase
    {
        private const string redirectUrl = "https://192.168.10.217:7023";

        [HttpGet("notif")]
        public ActionResult SimpleNotificationEvent([FromQuery] string UserLogin, [FromQuery] string header, [FromQuery] string descr
#if DEBUG
            , [FromQuery] bool Redirect = false
#endif
            )
        {
#if DEBUG


            if (Redirect)
            {
                //Log.Text("Redirect: "+redirectUrl+ Request.Path.Value+ Uri.UnescapeDataString(Request.QueryString.Value));
                var url = redirectUrl + Request.Path.Value + Uri.UnescapeDataString(Request.QueryString.Value);
                Log.Text(url);
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                // Pass the handler to httpclient(from you are calling api)

                using (var client = new HttpClient(clientHandler))
                {
                    client.Timeout = new TimeSpan(0, 4, 0);
                    var req = new HttpRequestMessage(HttpMethod.Get, url);
                    req.Headers.Add("scrtacasd", "Su$352fkagiss8vjjancuushryfavi38jgd-sdwoqr");
                    var res = client.Send(req);

                    var t = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Log.Text(t);
                    return Ok(t);

                }
            }
            else
            {
#endif
#if !DEBUG
                if (!Request.Headers.TryGetValue("scrtacasd", out var res)) return NotFound();
                    else if (res.Count() == 1)
                    {
                        if (res.First() != "Su$352fkagiss8vjjancuushryfavi38jgd-sdwoqr") return NotFound();
                    }
                    else return NotFound();
#endif


                Log.Text("yay");
                CER_SimpleTextNotification notif = new CER_SimpleTextNotification(header, descr);

                var clientres = ServiceController.ProceedEvent(notif, UserLogin);

                return Ok(clientres);
#if DEBUG
            }
#endif

        }
    }
}
