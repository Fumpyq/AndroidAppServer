using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan;
using Dadata.Model;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;
//CLIENT[ID]: 
//sessid: 3a5cf347c7418eb6ce5f442e60839d36
//CLIENT[COMPANY]: 111275
//CLIENT[TYPE]: INDIVIDUAL
//CLIENT[F_SURNAME]: Тестовый
//CLIENT[F_NAME]: Тести
//CLIENT[F_PATRONYMIC]: Тестович
//CLIENT[PASSPORT_CODE]: 
//CLIENT[PASSPORT_NUMBER]: 
//CLIENT[INN]: 
//CLIENT[F_REGION]: 
//CLIENT[F_RAION]: 
//CLIENT[F_CITY]: 
//CLIENT[F_SETTLEMENT]: 
//CLIENT[F_STREET]: 
//CLIENT[F_HOUSE]: 
//CLIENT[F_BLOCK]: 
//CLIENT[F_ROOM]: 
//CLIENT[POSTAL_CODE]: 
//CLIENT[FIAS_ID]: 
//CLIENT[PHONE][]: 
//CLIENT[PHONE][]: 
//CLIENT[EMAIL][]: 
//CLIENT[FACT_ADDRESS_SAME]: 
//CLIENT[REGION_FACT]: 
//CLIENT[RAION_FACT]: 
//CLIENT[CITY_FACT]: 
//CLIENT[SETTLEMENT_FACT]: 
//CLIENT[STREET_FACT]: 
//CLIENT[HOUSE_FACT]: 
//CLIENT[F_BLOCK_FACT]: 
//CLIENT[OFFICE_FACT]: 
//CLIENT[POSTAL_CODE_FACT]: 
//CLIENT[FIAS_ID_FACT]: 
//CLIENT[PHONE][]: 
//CLIENT[EMAIL][]: 
//CLIENT[POST_NAME]: 
//CLIENT[POST]: 
//CLIENT[NAME_OF_BANK]: 
//CLIENT[BIC]: 
//CLIENT[CHECKING_ACCOUNT]: 
//CLIENT[CORRESPONDENT_ACCOUNT]: 
//CLIENT[OKPO]: 
//CLIENT[OKVED]: 
//CLIENT[TYPE_VALUE]: INDIVIDUAL
namespace BinManParser.Api
{

    public enum ClientType
    {
        INDIVIDUAL,                 //Физическое лицо
        U,                          //Юридическое лицо
        MANAGEMENT_COMPANY          //Управляющая компания
         
    }
    public struct BaseBinApiResponse
    {
        public string success { get; set; }
        public string code { get; set; }
        public string error { get; set; }

        public void LogErrorCode()
        {
            switch (code)
            {
                case "1": Log.Warning("[1] BinMan Клиент уже существует | "+error); break;
                default: Log.Warning("BinMan не известная Ошибка | " + error); break;
            }
        }
    }
    /// <summary>
    /// <para> *** - Обязательное поле </para>
    /// <para> Управляющая компания == Юр лица. (Поля одинаковые) </para>
    /// </summary>
    public class ClientData
    {
        ///// <summary>
        ///// DEPRECATED!
        ///// </summary>
        ///// <param name="Guid"></param>
        ///// <returns></returns>
        ///// <exception cref="Exception"></exception>
        ////public ClientType GetTypeFromDbGuid(string Guid) => Guid switch 
        ////{
        ////    "00000020-C9F6-77AB-CCEF-1900C3854000" => ClientType.U,
        ////    _ => throw new Exception($"Неизвестный тип клиента ! {Guid}")
        ////};
        
             

