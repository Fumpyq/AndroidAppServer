using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using DocumentFormat.OpenXml.EMMA;
using HtmlAgilityPack;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManClientParser;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public static class BinManCarParser
    {
        public static string Url_Get(int page) => API.BaseUrl + $"cabinet/cars/?PAGEN_1={page}&is_ajax=y";
        public class BinManCar
        {
            public Guid dbGuid; public string name; public string binId; public string gosNum; public string IMEI; public List<string> ConateinerTypes;
        }


        public static Dictionary<string, string> SvgPath2ContainerType = new Dictionary<string, string>() {
                {"M0 0H16V3H15L14 14C14 14 13.5 16 12 16H4C2.5 16 2 14 2 14L1 3H0V0Z"
                ,"BD423E8B-2AC2-4C16-AFD3-20025D4F5B82" }//BD423E8B-2AC2-4C16-AFD3-20025D4F5B82	Под боковую загрузку
            , { "M14.0171 0H1.99829C1 0 1 1 1 1V2H0V4H16V2H15V1C15 1 15 0 14.0171 0ZM6 1H3V2H6V1ZM10 1H13V2H10V1ZM15 12V5H1V12H3C5 12 5 14 5 14V15H11V14C11 14 11 12 13 12H15ZM4 14.5C4 15.3284 3.32843 16 2.5 16C1.67157 16 1 15.3284 1 14.5C1 13.6716 1.67157 13 2.5 13C3.32843 13 4 13.6716 4 14.5ZM13.5 16C14.3284 16 15 15.3284 15 14.5C15 13.6716 14.3284 13 13.5 13C12.6716 13 12 13.6716 12 14.5C12 15.3284 12.6716 16 13.5 16Z"
                ,"B28E1AD5-570D-4271-897F-00005C224FE9"} //B28E1AD5-570D-4271-897F-00005C224FE9	Евроконтейнер
            , { "M10.154 0H10.8298C11.1053 0 11.1053 0.251488 11.1053 0.251488V4.88961L17.8834 11.5249C18.1114 11.7483 17.8617 11.979 17.8617 11.979L17.5002 12.3374C17.5002 12.3374 17.2602 12.6032 17.0125 12.3662L10.4063 5.89931L4.11841 12.274C3.86559 12.458 3.6411 12.2711 3.6411 12.2711L3.28296 11.8948C3.28296 11.8948 3.00116 11.6529 3.28547 11.3851L9.89474 4.69357V0.251488C9.89474 0.251488 9.89474 0 10.154 0ZM5.5 8H0V12L5 17H18L22 13.1426V11.9283L18.5 8H15.5L18.8086 11.3885C19.2102 11.7542 18.8624 12.0101 18.8624 12.0101L17.6361 13.342C17.6361 13.342 17.3175 13.6385 17.0085 13.378L11.5 8H9.5L4.13302 13.3694C3.85175 13.6382 3.52616 13.3775 3.52616 13.3775L2.17609 12.0101C2.17609 12.0101 1.89321 11.7069 2.1941 11.3913L5.5 8Z"
                ,"3527C2A0-58D6-44EB-B222-00010160C467"}//3527C2A0-58D6-44EB-B222-00010160C467	Лодочка под портал
            , { "M0 7H8.97941L3.71231 11.3394C3.71231 11.3394 3.16392 11.7894 3.69507 12.2416L4.86511 13.3534C5.43769 13.7917 6.01075 13.2972 6.01075 13.2972L13.5434 7H17L20 9.93928L15 16H1L0 15V7ZM4.82622 11.7647L18.5132 0.0895002C18.5132 0.0895002 18.6614 -0.0806962 18.8263 0.0727595L19.2247 0.471969C19.2247 0.471969 19.3611 0.64231 19.2077 0.809507L5.53809 12.467C5.37976 12.637 5.21686 12.5031 5.21686 12.5031L4.82109 12.1079C4.82109 12.1079 4.65323 11.9495 4.82622 11.7647Z"
                ,"7E050779-A962-4993-9CBE-0001224A0DA2"}//7E050779-A962-4993-9CBE-0001224A0DA2	Лодочка под лебедку
            , { "M5.07556 0H16V14H0V9H7V8H0V4.02295L5.07556 0Z"
                ,"250D87C0-D994-4593-93C0-C0BF2ED73450"}//250D87C0-D994-4593-93C0-C0BF2ED73450	Фронтальный
            , { "M2 0H22V11H2V5H0V1H2V0ZM6 3H7V5H17V3H18.0526V8H17V6H7V8H6V3Z"
                ,"FADC87A9-6241-4CFF-8D94-2201CA949C78"}//FADC87A9-6241-4CFF-8D94-2201CA949C78	Мультилифт
                     
            
        };

        public static List<BinManCar> TryParseCats(LoginData login)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, Url_Get(1));
            var cookie = API.GetDeffaultCookie(login, "");
            var req = API.SendRequest(hm,
                   new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                   },
                   cookie
                   );

            var txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

           

            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(txt);


            if (!BinManApi.TryGetPagesCount(htmlDoc, out int PagesCount))
            {
                return null;
            }


            var res = new List<BinManCar>(100);

            try
            {
                ParsePage(htmlDoc);
            }
            catch (Exception ex) { Log.Error(ex); }


            for (int page = 2; page < PagesCount; page++)
            {
                try
                {
                    hm = new(HttpMethod.Get, Url_Get(page));
                    cookie = API.GetDeffaultCookie(login, "");
                    req = API.SendRequest(hm,
                           new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                           cookie
                           );

                    txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();



                    htmlDoc = new HtmlDocument();

                    htmlDoc.LoadHtml(txt);

                    ParsePage(htmlDoc);
                }
                catch (Exception ex)
                { }
            }

            return res;

            void ParsePage(HtmlDocument htmlDoc)
            {

                var cars = htmlDoc.DocumentNode.Descendants("tr").Where(x => x.Attributes?["class"]?.Value == "data-list__row data-row car-row js-car-row");
                foreach (var c in cars)
                {
                    var name_ns = c.Descendants("a").Where(x => x.Attributes?["target"]?.Value == "_blank");
                    if (name_ns.Count() <= 0) { Log.Warning("Не получилось получить имя машины !!!"); continue; }
                    var name_n = name_ns.First();
                    var binId = BinManApi.CutIdFromUrl(name_n,1);
                    var Car_Name = BinManApi.TrimText(name_n.InnerText);

                    var Gos_Ns = c.Descendants("div").Where(x =>x.Attributes?["class"]?.Value == "section" && x.InnerText.Contains("Гос. номер:"));
                    if (Gos_Ns.Count() <= 0) { Log.Warning("Не получилось получить номер машины !!!"); continue; }
                    var Gos_N = Gos_Ns.First();  

                    var FullText=BinManApi.TrimText(Gos_N.InnerText).Trim();

                    var splt = FullText.Split("/");

                    var car_GosNum = BinManApi.TrimText(splt[0].Replace("Гос. номер:", "")).Replace("/", "").Trim();
                    var car_IMEI = BinManApi.TrimText(splt[1].Replace("IMEI:", "")).Replace("/", "").Trim();

                    var Containers_ns = c.Descendants("path").Where(x => SvgPath2ContainerType.ContainsKey(x.Attributes?["d"]?.Value ==null ? "": x.Attributes["d"].Value));

                    var Res = new BinManCar();

                    Res.IMEI = car_IMEI;
                    Res.gosNum = car_GosNum;
                    Res.ConateinerTypes = new List<string>();
                    Res.binId = binId;
                    Res.name = Car_Name;

                    foreach ( var cnt in Containers_ns)
                    {
                        var Cont_type = SvgPath2ContainerType[cnt.Attributes["d"].Value];
                        Res.ConateinerTypes.Add(Cont_type);
                    }
                    res .Add (Res);
                }


            }
        }
    }
}
