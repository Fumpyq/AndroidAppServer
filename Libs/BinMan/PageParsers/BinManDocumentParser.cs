using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static AndroidAppServer.Libs.BinMan.BinManObject;
using static AndroidAppServer.Libs.BinMan.BinManObjectData;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManClientParser;
using static System.Net.WebRequestMethods;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public class BinManDocumentParser
    {
        public static ConcurrentBag<string> ErrorPages = new ConcurrentBag<string>();
        //public class ObjectPars
        //{
        //    public string name { get; set; }
        //    public string address { get; set; }
        //    public string area { get; set; }
        //    public string tax { get; set; }
        //    public string accrual { get; set; }
        //    public string graphic { get; set; }
        //    public string graphDetails { get; set; }
        //}
        public record class BinDocDetails(
            string binid
            , string ndoc
            , string dateBegin
            , string dateEnd
            , string dateAccept
            , string Type
            , string Group);
        public record class BinDocumentParse(
             BinDocDetails docInfo
            , BinClientInfo client
            , List<DocObject> objects
            );

        public record class DocObject
        {
            /// <summary>
            /// Пока не везде  реализовано !!!
            /// </summary>
            public string tarif_volume;
            /// <summary>
            /// Пока не везде  реализовано !!!
            /// </summary>
            public string tarif_measure;

            public string binid;
               public string name; 
               public string address; 
               public string lot; 
               public string people; 
               public string tax; 
               public string taxSumm; 
               public string Graphic; 
               public List<string> GraphicDetail; 
               public string Container; 
               public string PeriodFrom; 
               public string PeriodTo; 
               public ObjectType_parse type;
               public DocObjectChange[] changes;

            public DateTime DT_PeriodFrom;
            public DateTime DT_PeriodTo;
            public float? ParseTarif_Volume()
            {
                Regex regex = new Regex(@"^\d+$");
                try
                {
                   var m =  regex.Match(people);
                    if (m.Success)
                        return float.Parse(m.Value);
                    else return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        };
       public enum DocObjectChangeStatus
        {
            Active,
            Disabled
        }
        public struct DocObjectChange
        {
            public static DocObjectChange Empty => new DocObjectChange() { ISNULL= true };
            public string link_bin_id;
            public bool IsNullOrEmpty => ISNULL || string.IsNullOrEmpty(status);
            public bool ISNULL;
            public string date; 
            public string status; 
            public string tarif_full_text;
            public string tarif_info; 
            public string people;

            /// <summary>
            /// Пока не везде  реализовано !!!
            /// </summary>
            public string tarif_volume;
            /// <summary>
            /// Пока не везде  реализовано !!!
            /// </summary>
            public string tarif_measure;

            public string people_reason;
            public string summ;
            public string graphy;
            public List<string> GraphicDetail; 
            public string container;
            public string PeriodFrom;
            public string PeriodTo;

            public DocObjectChangeStatus Status;

            public DateTime DT_PeriodFrom;
            public DateTime DT_PeriodTo;
        }
        public static string Url_GetDocumentObjectsPage(string docid, int page) => API.BaseUrl + $"cabinet/company/contracts/detail/{docid}/?param=OBJECTS&PAGEN_1={page}";
        public const string Url_GetDocuments = API.BaseUrl + "cabinet/documents/contracts/";


        public static List<KeyValuePair<string, string>> FiltersFormData(DateTime from,DateTime to, int page) =>
 // date.ToString("dd.MM.yyyy")
 new List<KeyValuePair<string, string>>()

 {             new("search",""),
               new ("date_create_from",from.ToString("dd.MM.yyyy")),
               new ("date_create_to",to.ToString("dd.MM.yyyy") ),
               new ("date_from","" ),
               new ("ate_to","" ),
               new ("active_from_range[0]","" ),
               new ("active_from_range[1]","" ),
               new ("active_to_range[0]","" ),
               new ("active_to_range[1]","" ),
               new ("objects_range[0]","" ),
               new ("objects_range[1]","" ),
               new ("object_count","" ),
               new ("areas_range[0]","" ),
               new ("areas_range[1]","" ),
               new ("area_count","" ),
               new ("client","" ),
               new ("address[PROPERTY_276]","" ),
               new ("address[PROPERTY_295]","" ),
               new ("address[PROPERTY_277]","" ),
               new ("address[PROPERTY_299]","" ),
               new ("address[PROPERTY_278]","" ),
               new ("address[PROPERTY_279]","" ),
               new ("sort","" ),
               new ("sort_order","" ),
               new ("PAGEN_1",page.ToString() ),
};
        
        public static Regex FloatRegex = new Regex(@"(([0-9]\.*)*[0-9])");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="docid"></param>
        /// <returns></returns>
        public static string GetLink(string docid) => API.BaseUrl + $"cabinet/company/contracts/detail/{docid}/?param=OBJECTS";
        public static string GetLinkDocumentDetail(string docid) => API.BaseUrl + $"cabinet/company/contracts/edit/{docid}/?is_ajax=y&is_edit=y";
        public static string GetLinkDocumentHatWithClient(string docid) => API.BaseUrl + $"cabinet/company/contracts/detail/{docid}/";

        /// <summary>
        /// Парсинг объектов указанного договора
        /// </summary>
        /// <param name="login"></param>
        /// <param name="docid"></param>
        /// <param name="resobjs"></param>
        /// <returns></returns>
        public static bool TryParseObjects(LoginData login, string docid, out List<DocObject> resobjs, int GlobalTimeOut = 32)
        {




            try
            {
                Log.ApiCall($"TryParseObjects [{docid}]");
                Stopwatch sw = Stopwatch.StartNew();
                HttpRequestMessage hm = new(HttpMethod.Get, GetLink(docid));
                var cookie = API.GetDeffaultCookie(login, "");
                var req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                       cookie, secTimeOut: GlobalTimeOut
                       );

                var t = req.Content.ReadAsStringAsync();

                t.Wait();

                var res = t.Result;
                //Log.Text(res);
                //string ress = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_objs.html");
                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(res);
                sw.Stop();
                Log.Text("Http Time: " + sw.ElapsedMilliseconds + " ms");
                sw.Restart();
                // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_objs.html");


                int pageCount = 1;
                var result = new List<DocObject>(2);
                var pagination = htmlDoc.DocumentNode.Descendants("a");
                if (pagination != null && pagination.Count() > 0)
                    pagination = pagination.Where(d => d != null && d.Attributes["class"] != null && d.Attributes["class"].Value != null && d.Attributes["class"].Value.Length > 0 && d.Attributes["class"].Value.Contains("modern-number"));
                if (pagination != null && pagination.Count() >= 2)
                    pageCount = int.Parse(pagination.TakeLast(2).First().InnerText.Trim());

                ParsePage(htmlDoc);

                for (int page = 2; page <= pageCount; page++)
                {
                    hm = new(HttpMethod.Get, Url_GetDocumentObjectsPage(docid, page));
                    cookie = API.GetDeffaultCookie(login, "");
                    req = API.SendRequest(hm,
                           new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                           cookie , secTimeOut: GlobalTimeOut
                           );

                    t = req.Content.ReadAsStringAsync();

                    t.Wait();

                    res = t.Result;
                    //Log.Text(res);
                    //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
                    htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(res);


                    ParsePage(htmlDoc);


#if DEBUG
                    Log.Text("Parse Time: " + sw.ElapsedMilliseconds + " ms");
#endif


                    string TrimText(string txt)
                    {
                        return Regex.Replace(txt, @"\s+", " ").Replace("\n\n", "").Trim();
                    }

                }

                void ParsePage(HtmlDocument htmlDoc)
                {
                    var p1 = htmlDoc.DocumentNode.Descendants("div").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
                    int count = p1.Count();
                    for (int i = 0; i < count; i++)
                    {

                        var CurrentElement = p1.ElementAt(i);

                        //var p3 = CurrentElement.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));


                        var ObjectNam = CurrentElement.Descendants("a").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                        var Lot = CurrentElement.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                        var AllMainInfo = CurrentElement.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item"));
                        var Address = CurrentElement.Descendants("a").Where(d => d.Attributes["class"].Value.Contains("inherit-link"));
                        var GraphicEl = AllMainInfo.ElementAt(3);
                        var ContainerVolume = CurrentElement.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                        //var NoInfo = GraphicEl.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item icon-dg icon-dg-calend"));

                        var GraphicDetail = GraphicEl.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-info-line__modal"));
                        var GraphycType = GraphicEl.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("js-show-popover"));
                        IEnumerable<HtmlNode>? GraphicDetailValues = null;

                        if (GraphicDetail != null && GraphicDetail.Count() > 0)
                        {
                            var GdTmp = GraphicDetail.First();
                            GraphicDetailValues = GdTmp.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("active"));
                        }
                        string binId = CutIdFromUrl(ObjectNam.First(), 1);
                        bool isObjectTypeParsed = false;
                        ObjectType_parse OTParse = null; 
                        Task t = Task.Run(() => {isObjectTypeParsed= BinManObject.TryParseObjectType(login, binId, out OTParse); });

                        
                        //int Poffest0 = 0;
                        //int Poffest1 = 0;
                        // int Poffest2 = 0;
                        // int Poffest3 = 0;
                        //int Poffest4 = 0;
                        //int Poffest5 = 0;


                        //data-list data-tbl dl-detail__data


                        #region Изменения по объекту
                        var nodes = htmlDoc.DocumentNode.Descendants("table").Where(d => d.Attributes["class"].Value.Contains("data-list data-tbl dl-detail__data"));
                        //nodes = nodes.First().Descendants("div").Where(d => d.Attributes["class"].Value.Contains("data-list__detail dl-detail"));
                        //Log.Text("----=--=---=-=---=-=");
                        //foreach (var v in nodes)
                        //{
                        //    Log.Text(v.InnerHtml);
                        //}


                        var s1 = nodes.ElementAt(i);
                        var nooodse = s1.Descendants("tr");
                        int countInner = s1.Descendants("tr").Count() - 1;
                        List<DocObjectChange> changes = new List<DocObjectChange>();




                        string name = ObjectNam.ElementAt(0).InnerText;
     
                        string addres = Address.ElementAt(0).FirstChild.InnerText;
                        string lot = Lot.ElementAt(0).LastChild.InnerText;
                        string peopleP = BinManApi.TrimText(AllMainInfo.ElementAt(0).InnerText.Trim()).Trim();
                        string taxP = AllMainInfo.ElementAt(1).InnerText;
                        string taxSumm = AllMainInfo.ElementAt(2).InnerText;

                        string Container = BinManApi.TrimText(ContainerVolume.ElementAt(0).InnerText);
                        string PerioudFrom = AllMainInfo.ElementAt(5).InnerText;
                        string PerioudTo = AllMainInfo.ElementAt(6).InnerText;

                        string GraphicType = "Нет данных.";

                        string tarif_vP = "";
                        string tarif_mP = "";


                        try
                        {
                            var m = FloatRegex.Match(peopleP);
                            if (m.Success)
                            {
                                tarif_vP = m.Value;
                                tarif_mP = peopleP.Replace(tarif_vP, "").Trim();
                            }
                        }
                        catch (Exception ex) { }




                        List<string> GraphicDetails = new List<string>();
                        if (GraphicDetail.Count() == 0)
                        {
                            GraphicType = BinManApi.TrimText(GraphicEl.InnerText);
                        }
                        else
                        {
                            GraphicType = BinManApi.TrimText(GraphycType.First().InnerText);
                            if (GraphicDetailValues != null)
                                foreach (var v in GraphicDetailValues) GraphicDetails.Add(BinManApi.TrimText(v.InnerText));
                        }




                        //var GraphicParse = AllMainInfo.ElementAt(Soffest1++).Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-info-line__modal")).First();

                        //var ExactDays = GraphicParse.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("active"));

                        //List<string> Days = new List<string>();

                        //foreach (var day in ExactDays) { Days.Add(TrimText(day.InnerText)); }




                        for (int j = 0; j < countInner; j++)
                        {
                            s1 = nooodse.ElementAt(j + 1);
                            var _MainInfo = s1.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item"));
                            var _Tarif = s1.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                            var _Containers = s1.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                            var _TarifType = s1.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                            var _GraphycType = _MainInfo.ElementAt(5).Descendants("div").Where(d => d.Attributes["class"].Value.Contains("js-show-popover"));

                            // int Soffest1 = 0; //item
                            // int Soffest2 = 0; //tl
                            // int Soffest3 = 0; //val
                            // int Soffest4 = 0; //span
                            string LinkBinId = s1.Attributes["data-object-log-id"].Value;
                            string date = _MainInfo.ElementAt(0).InnerText;//Изменено
                            string status = _MainInfo.ElementAt(1).InnerText;//Статус
                            string tax = _Tarif.ElementAt(0).InnerText;//Тариф ч.1
                            string tax_info = _TarifType.ElementAt(0).InnerText;//Тариф ч.2
                            string people = BinManApi.TrimText(_MainInfo.ElementAt(2).InnerText.Trim()).Trim();//Показатель и объем
                            string people_reason = _MainInfo.ElementAt(3).InnerText;//Тариф ч.2
                            string summ = _MainInfo.ElementAt(4).InnerText;//График TODO
                            string graphy = BinManApi.TrimText(_GraphycType.First().InnerText);
                            string container = BinManApi.TrimText(_Containers.ElementAt(0).InnerText);//Контейнер
                                                                                                      //Soffest1++;//На контейнер;
                            string PeriodFrom = _MainInfo.ElementAt(7).InnerText;//Дата от
                            string PeriodTo = _MainInfo.ElementAt(8).InnerText;//Дата до

                            var _GraphicParse = _MainInfo.ElementAt(5).Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-info-line__modal"));


                            string tarif_v = "";
                            string tarif_m = "";


                            try
                            {
                                var m = FloatRegex.Match(people);
                                if (m.Success)
                                {
                                    tarif_v = m.Value;
                                    tarif_m = people.Replace(tarif_v, "").Trim();
                                }
                            }
                            catch (Exception ex) { }


                            IEnumerable<HtmlNode>? _GraphicDetailValues = null;

                            if (_GraphicParse != null && _GraphicParse.Count() > 0)
                            {
                                var _GdTmp = _GraphicParse.First();
                                _GraphicDetailValues = _GdTmp.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("active"));
                            }


                            List<string> _Days = new List<string>();
                            if (_GraphicDetailValues != null)
                                foreach (var day in _GraphicDetailValues) { _Days.Add(BinManApi.TrimText(day.InnerText)); }


                            var From1 = DateTime.MinValue;
                            DateTime.TryParse(PeriodFrom.Trim(), out From1);

                            var To1 = DateTime.MinValue;
                            DateTime.TryParse(PeriodTo.Trim(), out To1);

                            var st = status.Trim() == "Приостановленный" ? DocObjectChangeStatus.Disabled : DocObjectChangeStatus.Active;

                            DocObjectChange dd = new DocObjectChange()
                            {
                                date = date.Trim(),//Изменено
                                status = status.Trim(),//Статус
                                tarif_full_text = tax.Trim(),//Тариф ч.1
                                tarif_info = tax_info.Trim(),//Тариф ч.2
                                people = people.Trim(),//Показатель и объем
                                people_reason = people_reason.Trim(),//Показательи объем ч.2
                                summ = summ.Trim(),//Начисления
                                graphy = graphy.Trim(),//График TODO
                                GraphicDetail = _Days,
                                container = container.Trim(),//График TODO
                                PeriodFrom = PeriodFrom.Trim(),
                                PeriodTo = PeriodTo.Trim(),
                                tarif_volume = tarif_v.Trim(),
                                tarif_measure = tarif_m.Trim(),
                                DT_PeriodFrom = From1,
                                DT_PeriodTo = To1,
                                Status = st,
                                link_bin_id = LinkBinId
                            };
                            changes.Add(dd);
                        }

                        #endregion
                        var From = DateTime.MinValue;
                        DateTime.TryParse(PerioudFrom, out From);

                        var To = DateTime.MinValue;
                        DateTime.TryParse(PerioudTo, out To);

                        DocObject doc = new DocObject()
                        {
                            binid = binId,
                            name = name.Trim(),
                            address = addres.Trim(),
                            lot = lot.Trim(),
                            people = peopleP.Trim(),
                            tax = taxP.Trim(),
                            taxSumm = taxSumm.Trim(),
                            Graphic = GraphicType.Trim(),
                            GraphicDetail = GraphicDetails,
                            Container = Container.Trim(),
                            PeriodFrom = PerioudFrom.Trim(),
                            PeriodTo = PerioudTo.Trim(),
                            changes = changes.ToArray(),
                            tarif_measure = tarif_mP.Trim(),
                            tarif_volume = tarif_vP.Trim(),
                            DT_PeriodFrom = From,
                            DT_PeriodTo = To,
                        };

                        t.Wait();
                        if (isObjectTypeParsed)
                        {
                            doc.type = OTParse;
                        }
                        else
                        {
                            Log.Error("Object type wasn't parsed !!!!!!!!!");
                        }

                        result.Add(doc);
                    }
                }

                resobjs = result;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                ErrorPages.Add(docid);
                resobjs = null;
                return false;
            }



        }

        public static bool TryFindObject(LoginData login, string docid,string objId2Find, out DocObject resobj, int GlobalTimeOut = 32)
        {

            resobj = null;


            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                HttpRequestMessage hm = new(HttpMethod.Get, GetLink(docid));
                var cookie = API.GetDeffaultCookie(login, "");
                var req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                       cookie, secTimeOut: GlobalTimeOut
                       );

                var t = req.Content.ReadAsStringAsync();

                t.Wait();
                
                var res = t.Result;
                //Log.Text(res);
                //string ress = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_objs.html");
                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(res);
                sw.Stop();
                Log.Text("Http Time: " + sw.ElapsedMilliseconds + " ms");
                sw.Restart();
                // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_objs.html");


                int pageCount = 1;
                var result = new List<DocObject>(2);
                var pagination = htmlDoc.DocumentNode.Descendants("a");
                if (pagination != null && pagination.Count() > 0)
                    pagination = pagination.Where(d => d != null && d.Attributes["class"] != null && d.Attributes["class"].Value != null && d.Attributes["class"].Value.Length > 0 && d.Attributes["class"].Value.Contains("modern-number"));
                if (pagination != null && pagination.Count() >= 2)
                    pageCount = int.Parse(pagination.TakeLast(2).First().InnerText.Trim());

                var buff = ParsePage(htmlDoc);

                var exists = buff.FirstOrDefault(d => d.binid == objId2Find, null);
                if (exists !=null) { resobj = exists; return true; }

                
                for (int page = 2; page <= pageCount; page++)
                {
                    hm = new(HttpMethod.Get, Url_GetDocumentObjectsPage(docid, page));
                    cookie = API.GetDeffaultCookie(login, "");
                    req = API.SendRequest(hm,
                           new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                           cookie, secTimeOut: GlobalTimeOut
                           );

                    t = req.Content.ReadAsStringAsync();

                    t.Wait();

                    res = t.Result;
                    //Log.Text(res);
                    //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
                    htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(res);


                    //ParsePage(htmlDoc);s
                     buff = ParsePage(htmlDoc);

                     exists = buff.FirstOrDefault(d => d.binid == objId2Find, null);
                    if (exists != null) { resobj = exists; return true; }


                    Log.Text("Parse Time: " + sw.ElapsedMilliseconds + " ms");


                    string TrimText(string txt)
                    {
                        return Regex.Replace(txt, @"\s+", " ").Replace("\n\n", "").Trim();
                    }

                }

                List<DocObject> ParsePage(HtmlDocument htmlDoc)
                {
                    var result = new List<DocObject>();
                    var p1 = htmlDoc.DocumentNode.Descendants("div").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
                    int count = p1.Count();
                    for (int i = 0; i < count; i++)
                    {

                        var CurrentElement = p1.ElementAt(i);

                        //var p3 = CurrentElement.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));


                        var ObjectNam = CurrentElement.Descendants("a").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                        var Lot = CurrentElement.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                        var AllMainInfo = CurrentElement.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item"));
                        var Address = CurrentElement.Descendants("a").Where(d => d.Attributes["class"].Value.Contains("inherit-link"));
                        var GraphicEl = AllMainInfo.ElementAt(3);
                        var ContainerVolume = CurrentElement.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                        //var NoInfo = GraphicEl.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item icon-dg icon-dg-calend"));

                        var GraphicDetail = GraphicEl.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-info-line__modal"));
                        var GraphycType = GraphicEl.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("js-show-popover"));
                        IEnumerable<HtmlNode>? GraphicDetailValues = null;

                        if (GraphicDetail != null && GraphicDetail.Count() > 0)
                        {
                            var GdTmp = GraphicDetail.First();
                            GraphicDetailValues = GdTmp.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("active"));
                        }
                        string binId = CutIdFromUrl(ObjectNam.First(), 1);
                        bool isObjectTypeParsed = false;
                        ObjectType_parse OTParse = null;
                        Task t = Task.Run(() => { isObjectTypeParsed = BinManObject.TryParseObjectType(login, binId, out OTParse); });


                        //int Poffest0 = 0;
                        //int Poffest1 = 0;
                        // int Poffest2 = 0;
                        // int Poffest3 = 0;
                        //int Poffest4 = 0;
                        //int Poffest5 = 0;


                        //data-list data-tbl dl-detail__data


                        #region Изменения по объекту
                        var nodes = htmlDoc.DocumentNode.Descendants("table").Where(d => d.Attributes["class"].Value.Contains("data-list data-tbl dl-detail__data"));
                        //nodes = nodes.First().Descendants("div").Where(d => d.Attributes["class"].Value.Contains("data-list__detail dl-detail"));
                        //Log.Text("----=--=---=-=---=-=");
                        //foreach (var v in nodes)
                        //{
                        //    Log.Text(v.InnerHtml);
                        //}


                        var s1 = nodes.ElementAt(i);
                        var nooodse = s1.Descendants("tr");
                        int countInner = s1.Descendants("tr").Count() - 1;
                        List<DocObjectChange> changes = new List<DocObjectChange>();




                        string name = ObjectNam.ElementAt(0).InnerText;

                        string addres = Address.ElementAt(0).FirstChild.InnerText;
                        string lot = Lot.ElementAt(0).LastChild.InnerText;
                        string peopleP = BinManApi.TrimText(AllMainInfo.ElementAt(0).InnerText.Trim()).Trim();
                        string taxP = AllMainInfo.ElementAt(1).InnerText;
                        string taxSumm = AllMainInfo.ElementAt(2).InnerText;

                        string Container = BinManApi.TrimText(ContainerVolume.ElementAt(0).InnerText);
                        string PerioudFrom = AllMainInfo.ElementAt(5).InnerText;
                        string PerioudTo = AllMainInfo.ElementAt(6).InnerText;

                        string GraphicType = "Нет данных.";
                        string tarif_vP = "";
                        string tarif_mP = "";
                        try
                        {
                            var m = FloatRegex.Match(peopleP);
                            if (m.Success)
                            {
                                tarif_vP = m.Value;
                                tarif_mP = peopleP.Replace(tarif_vP, "").Trim();
                            }
                        }
                        catch (Exception ex) { }




                        List<string> GraphicDetails = new List<string>();
                        if (GraphicDetail.Count() == 0)
                        {
                            GraphicType = BinManApi.TrimText(GraphicEl.InnerText);
                        }
                        else
                        {
                            GraphicType = BinManApi.TrimText(GraphycType.First().InnerText);
                            if (GraphicDetailValues != null)
                                foreach (var v in GraphicDetailValues) GraphicDetails.Add(BinManApi.TrimText(v.InnerText));
                        }




                        //var GraphicParse = AllMainInfo.ElementAt(Soffest1++).Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-info-line__modal")).First();

                        //var ExactDays = GraphicParse.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("active"));

                        //List<string> Days = new List<string>();

                        //foreach (var day in ExactDays) { Days.Add(TrimText(day.InnerText)); }




                        for (int j = 0; j < countInner; j++)
                        {
                            s1 = nooodse.ElementAt(j + 1);
                            var _MainInfo = s1.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item"));
                            var _Tarif = s1.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                            var _Containers = s1.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                            var _TarifType = s1.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                            var _GraphycType = _MainInfo.ElementAt(5).Descendants("div").Where(d => d.Attributes["class"].Value.Contains("js-show-popover"));
                            // int Soffest1 = 0; //item
                            // int Soffest2 = 0; //tl
                            // int Soffest3 = 0; //val
                            // int Soffest4 = 0; //span

                            string date = _MainInfo.ElementAt(0).InnerText;//Изменено
                            string status = _MainInfo.ElementAt(1).InnerText;//Статус
                            string tax = _Tarif.ElementAt(0).InnerText;//Тариф ч.1
                            string tax_info = _TarifType.ElementAt(0).InnerText;//Тариф ч.2
                            string people = BinManApi.TrimText(_MainInfo.ElementAt(2).InnerText.Trim()).Trim();//Показатель и объем
                            string people_reason = _MainInfo.ElementAt(3).InnerText;//Тариф ч.2
                            string summ = _MainInfo.ElementAt(4).InnerText;//График TODO
                            string graphy = BinManApi.TrimText(_GraphycType.First().InnerText);
                            string container = BinManApi.TrimText(_Containers.ElementAt(0).InnerText);//Контейнер
                                                                                                      //Soffest1++;//На контейнер;
                            string PeriodFrom = _MainInfo.ElementAt(7).InnerText;//Дата от
                            string PeriodTo = _MainInfo.ElementAt(8).InnerText;//Дата до

                            string tarif_v = "";
                            string tarif_m = "";


                            try
                            {
                                var m = FloatRegex.Match(people);
                                 tarif_v = m.Value;
                                    tarif_m = people.Replace(tarif_v,"").Trim();
                               
                            }
                            catch (Exception ex) { }
                           



                            var _GraphicParse = _MainInfo.ElementAt(5).Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-info-line__modal"));

                            IEnumerable<HtmlNode>? _GraphicDetailValues = null;

                            if (_GraphicParse != null && _GraphicParse.Count() > 0)
                            {
                                var _GdTmp = _GraphicParse.First();
                                _GraphicDetailValues = _GdTmp.Descendants("span").Where(d => d.Attributes["class"].Value.Contains("active"));
                            }


                            List<string> _Days = new List<string>();
                            if (_GraphicDetailValues != null)
                                foreach (var day in _GraphicDetailValues) { _Days.Add(BinManApi.TrimText(day.InnerText)); }

                            var From1 = DateTime.MinValue;
                            DateTime.TryParse(PeriodFrom, out From1);

                            var To1 = DateTime.MinValue;
                            DateTime.TryParse(PeriodTo, out To1);


                            var st =  status.Trim() == "Приостановленный" ? DocObjectChangeStatus.Disabled : DocObjectChangeStatus.Active;

                            DocObjectChange dd = new DocObjectChange()
                            {
                                date = date.Trim(),//Изменено
                                status = status.Trim(),//Статус
                                tarif_full_text = tax.Trim(),//Тариф ч.1
                                tarif_info = tax_info.Trim(),//Тариф ч.2
                                people = people.Trim(),//Показатель и объем
                                people_reason = people_reason.Trim(),//Показательи объем ч.2
                                summ = summ.Trim(),//Начисления
                                graphy = graphy.Trim(),//График TODO
                                GraphicDetail = _Days,
                                container = container.Trim(),//График TODO
                                PeriodFrom = PeriodFrom.Trim(),
                                PeriodTo = PeriodTo.Trim(),
                                tarif_volume = tarif_v.Trim(),
                                tarif_measure = tarif_m.Trim(),
                                DT_PeriodFrom = From1,
                                DT_PeriodTo = To1,
                                Status = st,
                            };
                            changes.Add(dd);
                        }

                        #endregion

                        var From = DateTime.MinValue;
                        DateTime.TryParse(PerioudFrom, out From);

                        var To = DateTime.MinValue;
                        DateTime.TryParse(PerioudTo, out To);

                        DocObject doc = new DocObject()
                        {
                            binid = binId,
                            name = name.Trim(),
                            address = addres.Trim(),
                            lot = lot.Trim(),
                            people = peopleP.Trim(),
                            tax = taxP.Trim(),
                            taxSumm = taxSumm.Trim(),
                            Graphic = GraphicType.Trim(),
                            GraphicDetail = GraphicDetails,
                            Container = Container.Trim(),
                            PeriodFrom = PerioudFrom.Trim(),
                            PeriodTo = PerioudTo.Trim(),
                            changes = changes.ToArray(),
                            tarif_volume = tarif_vP.Trim(),
                            tarif_measure = tarif_mP.Trim(),

                            DT_PeriodFrom = From,
                            DT_PeriodTo = To,
                        };

                        t.Wait();
                        if (isObjectTypeParsed)
                        {
                            doc.type = OTParse;
                        }
                        else
                        {
                            Log.Error("Object type wasn't parsed !!!!!!!!!");
                        }

                        result.Add(doc);
                    }
                    return result;
                }

                //resobjs = result;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                ErrorPages.Add(docid);
              //  resobjs = null;
                return false;
            }



        }


        public enum DocumentParseResult{
            Unset,
            NotFound,
            ParseError,
            OK,
            FatalException
        }

        public static DocumentParseResult TryParseDocumentInfo(LoginData ld, string docid, out BinDocumentParse docParse, int GlobalTimeOut=32)
        {
            docParse = null;
            List<DocObject> objs = null;
            BinClientInfo client = null;
            BinDocDetails docdetails = null;
            bool SomeError = false;
            object resLock = new object();
            DocumentParseResult Result = DocumentParseResult.Unset;
            try
            {
                Task Client = Task.Run(() => {
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        Log.Text("Parsing Clients");
                        HttpRequestMessage hm = new(HttpMethod.Get, GetLinkDocumentHatWithClient(docid));
                        var cookie = API.GetDeffaultCookie(ld, "");
                        var req = API.SendRequest(hm,
                               new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                               },
                               cookie,secTimeOut: GlobalTimeOut
                               );

                        var t = req.Content.ReadAsStringAsync();

                        t.Wait();

                        var res = t.Result;

                        var htmlDoc = new HtmlDocument();

                        htmlDoc.LoadHtml(res);
                        sw.Stop();
                        Log.Text("ClientId Http Time: " + sw.ElapsedMilliseconds + " ms");
                        sw.Restart();

                        var HeaderInfo = htmlDoc.DocumentNode.Descendants("div").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("dashboard-header__info"); return res.HasValue ? res.Value : false; });

                        var ClientLink = HeaderInfo.First().Descendants("a").Where(d => d.Attributes["class"].Value is not null and "inherit-link inherit-link--client");
                        var ClientId = CutIdFromUrl(ClientLink.First(), 1);
                        Log.Text("ClientId Parse Time: " + sw.ElapsedMilliseconds + " ms");
                        sw.Stop();
                        sw = null;

                        if (!BinManClientParser.TryParseClientInfo(ld, ClientId , GlobalTimeOut, out client))
                        {
                            SomeError = true;
                            lock (resLock) if (Result != DocumentParseResult.NotFound) Result = DocumentParseResult.ParseError;
                        }
                    }
                    catch(Exception ex) { Log.Error(ex);SomeError = true; lock (resLock)if(Result!= DocumentParseResult.NotFound) Result = DocumentParseResult.ParseError; }

                });
                Task DocumentDetails = Task.Run(() =>
                {
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        Log.Text("Doc Details " + GetLinkDocumentDetail(docid));
                        HttpRequestMessage hm = new(HttpMethod.Get, GetLinkDocumentDetail(docid));
                        var cookie = API.GetDeffaultCookie(ld, "");
                        var req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                               cookie, secTimeOut: GlobalTimeOut
                               );

                        var t = req.Content.ReadAsStringAsync();

                        t.Wait();

                        var res = t.Result;

                        var htmlDoc = new HtmlDocument();

                        htmlDoc.LoadHtml(res);
                        sw.Stop();
                        Log.Text("Http Time: " + sw.ElapsedMilliseconds + " ms");
                        sw.Restart();
                        var all = htmlDoc.DocumentNode.Descendants();
                        var Fields = all.Where(d =>
                        {
                            var a = d.Attributes["name"];
                            return a != null && a.Value != null
                             && a.Value.Contains("CONTRACT");
                        });



                        var type = BinManApi.TrimText(Fields.First(f => f.Attributes["name"].Value.Contains("TYPE")).Descendants("option").FirstOrDefault(d => d.Attributes["selected"] != null)?.InnerText);
                        var Number = Fields.First(f => f.Attributes["name"].Value.Contains("NUMBER")).Attributes["value"].Value;
                        var Group = BinManApi.TrimText(Fields.First(f => f.Attributes["name"].Value.Contains("GROUP")).Descendants("option").FirstOrDefault(d => d.Attributes["selected"] != null)?.InnerText);
                        var DateActiveFrom = Fields.First(f => f.Attributes["name"].Value.Contains("DATE_ACTIVE_FROM")).Attributes["value"].Value;
                        var DateActiveTo = Fields.First(f => f.Attributes["name"].Value.Contains("DATE_ACTIVE_TO")).Attributes["value"].Value;
                        var DateActiveAccept = Fields.First(f => f.Attributes["name"].Value.Contains("DATE_SIGNING")).Attributes["value"].Value;
                        if (type == null) throw new Exception("No type field");
                        if (string.IsNullOrEmpty(Group)) Group = "";
                        if (string.IsNullOrEmpty(type)) type = "";

                        docdetails = new BinDocDetails(docid, Number, DateActiveFrom, DateActiveTo, DateActiveAccept, type, Group);
                        Log.Text("Doc Parse Time: " + sw.ElapsedMilliseconds + " ms");
                        sw.Stop();
                        sw = null;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("DocumentDetailsNotFound");
                        Log.Error(ex);
                        SomeError = true;
                        lock (resLock) Result = DocumentParseResult.NotFound;
                    }
                });
                Task Objects = Task.Run(() =>
                {
                    try
                    {
                        Log.Text("Parsing Objects");
                        Stopwatch sw = Stopwatch.StartNew();
                        Task.WaitAll(DocumentDetails);
                        lock (resLock) if (Result != DocumentParseResult.NotFound)
                                if (!TryParseObjects(ld, docid, out objs, GlobalTimeOut))
                        {
                            SomeError = true;
                            lock (resLock) if (Result != DocumentParseResult.NotFound) Result = DocumentParseResult.ParseError;
                        }
                        Log.Text("Objects Total Time: " + sw.ElapsedMilliseconds + " ms");
                        sw.Stop();
                        sw = null;
                    }
                    catch (Exception ex) { Log.Error(ex); SomeError = true; lock (resLock) if (Result != DocumentParseResult.NotFound) Result = DocumentParseResult.ParseError; }

                });
      

                Task.WaitAll(Client, Objects, DocumentDetails);
                if (!SomeError)
                {
                    docParse = new BinDocumentParse(docdetails, client, objs);
                    Result = DocumentParseResult.OK;
                    return Result;
                }
                return Result;

                // return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);


                return DocumentParseResult.FatalException;
            }



        }
       
        public static List<string> ParseAllDocIdsInPeriod(LoginData ld , DateTime from,DateTime to)
        {
           

           var uri= QueryHelpers.AddQueryString(Url_GetDocuments, FiltersFormData(DateTime.Now, DateTime.Now, 1));



            HttpRequestMessage hm = new(HttpMethod.Post, uri);



            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                   cookie,ld,true
                   );



            var txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //Log.Text(res);
            //string ress = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_objs.html");
            var htmlDoc = new HtmlDocument();
            var res =new List<string>();
            htmlDoc.LoadHtml(txt);


            if(!BinManApi.TryGetPagesCount(htmlDoc,out int PagesCount))
            {
                return null;
            }




            try
            {
                ParsePage(htmlDoc);
            }
            catch(Exception ex) { Log.Error(ex);}


            for(int page= 2; page < PagesCount; page++)
            {
                try
                {
                    uri = QueryHelpers.AddQueryString(Url_GetDocuments, FiltersFormData(DateTime.Now, DateTime.Now, page));



                    hm = new(HttpMethod.Post, uri);



                    cookie = API.GetDeffaultCookie(ld, "");
                    req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                           cookie, ld, true
                           );



                    txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    //Log.Text(res);
                    //string ress = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_objs.html");
                    htmlDoc = new HtmlDocument();
                    
                    htmlDoc.LoadHtml(txt);
                    try
                    {
                        ParsePage(htmlDoc);
                    }
                    catch (Exception ex) { Log.Error("Doc ids parse error"); Log.Error(ex); }
                }
                catch(Exception ex) { Log.Error("Doc ids global error"); Log.Error(ex); }

            }


            void ParsePage(HtmlDocument htmlDoc)
            {

                var urlNodes = htmlDoc.DocumentNode.Descendants("a").Where(d => d.Attributes["href"] !=null && d.Attributes["href"].Value !=null && d.Attributes["href"].Value.Contains("/cabinet/company/contracts/detail/"));
                foreach (var n in urlNodes)
                {
                    var BinId = CutIdFromUrl(n,1);
                    if (!string.IsNullOrEmpty(BinId))
                        res.Add(BinId);
                    else { Log.Error("<BinManDocParse.ParseAllDocIdsInPeriod> Hmm...,  Bin id is null !"); }
                }

            }
            return res;
        }




        public static string CutIdFromUrl(HtmlNode node, int offest = 0)
        {
            return CutIdFromUrl(node.Attributes["href"].Value, offest);
        }
        public static string CutIdFromUrl(string url, int offest = 0)
        {
            return url.Split("/")[^(offest + 1)];
        }

    }
}