        public ClientType Form { get; set; }
        public ClientType TYPE { set => type_Code= ClientTypeToCode(value);  }
        public string type_Code;
        //STEP 1
        public string ID { get; set; }
        /// <summary>
        /// Фамилия   Физ лицо.             ***
        /// </summary>
        public string F_SURNAME { get; set; }
        /// <summary>
        /// Имя       Физ лицо.              ***
        /// </summary>
        public string F_NAME { get; set; }
        /// <summary>
        /// Отчество  Физ лицо.
        /// </summary>
        public string F_PATRONYMIC { get; set; }
        /// <summary>
        /// Имя       Юр лица. НАИМЕНОВАНИЕ ОРГАНИЗАЦИИ ***
        /// </summary>
        public string UR_NAME { get; set; }
        /// <summary>
        /// Полн Наим Юр лица. 
        /// </summary>
        public string UR_FULLNAME { get; set; }
        /// <summary>
        /// Юр лица.    ОГРН
        /// </summary>
        public string UR_OGRN { get; set; }
        /// <summary>
        /// Юр лица.    КПП
        /// </summary>
        public string UR_KPP { get; set; }
        /// <summary>
        /// Юр лица.  12.07.2023 (dd.MM.yyyy)   ДАТА РЕГИСТРАЦИИ
        /// </summary>
        public string UR_REG_DATE { get; set; }
        /// <summary>
        /// Физ лица.    
        /// </summary>
        public string PASSPORT_CODE { get; set; }
        /// <summary>
        /// Физ лица.    
        /// </summary>
        public string PASSPORT_NUMBER { get; set; }
        /// <summary>
        /// Физ лица. | Юр лица.      ИНН *** (Должен быть уникальным , т.е обязательный)
        /// </summary>
        public string INN { get; set; }
        // STEP 2
        public Suggestion<Address> address;
        public Suggestion<Address> factAddress;
        //public string F_REGION { get; set; }        //Регион
        //public string F_RAION { get; set; }         //Район
        //public string F_CITY { get; set; }          //Город
        //public string F_SETTLEMENT { get; set; }    //Населенный пункт
        //public string F_STREET { get; set; }        //Улица
        //public string F_HOUSE { get; set; }         //Дом
        //public string F_BLOCK { get; set; }         //Корпус
        //public string F_ROOM { get; set; }          //Квартира
        //public string POSTAL_CODE { get; set; }     //Индекс
        //public string FIAS_ID { get; set; }         //Код Фиас
        public string[] PHONE { get; set; }           // Формат +7 (666) 666-66-66
        public string[] EMAIL { get; set; }           //
        public string FACT_ADDRESS_SAME { get; set; }           //?
        //public string REGION_FACT { get; set; }           //?
        //public string RAION_FACT { get; set; }           //?
        //public string CITY_FACT { get; set; }           //?
        //public string SETTLEMENT_FACT { get; set; }           //?
        //public string STREET_FACT { get; set; }           //?
        //public string HOUSE_FACT { get; set; }           //?
        //public string F_BLOCK_FACT { get; set; }           //?
        //public string OFFICE_FACT { get; set; }           //?
        //public string POSTAL_CODE_FACT { get; set; }           //?
        //public string FIAS_ID_FACT { get; set; }           //?
        public string POST_NAME { get; set; }           //?
        public string POST { get; set; }           //?
        public string NAME_OF_BANK { get; set; }           //?
        public string BIC { get; set; }           //?
        public string CHECKING_ACCOUNT { get; set; }           //?
        public string CORRESPONDENT_ACCOUNT { get; set; }           //?
        public string OKPO { get; set; }           //?
        public string OKVED { get; set; }           //?

        //Тестовая тест
        public string SearchTypeString => Form switch
        {
            ClientType.INDIVIDUAL => "INDIVIDUAL",
            ClientType.MANAGEMENT_COMPANY => "MANAGEMENT_COMPANY",
            ClientType.U => "U",
        };
        public string Type_value => Form switch
        {
            ClientType.INDIVIDUAL => "INDIVIDUAL",
            ClientType.MANAGEMENT_COMPANY => "MANAGEMENT_COMPANY",
            ClientType.U => "LEGAL",
            _ => throw new Exception($"Не известный тип клиента ?: `{Form}`")
        };
        public string ClientTypeToCode(ClientType ct) => ct switch
        {
            ClientType.INDIVIDUAL => "INDIVIDUAL",
            ClientType.MANAGEMENT_COMPANY => "MANAGEMENT_COMPANY",
            ClientType.U => "LEGAL",
            _ => throw new Exception($"Не известный тип клиента ?: `{Form}`")
        };

    }
    public static class BinManKa
    {


        public static string Url_UpdateSsid(string id) => API.BaseUrl + $"cabinet/clients/add/?id={id}&is_ajax=y";
        public static string Url_Delete(string id) => API.BaseUrl + $"cabinet/clients/detail/{id}/?delete_client={id}";
        public const string Url_Update = API.BaseUrl + $"cabinet/clients/add/";
        public const string Url_CreateSsid = API.BaseUrl + $"cabinet/clients/add/?is_ajax=y";
        public const string Url_Create = API.BaseUrl + $"cabinet/clients/add/";
        public const string Url_Parse_id = API.BaseUrl + "cabinet/clients/";

