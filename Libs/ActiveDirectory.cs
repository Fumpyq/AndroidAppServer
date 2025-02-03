using ADCHGKUser4.Controllers;
using ADCHGKUser4.Controllers.Libs;

using System.DirectoryServices.AccountManagement;

namespace CHGKManager.Libs
{    /// <summary>
     /// Любой запрос проверяющий токен возвращает <see cref="UnauthorizedResult"/>, в случае не валидности токена или недостатка ролей у владельца токена
     /// </summary>
    public enum User_Roles
    {
        Refused = -1,
        deffault = 0,
        TestBdAccess = 1,
        MainDBAccess = 2,
        Admin = 666,
    }
    public class ActiveDirectory
    {
        private static PrincipalContext pc;
        private static IConfiguration _conf;
        private static string AdDomain;
        private static string AdLogin;
        private static string AdPassword;
        private static object pcLock = new object();

        public ActiveDirectory(IConfiguration configuration)
        {
            _conf = configuration;
            AdDomain = configuration.GetValue<string>("AD_Domain");
            if (string.IsNullOrEmpty(AdDomain))
            {
                Log.Warning("В конфигурации не указан/неверный AD_Domain -> Используется стандартный \"CHGK\"");
                AdDomain = "CHGK";
            }
            AdLogin = configuration.GetValue<string>("AD_Login");
            if (string.IsNullOrEmpty(AdLogin))
            {
                Log.Warning("В конфигурации не указан/не правилный формат AD_Login -> вход по логину/паролю не используется");
                AdLogin = string.Empty;
            }
            AdPassword = configuration.GetValue<string>("AD_Password");
            if (string.IsNullOrEmpty(AdPassword))
            {
                Log.Warning("В конфигурации не указан/не правилный формат AD_Password -> вход по логину/паролю не используется");
                AdPassword = string.Empty;
            }

            ReconectAD();
        }
        public static void ReconectAD()
        {
            bool Connected = true;
            bool Succes = false;
            int limit = 5;
            while (!Succes)
            {
                try
                {
                    if (limit <= 0)
                    {
                        Succes = true;
                        Connected = false;
                    }
                    pc = AdLogin != string.Empty ? new PrincipalContext(ContextType.Domain, AdDomain, AdLogin, AdPassword) :
                       new PrincipalContext(ContextType.Domain, AdDomain)
                       ;
                    Succes = true;

                }
                catch (Exception e)
                {
                    Log.Error("Retry After 200ms");
                    Thread.Sleep(200);

                    limit--;
                }

            }
            if (!Connected)
            {
                Log.Error("Can't connect to AD after 10 try limit");
            }

        }
        /// <summary>
        /// Проверка логина/пароля в AD
        /// </summary>
        /// <param name="login"></param>
        /// <param name="pass"></param>
        /// <returns>Прошла ли проверку комбинация логина/пароля в AD</returns>
        /// <remarks>
        /// В случае неудачного подключения к AD:
        /// Ожидание 200 ms, повторная попытка подключения, Лимит попыток переподключения - 100
        /// </remarks>
        public static bool IsValid(string login, string pass, int TryCount = 3)
        {
            bool Valid = false;
            bool Succes = false;
            int limit = TryCount;
          
            while (!Succes)
            {
                lock (pcLock)
                {
                    try
                    {
                        if (limit <= 0) { Succes = true; Log.Warning("AD login Failed"); }
                   
                        Valid = pc.ValidateCredentials(login, pass);
                   
                        Succes = true;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            pc = AdLogin != string.Empty ? new PrincipalContext(ContextType.Domain, AdDomain, AdLogin, AdPassword) :
                              new PrincipalContext(ContextType.Domain, AdDomain);
                        }
                        catch (Exception ex) { Log.Error(ex); return false; }


                        Log.Error("AD Failed to connect, Retry After 200ms with ex: " + e.Message);
                        Thread.Sleep(50);
                        limit--;
                        ReconectAD();
                    }
                }
            }
            if(Valid) Log.Text("[AD] Validated: " + $"'{login}' - OK");
            return Valid;
        }


        public record struct Role(string name, int id);
        /// <summary>
        /// Структура для отправки на фронт
        /// </summary>
        /// <param name="token">строка - токен</param>
        /// <param name="roles">роли принадлежащие пользователю с этим токеном</param>
        public record struct Token(string token, Role[] roles);
        public record struct TokenId(string token, Role[] roles,int Id) ;

        private static bool ADIsValid(string login, string pass)
        {
            bool Valid = false;
            bool Succes = false;
            int limit = 100;
            while (!Succes)
            {
                try
                {
                    if (limit <= 0) Succes = true;
                    Log.Text("[AD] Validation: " + $"'{login}'");
                    Valid = pc.ValidateCredentials(login, pass);
                    Succes = true;
                }
                catch (Exception e)
                {
                    Log.Error("AD Failed to connect, Retry After 200ms with ex: " + e.Message);
                    Thread.Sleep(200);
                    limit--;
                }
            }

            return Valid;
        }
    }
}

