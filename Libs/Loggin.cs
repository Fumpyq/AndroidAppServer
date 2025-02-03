using ADCHGKUser4.Controllers.Libs;
using static CHGKManager.Libs.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using CHGKManager.Libs;
using System.Diagnostics;
using static AndroidAppServer.Libs.Login;
using System.Data;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AndroidAppServer.Libs
{
    public class Login
    {
        public class UserCash
        {
            public string DBGuid;
            public string login;
        }
        public struct LoginTokenInfo {
            string login;
            string pass;
            string version;

            public LoginTokenInfo(string login, string pass, string version)
            {
                this.login = login;
                this.pass = pass;
                this.version = version;
            }
        }

        private static ConcurrentDictionary<string, UserCash> UsersCash = new ConcurrentDictionary<string, UserCash>();

        const string Separator = "!?2@\\";
        public static bool TryLogIn(string login, string? pass, string version, out Token token, out string UserGuid, out bool isVrongVersion)
        {
            
            bool AD = (ActiveDirectory.IsValid(login, pass, 3));
            if (SQL.CheckLogAPass(login, pass, AD, out UserGuid))
            {
                if (version == CurrentVersion) isVrongVersion = false;
                else
                {
                    token = new Token();
                    isVrongVersion = true;
                    return false;
                }
                var roles = SQL.GetUserRoles(UserGuid);

                token = new Token(login + Separator + pass + Separator + CurrentVersion, roles);

                return true;
            }

            else
            {
                isVrongVersion = (version != CurrentVersion);
                token = new Token();
                return false;
            }
        }
        public static bool DecodeToken(string t, out string login, out string pass, out string version)
        {
            login = "";
            pass = "";
            version = "";

            string[] decod = /*decode(t)*/t.Split(Separator);

            if (decod.Length == 2) { Log.Warning("Старая версия приложения (не отправлена версия)"); return false; }
            Log.Text("Проверка токена: " + (decod.Length > 0 ? decod[0] : "Не указан или неверный формат токена") + " | " + $"'{(decod.Length > 1 ? decod[1] : "Не указан или неверный формат токена")}'");

            if (decod.Length > 2)
            {

                login = decod[0];
                pass = decod[1];
                version = decod[2];
                if (version != CurrentVersion) { Log.Warning($"Старая версия: {login} [{decod[2]}]"); return true; }

            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="Required">Роли которые могут пройти проверку токена</param>
        /// <returns>
        /// Верный ли токем И Имеет ли пользователь с этим токеном хотя бы одну роль из Required
        /// </returns>
        public static bool ValidateToken(string t, params User_Roles[] Required)
        {
            bool Istoken = ValidateToken(t, out _, out Role[] roles, out _, out _);
            if (Istoken)
            {
                return roles.ContainAnyRole(Required);
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="Required">Роли которые могут пройти проверку токена</param>
        /// <returns>
        /// Верный ли токем И Имеет ли пользователь с этим токеном хотя бы одну роль из Required
        /// </returns>
        public static bool ValidateToken(string t, out string UserGuid, params User_Roles[] Required)
        {
            bool Istoken = ValidateToken(t, out _, out Role[] roles, out UserGuid, out _);
            if (Istoken)
            {
                return roles.ContainAnyRole(Required);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="Required">Роли которые могут пройти проверку токена</param>
        /// <returns>
        /// Верный ли токем И Имеет ли пользователь с этим токеном хотя бы одну роль из Required
        /// </returns>
        public static bool ValidateToken(string t, out string UserGuid, params string[] Required)
        {
            bool Istoken = ValidateToken(t, out _, out Role[] roles, out UserGuid, out _);
            if (Istoken)
            {
                return roles.ContainAnyRole(Required);
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="Required">Роли которые могут пройти проверку токена</param>
        /// <returns>
        /// Верный ли токем И Имеет ли пользователь с этим токеном хотя бы одну роль из Required
        /// </returns>
        public static bool ValidateToken(string t, out string UserGuid, params int[] Required)
        {
            bool Istoken = ValidateToken(t, out _, out Role[] roles, out UserGuid, out _);
            if (Istoken)
            {
                return roles.ContainAnyRole(Required);
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="Required">Роли которые могут пройти проверку токена</param>
        /// <returns>
        /// Верный ли токем И Имеет ли пользователь с этим токеном хотя бы одну роль из Required
        /// </returns>
        public static bool ValidateToken(string t, out string UserGuid, params Role[] Required)
        {
            bool Istoken = ValidateToken(t, out _, out Role[] roles, out UserGuid, out _);
            if (Istoken)
            {
                return roles.ContainAnyRole(Required);
            }
            return false;
        }
        public static bool ValidateToken(string t, out string login)
        {
            return ValidateToken(t, out login, out _, out _, out _);

        }
        public static bool ValidateToken(string t, out string login, out string UserGuid, bool IgnoreVersion = false)
        {
            return ValidateToken(t, out login, out _, out UserGuid, out _, IgnoreVersion: IgnoreVersion);

        }
        public static bool ValidateToken(string t, out string login, out bool isVrongVersion)
        {
            return ValidateToken(t, out login, out _, out _, out isVrongVersion);

        }
        /// <summary>
        /// UseCash - проверка без участия бд, если пользователь есть в кеше
        /// </summary>
        public static bool ValidateToken(string t, out string login, bool UseCash)
        {
            return ValidateToken(t, out login, out _, out _, out _, UseCash);

        }
        /// <summary>
        /// UseCash - проверка без участия бд, если пользователь есть в кеше
        /// </summary>
        public static bool ValidateToken(string t, out string login, out string UserGuid, bool UseCash, bool IgnoreVersion = false)
        {
            return ValidateToken(t, out login, out _, out UserGuid, out _, UseCash, IgnoreVersion);

        }
        /// <summary>
        /// UseCash - проверка без участия бд, если пользователь есть в кеше
        /// </summary>
        public static bool ValidateToken(string t, out string login, out bool isVrongVersion, bool UseCash)
        {
            return ValidateToken(t, out login, out _, out _, out isVrongVersion, UseCash);

        }
        public static bool ValidateToken(string t, out string login, out string UserGuid, out bool isVrongVersion)
        {
            return ValidateToken(t, out login, out _, out UserGuid, out isVrongVersion);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="Required">Роли которые могут пройти проверку токена</param>
        /// <returns>
        /// Верный ли токем И Имеет ли пользователь с этим токеном хотя бы одну роль из Required
        /// </returns>
        public static bool ValidateToken(string t, out string login, out string UserGuid, params Role[] Required)
        {
            bool Istoken = ValidateToken(t, out login, out Role[] roles, out UserGuid, out _);
            if (Istoken)
            {
                return roles.ContainAnyRole(Required);
            }
            return false;
        }
        /// <summary>
        /// Валидация токена
        /// (отброшены все out, если не нужны)
        /// </summary>
        /// <param name="t">Токен</param>
        /// <returns></returns>
        public static bool ValidateToken(string t)
        {
            return ValidateToken(t, out _, out _, out _, out _);
        }
        public static bool ValidateToken(string t, bool UseCash)
        {
            return ValidateToken(t, out _, out _, out _, out _, UseCash);
        }
        public static bool ValidateToken(string t, out bool isVrongVersion, bool IgnoreVersion = false)
        {
            return ValidateToken(t, out _, out _, out _, out isVrongVersion, IgnoreVersion: IgnoreVersion);
        }

        /// <summary>
        /// Основной метод валидации токена,
        /// проверяет валидность токена, логин пароль владельца токена в AD
        /// UseCash - Отключает роли
        /// </summary>
        /// <param name="t">Токен</param>
        /// <param name="login">Если токен верный, то логин пользователя, которому принадлежит токен<br/> Иначе <see cref="string.Empty"/></param>
        /// <param name="roles">Если токен верный, то роли пользователя, которому принадлежит токен<br/> Иначе <c>new Role[0]</c> </param>
        /// <returns>Валидный ли токен</returns>
        /// <seealso cref="ValidateToken"/>
        /// <seealso cref=" ValidateAdminToken"/>
        /// <seealso cref=" ValidateDivisionToken"/>
        ///
        public static bool ValidateToken(string t, out string login, out Role[] roles, out string UserGuid, out bool isVrongVersion, bool UseCash = false, bool IgnoreVersion = false)
        {
            isVrongVersion = false;
            string[] decod = /*decode(t)*/t.Split(Separator);

            if (decod.Length == 2) { Log.Warning("Старая версия приложения (не отправлена версия)" + (IgnoreVersion ? "Инор версии включен, токен валиден" : "")); }
            Log.Text("Проверка токена: " + (decod.Length > 0 ? decod[0] : "Не указан или неверный формат токена") + " | " + $"'{(decod.Length > 1 ? decod[1] : "Не указан или неверный формат токена")}'");

            if (decod.Length > (IgnoreVersion ? 1 : 2))
            {

                login = decod[0];
                string pass = decod[1];
                //  Stopwatch sw = Stopwatch.StartNew();
                bool Valid = false;
                if (UseCash)
                {
                    if (UsersCash.TryGetValue(t, out var cash))
                    {
                        UserGuid = cash.DBGuid;
                        login = cash.login;
                        roles = new Role[0];
                        Valid = true;
                    }
                    else
                    {
                        bool AD = false;
                        if (ActiveDirectory.IsValid(login, pass, 3)) AD = true;

                        Valid = SQL.CheckLogAPass(login, pass,AD, out UserGuid);
                        if (Valid)
                        {
                            UsersCash.TryAdd(t, new UserCash() { login = login, DBGuid = UserGuid });
                        }
                    }
                }
                else
                {
                    bool AD = false;
                    if (ActiveDirectory.IsValid(login, pass, 3)) AD = true;
                  //  if (AD == false)
                 //   {
                        if (UsersCash.TryGetValue(t, out var cash))
                        {
                            UserGuid = cash.DBGuid;
                            login = cash.login;
                            roles = new Role[0];
                            Valid = true;
                            goto ggg;
                        }
                  //  }
                    Valid = SQL.CheckLogAPass(login, pass,AD, out UserGuid);
                    ggg:
                    if (Valid&& !UsersCash.ContainsKey(t))
                    {
                        UsersCash.TryAdd(t, new UserCash() { login = login, DBGuid = UserGuid });
                    }
                }
                //  sw.Stop();
                //  Log.Text($"Token Check: {sw.Elapsed.TotalMilliseconds} ms");
                if (!IgnoreVersion)
                    if (decod[2] != CurrentVersion) { isVrongVersion = true; roles = new Role[0]; Log.Warning($"Старая версия: {login} [{decod[2]}]"); return false; }
                    else { isVrongVersion = true; }
                if (Valid)
                {
                    Log.Text("Success enter by token: " + t);
                    roles = new Role[0];

                    return true;

                }
            }
            Log.Message("bad try to enter by token: " + t);
            login = string.Empty;
            roles = new Role[0];
            UserGuid = string.Empty;
            isVrongVersion = false;
            return false;
        }
        public static string CurrentVersion = "0.17.4.1";
    }
}