        public static Dictionary<string, string> GetCreateFormData(string sessid, ClientData Data)
        {

            var v = new Dictionary<string, string>()
           {
               {"CLIENT[ID]","" },
               {"sessid",sessid},
{"CLIENT[COMPANY]"              ,API.CompanyId },
{"CLIENT[TYPE]"                 ,Data.Form.ToString() },
{"CLIENT[INN]"                  ,Data.INN },
{"CLIENT[F_REGION]"             ,Data.address?.data?.region_with_type},
{"CLIENT[F_RAION]"              ,Data.address?.data?.area_with_type },
{"CLIENT[F_CITY]"               ,Data.address?.data?.city_with_type },
{"CLIENT[F_SETTLEMENT]"         ,Data.address?.data?.settlement_with_type },
{"CLIENT[F_STREET]"             ,Data.address?.data?.street_with_type },
{"CLIENT[F_HOUSE]"              ,BinManObject.formatHome(Data.address?.data?.house_with_type) },
{"CLIENT[F_BLOCK]"              ,Data.address?.data?.block },
{"CLIENT[F_ROOM]"               ,Data.address?.data?.flat },
{"CLIENT[POSTAL_CODE]"          ,Data.address?.data?.postal_code },
{"CLIENT[FIAS_ID]"              ,Data.address?.data?.house_fias_id },
{"CLIENT[FACT_ADDRESS_SAME]"    ,Data.FACT_ADDRESS_SAME },
{"CLIENT[REGION_FACT]"          ,Data.factAddress?.data?.region_with_type },
{"CLIENT[RAION_FACT]"           ,Data.factAddress?.data?.area_with_type },
{"CLIENT[CITY_FACT]"            ,Data.factAddress?.data?.city_with_type },
{"CLIENT[SETTLEMENT_FACT]"      ,Data.factAddress?.data?.settlement_with_type },
{"CLIENT[STREET_FACT]"          ,Data.factAddress?.data?.street_with_type },
{"CLIENT[HOUSE_FACT]"           ,BinManObject.formatHome(Data.factAddress?.data?.house_with_type) },
{"CLIENT[F_BLOCK_FACT]"         ,Data.factAddress?.data?.block },
{"CLIENT[OFFICE_FACT]"          ,Data.factAddress?.data?.flat},
{"CLIENT[POSTAL_CODE_FACT]"     ,Data.factAddress?.data?.postal_code },
{"CLIENT[FIAS_ID_FACT]"         ,Data.factAddress?.data?.house_fias_id },
{"CLIENT[POST_NAME]"            ,Data.POST_NAME },
{"CLIENT[POST]"                 ,Data.POST },
{"CLIENT[NAME_OF_BANK]"         ,Data.NAME_OF_BANK },
{"CLIENT[BIC]"                  ,Data.BIC },
{"CLIENT[CHECKING_ACCOUNT]"     ,Data.CHECKING_ACCOUNT },
{"CLIENT[CORRESPONDENT_ACCOUNT]",Data.CORRESPONDENT_ACCOUNT },
{"CLIENT[OKPO]"                 ,Data.OKPO },
{"CLIENT[OKVED]"                ,Data.OKVED },
{"CLIENT[TYPE_VALUE]"           ,Data.type_Code// Data.Type_value
                },
          };


            switch (Data.Form)
            {
                case ClientType.INDIVIDUAL:
                    v.Add("CLIENT[F_SURNAME]"      , Data.F_SURNAME);
                    v.Add("CLIENT[F_NAME]"         , Data.F_NAME);
                    v.Add("CLIENT[F_PATRONYMIC]"   , Data.F_PATRONYMIC);
                    v.Add("CLIENT[PASSPORT_CODE]"  , Data.PASSPORT_CODE);
                    v.Add("CLIENT[PASSPORT_NUMBER]", Data.PASSPORT_NUMBER);
                    break;
                case ClientType.MANAGEMENT_COMPANY:
                case ClientType.U:
                    v.Add("CLIENT[NAME]"           , Data.UR_NAME);
                    v.Add("CLIENT[FULL_NAME]"      , Data.UR_FULLNAME);
                    v.Add("CLIENT[OGRN]"           , Data.UR_OGRN);
                    v.Add("CLIENT[KPP]"            , Data.UR_KPP);
                    v.Add("CLIENT[REG_DATE]"       , Data.UR_REG_DATE);
                    break;
            }




            if (Data.PHONE != null)
                foreach (var t in Data.PHONE) {if(BinManApi.TryFormatPhoneNumberAsKa(t,out var res)) v.Add("CLIENT[PHONE][]", res); }
            if (Data.EMAIL != null)
                foreach (var t in Data.EMAIL) { v.Add("CLIENT[EMAIL][]", t); }

            return v;
        }

