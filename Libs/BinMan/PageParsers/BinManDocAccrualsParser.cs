using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Globalization;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public class BinManDocAccrualsParser
    {
        public static string GetUrl(string Dog_bin_id,int page) => API.BaseUrl + $"cabinet/company/contracts/detail/{Dog_bin_id}/?param=ACCRUALS&PAGEN_1={page}";
        public static string GetAccrualDetail(string Dog_bin_id, string Accr_bin_id) => API.BaseUrl + $"cabinet/company/contracts/detail/{Dog_bin_id}/?action=getAccrual&id={Accr_bin_id}";
        public class DocAccrualListInfo {
            [JsonConverter(typeof(MicrosecondEpochConverter))]
            public DateTime DATE_ACCURAL_TIMESTAMP { get; set; }
            /// <summary>
            /// Начисление
            /// </summary>
            public float SUMM { get; set; }
            /// <summary>
            /// Платеж
            /// </summary>
            public float payment;
            public string randomComment;
            /// <summary>
            /// idNach
            /// </summary>
            public string ID { get; set; }
            public DocAccrualListInfo HeadRow;
            public bool IsNach;
            public List<DocAccrualListInfo> childs = new List<DocAccrualListInfo>();
        }
        public class MicrosecondEpochConverter : DateTimeConverterBase
        {
            private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds + "000");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.Value == null) { return null; }
                return _epoch.AddMilliseconds((long)reader.Value / 1000d);
            }
        }
        public class BinManAccrualDetailsResponse : DocAccrualListInfo
        {
           
            public string CREATED_BY { get; set; }
            public string CREATED_AT { get; set; }
            public string DATE_ACCURAL { get; set; }
            public string DATE_ACCURAL_PAY { get; set; }
            public string PERIOD_FROM { get; set; }
            public string PERIOD_TO { get; set; }
            public string VOLUME { get; set; }
            public string? SUMM_PAY { get; set; }
            public string COMMENT { get; set; }
            public string ACCURAL_BASIS { get; set; }
            public string OPERATION_TYPE { get; set; }
            public string? PARENT_ID { get; set; }
            public string? PAY_UID { get; set; }
            public string? CORRECTION_SUMM { get; set; }
            public string in_check { get; set; }
            public string? in_check_date { get; set; }
            public string? in_check_user { get; set; }
            public string basis_name { get; set; }
            public string type_name { get; set; }
            public string? tarif_name { get; set; }
            public string? tarif_value { get; set; }
            public string? tarif_unit { get; set; }
            public string DATE_CLOSE_PERIOD { get; set; }
            public string user_name { get; set; }
            public string user_email { get; set; }
            [JsonConverter(typeof(MicrosecondEpochConverter))]
            public DateTime CREATED_AT_TIMESTAMP { get; set; }
            [JsonConverter(typeof(MicrosecondEpochConverter))]
            public DateTime DATE_CLOSE_PERIOD_TIMESTAMP { get; set; }
            public string in_check_user_name { get; set; }
            public string? in_check_user_email { get; set; }
            public string in_check_user_date { get; set; }
            public string? CHILDS { get; set; }
            public string? LAST_CHILD_ID { get; set; }
            public float CORRECTED_SUM { get; set; }
        }


        public static IEnumerable<DocAccrualListInfo> ForEachAccrual(LoginData ld, string Dog_bin_Id)
        {
            var htmlDoc = GetPage(ld, Dog_bin_Id, 1);
            if (TryParsePage(htmlDoc, out var res))
            {
                foreach (var accr in res) yield return accr;
                if (BinManApi.TryGetPagesCount(htmlDoc, out var PageCount))
                {
                    for (int i = 2; i <= PageCount; i++)
                    {


                        htmlDoc = GetPage(ld, Dog_bin_Id, i);
                        if (TryParsePage(htmlDoc, out res)) foreach (var accr in res) yield return accr;
                    }
                }
            }
            
        }


        public static BinManAccrualDetailsResponse GetAccrualDetails(LoginData ld, string Dog_bin_id,string Accr_bin_id)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, GetAccrualDetail(Dog_bin_id,Accr_bin_id));
            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm, null,
                   cookie
                   );

            var txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            try
            {
                var resp = JsonConvert.DeserializeObject<BinManAccrualDetailsResponse>(txt);

                return resp;
            }
            catch (Exception ex)
            {
                Log.Error(txt, ex);
                return null;
            }
        }
        //public static FullAccrualsDetails ParseAccrualDetails(string txt)
        //{
        //    var res = new FullAccrualsDetails();

        //    var 
        //}
        public enum SearchCommand
        {
            DoNothing,
            DoAction,
            Break
        }
        public static bool TrySearchUntil(Func<DocAccrualListInfo, SearchCommand> SendRequestDetails, Func<BinManAccrualDetailsResponse, SearchCommand> func, LoginData ld, string Dog_bin_Id, out BinManAccrualDetailsResponse res)
        {
            var htmlDoc= GetPage(ld, Dog_bin_Id, 1);
            if (TryFind(SendRequestDetails, func, ld, Dog_bin_Id, htmlDoc, out res)) return true;
            else
            if (BinManApi.TryGetPagesCount(htmlDoc, out var PageCount)) {
                for (int i = 2; i <= PageCount; i++)
                {


                    htmlDoc = GetPage(ld, Dog_bin_Id, i);
                    if (TryFind(SendRequestDetails, func, ld, Dog_bin_Id, htmlDoc, out res)) return true;
                    else continue;//...
                }
            }
            return false;
        }
        public static HtmlDocument GetPage(LoginData ld, string Dog_bin_Id,int pageN)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, GetUrl(Dog_bin_Id, pageN));
            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
              new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                         },
                   cookie
                   );

            var txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(txt);
            return htmlDoc;
        }
        public static bool TryFind(Func<DocAccrualListInfo, SearchCommand> SendRequestDetails, Func<BinManAccrualDetailsResponse, SearchCommand> func,LoginData ld,string Dog_bin_Id, HtmlDocument htmlDoc, out BinManAccrualDetailsResponse res)
        {

            res = null;

            HashSet<string> AlreadyParsed = new HashSet<string>();
            var HeaderRows = htmlDoc.DocumentNode.Descendants("div").Where(x => {
                var val = x.Attributes?["class"]?.Value;
                if (val == null) return false;
                var res = val.Contains("data-list__row");
                res &= !val.Contains("data-header");
                if (res && val.Contains("data-list__row_big"))
                {
                    res = !val.Contains("b-accruals-row");
                }
                return res;
            });
            Log.Text($"Searching Accrual in {Dog_bin_Id}");
            foreach (var Head in HeaderRows.Reverse())
            {
                var rows = Head.Descendants("div").Where(x => {
                    var val = x.Attributes?["class"]?.Value;
                    if (val == null) return false;
                    var res = val.Contains("data-list__row");
                    res &= !val.Contains("data-header");
                    if (res && val.Contains("data-list__row_big"))
                    {
                        res = val.Contains("b-accruals-row");
                    }
                    return res;
                }); 
                var col = 0;
                var cols =Head.Descendants("div").Where(d => d.Attributes?["class"]?.Value?.Contains("col-auto") == true);

                //  var id = row.Attributes?["id"]?.Value;
                //  if (id == null ) { Log.Warning("А откуда id = null, что-то спарсилось не так ?!"); continue; }
                var dateAccHead = DateTime.Parse(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText));
                var summNach_txtHead = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                float.TryParse(summNach_txtHead, CultureInfo.InvariantCulture,out var summNachHead);
                var paymentHead = 0.0f;
                if (cols.Count() > 4)// col++;
                                     //else
                {
                    var payment_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                    if (payment_txt == "-") paymentHead = 0;
                    else float.TryParse(payment_txt, CultureInfo.InvariantCulture, out paymentHead);
                }
                var randomCommentHead = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                var idHead = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);
                var Headinf = new DocAccrualListInfo()
                {
                    DATE_ACCURAL_TIMESTAMP = dateAccHead,
                    ID = idHead,
                    //payment=  ,
                    SUMM = summNachHead,
                    randomComment = randomCommentHead
                };
                foreach (var row in rows.Reverse())
                {

                    try
                    {
                        col = 0;
                        //if(Dog_bin_Id == "648759")
                        //{

                        //}
                        cols = row.Descendants("div").Where(d => d.Attributes?["class"]?.Value?.Contains("col-auto") == true);
                        try
                        {
                            var dateAcc = DateTime.Parse(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText));
                            var summNach_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                            //var summNach = "0";
                            float.TryParse(summNach_txt, CultureInfo.InvariantCulture, out var summNach);
                            var payment = 0.0f;
                            if (cols.Count() > 4)// col++;
                                                 //else
                            {
                                var payment_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                                if (payment_txt == "-") payment = 0;
                                else float.TryParse(payment_txt, CultureInfo.InvariantCulture, out payment);
                            }
                            var randomComment = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            var id = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            if (!AlreadyParsed.Add(id)) continue;

                            var inf = new DocAccrualListInfo()
                            {
                                DATE_ACCURAL_TIMESTAMP = dateAcc,
                                ID = id,
                                //payment=  ,
                                SUMM = summNach,
                                randomComment = randomComment,
                                HeadRow = Headinf
                            };
                            var code = SendRequestDetails(inf);
                            if (code == SearchCommand.DoAction)
                            {
                                try
                                {
                                    res = GetAccrualDetails(ld, Dog_bin_Id, id);
                                    if (res != null)
                                    {
                                        var code1 = func(res);
                                        if (code1 == SearchCommand.DoAction)
                                        {
                                            Log.Text($"Searching Accrual in {Dog_bin_Id}: Finded! ({inf.ID})"); return true;
                                        }
                                        else if (code == SearchCommand.Break) break;

                                    }
                                    else continue;
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("AccParse", ex);
                                }
                            }
                            else if (code == SearchCommand.Break) break; else continue;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("AccParse2.1", ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("AccParse2", ex);
                    }
                }
            }
            {
                var rows = htmlDoc.DocumentNode.Descendants("div").Where(x => {
                    var val = x.Attributes?["class"]?.Value;
                    if (val == null) return false;
                    var res = val.Contains("data-list__row");
                    res &= !val.Contains("data-header");
                    if (res && val.Contains("data-list__row_big"))
                    {
                        res = val.Contains("b-accruals-row");
                    }
                    return res;
                });

                foreach (var row in rows.Reverse())
                {

                    try
                    {
                        var col = 0;

                        var cols = row.Descendants("div").Where(d => d.Attributes?["class"]?.Value?.Contains("col-auto") == true);
                        try
                        {
                            var dateAcc = DateTime.Parse(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText));
                            var summNach_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                            float.TryParse(summNach_txt, CultureInfo.InvariantCulture, out var summNach);
                            var payment = 0.0f;
                            if (cols.Count() > 4)// col++;
                                                 //else
                            {
                                var payment_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                                if (payment_txt == "-") payment = 0;
                                else float.TryParse(payment_txt, CultureInfo.InvariantCulture, out payment);
                            }
                            var randomComment = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            var id = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            if (!AlreadyParsed.Add(id)) continue;

                            var inf = new DocAccrualListInfo()
                            {
                                DATE_ACCURAL_TIMESTAMP = dateAcc,
                                ID = id,
                                //payment=  ,
                                SUMM = summNach,
                                randomComment = randomComment,
                            };
                            var code = SendRequestDetails(inf);
                            if (code == SearchCommand.DoAction)
                            {
                                try
                                {
                                    res = GetAccrualDetails(ld, Dog_bin_Id, id);
                                    if (res != null)
                                    {
                                        var code1 = func(res);
                                        if (code1 == SearchCommand.DoAction)
                                        {
                                            Log.Text($"Searching Accrual in {Dog_bin_Id}: Finded! ({inf.ID})"); return true;
                                        }
                                        else if (code == SearchCommand.Break) break;

                                    }
                                    else continue;
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("AccParse", ex);
                                }
                            }
                            else if (code == SearchCommand.Break) break; else continue;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("AccParse2.1", ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("AccParse2", ex);
                    }
                }
            }

            return false;
        }
        
    
        public static bool TryParsePage(HtmlDocument htmlDoc,out List<DocAccrualListInfo> res)
        {

            res = new List<DocAccrualListInfo>();

            HashSet<string> AlreadyParsed = new HashSet<string>();
            var HeaderRows = htmlDoc.DocumentNode.Descendants("div").Where(x => {
                var val = x.Attributes?["class"]?.Value;
                if (val == null) return false;
                var res = val.Contains("data-list__row");
                res &= !val.Contains("data-header");
                //if (res && val.Contains("data-list__row_big"))
                //{
                //    res = !val.Contains("b-accruals-row");
                //}
                res &= x.ChildNodes.Count > 1;
                return res;
            });

            foreach (var Head in HeaderRows.Reverse())
            {
                var rows = Head.Descendants("div").Where(x => {
                    var val = x.Attributes?["class"]?.Value;
                    if (val == null) return false;
                    var res = val.Contains("data-list__row");
                    res &= !val.Contains("data-header");
                    if (res && val.Contains("data-list__row_big"))
                    {
                        res = val.Contains("b-accruals-row");
                    }
                    return res;
                });
                var col = 0;
                var cols = Head.Descendants("div").Where(d => d.Attributes?["class"]?.Value?.Contains("col-auto") == true);

                //  var id = row.Attributes?["id"]?.Value;
                //  if (id == null ) { Log.Warning("А откуда id = null, что-то спарсилось не так ?!"); continue; }
                var dateAccHead = DateTime.Parse(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText));
                var summNach_txtHead = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                float.TryParse(summNach_txtHead, CultureInfo.InvariantCulture, out var summNachHead);
                var paymentHead = 0.0f;
                if (cols.Count() > 4)// col++;
                                     //else
                {
                    var payment_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                    if (payment_txt == "-") paymentHead = 0;
                    else float.TryParse(payment_txt, CultureInfo.InvariantCulture, out paymentHead);
                }
                var randomCommentHead = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                var idHead = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);
                var Headinf = new DocAccrualListInfo()
                {
                    DATE_ACCURAL_TIMESTAMP = dateAccHead,
                    ID = idHead,
                    //payment=  ,
                    SUMM = summNachHead,
                    randomComment = randomCommentHead
                };
                foreach (var row in rows.Reverse())
                {

                    try
                    {
                        col = 0;
                        //if(Dog_bin_Id == "648759")
                        //{

                        //}
                        cols = row.Descendants("div").Where(d => d.Attributes?["class"]?.Value?.Contains("col-auto") == true);
                        try
                        {
                            var dateAcc = DateTime.Parse(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText));
                            var summNach_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                            //var summNach = "0";
                            float.TryParse(summNach_txt, CultureInfo.InvariantCulture, out var summNach);
                            var payment = 0.0f;
                            if (cols.Count() > 4)// col++;
                                                 //else
                            {
                                var payment_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                                if (payment_txt == "-") payment = 0;
                                else float.TryParse(payment_txt, CultureInfo.InvariantCulture, out payment);
                            }
                            var randomComment = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            var id = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            if (!AlreadyParsed.Add(id)) continue;

                            var inf = new DocAccrualListInfo()
                            {
                                DATE_ACCURAL_TIMESTAMP = dateAcc,
                                ID = id,
                                //payment=  ,
                                SUMM = summNach,
                                randomComment = randomComment,
                                HeadRow = Headinf
                            };
                            Headinf.childs.Add(inf);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("AccParse2.1", ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("AccParse2", ex);
                    }
                }
                res.Add(Headinf);
            }
            {
                var rows = htmlDoc.DocumentNode.Descendants("div").Where(x => {
                    var val = x.Attributes?["class"]?.Value;
                    if (val == null) return false;
                    var res = val.Contains("data-list__row");
                    res &= !val.Contains("data-header");
                    //if (res && val.Contains("data-list__row_big"))
                    //{
                    //    res = val.Contains("b-accruals-row");
                    //}
                    res &= x.ChildNodes.Count <= 1;
                    return res;
                });

                foreach (var row in rows.Reverse())
                {

                    try
                    {
                        var col = 0;

                        var cols = row.Descendants("div").Where(d => d.Attributes?["class"]?.Value?.Contains("col-auto") == true);
                        try
                        {
                            var dateAcc = DateTime.Parse(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText));
                            var summNach_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                            float.TryParse(summNach_txt, CultureInfo.InvariantCulture, out var summNach);
                            var payment = 0.0f;
                            bool isNach = true;
                            if (cols.Count() > 4)// col++;
                                                 //else
                            {
                                var payment_txt = string.Concat(BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText).Replace(",", ".").Where(c => char.IsDigit(c) || c == '.'));
                                if (payment_txt == "-") payment = 0;
                                else isNach = !float.TryParse(payment_txt, CultureInfo.InvariantCulture, out payment);
                            }
                            var randomComment = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);

                            var id = BinManApi.TrimText(cols.ElementAt(col++).Descendants("strong").First().InnerText);
                            if ( id == "Сгруппированное начисление")
                            {

                            }
                            if (!AlreadyParsed.Add(id)) continue;

                            var inf = new DocAccrualListInfo()
                            {
                                DATE_ACCURAL_TIMESTAMP = dateAcc,
                                ID = id,
                                //payment=  ,
                                SUMM = isNach ? summNach : payment,
                                IsNach = isNach,
                                randomComment = randomComment,
                            };
                            res.Add(inf);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("AccParse2.1", ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("AccParse2", ex);
                    }
                }
            }

            return true;
        }
    }
}
