using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using BinManParser.Api;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web;
using static System.Net.WebRequestMethods;

namespace AndroidAppServer.Libs.BinMan
{
    // ID =
    // &ACCURAL_BASIS = 19
    // & DATE_ACCURAL = 28.09.2023
    // & PERIOD_FROM =
    // &PERIOD_TO =
    // &VOLUME =
    // &VOLUME =
    // &PERIOD_FROM =
    // &PERIOD_TO =
    // &SUMM = 1.111 ₽ // ' ₽' -- ???
    // & SUMM_PRECISION = 0
    // & COMMENT =//just text
    // get

    public enum AccrualsCreationResult
    {
        Ok,
        NoErrorButNotFound,
            Failed,
    }

    public enum AccrualsType
    {
        /// <summary> Начисление по договору  </summary>
        accr_by_doc =1,
        ///<summary>Начисление по заявке или за дополнительный вывоз </summary>
        accr_by_req_or_add =2,
        ///<summary>Пеня</summary>
        penny =4,
        /// <summary>Начисление на основании дополнительных вывозов и заявок </summary>
        accr_based_on_req_or_add =18,
        /// <summary> Произвольная сумма </summary>
        accr_any_summ =19,
        /// <summary> СТОРНО </summary>
        STORNO =20
    }
    public class BinManAcctualBase
    {
        public string db_guid;
        public DateTime? date;
        public string comment;
        public string doc_BinId = "";
        //public AccrualsType _tt;
        //public AccrualsType type { get => _tt; set { typeRaw = Enum.GetName(value); _tt = value; } }
        public string typeRaw;
    }
    //public class BinManAccrualCorrect: BinManAcctualBase
    //{
        
    //   // public string parentBinId;
    //   // public string correctSumm;
    //    //public string FinalSumm;
      
        
    //    //public AccrualsType type = 
    //}
    public class BinManAccrual: BinManAcctualBase
    {
       
        public DateTime? date;
        public DateTime? dateFrom;
        public DateTime? dateTo;
        public string volume = "";
        public string doc_BinId = "";
        public string parentBinId;
        /// <summary>
        ///  
        /// </summary>
        public string summ = "";
        public string finalSumm = "";
        public string comment = "";
        public string db_guid;
        public BinManAccrual()
        {
        }
    }
  public class BinManDocAccruals
    {
        public const string RequestUrl = API.BaseUrl + "cabinet/company/contracts/detail/";
        public static string AccrualDetail(string bin_id) => API.BaseUrl + "cabinet/company/contracts/detail/5906259/?action=getAccrual&id=57213386";
        private static string GetSummByDates(string dog_bin_id, DateTime From, DateTime To) => API.BaseUrl + $"cabinet/company/contracts/detail/{dog_bin_id}/?action=getSumByDates&date_from={From.ToString("dd.MM.yyyy")}&date_to={To.ToString("dd.MM.yyyy")}";
        //    public static BinManAccrualDetailsResponse GetAccrualDetails(LoginData ld, string bin_id)
        //    {
        //        HttpRequestMessage hm = new(HttpMethod.Get, AccrualDetail(bin_id));
        //        var cookie = API.GetDeffaultCookie(ld, "");
        //        var req = API.SendRequest(hm,
        //null,
        //cookie
        //);
        //        string res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();


        public static bool TryGetAccrualSumm(LoginData ld , string dog_bin_id,DateTime from, DateTime to,out float Summ)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, GetSummByDates(dog_bin_id,from,to));
            var cookie = API.GetDeffaultCookie(ld, "");
            //   Log.Text(hm.ToString());
            //   Log.Text(hm.RequestUri.Query);
            //Log.Action("Bin Add acc to doc", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date}");