        public static Dictionary<string, string> GetUpdateFormData(string sessid, ClientData Data)
        {

            var v = new Dictionary<string, string>()
           {
               {"CLIENT[ID]"    ,Data.ID.ToString() },
         {"sessid"              ,sessid},
{"CLIENT[COMPANY]"              ,API.CompanyId },
{"CLIENT[TYPE]"                 ,Data.Form.ToString() },
{"CLIENT[INN]"                  ,Data.INN },
{"CLIENT[F_REGION]"             ,Data.address.data.region_with_type },
{"CLIENT[F_RAION]"              ,Data.address.data.area_with_type },
{"CLIENT[F_CITY]"               ,Data.address.data.city_with_type },
{"CLIENT[F_SETTLEMENT]"         ,Data.address.data.settlement_with_type },
{"CLIENT[F_STREET]"             ,Data.address.data.street_with_type },
{"CLIENT[F_HOUSE]"              ,BinManObject.formatHome(Data.address.data.house_with_type) },
{"CLIENT[F_BLOCK]"              ,Data.address.data.block },
{"CLIENT[F_ROOM]"               ,Data.address.data.flat },
{"CLIENT[POSTAL_CODE]"          ,Data.address.data.postal_code },
{"CLIENT[FIAS_ID]"              ,Data.address.data.house_fias_id },
{"CLIENT[FACT_ADDRESS_SAME]"    ,Data.FACT_ADDRESS_SAME },
{"CLIENT[REGION_FACT]"          ,Data.factAddress.data.region_with_type },
{"CLIENT[RAION_FACT]"           ,Data.factAddress.data.area_with_type },
{"CLIENT[CITY_FACT]"            ,Data.factAddress.data.city_with_type },
{"CLIENT[SETTLEMENT_FACT]"      ,Data.factAddress.data.settlement_with_type },
{"CLIENT[STREET_FACT]"          ,Data.factAddress.data.street_with_type },
{"CLIENT[HOUSE_FACT]"           ,BinManObject.formatHome(Data.factAddress.data.house_with_type) },
{"CLIENT[F_BLOCK_FACT]"         ,Data.factAddress.data.block },
{"CLIENT[OFFICE_FACT]"          ,Data.factAddress.data.flat },
{"CLIENT[POSTAL_CODE_FACT]"     ,Data.factAddress.data.postal_code },
{"CLIENT[FIAS_ID_FACT]"         ,Data.factAddress.data.house_fias_id },
{"CLIENT[POST_NAME]"            ,Data.POST_NAME },
{"CLIENT[POST]"                 ,Data.POST },
{"CLIENT[NAME_OF_BANK]"         ,Data.NAME_OF_BANK },
{"CLIENT[BIC]"                  ,Data.BIC },
{"CLIENT[CHECKING_ACCOUNT]"     ,Data.CHECKING_ACCOUNT },
{"CLIENT[CORRESPONDENT_ACCOUNT]",Data.CORRESPONDENT_ACCOUNT },
{"CLIENT[OKPO]"                 ,Data.OKPO },
{"CLIENT[OKVED]"                ,Data.OKVED },
{"CLIENT[TYPE_VALUE]"           ,Data.type_Code //Data.Type_value
                },
          };


            switch (Data.Form)
            {
                case ClientType.INDIVIDUAL:
                    v.Add("CLIENT[F_SURNAME]"      , Data.F_SURNAME);
                    v.Add("CLIENT[F_NAME]"         , Data.F_NAME);
                    v.Add("CLIENT[F_PATRONYMIC]"   , Data.F_PATRONYMIC);
                    v.Add("CLIENT[PASSPORT_CODE]"  , Data.PASSPORT_CODE);
                    v.Add("CLIENT[PASSPORT_NUMBER]", Data.PASSPORT_NUMBER);
                    break;
                case ClientType.MANAGEMENT_COMPANY:
                case ClientType.U:
                    v.Add("CLIENT[NAME]"           , Data.UR_NAME);
                    v.Add("CLIENT[FULL_NAME]"      , Data.UR_FULLNAME);
                    v.Add("CLIENT[OGRN]"           , Data.UR_OGRN);
                    v.Add("CLIENT[KPP]"            , Data.UR_KPP);
                    v.Add("CLIENT[REG_DATE]"       , Data.UR_REG_DATE);
                    break;
            }




            if (Data.PHONE != null)
                foreach (var t in Data.PHONE) { v.Add("CLIENT[PHONE][]", t); }
            if (Data.EMAIL != null)
                foreach (var t in   Data.EMAIL) { v.Add("CLIENT[EMAIL][]", t); }

            return v;
        }

