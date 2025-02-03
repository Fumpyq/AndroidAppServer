using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using AndroidAppServer.Libs.BinMan;
using AndroidAppServer.Libs.BinMan.PageParsers;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using Org.BouncyCastle.Asn1.Ocsp;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BinManParser.Api
{
    public enum BinManTaskType
    {
        archive,
        delete,
        update,
        insert,
    }
    public class BinManDocWorker : IJob
    {
        public bool IsExecuting;
        public async Task Execute(IJobExecutionContext context)
        {
            if (IsExecuting)
            {
                return;
            }
            IsExecuting = true;
            var ld = BinManApi.GetNextAccount().AsParralel();

            
            var ress = BinManDocumentParser.ParseAllDocIdsInPeriod(ld, DateTime.Now, DateTime.Now);
            var ress2 = ress;
            ProfilerInstance st = new ProfilerInstance();
            List<Task> tskts = new List<Task>();
            foreach (var v in ress2)
            {
                var tts = Task.Run(() =>
                {
                    st.StartTimer("Документ №" + v);
                    string DocGuid = Guid.NewGuid().ToString();
                    var Status = BinManDocumentParser.TryParseDocumentInfo(ld, v, out var data, 400);
                    if (Status == BinManDocumentParser.DocumentParseResult.OK)
                    {
                        Log.Text("Успешно Документ №" + v);
                        Log.Json(data);
                        BinManHelper.LoadFullDocumentParse(data, DocGuid);
                    }
                    if (Status == BinManDocumentParser.DocumentParseResult.ParseError) Log.Text("Не удалось считать информацию");
                    if (Status == BinManDocumentParser.DocumentParseResult.NotFound) Log.Text("Договор не найден в BinMan");
                    if (Status == BinManDocumentParser.DocumentParseResult.FatalException) Log.Text("Что-то пошло совсем не так xd");
                    st.StopTimer("Документ №" + v);
                });
                Thread.Sleep(1150);
                tskts.Add(tts);
            }
            
           await Task.Run(()=> { Task.WaitAll(tskts.ToArray()); });
            st.LogResult();
            IsExecuting = false;
        }
    }
    public static class API
    {
        public const string CompanyId = "111275";
        public const string BaseUrl = "https://binman.ru/";
        public const string SavePath = "\\\\192.168.11.163\\office smb\\binman_doc";

        public const string Url_Login = BaseUrl + "cabinet/?login=yes"; // POST

        public static string DeffaultCokie(LoginData login) => $"" +
            // $" BITRIX_SM_SOUND_LOGIN_PLAYED=Y;" +
            // $" BITRIX_SM_TIME_ZONE=-420;" +
            // $" BITRIX_SM_LOGIN={login.BITRIX_SM_LOGIN};" +
            // $" BX_USER_ID=6f583348813445e795f00b64e9d945bc" +
            $" PHPSESSID={login.PHPSESSID}";
        public static bool  GetSessIdFrom(LoginData ld, string url, out string sessId, HttpMethod meth = null, KeyValuePair<string, string>[] Headers = null)
        {
            if (meth == null) meth = HttpMethod.Post;
            sessId = string.Empty;
            Log.Text($"GettingSessIdFor: {url} , {ld.PHPSESSID},{ld.BITRIX_SM_LOGIN}");
            HttpRequestMessage hm = new(meth, url);

            //var data = GetEditFormData(
            //    //ld.PHPSESSID
            //    "e7ea4754e26971c103c8318702e00a51"
            //    );

            //Log.Json(data);


            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm,
                Headers,
                cookie,ld,true
                );
            //Log.Json(req);
            var t = req.Content.ReadAsStringAsync();

            t.Wait();

            var html = t.Result;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            
            //sessid
            try
            {
                var SesIdParse = htmlDoc.DocumentNode.Descendants("input");
                // int l = SesIdParse.Count();

                SesIdParse = SesIdParse.Where(d => d != null && d.Attributes["name"] != null && d.Attributes["name"].Value != null && d.Attributes["name"].Value.Length > 0 &&
                d.Attributes["name"].Value.Contains("sessid"));
                sessId = SesIdParse.First().Attributes["value"].Value.Trim();
                Log.Text(sessId);
                return true;
            }
            catch (Exception e)
            {
                Log.Warning(html.Substring(0,Math.Min(html.Length,128)) + ((html.Length>128)?"...":""));
                Log.Error(e);
                return false;
            }
            return false;
            // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test2.html");


        }
        public static CookieContainer GetDeffaultCookie(LoginData login, string superGuid)
        {
            var cookie = new CookieContainer();
            cookie.Add(new Uri("https://binman.ru"), new Cookie("BITRIX_SM_SOUND_LOGIN_PLAYED", "Y", "/", "binman.ru"));
            cookie.Add(new Uri("https://binman.ru"), new Cookie("BITRIX_SM_TIME_ZONE", "-420", "/", "binman.ru"));

            cookie.Add(new Uri("https://binman.ru"), new Cookie("BITRIX_SM_LOGIN", login.BITRIX_SM_LOGIN, "/", "binman.ru"));
            cookie.Add(new Uri("https://binman.ru"), new Cookie("PHPSESSID", login.PHPSESSID, "/", "binman.ru"));
            cookie.Add(new Uri("https://binman.ru"), new Cookie("BX_USER_ID", "d6e4f02ed65459f51af72b38ee118942", "/", "binman.ru"));

            // _ym_uid = 1684211647817284461; _ym_d = 1684211647; _ym_isad = 2; _ym_visorc = w;

            return cookie;
        }



        //public const int accid = 5710334;
        private static HttpRequestMessage CloneRequest(this HttpRequestMessage req)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);
            clone.Content = req.Content;
            // clone.Version = req.Version;

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }
        public static HttpResponseMessage SendRequest(HttpRequestMessage req, KeyValuePair<string, string>[] headers, CookieContainer cookies,LoginData ld=null,bool skipLogin=false, int secTimeOut = 32)
        {
            //Log.Text("before");
           // Log.Json(ld);
            if (ld !=null &&!skipLogin
                //&&!ld.IsAuthorized
                )
            {
                API.Login(ld.Login, ld.Password, ld);
                cookies = GetDeffaultCookie(ld,null);
            }
           // Log.Text("after");
            //Log.Json(ld);
            HttpResponseMessage resp = SendREq(req, headers, cookies, secTimeOut);
            
          
            // Log.Warning($"Запрос в BinMan {req.RequestUri}");
            //Log.Warning("Запрос");
            //Log.Text(req.RequestUri.ToString());
            //Log.Json(client);
            // Log.Text(" - - - - -- - - - - - - - ");
            // Log.Json(req);



            var text = resp.Content.ReadAsStringAsync();

            text.Wait();
            var content = text.Result;
            if (content.Contains("form class=\"authorization-form\" name=\"form_auth\""))
            {
                try
                {
                    Log.Text("Request was failed caz token outdate");
                    if (ld != null) { API.Login(ld.Login, ld.Password, ld); cookies = GetDeffaultCookie(ld, null); }
                    else { BinManApi.LogInAccounts().Wait(); }
                    Thread.Sleep(2000);
                    resp = SendREq(CloneRequest(req), headers, cookies,secTimeOut);
                    Log.Text("Request was resended");
                }
                catch (Exception ex) { Log.Error(ex); }
            }

            //
            return resp;

            static HttpResponseMessage SendREq(HttpRequestMessage req, KeyValuePair<string, string>[] headers, CookieContainer cookies , int secTimeOut=60)
            {
                try
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    if(cookies!=null)
                    handler.CookieContainer = cookies;
                   // handler.MaxAutomaticRedirections = 2;
                   // handler.AllowAutoRedirect = true;
                    HttpClient client = new HttpClient(handler);
                    client.Timeout = new TimeSpan(00, 00, secTimeOut);
                    if (headers != null)
                        foreach (var v in headers)
                            client.DefaultRequestHeaders.Add(v.Key, v.Value);

                    var resp = client.Send(req);
                    return resp;
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    // Handle timeout.
                    Log.Error($"Time Out {secTimeOut} sec. {req.ToString()}");
                    throw (ex);
                }
              
            }
        }



        public static LoginData? Login(string login, string pass,LoginData ld= null)
        {
            Dictionary<string, string> d = new Dictionary<string, string>()
        {
            { "AUTH_FORM","Y"},
            { "TYPE","AUTH"},
            { "backurl", "/cabinet/"},
            { "USER_LOGIN",login},
            { "USER_PASSWORD",pass},
        };




            HttpRequestMessage hm = new(HttpMethod.Post, Url_Login);

            hm.Content = new FormUrlEncodedContent(d);
            //Log.Text("@ - - - - -- - - - - - - - ");
            //Log.Text(await hm.Content.ReadAsStringAsync());
            //Log.Text("@ - - - - -- - - - - - - - ");
            Log.System("Попытка залогиниться");
            var cookie = new CookieContainer();
            var req = SendRequest(hm,
                //new KeyValuePair<string, string>[]
                //{
                //   new KeyValuePair<string, string>()
                //}
                null,
                cookie
                );

            var cc = cookie.GetAllCookies();
            string? sesId = cc["PHPSESSID"]?.Value;
            string? bit_sm_log = cc["BITRIX_SM_LOGIN"]?.Value;
            string? bit_sm_us_id = cc["BX_USER_ID"]?.Value;

            if (string.IsNullOrEmpty(sesId))
            {
                Log.Error("запрос не вернул PHPSESSID в куки");
                return null;
            }
            if (string.IsNullOrEmpty(bit_sm_log))
            {
                Log.Error("запрос не вернул BITRIX_SM_LOGIN в куки");
                return null;
            }



            if (ld == null)
            {
                ld = new LoginData(bit_sm_log, sesId);
            }
            else
            {
                ld.BITRIX_SM_LOGIN=bit_sm_log;
                ld.PHPSESSID = sesId;
            }
            ld.Login = login;
            ld.Password = pass;
            ld.IsAuthorized = true;
            Log.System("Логин успешен!");
            Log.Json(ld);
            return ld;
        }




        static bool Firste = false;
        public static string TrimText(string txt)
        {
            return Regex.Replace(txt, @"\s+", " ").Replace("пнвтсрчтптсбвс", "").Replace("\n\n", "").Trim();
        }
    }
    public static class BinManApi
    {

        public static  LoginPassword[] Accounts = new LoginPassword[] {
            new LoginPassword(log,pass),


};
        public static LoginData[] LoginTikens = new LoginData[Accounts.Length];
        static object laLock = new object();
        static long LastAccount = 0;
        public static void Init()
        {
            if (!IsApiEnabled) return;
            //await LogInAccounts();
#if !DEBUG || true
            Task.Run(() => RunBinManSyncService());
#endif
            Log.Text("BinMan Api Initialized", force: true);
           // StartDocumentParserWorker();






            // BinManGeozone g = new BinManGeozone();
            //  g.LAST_AREA = 5758859;
            //  if(g.AttachToObject(GetNextAccount(),new List<int>() { 5757727 }, 5758854))Log.System("Succes ?");

            //var cd = new ClientData();
            //cd.F_NAME = "ТЕСТ 2";
            //cd.F_SURNAME = "ТЕСТ 2";
            //cd.F_PATRONYMIC = "ТЕСТ 2";
            //cd.UR_NAME = "ТЕСТ тест Тест";
            //cd.UR_FULLNAME = "Тестовик тестовый";
            //cd.INN = "7716652352";
            //cd.ID = 5767676;
            //cd.F_REGION = "Алтайский край";
            //cd.EMAIL = new string[] { "test@test.tste" };
            //cd.TYPE = ClientType.MANAGEMENT_COMPANY;

            //cd.SendCreateRequest(GetNextAccount(), out string BinId);
            // cd.SendEditRequest(GetNextAccount());

            //BinManObject b = new BinManObject();
            //b.REGION = "Кемеровская область - Кузбасс";
            //b.CITY = "г Кемерово";
            //b.STREET = "ул Пушкина";
            //b.ADDRESS = "Кемеровская область - Кузбасс, г Кемерово, ул Пушкина";
            //b.LAT = 55.358542f;
            //b.LON = 86.08729f;
            //b.lot_id = Geo_Lot.lot_2;
            //b.CATEGORY = BinManObject.ObjectMainCattegory.Trade;
            //b.SUBCATTEGORY = BinManObject.ObjectSubCattegory.Supermarket;
            //b.NAME = "Продовольственный магазин, г Кемерово, ул Пушкина, TEST";
            //b.SendCreateRequest(GetNextAccount(), out var res);
            //Log.Warning(res.ToString());

        }
        public static void StartDocumentParserWorker()
        {
            ISchedulerFactory schedFact1 = new StdSchedulerFactory();
            // get a scheduler, start the schedular before triggers or anything else
            IScheduler sched1 = schedFact1.GetScheduler().GetAwaiter().GetResult();
            sched1.Start();
            IJobDetail job1 = JobBuilder.Create<BinManDocWorker>()
             .WithIdentity("BinManDocWorker")
             .Build();

            ITrigger t1 = TriggerBuilder.Create()
                    .WithIdentity("DayliCompleteEveryHour")
                    .ForJob("BinManDocWorker")
                    .StartAt(DateBuilder.EvenHourDate(null)) // get the next even-hour (minutes and seconds zero ("00:00"))
.WithSimpleSchedule(x => x
    .WithIntervalInHours(1)
    .RepeatForever())
                    .Build();
            sched1.ScheduleJob(job1, t1);
        }

        public static string CutIdFromUrl(HtmlNode node, int offest = 0)
        {
            return CutIdFromUrl(node.Attributes["href"].Value, offest);
        }
        public static string CutIdFromUrl(string url, int offest = 0)
        {
            return url.Split("/")[^(offest + 1)];
        }
        public static bool TryGetPagesCount(HtmlDocument doc, out int pagesCount)
        {
            return  TryGetPagesCount(doc, out pagesCount, out _);
        }
        public static bool TryGetPagesCount(HtmlDocument doc, out int pagesCount,out int ActivePage)
        {
            pagesCount = -1;
            ActivePage = -1;
            try
            {
                var pagination = doc.DocumentNode.Descendants("a");
                if (pagination != null && pagination.Count() > 0)
                    pagination = pagination.Where(d => d != null && d.Attributes["class"] != null && d.Attributes["class"].Value != null && d.Attributes["class"].Value.Length > 0 && d.Attributes["class"].Value.Contains("modern-number"));
                if (pagination != null && pagination.Count() >= 2)
                    pagesCount = int.Parse(pagination.TakeLast(2).First().InnerText.Trim());

                var Ap = doc.DocumentNode.Descendants("span");
                var Active = Ap.Where(d => d.Attributes?["class"]?.Value == "modern-page-current");
                if(Active.Count()>0)
                ActivePage = int.Parse(Active.First().InnerText.Trim());
            }
            catch(Exception ex) { return false; }
            return true;
        }
        public static async Task LogInAccounts()
        {
            await Task.Run(() =>
            {
                try
                {
                    var res = new List<LoginData>();
                    for (int i = 0; i < Accounts.Length; i++)
                    {
                        LoginPassword lp = Accounts[i];
                        LoginData ld = API.Login(lp.Login, lp.Password);
                        if (ld == null)
                        {
                            Log.Error("BAD ACCOUNT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n" +
                                                        "BAD ACCOUNT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n" +
                                                        "BAD ACCOUNT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n" +
                                                        $"{lp.Login}   {lp.Password}", force: true);
                        }
                        else
                        {
                            res.Add(ld);

                        }
                    }

                    LoginTikens = res.ToArray();
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            });
            Log.Text("BinMan Api LogginedInAccounts", force: true);

        }
        public static LoginData GetNextAccount()
        {
            
            lock (laLock)
            {
                var acc = LoginTikens.Length - 1 > LastAccount ? LoginTikens[(LastAccount += 1)] : LoginTikens[(LastAccount = 0)];
                Log.Text("Getted Acc:" + acc.Login +"p: "+acc.PHPSESSID);
                return acc;
            }
        }
        public static LoginData GetCustomAccount(string login, string pass )
        {
            if(CustomAccountCash.TryGetValue(login,out var ld))
            {
                return ld;
            }
            else
            {
                ld = new LoginData(login, pass, true);
                CustomAccountCash.Add(login, ld);
                //ld =  API.Login(login, pass);
                return ld;
            }
        }

        public static bool IsApiEnabled=true;

        public const string CompanyId = "111275";
            public const string BaseUrl = "https://binman.ru/";

            public const string Url_Login = BaseUrl + "cabinet/?login=yes"; // POST

        public static Dictionary<string, LoginData> CustomAccountCash = new Dictionary<string, LoginData>();

            public static string DeffaultCokie(LoginData login) => $"" +

                $" PHPSESSID={login.PHPSESSID}";
            public static string GetSessIdFrom(LoginData ld, string url)
            {
                 Log.Text($"GettingSessIdFor: {url}");
                HttpRequestMessage hm = new(HttpMethod.Post, url);


                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    null,
                    cookie
                    );
                //Log.Json(req);
                var t = req.Content.ReadAsStringAsync();

                t.Wait();

                var html = t.Result;

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                //sessid

                var SesIdParse = htmlDoc.DocumentNode.Descendants("input");
                // int l = SesIdParse.Count();

                SesIdParse = SesIdParse.Where(d => d != null && d.Attributes["name"] != null && d.Attributes["name"].Value != null && d.Attributes["name"].Value.Length > 0 &&
                d.Attributes["name"].Value.Contains("sessid"));
                string sesid = SesIdParse.First().Attributes["value"].Value.Trim();
                Log.Text(sesid);

            // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test2.html");
           
                return sesid;
            }
            public static CookieContainer GetDeffaultCookie(LoginData login, string superGuid)
            {
                var cookie = new CookieContainer();
                cookie.Add(new Uri("https://binman.ru"), new Cookie("BITRIX_SM_SOUND_LOGIN_PLAYED", "Y", "/", "binman.ru"));
                cookie.Add(new Uri("https://binman.ru"), new Cookie("BITRIX_SM_TIME_ZONE", "-420", "/", "binman.ru"));

                cookie.Add(new Uri("https://binman.ru"), new Cookie("BITRIX_SM_LOGIN", login.BITRIX_SM_LOGIN, "/", "binman.ru"));
                cookie.Add(new Uri("https://binman.ru"), new Cookie("PHPSESSID", login.PHPSESSID, "/", "binman.ru"));
                cookie.Add(new Uri("https://binman.ru"), new Cookie("BX_USER_ID", "d6e4f02ed65459f51af72b38ee118942", "/", "binman.ru"));
                return cookie;
            }



            public static HttpResponseMessage SendRequest(HttpRequestMessage req, KeyValuePair<string, string>[] headers, CookieContainer cookies)
            {

                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;
                HttpClient client = new HttpClient(handler);
            client.Timeout = new TimeSpan(00, 00, 30);
            if (headers != null)
                    foreach (var v in headers)
                        client.DefaultRequestHeaders.Add(v.Key, v.Value);
                Log.Warning("Запрос в BinMan");
                //Log.Json(client);
                // Log.Text(" - - - - -- - - - - - - - ");
                // Log.Json(req);

                return client.Send(req);
            }



            public static LoginData? Login(string login, string pass)
            {
                Dictionary<string, string> d = new Dictionary<string, string>()
        {
            { "AUTH_FORM","Y"},
            { "TYPE","AUTH"},
            { "backurl", "/cabinet/"},
            { "USER_LOGIN",login},
            { "USER_PASSWORD",pass},
        };




                HttpRequestMessage hm = new(HttpMethod.Post, Url_Login);

                hm.Content = new FormUrlEncodedContent(d);
                //Log.Text("@ - - - - -- - - - - - - - ");
                //Log.Text(await hm.Content.ReadAsStringAsync());
                //Log.Text("@ - - - - -- - - - - - - - ");
                Log.System("Попытка залогиниться");
                var cookie = new CookieContainer();
                var req = SendRequest(hm,
                    //new KeyValuePair<string, string>[]
                    //{
                    //   new KeyValuePair<string, string>()
                    //}
                    null,
                    cookie
                    );

                var cc = cookie.GetAllCookies();
                string? sesId = cc["PHPSESSID"]?.Value;
                string? bit_sm_log = cc["BITRIX_SM_LOGIN"]?.Value;
                string? bit_sm_us_id = cc["BX_USER_ID"]?.Value;

                if (string.IsNullOrEmpty(sesId))
                {
                    Log.Error("запрос не вернул PHPSESSID в куки");
                    return null;
                }
                if (string.IsNullOrEmpty(bit_sm_log))
                {
                    Log.Error("запрос не вернул BITRIX_SM_LOGIN в куки");
                    return null;
                }




                var res = new LoginData(bit_sm_log, sesId);
                Log.System("Логин успешен!");
                Log.Json(res);
                return res;
            }




            public static string TrimText(string? txt)
            {
            if (string.IsNullOrEmpty(txt)) return txt;
                return Regex.Replace(txt, @"\s+", " ").Replace("пнвтсрчтптсбвс", "").Replace("\n\n", "").Trim();
            }
        public const string Company_BinId= "111275";

        public static void LogReqContent(HttpRequestMessage hm)
        {
            var dd = hm.Content.ReadAsStringAsync();
            dd.Wait();

            StringBuilder sb = new StringBuilder();

            var vvv = Uri.UnescapeDataString(dd.Result).Split("&");

            foreach (var v in vvv)
            {
                sb.AppendLine(v);
            }


            Log.Text("+++" + sb.ToString());
        }
        public static async void RunBinManSyncService()
        {
            SQL.GetObjectTypes();
            // this line create type cash for sync with bin man;
            Task.Run(() =>
            {
            Log.Text("BinMan Synchronizer Initialized", force: true);
            int antiSpam1 = 3;
            int antiSpam2 = 3;
            while (true)
            {

                try
                {
                    int cycleCount = 10;
                    for (int i = 0; i <= cycleCount * 2; i++)
                    {
                        try
                        {
                            Thread.Sleep(500);
                            // if(i==cycleCount/2)Log.Text($"До синхронизации {TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(cycleCount - i / 2)).ToString("H:mm:ss")}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Sync Timer error");
                            Log.Error(ex);
                        }
                    }
                        // 


                        //   Log.System("Count: " + res.Count);

                        //Task KaUpdates = Task.УдалитьВсеНе0Начисления(() =>
                        //{
                        //Log.ApiCall("Inserting Ka");
                        //    var recs = SQL.GetBinManClientsTaskList();

                        //    foreach (var v in recs)
                        //    {
                        //        var ld = BinManApi.GetNextAccount();
                        //        if (
                        //        BinManKa.SendCreateRequest(ld, v, out var BinId))
                        //        {
                        //            SQL.BinManMarkClientSucces(v.KA_DbGuid,BinId,SQL.BinManOperationStatusString.OK);
                        //        }
                        //        else
                        //        {
                        //            SQL.KaIgnoreList.TryAdd(v.KA_DbGuid,"BinMan failed");
                        //            SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.Failed);
                        //        }
                        //    }

                        //});
#if !DEBUG 

                        Task GeozoneUpdate = Task.Run(() =>
                        {
                            try { 
                            var res = SQL.GetBinmanGeozonesUpdateList();
                            if (res.Count > 0) { Log.System("Updating Geozones"); antiSpam1 = 3; }
                            else { if (antiSpam1 > 0) { Log.System("no Geozones to update"); antiSpam1--; } return; }
                                // var tasks = new List<Task>();
                                foreach (var v in res)
                                {
                                    var vv = v;
                                    // v.SendCreateRequest(GetNextAccount(),out var bin_id1);
                                    // tasks.Add(Task.Run(() => { 
                                    int tryCount = 2;
                                    bool Break = false;
                                    while (!Break)
                                    {

                                        if (tryCount <= 0) { //SQL.GuidIgnoreList.Add(v.DataBase_Guid);
                                            if (SQL.GuidIgnoreList.ContainsKey(v.DataBase_Guid))
                                            {
                                                SQL.GuidIgnoreList[v.DataBase_Guid] += 1; // Не потоко безопасно !!
                                            }
                                            else
                                                SQL.GuidIgnoreList.TryAdd(v.DataBase_Guid, 0);
                                            Break = true; break; }
                                        LoginData ld = v.ld;
                                        if (ld == null)
                                        {
                                            ld = GetNextAccount();
                                        }
                                        switch (v.TaskType)
                                        {
                                            case BinManTaskType.archive:
                                                break;
                                            case BinManTaskType.delete:
                                                break;
                                            case BinManTaskType.update:
                                                {
                                                   
                                                    if (v.SendEditRequest(ld))
                                                    {
                                                        Log.ApiCall($"[SYNC G U] Updated Geozone {v.DataBase_Guid} ({v.NAME}) BIN ID: {v.LAST_AREA}");
                                                        SQL.MarkGeozoneBinmanUpdated(v);
                                                        if (v.NeedToBeArchived)
                                                        {
                                                            Log.Text("Archive request");
                                                            if (BinManGeozone.SendSetArchiveRequest(ld, v.LAST_AREA.ToString(), v.IsArchive))
                                                            {
                                                                SQL.MarkGeozoneBinmanArchived(v);
                                                                Break = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            tryCount--;
                                                            break;
                                                        }
                                                        Break = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        tryCount--;
                                                        break;
                                                    }
                                                    
                                                }
                                            case BinManTaskType.insert:

                                                var bin_id = -1;

                                                bool IsGeoCreated = false;
                                                Task tg = Task.Run(() => {
                                                    if (v.SendCreateRequest(ld, out bin_id))
                                                    {


                                                        Log.Message($"[SYNC G I] Created Geozone {v.DataBase_Guid} ({v.NAME}) BIN ID: {v.LAST_AREA}");


                                                        IsGeoCreated = true;


                                                        //return 
                                                        // SQL.GeozoneAddBinId(zone.guid, bg.NAME, geozoneBin_Id);
                                                    }
                                                });


                                                //Task tc = Task.Run(() =>
                                                //{
                                                //    tg.Wait();
                                                //    if(IsGeoCreated)

                                                //    foreach (var v in v.ContainerList)
                                                //    {

                                                //        BinManContainers binContainer = new BinManContainers();



                                                //        binContainer.NAME = "Контейнер геозоны " + bin_id;
                                                //        binContainer.VOLUME = v.volume.ToString();
                                                //        binContainer.TYPE = v.GetBinManType();


                                                //        if (binContainer.SendCreateRequest(ld, out var containerBin_Id))
                                                //        {
                                                //                SQL.ContainerAddBinId(v.guid, containerBin_Id);
                                                //               // Task.WaitAll(WaitToInsertGeozone);
                                                //                binContainer.SendAttachRequest(ld, containerBin_Id, bin_id.ToString());
                                                //            }
                                                //    }

                                                // });
                                                Task tr = Task.Run(() => {
                                                    tg.Wait();
                                                    if (IsGeoCreated)
                                                    {
                                                        bool IsError = false;
                                                        string oldName = v.NAME;
                                                        try
                                                        {
                                                            vv = SQL.GetBinmanGeozonesUpdate_SingleGeo(v.DataBase_Guid);
                                                            vv.LAST_AREA = bin_id;
                                                           
                                                            vv.NAME = BinManGeozone.GenerateName(bin_id, vv.Db_groupGuid, vv.ContainerList.ToArray());
                                                            IsError = false;
                                                        }
                                                        catch(Exception ex)
                                                        {
                                                            Log.Error(ex);
                                                            SQL.GeozoneAddBinId(vv.DataBase_Guid, vv.NAME, bin_id);
                                                            IsError = true;
                                                        }
                                                        //
                                                        if (!IsError)
                                                        {
                                                            SQL.GeozoneAddBinId(vv.DataBase_Guid, vv.NAME, bin_id);

                                                            if (vv.SendEditRequest(ld))
                                                            {
                                                                Log.Message($"[SYNC G I.2] Renamed Geozone {vv.DataBase_Guid} ({vv.NAME}) BIN ID: {vv.LAST_AREA}");
                                                            }
                                                            else { Log.Warning($"[SYNC G I.2] Failed to Rename Geozone {vv.DataBase_Guid} (`{oldName}` -> `{vv.NAME}`) BIN ID: {vv.LAST_AREA}"); }
                                                        }
                                                    }
                                                });

                                                Task.WaitAll(tg);
                                                if (IsGeoCreated) { Break = true; break; }
                                                else
                                                {
                                                    tryCount--;
                                                    break;
                                                }
                                            default:
                                                break;
                                        }

                                    }
                                }
                                
                               // }));
                               // Thread.Sleep(500);
                            } catch (Exception ex)
                            {
                                Log.Error("GeozoneUpdate");
                                Log.Error(ex);
                            }
                           // Task.WaitAll(tasks.ToArray());
                        });
                        //Task ObjectUpdate = Task.Run(() =>
                        //{
                        //    try
                        //    {
                        //        Task.WaitAll(GeozoneUpdate);

                        //        var ObjectUpdateList = SQL.GetObjectListToSyncBinMan();
                        //        if (ObjectUpdateList.Count > 0) { Log.System("Updating Objects"); antiSpam2 = 3; }
                        //        else { if (antiSpam2 > 0) { Log.System("no Objects to update"); antiSpam2--; } return; }
                        //        //  var tasks = new List<Task>();
                        //        foreach (var v in ObjectUpdateList)
                        //        {
                        //            //  tasks.Add(Task.Run(() =>
                        //            // {
                        //            try
                        //            {
                        //                // if (!DadataApi.TryFillAddres(v)) continue;
                        //                LoginData ld = v.ld;
                        //                if (ld == null)
                        //                {
                        //                    ld = GetNextAccount();
                        //                }
                        //                if (string.IsNullOrEmpty(v.BinId) || v.BinId == "0" || v.BinId.Contains("-"))
                        //                {

                        //                    if (BinManObject.SendCreateRequest(ld, v, out long binId))
                        //                    {
                        //                        SQL.UpdateObjectBinId(v.DataBase_Guid, binId);
                        //                        Log.Message($"[SYNC O I] Created object {v.DataBase_Guid} ({v.NAME}) BIN ID: {binId}");
                        //                    }
                        //                    else
                        //                    {
                        //                        Log.Error($"{v.DataBase_Guid}({v.NAME}) failed to create in binman");
                        //                        SQL.IgnoredObjectsId.TryAdd(v.DataBase_Guid, $"{v.DataBase_Guid}({v.NAME}) failed to create in binman");
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    if (BinManObject.SendEditRequest(ld,v))
                        //                    {
                        //                        SQL.SetObjectStatus(v.DataBase_Guid, BinManSyncStatus.ok);
                        //                        Log.Message($"[SYNC O U] Updated object {v.DataBase_Guid} ({v.NAME}) BIN ID: {v.BinId}");
                        //                    }
                        //                    else
                        //                    {
                        //                        SQL.IgnoredObjectsId.TryAdd(v.DataBase_Guid, $"{v.DataBase_Guid}({v.NAME}) failed to update in binman");
                        //                    }
                        //                }
                        //            }
                        //            catch (Exception ex) { Log.Error("Error while updating Objects Inner"); Log.Error(ex); }
                        //            //  }));


                        //            //  Thread.Sleep(500);
                        //        }

                        //        // Task.WaitAll(tasks.ToArray());
                        //    }
                        //    catch (Exception ex) { Log.Error("Error while updating Objects"); Log.Error(ex); }
                        //}
                        //    );


#endif

                        Task ContaienrsUpdate = Task.Run(() =>
                        {
                            var res = SQL.GetBinmanContainersUpdateList();
                            if (res.Count > 0) { Log.System("Updating Containers"); antiSpam1 = 3; }
                            else { if (antiSpam1 > 0) { Log.System("no Containers to update"); antiSpam1--; } return; }
#if !DEBUG
                            Task.WaitAll(GeozoneUpdate);
#endif
                            // var tasks = new List<Task>();
                            foreach (var v in res)
                            {
                                Log.ApiCall("Containers Login");
                                LoginData ld = v.ld;
                                if (ld == null)
                                {
                                    ld = GetNextAccount();
                                }
                                switch (v.type)
                                {
                                    case BinManTaskType.delete:
                                        {
                                            if (BinManGeozone.GetGeozoneContainers(ld, v.geo_binid, out var conts))
                                            {
                                                bool finded = false;
                                                foreach(var c in conts)
                                                {
                                                    if (v.volume == c.volume && v.typeGuid.ToLower() == c.typeGuid.ToLower())
                                                    {
                                                        finded = true;
                                                        if (BinManContainers.SendDeleteRequest(ld, c.guid))
                                                        {
                                                          
                                                            Thread.Sleep(125);
                                                            if(BinManContainers.IsContainerExists(ld,c.guid,out var exists))
                                                            {
                                                                if (exists)
                                                                {
                                                                    SQL.ContainerBinManResult(v.guid, c.guid, BinManContainers.ContainerStatus.Failed_To_Delete);
                                                                    try
                                                                    {
                                                                        SQL.ContainersGuidIgnoreList.Add(v.guid);
                                                                    }
                                                                    catch (Exception ex) { }
                                                                }
                                                                else SQL.ContainerBinManResult(v.guid, c.guid, BinManContainers.ContainerStatus.Deleted);
                                                            }
                                                            else
                                                            {
                                                                SQL.ContainerBinManResult(v.guid, c.guid, BinManContainers.ContainerStatus.Deleted);
                                                            }
                                                        }
                                                        break;
                                                    }
                                                }
                                                if (!finded) { SQL.ContainerBinManResult(v.guid, null, BinManContainers.ContainerStatus.Not_found);

                                                    try
                                                    {
                                                        SQL.ContainersGuidIgnoreList.Add(v.guid);
                                                    }
                                                    catch (Exception ex) { }
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    SQL.ContainersGuidIgnoreList.Add(v.guid);
                                                }
                                                catch (Exception ex) { }
                                            }
                                            Thread.Sleep(225);
                                            break;
                                        }
                                    case BinManTaskType.insert:
                                        {
                                            try
                                            {
                                                BinManContainers binContainer = new BinManContainers();



                                                binContainer.NAME = "Контейнер геозоны " + v.geo_binid;
                                                binContainer.VOLUME = v.volume.ToString();
                                                binContainer.TYPE = v.GetBinManType();

                                                var code = binContainer.SendCreateRequest(ld, out var containerBin_Id);
                                                if (code == BinManContainers.BinManResult.Ok)
                                                {
                                                    SQL.ContainerBinManResult(v.guid, containerBin_Id, BinManContainers.ContainerStatus.Created);
                                                    BinManContainers.SendAttachRequest(ld, containerBin_Id, v.geo_binid);

                                                    //binContainer.SendAttachRequest(ld, containerBin_Id, containerBin_Id);
                                                }
                                                else
                                                {
                                                    SQL.ContainerBinManResult(v.guid, containerBin_Id, BinManContainers.ContainerStatus.Failed_To_Create);
                                                    try
                                                    {
                                                        SQL.ContainersGuidIgnoreList.Add(v.guid);
                                                    }
                                                    catch (Exception ex) { }
                                                }
                                            }
                                            catch (Exception ex) { Log.Error("Containers unhandled err.",ex);
                                                SQL.ContainerBinManResult(v.guid, "", BinManContainers.ContainerStatus.Failed_To_Create) ; }
                                            break;
                                        }
                                    default:
                                        break;
                                }
                            }
                        });
#if !DEBUG
                        Task.WaitAll(
                            GeozoneUpdate,
                            //ObjectUpdate,
                            
                            ContaienrsUpdate
                             //KaUpdates
                            ); 
#else
                        Task.WaitAll(ContaienrsUpdate);
#endif


                        //Log.System(" SYNC Insert");
                        //var res2 = SQL.GetBinmanGeozonesInsertList();
                        //Log.System("Count: " + res2.Count);
                        //foreach (var v in res2)
                        //{
                        //    // v.SendCreateRequest(GetNextAccount(),out var bin_id1);
                        //    if (v.SendCreateRequest(GetNextAccount(), out int bin_id))
                        //    {
                        //        v.LAST_AREA = bin_id;
                        //        v.NAME = $"{v.LAST_AREA} {bg.getGroupNameFromGuid(zone.geozoneGroup)}";
                        //        v.SendEditRequest(ld);
                        //        SQL.GeozoneAddBinId(zone.guid, bg.NAME, geozoneBin_Id);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        Log.Error("While");
                        Log.Error(ex);
                    }

                }
            });
            Task.Run(() =>
            {
                int antiSpam1 = 3;
                int antiSpam2 = 3;
                while (true)
                {
                    try
                    {
                        Task ObjChainUpdate = Task.Run(() =>
                        {
                            var ObjectUpdateList = SQL.GetBinManObjectGeozonesLinks();
                            var GRoupdeList = from t in ObjectUpdateList
                                              group t by new { t.ObjectBinId, t.TaskType };
                            if (ObjectUpdateList.Count > 0) { Log.System("Updating Objects chains"); antiSpam2 = 3; }
                            else { if (antiSpam2 > 0) { Log.System("no Objects chains to update"); antiSpam2--; } return; }

                            var tasks = new List<Task>();
                            foreach (var v in GRoupdeList.Take(Math.Min(3, GRoupdeList.Count())))
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    try
                                    {
                                        switch (v.Key.TaskType)
                                        {
                                            case BinManTaskType.archive:
                                                break;
                                            case BinManTaskType.delete:
                                                {
                                                    Log.ApiCall("DeletingObject Chain");
                                                    LoginData ld = BinManApi.GetNextAccount();
                                                    if (BinManObject.GetAttachedGeozones(ld, v.Key.ObjectBinId, out var links))
                                                    {
                                                        bool frst = false;
                                                        foreach (var t in v)
                                                        {
                                                            if (!frst)
                                                            {
                                                                frst = true;
                                                                continue;
                                                            }
                                                            if (int.TryParse(t.GeoBinId, out var pr))
                                                            {
                                                                if (links.Contains(pr)) links.Remove(pr);
                                                            }
                                                        }
                                                        var stats = BinManGeozone.AttachToObject(ld, links, v.First().GeoBinId, v.Key.ObjectBinId, false);
                                                        if (stats != BinManGeozone.AttachResult.Failed)
                                                        {
                                                            foreach (var t in v)
                                                            {
                                                                SQL.MarkDeletedBinManObjectGeozonesLink(t, stats);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Log.Text("Can't Create link");
                                                            Log.Json(v);
                                                            foreach (var t in v)
                                                            {
                                                                SQL.LinkIgnoreList.TryAdd(t.DbGuid, "");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Log.Text("Can't Get already Attached link link");
                                                        Log.Json(v);
                                                        foreach (var t in v)
                                                        {
                                                            SQL.LinkIgnoreList.TryAdd(t.DbGuid, "");
                                                        }
                                                    }

                                                    break;
                                                }

                                            case BinManTaskType.update:
                                                break;
                                            case BinManTaskType.insert:
                                                {
                                                    Log.ApiCall("Insert Object Chain");
                                                    LoginData ld = BinManApi.GetNextAccount();
                                                    if (BinManObject.GetAttachedGeozones(ld, v.Key.ObjectBinId, out var links))
                                                    {
                                                        bool frst = false;
                                                        foreach (var t in v)
                                                        {
                                                            if (!frst)
                                                            {
                                                                frst = true;
                                                                continue;
                                                            }
                                                            if (int.TryParse(t.GeoBinId, out var pr))
                                                            {
                                                                if (!links.Contains(pr)) links.Add(pr);
                                                            }
                                                        }
                                                        if (BinManGeozone.AttachToObject(ld, links, v.First().GeoBinId, v.Key.ObjectBinId) != BinManGeozone.AttachResult.Failed)
                                                        {
                                                            foreach (var t in v)
                                                            {
                                                                SQL.MarkInsertedBinManObjectGeozonesLink(t);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Log.Text("Can't Create link");
                                                            Log.Json(v);
                                                            foreach (var t in v)
                                                            {
                                                                SQL.LinkIgnoreList.TryAdd(t.DbGuid, "");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Log.Text("Can't Get already Attached link link");
                                                        Log.Json(v);
                                                    }

                                                    break;
                                                }
                                        }
                                    }
                                    catch (Exception ex) { Log.Error("Error while updating Object Geo Chains"); Log.Error(ex); }

                                }));
                                Thread.Sleep(500);
                            }


                            Task.WaitAll(tasks.ToArray());
                        });
                        Task.WaitAll(ObjChainUpdate);
                        Thread.Sleep(1000);
                    }
                    catch(Exception ex)
                    {
                        Log.Error("BinMan Geozone Updater Cycle", ex);
                    }
                }
            });
            Task.Run(() =>
            {
                int antiSpam1 = 3;
                int antiSpam2 = 3;
                while (true)
                {
                    try
                    {
                        var recs = SQL.GetBinManClientsTaskList();
                        if (recs.Count > 0) { Log.System("Updating Kas"); antiSpam2 = 3; }
                        else { if (antiSpam2 > 0) { Log.System("no Kas to update"); antiSpam2--; } Thread.Sleep(7000); continue; }

                        foreach (var v in recs)
                        {
                            var ld = BinManApi.GetNextAccount();
                            if (string.IsNullOrEmpty(v.ID) || v.ID.Length < 2)
                            {
                                if (BinManKa.SendCreateRequest(ld, v, out var BinId))
                                {
                                    SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.OK);
                                }
                                else
                                {
                                    SQL.KaIgnoreList.TryAdd(v.KA_DbGuid, "BinMan failed");
                                    SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.Failed);
                                }

                            }
                            else
                            {
                                if (BinManKa.SendEditRequest(ld, v))
                                {
                                    SQL.BinManMarkClientSucces(v.KA_DbGuid, string.Empty, SQL.BinManOperationStatusString.OK);
                                }
                                else
                                {
                                    SQL.KaIgnoreList.TryAdd(v.KA_DbGuid, "BinMan failed");
                                    SQL.BinManMarkClientSucces(v.KA_DbGuid, string.Empty, SQL.BinManOperationStatusString.Failed);
                                }
                            }
                        }
                        Thread.Sleep(7000);
                    }
                    catch(Exception ex)
                    {
                        Log.Error("KA BinManUpdate Cycle",ex);
                    }
                }
            });

            Task.Run(() =>
            {
                Log.Text("BinMan: Синхронизация по начислениям включена, остальные лень писать , но с этого момента можно и писать");
                int antiSpam1 = 3;
                int antiSpam2 = 3;
                while (true)
                {
                    try
                    {
                        var recs = SQL.BinMan_GetAccruals2Create();
                        if (recs.Count > 0) { Log.System("Updating Accruals"); antiSpam2 = 3; }
                        else { if (antiSpam2 > 0) { Log.System("no Accruals to update"); antiSpam2--; } Thread.Sleep(7000); continue; }

                        foreach (var v in recs)
                        {
                            try
                            {
                                var ld = BinManApi.GetNextAccount();


                                if (!string.IsNullOrEmpty(v.parentBinId))
                                {
                                    var resese = BinManDocAccruals.CreateCorrectir(ld, v, out var binid);

                                    SQL.UpdateAccruaBinManStatus(resese, v.db_guid, binid);
                                }
                                else
                                {

                                    var resese = BinManDocAccruals.AddAccrualToDoc(ld, v, out var binid);

                                    SQL.UpdateAccruaBinManStatus(resese, v.db_guid, binid);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                                SQL.UpdateAccruaBinManStatus(AccrualsCreationResult.Failed, v.db_guid, string.Empty);
                            }

                        }
                        Thread.Sleep(10000);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(7000);
                        Log.Error("Accruals BinManUpdate Cycle", ex);
                    }
                }
            });

        }
        /// <summary>
        /// +7 (666) 666-66-66 
        /// </summary>
        /// <param name="Phone"></param>
        /// <param name="FormatedPhone"></param>
        /// <returns> +7 (666) 666-66-66 </returns>
        public static bool TryFormatPhoneNumberAsKa(string Phone,out string FormatedPhone)
        {
            FormatedPhone = "";
            try
            {
               
                var Dd = string.Concat(Phone.Where(x => char.IsDigit(x)));
                if ((Dd.Length == 11 || Dd.Length==10))
                {
                    if ((Dd[0] == '7' && Dd.Length == 11))
                    {
                        FormatedPhone += "+7";
                    }
                    else

                    if (Dd[0] == 8)
                    {
                        FormatedPhone += "+7";
                    }
                    else
                    {
                        if (Dd.Length == 10)
                            FormatedPhone += "+7";
                        else return false;
                    }
                    FormatedPhone += $" ({Dd.Substring(1, 3)})";
                    FormatedPhone += $" {Dd.Substring(3, 3)}-{Dd.Substring(6, 2)}-{Dd.Substring(8, 2)}";
                    return true;
                }

            }
            catch(Exception ex)
            {
                Log.Error(ex);
                return false;
            }
            return false;
        }


    }
}