            var req = API.SendRequest(hm,
                         new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
            },
                cookie
                );

            string res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if(! float.TryParse(res.Replace(".",","), out Summ))
            {
                Log.Error(res);
                return false;
            }
            else
            {
                return true;
            }
            
        }

        //    }
        /// <summary>
        /// Только если был указан db_guid будет bin_id и осуществляется поиск по сайту это guid'a в коментариях
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="accr"></param>
        /// <param name="bin_id">Только если был указан db_guid</param>
        /// <returns></returns>
        public static AccrualsCreationResult AddAccrualToDoc(LoginData ld, BinManAccrual accr,out string bin_id)
        {
           string email = MailService.DefaultEmail;
            if (string.IsNullOrEmpty(accr.db_guid))
            {
                Log.Warning("accr.db_guid -Не указан ?");
              //  accr.db_guid = Guid.NewGuid().ToString();

            }
            var summ= accr.summ.Replace(",", ".");
            var from = accr.dateFrom.HasValue ? accr.dateFrom.Value.ToString("dd.MM.yyyy") : "";
            var to = accr.dateTo.HasValue ? accr.dateTo.Value.ToString("dd.MM.yyyy") : "";
            var volume = accr.volume.Replace(",", ".");
            var date = accr.date.HasValue ? accr.date.Value.ToString("dd.MM.yyyy") : "";
            var type = (accr.typeRaw).ToString();

            var uriBuilder = new UriBuilder(RequestUrl+accr.doc_BinId+"/");
            //var parameters = HttpUtility.ParseQueryString(string.Empty);
            //parameters["action"] = "add_accrual";
            //parameters["ID"] = "";
            //parameters["ACCURAL_BASIS"] =type;
            //parameters["DATE_ACCURAL"] =date;
            //parameters["PERIOD_FROM"] = (from);
            //parameters["PERIOD_TO"] = (to);
            //parameters["VOLUME"] = (volume);
            //parameters["VOLUME"] =(volume);
            //parameters["PERIOD_FROM"] = (from);
            //parameters["PERIOD_TO"] = (to);
            //parameters["SUMM"] =(accr.summ.Replace(",", ".") + " ₽");
            //parameters["SUMM_PRECISION"] ="0";
            //parameters["COMMENT"] =accr.comment;

            //string qr =
            //    $"?action=add_accrual&data=ID=&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&PERIOD_FROM={from}&PERIOD_TO={to}&VOLUME={volume}" +
            //    $"&VOLUME={volume}&PERIOD_FROM={from}&PERIOD_TO={to}&SUMM={summ}₽&SUMM_PRECISION=0&COMMENT={accr.db_guid+" | \n"+accr.comment}&";
            var parameters = HttpUtility.ParseQueryString(string.Empty);


                                    if (type == "19")
                        {
                parameters["action"] = "add_accrual";
                parameters["data"] = $"ID=&ACCURAL_BASIS={type}&DATE_ACCURAL={date}" +
                    $"&SUMM={summ} ₽&SUMM_PRECISION=0&COMMENT={(string.IsNullOrEmpty(accr.db_guid) ? accr.comment : accr.db_guid + " | \n" + accr.comment)}&";
            }
                        else
                        {
                            parameters["action"] = "add_accrual";
                            parameters["data"] = $"ID=&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&PERIOD_FROM={from}&PERIOD_TO={to}&VOLUME={volume}&VOLUME={volume}&PERIOD_FROM={from}&PERIOD_TO={to}&SUMM={summ} ₽&SUMM_PRECISION=0&COMMENT={(string.IsNullOrEmpty(accr.db_guid) ? accr.comment : accr.db_guid + " | \n" + accr.comment)}&";
                        }

            uriBuilder.Query = parameters.ToString();


            //?ID=&ACCURAL_BASIS=19&DATE_ACCURAL=28.09.2023&PERIOD_FROM=&PERIOD_TO=&VOLUME=&VOLUME=&PERIOD_FROM=&PERIOD_TO=&SUMM=1.111%20%E2%82%BD&SUMM_PRECISION=0&COMMENT=%D0%A2%D0%B5%D1%81%D1%82&
            //ID=&ACCURAL_BASIS=19&DATE_ACCURAL=28.09.2023&PERIOD_FROM=&PERIOD_TO=&VOLUME=&VOLUME=&PERIOD_FROM=&PERIOD_TO=&SUMM=1.111+%E2%82%BD&SUMM_PRECISION=0&COMMENT=%D0%A2%D0%B5%D1%81%D1%82&

            //ID=&ACCURAL_BASIS=19&DATE_ACCURAL=28.09.2023&PERIOD_FROM=&PERIOD_TO=&VOLUME=&VOLUME=&PERIOD_FROM=&PERIOD_TO=&SUMM=1.111+%E2%82%BD&SUMM_PRECISION=0&COMMENT=%D0%A2%D0%B5%D1%81%D1%82&
            //ID=&ACCURAL_BASIS=19&DATE_ACCURAL=28.09.2023&PERIOD_FROM=&PERIOD_TO=&VOLUME=&VOLUME=&PERIOD_FROM=&PERIOD_TO=&SUMM=1.111%20%E2%82%BD&SUMM_PRECISION=0&COMMENT=%D0%A2%D0%B5%D1%81%D1%82&
           // uriBuilder.Query = qr;
                //parameters.ToString();

          //  Log.Text(parameters.ToString());
            HttpRequestMessage hm = new(HttpMethod.Get, uriBuilder.Uri);
            var cookie = API.GetDeffaultCookie(ld, "");
            //   Log.Text(hm.ToString());
            //   Log.Text(hm.RequestUri.Query);
            Log.Action("Bin Add acc to doc", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date}");


            var req = API.SendRequest(hm,
                null,
                cookie
                );

            string res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();//14.06.2024 Здесь всегда приходит форма?

            
            if (res.Length > 100)
            {
                if (!string.IsNullOrEmpty(accr.db_guid)) {
                    if (BinManDocAccrualsParser.TrySearchUntil(
                        (inf) => {
                          return  (inf.DATE_ACCURAL_TIMESTAMP.Date == (accr.date.HasValue ? accr.date.Value.Date : DateTime.Today))
                            ?BinManDocAccrualsParser.SearchCommand.DoAction:BinManDocAccrualsParser.SearchCommand.DoNothing;
                            },
                        (d) => {
                            return (d.COMMENT.ToLower().Contains(accr.db_guid.ToLower()))
                             ? BinManDocAccrualsParser.SearchCommand.DoAction : BinManDocAccrualsParser.SearchCommand.DoNothing;
                        }
                        ,ld,accr.doc_BinId,out var accrInfo))
                    {
                        bin_id = accrInfo.ID;
                        uriBuilder = new UriBuilder(RequestUrl + accr.doc_BinId + "/");


                        parameters = HttpUtility.ParseQueryString(string.Empty);
                        parameters["action"] = "add_accrual";
                        if (type == "19")
                        {
                            parameters["data"] = $"ID={bin_id}&ACCURAL_BASIS={type}&DATE_ACCURAL={date}" +
                                $"&SUMM={summ} ₽&SUMM_PRECISION=0&COMMENT={accr.comment}&";
                        }
                        else
                        {

                        }

                        uriBuilder.Query = parameters.ToString();

                        hm = new(HttpMethod.Get, uriBuilder.Uri);
                        cookie = API.GetDeffaultCookie(ld, "");
                        //   Log.Text(hm.ToString());
                        //   Log.Text(hm.RequestUri.Query);
                        Log.Action("Bin removing accrual commentary", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date} + URI: {uriBuilder.Uri}");


                        req = API.SendRequest(hm,
                           null,
                           cookie
                           ,skipLogin:true
                           );

                        res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (res.Length > 100)
                        {
                            Log.Action("Bin removing accrual commentary", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date} - Success");
                        }
                        else
                        {
                            Log.Action("Bin removing accrual commentary", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date} - Failed With:{res}");

                        }

                        return AccrualsCreationResult.Ok;
                    }
                }

                bin_id = string.Empty;

                return AccrualsCreationResult.NoErrorButNotFound;
            }
            else
            {
                Log.Error(res);
                bin_id = string.Empty;
                return AccrualsCreationResult.Failed;
            }
        }
        public static AccrualsCreationResult CreateCorrectir(LoginData ld, BinManAccrual accr, out string bin_id)
        {
            if (string.IsNullOrEmpty(accr.parentBinId)) { bin_id = string.Empty; return AccrualsCreationResult.Failed; }

            var summ = accr.summ.Replace(",", ".");
            var summFinal = accr.finalSumm.Replace(",", ".");
            var from = accr.dateFrom.HasValue ? accr.dateFrom.Value.ToString("dd.MM.yyyy") : "";
            var to = accr.dateTo.HasValue ? accr.dateTo.Value.ToString("dd.MM.yyyy") : "";
            var volume = accr.volume.Replace(",", ".");
            var date = accr.date.HasValue ? accr.date.Value.ToString("dd.MM.yyyy") : "";
            var type = (accr.typeRaw).ToString();
          
            var fSumm = accr.summ.Replace(",", ".");


            var uriBuilder = new UriBuilder(RequestUrl + accr.doc_BinId + "/");


            //string qr =
            //    $"?action=add_accrual&data=ID=&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&PERIOD_FROM={from}&PERIOD_TO={to}&VOLUME={volume}" +
            //    $"&VOLUME={volume}&PERIOD_FROM={from}&PERIOD_TO={to}&SUMM={summ}₽&SUMM_PRECISION=0&COMMENT={accr.comment}&";
            var parameters = HttpUtility.ParseQueryString(string.Empty);

            parameters["action"] = "add_accrual";
            parameters["data"] = $"PARENT_ID={accr.parentBinId}&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&SUMM_CORRECT={summ}&SUMM={summFinal}&COMMENT={(string.IsNullOrEmpty(accr.db_guid) ? accr.comment : accr.db_guid + " | \n" + accr.comment)}";

            uriBuilder.Query = parameters.ToString();



            HttpRequestMessage hm = new(HttpMethod.Get, uriBuilder.Uri);
            var cookie = API.GetDeffaultCookie(ld, "");
            //   Log.Text(hm.ToString());
            //   Log.Text(hm.RequestUri.Query);
            Log.Action("Bin Add Создана корректировка", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date}");


            var req = API.SendRequest(hm,
                null,
                cookie
                );

            var t = req.Content.ReadAsStringAsync();

            t.Wait();

            string res = t.Result;
            if (res.Length > 100)
            {
                if (!string.IsNullOrEmpty(accr.db_guid))
                {
                    if (BinManDocAccrualsParser.TrySearchUntil(
                        (inf) => {

                            return (inf.DATE_ACCURAL_TIMESTAMP.Date == (accr.date.HasValue ? accr.date.Value.Date : DateTime.Today))
                             ?BinManDocAccrualsParser.SearchCommand.DoAction:BinManDocAccrualsParser.SearchCommand.DoNothing;
                        },
                        (d) => {
                            return (d.COMMENT.ToLower().Contains(accr.db_guid.ToLower()))
                             ? BinManDocAccrualsParser.SearchCommand.DoAction : BinManDocAccrualsParser.SearchCommand.DoNothing;
                        }
                        , ld, accr.doc_BinId, out var accrInfo))
                    {
                        bin_id = accrInfo.ID;
                        uriBuilder = new UriBuilder(RequestUrl + accr.doc_BinId + "/");


                        parameters = HttpUtility.ParseQueryString(string.Empty);
                        if (type == "19")
                        {
                            parameters["action"] = "add_accrual";
                            parameters["data"] = $"ID={bin_id}&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&SUMM={summ} ₽&SUMM_PRECISION=0&COMMENT={accr.comment}&";
                        }
                        else if (type == "1")
                        {
                            parameters["action"] = "add_accrual";
                            parameters["data"] = $"ID={bin_id}&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&SUMM={summ} ₽&SUMM_PRECISION=0&COMMENT={accr.comment}&";

                        }
                        else 
                        {
                            parameters["action"] = "add_accrual";
                            parameters["data"] = $"ID=&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&PERIOD_FROM={from}&PERIOD_TO={to}&VOLUME={volume}&VOLUME={volume}&PERIOD_FROM={from}&PERIOD_TO={to}&SUMM={summ} ₽&SUMM_PRECISION=0&COMMENT={(string.IsNullOrEmpty(accr.db_guid) ? accr.comment : accr.db_guid + " | \n" + accr.comment)}&";

                        }

                        uriBuilder.Query = parameters.ToString();

                        hm = new(HttpMethod.Get, uriBuilder.Uri);
                        cookie = API.GetDeffaultCookie(ld, "");
                        //   Log.Text(hm.ToString());
                        //   Log.Text(hm.RequestUri.Query);
                        Log.Action("Bin removing accrual commentary", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date} + URI: {uriBuilder.Uri}");


                        req = API.SendRequest(hm,
                           null,
                           cookie
                           );

                        res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (res.Length > 100)
                        {
                            Log.Action("Bin removing accrual commentary", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date} - Success");
                        }
                        else
                        {
                            Log.Action("Bin removing accrual commentary", $"doc: {accr.doc_BinId}, {accr.comment}, {accr.summ}, {accr.date} - Failed With:{res}");

                        }
                        return AccrualsCreationResult.Ok;
                    }
                }

                bin_id = string.Empty;

                return AccrualsCreationResult.NoErrorButNotFound;
               // return true;
            }
            else
            {
                bin_id = string.Empty;
                Log.Error(res);
                return AccrualsCreationResult.Failed;
            }
        }
        public static bool DeleteAccrual(LoginData ld, string acc_binId,string doc_Binid)
        {



            var uriBuilder = new UriBuilder(RequestUrl + doc_Binid + "/");


            //string qr =
            //    $"?action=add_accrual&data=ID=&ACCURAL_BASIS={type}&DATE_ACCURAL={date}&PERIOD_FROM={from}&PERIOD_TO={to}&VOLUME={volume}" +
            //    $"&VOLUME={volume}&PERIOD_FROM={from}&PERIOD_TO={to}&SUMM={summ}₽&SUMM_PRECISION=0&COMMENT={accr.comment}&";
            var parameters = HttpUtility.ParseQueryString(string.Empty);

            parameters["action"] = "accruals_delete";
            parameters["filter"] = $"DATE_CREATE_FROM=&DATE_CREATE_TO=&DATE_ACCURAL_FROM=&DATE_ACCURAL_TO=&in_check=";
            parameters["id"] = acc_binId;

            uriBuilder.Query = parameters.ToString();



            HttpRequestMessage hm = new(HttpMethod.Get, uriBuilder.Uri);
            var cookie = API.GetDeffaultCookie(ld, "");
            //   Log.Text(hm.ToString());
            //   Log.Text(hm.RequestUri.Query);
            Log.Action("Bin Удаление начисления", $"doc:{doc_Binid}->{acc_binId}");


            var req = API.SendRequest(hm,
                null,
                cookie
                );

            var t = req.Content.ReadAsStringAsync();

            t.Wait();

            string res = t.Result;
            if (res.Length > 100)
            {
                return true;
            }
            else
            {
                Log.Error(res);
                return false;
            }
        }
    }
}