        public static bool SendEditRequest(LoginData ld, ClientData Data)
        {


            Log.ApiCall("KA SendEditRequest");
            try
            {
                if (!ValidateValues(Data)) return false;

                string url = Url_UpdateSsid(Data.ID.ToString());
                HttpRequestMessage hm = new(HttpMethod.Post, Url_Update);
                if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;
                var data = GetUpdateFormData(
                     //ld.PHPSESSID
                     sessId,Data
                    );

                Log.Json(data);

                hm.Content = new FormUrlEncodedContent(data);
                var dd = hm.Content.ReadAsStringAsync();
                dd.Wait();

                StringBuilder sb = new StringBuilder();

                var vvv = Uri.UnescapeDataString(dd.Result).Split("&");

                foreach (var v in vvv)
                {
                    sb.AppendLine(v);
                }


                Log.Text("+++" + sb.ToString());

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );
                Log.Json(req);
                var t = req.Content.ReadAsStringAsync().GetAwaiter().GetResult().ToString();// Удивительно, но здесь приходит вполне нормальный json ответ, даже с указанием ошибки
              
                if(t.Contains("\"success\":true"))
                {
                    return true;
                }
                else
                {
                    Log.Error(t);
                    return false;
                }
                //var html = t.Result;

                //var htmlDoc = new HtmlDocument();
                //htmlDoc.LoadHtml(html);
                return true;
                // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test.html");


            }
            catch (Exception e) { Log.Error(e); return false; }
        }

        private static bool ValidateValues(ClientData Data)
        {
            bool bad = false;
            switch (Data.Form)
            {
                case ClientType.INDIVIDUAL:
                    if (string.IsNullOrEmpty(Data.F_NAME)) Log.Warning("BinMan F_NAME - Не указано !!!"); bad = true;
                    if (string.IsNullOrEmpty(Data.F_SURNAME)) Log.Warning("BinMan F_NAME - Не указано !!!"); bad = true;
                    if (string.IsNullOrEmpty(Data.F_NAME)) Log.Warning("BinMan F_NAME - Не указано !!!"); bad = true;
                    break;
                case ClientType.U:
                case ClientType.MANAGEMENT_COMPANY:
                    if (string.IsNullOrEmpty(Data.UR_NAME)) Log.Warning("BinMan UR_NAME - Не указано !!!"); bad = true;
                    break;
            }
            return bad;
        }
        public static bool FindClientId(LoginData ld, ClientData Data,DateTime SearchDate, out string BinId)
        {
            BinId = string.Empty;

            try
            {
                string url = Url_Parse_id;

                //        public string F_SURNAME { get; set; }       // Фамилия
                //public string F_NAME { get; set; }       // Имя
                //public string F_PATRONYMIC { get; set; }    // Отчество

                var param = new List<KeyValuePair<string, string>>() { };

                if (Data.Form == ClientType.INDIVIDUAL)

                    param.Add(new KeyValuePair<string, string>("search", $"{Data.F_SURNAME} {Data.F_NAME} {Data.F_PATRONYMIC}".Trim()));
                else
                    param.Add(new KeyValuePair<string, string>("search", $"{Data.UR_NAME}"));
                param.Add(new KeyValuePair<string, string>("sort", ""));
                param.Add(new KeyValuePair<string, string>("sort_order", ""));
                param.Add(new KeyValuePair<string, string>("date_from", SearchDate.AddDays(-1).ToString("dd.MM.yyyy")));
                param.Add(new KeyValuePair<string, string>("date_to", SearchDate.AddDays(1).ToString("dd.MM.yyyy")));
                param.Add(new KeyValuePair<string, string>("type[]", Data.type_Code//Data.Type_value
                    ));
                param.Add(new KeyValuePair<string, string>("address[PROPERTY_276]", ""));
                param.Add(new KeyValuePair<string, string>("address[PROPERTY_295]", ""));
                param.Add(new KeyValuePair<string, string>("address[PROPERTY_277]", ""));
                param.Add(new KeyValuePair<string, string>("address[PROPERTY_299]", ""));
                param.Add(new KeyValuePair<string, string>("address[PROPERTY_278]", ""));
                param.Add(new KeyValuePair<string, string>("address[PROPERTY_279]", ""));
                param.Add(new KeyValuePair<string, string>("objects_count_from", ""));
                param.Add(new KeyValuePair<string, string>("objects_count_to", ""));
                param.Add(new KeyValuePair<string, string>("objectcount", ""));
                param.Add(new KeyValuePair<string, string>("contracts_count_from", ""));
                param.Add(new KeyValuePair<string, string>("contracts_count_to", ""));
                param.Add(new KeyValuePair<string, string>("concount", ""));



                var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));
                Log.Text(Uri
                    .UnescapeDataString( newUrl.ToString()));

