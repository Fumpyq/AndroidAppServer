using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using CHGKManager.Libs;
using Microsoft.AspNetCore.Mvc;
using static CHGKManager.Libs.ActiveDirectory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        public class LoginResponse
        {
            public Token tok { get; set; }
            public string userName { get; set; }
            public bool isGpsTrackingEnabled { get; set; }

            public TimeOnly workTimeStart { get; set; }
            public TimeOnly workTimeEnd { get; set; }
            public bool isDebugEnabled { get; set; }
            
        }

        /// <summary>
        /// Валидация токена
        /// </summary>
        /// <param name="t"></param>
        /// <param name="pass">Пароль</param>
        /// <returns>
        /// Если токен верный (<see cref="ValidateToken"/>)  - <see cref="OkResult"/>
        /// <br/>
        /// Иначе - <see cref="UnauthorizedResult"/>
        /// </returns>
        /// <seealso cref="ValidateToken"/>
        [HttpGet("token/{t}")]
            public IActionResult ValidateCookieToken(string t, [FromQuery] string? ver)
            {
            if (Login.DecodeToken(t, out string login, out string pass, out string version))
            {
                if (ver != Login.CurrentVersion) { Log.Warning($"LOGIN: '{login}'  '{pass}' [token:{version}, app:{ver}] OLD VERSION"); return BadRequest(); }
                version = ver;
                if (Login.TryLogIn(login, pass, version, out Token tok, out var UserGuid, out bool isVrongVersion))
                    {
                        Log.Text($"LOGIN: '{login}'  '{pass}' - {UserGuid} [{version}] OK");
                        UserAccount Uacc = SQL.GetUserAdditionLoginData(UserGuid);

                        return Ok(new LoginResponse()
                        {
                            tok = tok,
                            userName = Uacc.nickName,
                            isGpsTrackingEnabled = Uacc.isGpsTrackingEnabled,
                            workTimeStart = Uacc.WorkTimeStart,
                            workTimeEnd = Uacc.WorkTimeEnd,
                            isDebugEnabled = Uacc.isDebugEnabled
                        });
                    }
                if (isVrongVersion) { Log.Warning($"LOGIN: '{login}'  '{pass}' [{version}] OLD VERSION"); return BadRequest(); }
                else
                {
                    Log.Text($"LOGIN: '{login}'  '{pass}' [{version}] UNAUTHORIZED");
                }


                return Unauthorized();
            }
            else
            {
                Log.Text($"LOGIN: '{login}'  '{pass}' [{version}] CAN'T DECODE TOKEN");
                return Unauthorized();
            }
              
                
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="login">Логин</param>
            /// <param name="pass">Пароль</param>
            /// <returns>
            /// Если верный логин и пароль то возвращает токен пользователя (<see cref="OkResult"/>)
            /// <br/>
            /// Иначе - <see cref="UnauthorizedResult"/>
            /// </returns>
            [HttpGet]
            public IActionResult LogIn([FromQuery] string login, [FromQuery] string? pass, [FromQuery] string version)
            {

      

                if (Login.TryLogIn(login, pass,version, out Token tok,out var UserGuid,out bool isVrongVersion))
                {
                    Log.Text($"LOGIN: '{login}'  '{pass}' - {UserGuid} [{version}] OK");
                UserAccount Uacc = SQL.GetUserAdditionLoginData(UserGuid);
                
                return Ok(new LoginResponse() { 
                    tok=tok,
                    userName=Uacc.nickName,
                    isGpsTrackingEnabled=Uacc.isGpsTrackingEnabled,                          
                    workTimeStart = Uacc.WorkTimeStart,
                    workTimeEnd = Uacc.WorkTimeEnd,
                    isDebugEnabled = Uacc.isDebugEnabled
                });
                }
                if (isVrongVersion) { Log.Warning($"LOGIN: '{login}'  '{pass}' [{version}] OLD VERSION"); return BadRequest(); }
                else
                {
                    Log.Text($"LOGIN: '{login}'  '{pass}' [{version}] UNAUTHORIZED");
                }


            return Unauthorized();
            }
        
    }
}