                HttpRequestMessage hm = new(HttpMethod.Get, newUrl);

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] { },
                    cookie
                    );


                var res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                //Log.Text(res);
                //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(res);
                try
                {
                    var Base = htmlDoc.DocumentNode.Descendants("a");
                    var NotEmpty = Base.Where((d) => {
                        var br = d != null;
                        br &= d.Attributes["href"] != null;
                        br &= d.Attributes["href"].Value != null;
                        br &= d.Attributes["href"].Value.Length > 0;
                        return br;
                    });
                    var Links = NotEmpty.Where(d=> d.Attributes["href"].Value.Contains("/cabinet/clients/detail/"));


                    var ContainerEl = Links.ElementAt(0);
                    var ContainerLink = ContainerEl.Attributes["href"].Value;
                    int ContainerId = int.Parse(ContainerLink.Split("/")[^2].Replace("/", ""));

                    BinId = ContainerId.ToString();
                }
                catch(Exception ex)
                {
                    Log.Text(res);
                    Log.Error(ex);
                    return false;
                }
                Log.Warning(BinId);
                return true;

            }


            catch (Exception e) { Log.Error(e); return false; }
        }
        public static bool SendCreateRequest(LoginData ld, ClientData Data, out string BinId)
        {

            BinId = string.Empty;
            Log.ApiCall("KA SendCreateRequest");
            try
            {

                if (!ValidateValues(Data)) return false;

                string url = Url_CreateSsid;
                HttpRequestMessage hm = new(HttpMethod.Post, Url_Create);
                if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;
                var data = GetCreateFormData(
                     //ld.PHPSESSID
                     sessId, Data
                    );

                Log.Json(data);

                hm.Content = new FormUrlEncodedContent(data);
                var dd = hm.Content.ReadAsStringAsync();
                dd.Wait();

                StringBuilder sb = new StringBuilder();

                var vvv = Uri.UnescapeDataString(dd.Result).Split("&");

                foreach (var v in vvv)
                {
                    sb.AppendLine(v);
                }


                Log.Text("+++" + sb.ToString());

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );
               var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Log.Text(resp);
                var res = JsonConvert.DeserializeObject<BaseBinApiResponse>(resp);
                Log.Json(res);
                if (res.success.ToLower() == "true" && string.IsNullOrEmpty(res.error))
                {

                    if (!FindClientId(ld, Data,DateTime.Now, out BinId)) return false;

                    return true;
                }
                else
                {
                    res.LogErrorCode();
                    return false;
                }

                // var htmlDoc = new HtmlDocument();
                // htmlDoc.LoadHtml(html);

                // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test.html");


            }
            catch (Exception e) { Log.Error(e); return false; }
        }
        public static bool DeleteKlient(LoginData ld, string kaId)
        {


            try
            {
                string url = Url_Delete(kaId);
                HttpRequestMessage hm = new(HttpMethod.Get, url);
                //if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;


                var cookie = API.GetDeffaultCookie(ld, "");

                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    , ld, true);
                Log.ApiCall($"Delete Ka {kaId}");
                //21.11.2024 - В случае успеха - редиректит на страницу поиска клиентов
                // Не успешно - обратно на страницу клиента
                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                
                if (req.Headers?.Location?.AbsoluteUri?.Contains("kaId")==true)
                {
                    return false;
                }
                else
                {
                    return true;
                }

                return true;



            }
            catch (Exception e) { Log.Error(e); return false; }


            return false;
        }
    }

}
