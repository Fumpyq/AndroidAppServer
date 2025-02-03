namespace AndroidAppServer.Libs.BinMan
{
    using ADCHGKUser4.Controllers.Libs;
    using AndroidAppServer.Libs;
    using BinManParser.Api;
    using Dadata.Model;
    using DocumentFormat.OpenXml.Office.CoverPageProps;
    using global::BinManParser.Api;
    using HtmlAgilityPack;
    using Microsoft.AspNetCore.WebUtilities;
    using Newtonsoft.Json;
    using Org.BouncyCastle.Utilities.Net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using static AndroidAppServer.Libs.BinMan.BinManObjectData;
    using static AndroidAppServer.Libs.DadataApi;
    using static BinManGeozone;

    namespace BinManParser.Api
    {

        public struct CreateGeozoneResponse
        {
            public int area_id { get; set; }
            public int success { get; set; }
        }


    }        /// <summary>
             /// *** - Обязательные поля
             /// </summary>
    public class BinManObjectData
    {
        public string Address_dbGuid;
        public string RawAddress;
        public Suggestion<Address> address;
        //{ID: "19", TITLE: "Клуб", CATEGORY_ID: "4", SHOW_ROOMS: "N"}
        public class BinManObjectSubType
        {
            public string ID { get; set; }
            public string TITLE { get; set; }
        }
        /*
       
address:    "Кемеровская область - Кузбасс, г Кемерово, ул Пушкина"
area_id:    5760120
name:       "Продовольственный магазин, г Кемерово, ул Пушкина"
success:    1
         */
        public class BinManObjectCreateResponse
        {
            public bool success { get; set; }
            /// <summary>
            /// Bin id
            /// </summary>
            public long area_id { get; set; }
            public string name { get; set; }
            public string address { get; set; }
        }
        public class ObjectType_parse
        {
            public ObjectMainCattegory cattegory;
            public ObjectSubCattegory subCattegory;
        }
        public enum ObjectMainCattegory
        {

            Not_Specefied = -1,
            /// <summary>
            /// Жилой объект 
            /// </summary>
            Home = 1,
            /// <summary>
            /// Предприятия торговли 
            /// </summary>
            Trade = 2,
            /// <summary>
            /// Образовательные учреждения
            /// </summary>
            Education = 3,
            /// <summary>
            /// Учреждения культуры
            /// </summary>
            Culture = 4,
            /// <summary>
            /// Предприятия общественного питания
            /// </summary>
            FastFood = 5,//                                           
            /// <summary>
            /// Предприятия бытовой сферы обслуживания   
            /// </summary>
            Byt_Sphere = 6,
            /// <summary>
            /// Медицинские учреждения
            /// </summary>
            Medicine = 7,
            /// <summary>
            /// Административные здания и офисы
            /// </summary>
            Office = 8
        }
        public enum ObjectSubCattegory
        {
            Not_Specefied = -1,
            /// <summary> Многоквартирный дом  </summary>
            Mnogokvartirny_dom = 1,
            /// <summary> Жилой дом  </summary>
            Jiloy_dom = 2,
            /// <summary> Жилое помещение  </summary>
            Jiloe_pomeschenie = 53,
            /// <summary> Аптека </summary>
            Apteka = 3,
            /// <summary> Продовольственный магазин </summary>
            Prodovolstvenniy_magazin = 4,
            /// <summary> Промтоварный магазин </summary>
            Promtovarniy_magazin = 5,
            /// <summary> Супермаркет </summary>
            Supermarket = 6,
            /// <summary> Торговый павильон </summary>
            Torgoviy_pavilon = 7,
            /// <summary> Киоск </summary>
            Kiosk = 8,
            /// <summary> Рынок </summary>
            Rinok = 9,
            /// <summary> Оптовая базы </summary>
            Optovai_bazi = 10,
            /// <summary> Аэропорт </summary>
            Airoport = 11,
            /// <summary> Вокзал </summary>
            Vokzal = 12,
            /// <summary> Прочее </summary>
            Trade_Prochee = 13,
            /// <summary> Детский сад </summary>
            Detskiy_sad = 14,
            /// <summary> Школа </summary>
            SHkola = 15,
            /// <summary> Профессиональное образование </summary>
            Professionalnoe_obrazovanie = 16,
            /// <summary> ВУЗ </summary>
            VUZ = 17,
            /// <summary> Прочее </summary>
            Education_Prochee = 18,
            /// <summary> Клуб </summary>
            Klub = 19,
            /// <summary> Кинотеатр </summary>
            Kinoteatr = 20,
            /// <summary> Театр </summary>
            Teatr = 21,
            /// <summary> Выставочный зал </summary>
            Vistavochniy_zal = 22,
            /// <summary> Цирк </summary>
            CHirk = 23,
            /// <summary> Стадион </summary>
            Stadion = 24,
            /// <summary> Спортивный комплекс </summary>
            Sportivniy_kompleks = 25,
            /// <summary> Турбаза </summary>
            Turbaza = 26,
            /// <summary> Библиотека </summary>
            Biblioteka = 27,
            /// <summary> Прочее </summary>
            Culture_Prochee = 28,
            /// <summary> Кафе </summary>
            Kafe = 29,
            /// <summary> Ресторан </summary>
            Restoran = 30,
            /// <summary> Столовая </summary>
            Stolovai = 31,
            /// <summary> Бар </summary>
            Bar = 32,
            /// <summary> Прочее </summary>
            FastFood_Prochee = 33,
            /// <summary> АЗС </summary>
            AZS = 34,
            /// <summary> Автомойка </summary>
            Avtomoyka = 35,
            /// <summary> Парикмахерская </summary>
            Parikmaherskai = 36,
            /// <summary> Салон красоты </summary>
            Salon_krasoti = 37,
            /// <summary> Прачечная </summary>
            Prachechnai = 38,
            /// <summary> Мастерская </summary>
            Masterskai = 39,
            /// <summary> Сауна </summary>
            Sauna = 40,
            /// <summary> Ателье </summary>
            Atele = 41,
            /// <summary> Баня </summary>
            Bani = 42,
            /// <summary> Прочее </summary>
            Byt_Sphere_Prochee = 43,
            /// <summary> Больница </summary>
            Bolnicha = 44,
            /// <summary> Поликлиника </summary>
            Poliklinika = 45,
            /// <summary> Санаторий </summary>
            Sanatoriy = 46,
            /// <summary> Прочее </summary>
            Medicine_Prochee = 47,
            /// <summary> Офис </summary>
            Ofis = 48,
            /// <summary> Административное здание </summary>
            Administrativnoe_zdanie = 49,
            /// <summary> Организация </summary>
            Organizachii = 50,
            /// <summary> Прочее </summary>
            Office_Prochee = 51,
        }

        public LoginData ld;

        public string DataBase_Guid;

        public string BinId { get; set; }

        //public string COMPANY { get; set; }

        public double lon;
        public double lat;
        public string NAME { get; set; }
        /// <summary>
        /// ***
        /// </summary>

        // public string ROOMS { get; set; }
        public string FIAS { get; set; }
        public ObjectMainCattegory CATEGORY;
        public ObjectSubCattegory SUBCATTEGORY;

        public string CLIENTID { get; set; } = "5779536";
        public Geo_Group? GROUP { get; set; }        //Группа
        public Geo_Lot lot_id { get; set; }         //Лот


    }
    public static class BinManObject
    {
        public static string URL_PreEdit => API.BaseUrl + "api/1/geo/dadata.php";
        public static string URL_create => API.BaseUrl + "cabinet/objects/add/";
        public static string URL_edit(string objId) => API.BaseUrl + $"cabinet/objects/edit/{objId}/";
        public static string URL_Delete(string objId) => API.BaseUrl + $"cabinet/objects/detail/{objId}/?action=delete&object_id={objId}";
        public static string URL_AllAtached(string objId) => API.BaseUrl + $"cabinet/objects/detail/{objId}/?action=areas&param=AREA";
        /// <summary>
        /// https://binman.ru/cabinet/objects/detail/
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static string URL_isExists(string objId) => API.BaseUrl + $"/cabinet/objects/detail/{objId}/";
        public static string URL_getSubbCattegoies(ObjectMainCattegory CATEGORY) => API.BaseUrl + $"api/1/ajax/objecttype/?category_id={((int)(CATEGORY))}";

        public static string formatHome(string home)
        {
            if (string.IsNullOrEmpty(home) || home.Length < 1) return "";
            home = home.Trim().Replace("д. ", "").Replace("д ", "")
                .Replace("дом ", "")
                .Replace("д.", "")
                .Trim();
            if(!string.IsNullOrEmpty(home))
            home = "дом " + home;

            return home;
        }
        public static string formatHome(Suggestion<Address> addr)
        {
            var res = string.Empty;
            if (addr==null || string.IsNullOrEmpty(addr.data.house_with_type))
            {
                if (string.IsNullOrEmpty(addr.data.house))
                {
                    SetHomeByStringParse(addr);
                    if (string.IsNullOrEmpty(addr.data.house_with_type))
                    {
                        if (string.IsNullOrEmpty(addr.data.house))
                        {
                            return string.Empty;
                        }
                        else
                        {
                            res = addr.data.house;
                        }
                    }
                    else
                    {
                        res = addr.data.house_with_type;
                    }
                }
                else
                {
                    res = addr.data.house;
                }
            }
            else
            {
                res = addr.data.house_with_type;
            }
            return formatHome(res);
        }
        public static void SetHomeByStringParse(Suggestion<Address> address)
        {
            var s1 = address.unrestricted_value.Split(",");
            var h2 = s1.Where(s =>
            {
                var ss = s.Trim().ToLower();
                Log.Text("SS: " + "`" + ss + "`");
                return ss.Contains(" дом ") || ss.Contains(" д ") || ss.Contains("д. ");

            }).FirstOrDefault();
            if (!string.IsNullOrEmpty(h2)) // h2 - Кусок адреса с домом
            {
                Log.Text("H2: " + h2);
                var h = h2.Trim()
                    .Replace("д д ", "дом ")
                    .Replace("д. д ", "дом ")
                    .Replace("д  д. ", "дом ")
                    .Replace("д. д. ", "дом ")
                    .Replace("дом д ", "дом ")
                    .Replace("дом д. ", "дом ")
                    .Replace("д дом ", "дом ")
                    .Replace("д. дом ", "дом ")
                    .Replace("дом дом ", "дом ")
                    ;
                Log.Text("H: " + h);
                for (int i = 0; i < h.Length; i++)
                {
                    if (char.IsDigit(h[i]))
                    {
                        address.data.house = h.Substring(i - 1).Trim();
                        address.data.house_type = h.Substring(0, i).Trim();


                        if (!address.data.house_type.Contains("дом"))
                            address.data.house_type = address.data.house_type.Replace("д.", "дом").Replace("д ", "дом ").Trim();
                        address.data.house_with_type =
                           (string.IsNullOrEmpty(address.data.house_type) ?
                            address.data.house_type + " " + address.data.house : address.data.house);
                        break;

                    }
                    if (i == h.Length - 1)
                    {
                        var sps = h.Split(" ");
                        if (sps.Length > 1)
                        {
                            address.data.house = sps[1].Trim();
                            address.data.house_type = sps[0].Trim();
                        }
                        address.data.house_with_type = h;
                    }
                    //void SepparateBlockFromHouse()
                    //    {
                    //        if(address.data) address.data.house.ToLower().Contains("к")
                    //    }
                }
            }
        }

        public static Dictionary<string, string> GetCreateFormData(string sessid, BinManObjectData Data) =>

    new Dictionary<string      , string>()
   { { "sessid"                ,sessid                                                          },{ "OBJECT[COMPANY]","111275"},
{ "OBJECT[NO_AUTO_COORDINATES]",""                                                              },
{ "OBJECT[REGION]"             ,Data.address.data.region_with_type                              },
{ "OBJECT[AREA]"               ,Data.address.data.area_with_type                                },
{ "OBJECT[CITY]"               ,Data.address.data.city_with_type                                },
{ "OBJECT[SETTLEMENT]"         ,Data.address.data.settlement_with_type                          },
{ "OBJECT[CITY_DISTRICT]"      ,Data.address.data.city_district_with_type                       },
{ "OBJECT[STREET]"             ,Data.address.data.street_with_type                              },
{ "OBJECT[HOUSE]"              ,formatHome(Data.address)                   },
{ "OBJECT[BLOCK]"              ,Data.address.data.block                                         },
{ "OBJECT[ADDRESS]"            ,Data.address.unrestricted_value                                 },
{ "OBJECT[LAT]"                ,Data.lat.ToString("0.0000").Replace(",",".")                            },
{ "OBJECT[LON]"                ,Data.lon.ToString("0.0000").Replace(",",".")                            },
{ "lot_id"                     ,Data.lot_id.ToString().Replace(",",".")                         },
{ "OBJECT[FIAS]"               ,Data.address.data.house_fias_id                                 },
{ "OBJECT[CATEGORY]"           ,((int)(Data.CATEGORY)).ToString()                               },
{ "OBJECT[CATEGORY_TYPE]"      ,((int)(Data.SUBCATTEGORY)).ToString()                           },
{ "OBJECT[ROOMS]"              ,Data.address.data.flat                                          },
{ "OBJECT[NAME]"               ,Data.NAME                                                       },
{ "OBJECT[CLIENT]"             ,Data.CLIENTID                                                   },
                                                                                                };

        public static Dictionary<string, string> GetEditFormData(string sessid, BinManObjectData Data) =>

new Dictionary<string, string>()
{ { "sessid"                ,sessid                               },
    //,{ "OBJECT[COMPANY]","111275"},
{ "PROPS[NO_AUTO_COORDINATES]",""                                                               },
{ "PROPS[REGION]"            ,Data.address.data.region_with_type                               },
{ "PROPS[AREA]"              ,Data.address.data.area_with_type                                 },
{ "PROPS[CITY]"              ,Data.address.data.city_with_type                                 },
{ "PROPS[SETTLEMENT]"        ,Data.address.data.settlement_with_type                           },
{ "PROPS[CITY_DISTRICT]"     ,Data.address.data.city_district_with_type                        },
{ "PROPS[STREET]"            ,Data.address.data.street_with_type                               },
{ "PROPS[HOUSE]"             ,formatHome(Data.address)                    },
{ "PROPS[BLOCK]"             ,Data.address.data.block                                          },
{ "PROPS[ADDRESS]"           ,Data.address.unrestricted_value                                  },
{ "PROPS[LAT]"               ,Data.lat.ToString("0.0000").Replace(",",".")                             },
{ "PROPS[LON]"               ,Data.lon.ToString("0.0000").Replace(",",".")                             },
{ "PROPS[LOT]"                ,Data.lot_id.ToString() /*(Data.lot_id.ToString()=="0"?"Выберите лот":Data.lot_id.ToString())*/},

{ "PROPS[CATEGORY]"           ,((int)(Data.CATEGORY)).ToString()                                },
{ "PROPS[CATEGORY_TYPE]"      ,((int)(Data.SUBCATTEGORY)).ToString()                            },
{ "PROPS[ROOMS]"              ,Data.address.data.flat                                           },
{ "OBJECT[NAME]"              ,Data.NAME                                                        },
{ "PROPS[FIAS]"               , Data.address.data.house_fias_id                                  },
//{ "PROPS[CLIENT]"             ,Data.CLIENTID                                                    },
                                                                                                };
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ld"></param>
        /// <returns>is operation success</returns>
        public static bool SendEditRequest(LoginData ld,BinManObjectData Data)
        {

            
            try
            {
                string url = URL_edit(Data.BinId);
                HttpRequestMessage hm = new(HttpMethod.Post, url);
                if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;
                var data = GetEditFormData(
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

                Log.Text(hm.ToString());
                Log.Text("+++ " + sb.ToString());
                //Log.Json(hm);
                var cookie = API.GetDeffaultCookie(ld, "");

                //if(!SendPreEditRequest(ld, Data)) { Log.Warning("Object PreRequest Failed !"); return false; }
            


                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    , ld, true);
                //   Log.Json(req.Content);
                  // var t = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Log.Text(t);

                // t.Wait();

                //   Log.Text(t.Result);
                ////   Log.Json(t.Result);
                // var response = JsonConvert.DeserializeObject<BinManObjectCreateResponse>(t.Result);
                //  if (response!=null&& response.success)
                //  {
                //    Log.Json(response);

                //    BinId = response.area_id;
                //  }
                //else
                //{
                //    return false;
                //}

                return true;



            }
            catch (Exception e) { Log.Error(e); return false; }

            

            return false;
        }

        public static bool SendPreEditRequest(LoginData ld, BinManObjectData Data)
        {
            try
            {
                string url = URL_PreEdit;

                //        public string F_SURNAME { get; set; }       // Фамилия
                //public string F_NAME { get; set; }       // Имя
                //public string F_PATRONYMIC { get; set; }    // Отчество

                var param = new List<KeyValuePair<string, string>>() { };



                param.Add(new KeyValuePair<string, string>("locations[region_fias_id]", Data.address.data.region_fias_id    /*"393aeccb-89ef-4a7e-ae42-08d5cebc2e30"*/));
                param.Add(new KeyValuePair<string, string>("locations[city_fias_id]"  , Data.address.data.city_fias_id      /*"94bb19a3-c1fa-410b-8651-ac1bf7c050cd"*/));
                param.Add(new KeyValuePair<string, string>("locations[street_fias_id]", Data.address.data.settlement_fias_id/*"11707dca-565d-4f63-a5ee-99b8735e0669"*/));
                param.Add(new KeyValuePair<string, string>("locations[house_fias_id]" , Data.address.data.house_fias_id     /*"4aaa8190-210f-4576-b6f5-b5496d72096a"*/));
                param.Add(new KeyValuePair<string, string>("from_bound[value]"        , "house"));
                param.Add(new KeyValuePair<string, string>("restrict_value"           , "true"));
                param.Add(new KeyValuePair<string, string>("to_bound[value]"          , "house"));
                param.Add(new KeyValuePair<string, string>("type"                     , "address"));
                param.Add(new KeyValuePair<string, string>("query"                    , Data.address.data.house_with_type));
                param.Add(new KeyValuePair<string, string>("count"                    , "1"));

 



                var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));
                Log.Text(Uri
                    .UnescapeDataString(newUrl.ToString()));

                HttpRequestMessage hm = new(HttpMethod.Get, newUrl);

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] { },
                    cookie
                    );


                var res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Log.Text(res);
                return res.Contains("\"suggestions\":");

            }


            catch (Exception e) { Log.Error(e); return false; }

        }
        /// <summary>
        /// BinMan: Обычно при ошибке приходит сайт
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="Data"></param>
        /// <param name="bin_id"></param>
        /// <returns></returns>

        public static bool SendCreateRequest(LoginData ld,BinManObjectData Data, out long bin_id)
        {
            bin_id = -1;
            try
            {
                string url = URL_create;
                HttpRequestMessage hm = new(HttpMethod.Post, url);
                if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;
                var data = GetCreateFormData(
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


                Log.Text("+++ " + sb.ToString());
                //Log.Json(hm);
                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    ,ld, true);
                var t = req.Content.ReadAsStringAsync().GetAwaiter().GetResult(); // При ошибке в ответе приходит сайт

              //  t.Wait();

                Log.Text(t);
                //   Log.Json(t.Result);
                var response = JsonConvert.DeserializeObject<BinManObjectCreateResponse>(t);
                if (response != null && response.success)
                {
                    Log.Json(response);
                    bin_id = response.area_id;
                    Data.BinId = response.area_id.ToString();
                    return true;
                }
                else
                {
                    return false;
                }




                return true;


            }
            catch (Exception e) { Log.Error(e); return false; }

        }

        public static int GetPagesCount(LoginData login, string objId)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, GetLink(objId,1));
            //var cookie = API.GetDeffaultCookie(login, "");
            var req = API.SendRequest(hm,
                new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },null
               // cookie
                , login,false, 120);

            var t = req.Content.ReadAsStringAsync();

            t.Wait();

            string res = t.Result;
            //Log.Text(res);
            //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);
            //if (!Firste) { htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_new.html"); Firste = true; }

            //Log.Text("----=--=---=-=---=-=----");



            int pageCount = 1;
            //  List<DocObject> result = new List<DocObject>(2);
            var pagination = htmlDoc.DocumentNode.Descendants("a");
            if (pagination != null && pagination.Count() > 0)
                pagination = pagination.Where(d => d != null && d.Attributes["class"] != null && d.Attributes["class"].Value != null && d.Attributes["class"].Value.Length > 0 && d.Attributes["class"].Value.Contains("modern-number"));
            if (pagination != null && pagination.Count() >= 2)
                pageCount = int.Parse(pagination.TakeLast(2).First().InnerText.Trim());

            // TryParseMainPage(login, 1);

            return pageCount;
        }
        public static bool TryParseNearestGeozonesPage(LoginData login,string objId,out List<int> ActiveGeozones)
        {




            ActiveGeozones = new List<int>();
            HttpRequestMessage hm = new(HttpMethod.Get, URL_AllAtached(objId));
            //var cookie = API.GetDeffaultCookie(login, "");
            var req = API.SendRequest(hm,
                   new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                   }, null
                   //cookie
                   , login,false,200);// 29.02.2024 Поменяно на from false to  true Если что-то поломается, это тут xd //Уже отменено

            var t = req.Content.ReadAsStringAsync();

            t.Wait();

            var res = t.Result;
            //Log.Text(res);
            //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);



            //var MainSlice = htmlDoc.DocumentNode.Descendants("div").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
            //int count = MainSlice.Count();
            //if (count == 0) return false;
            //for (int i = 0; i < count; i++)
            //{
            //   

            //        var CurrentRow = MainSlice.ElementAt(i);
            try
            {
                var ds = htmlDoc.DocumentNode.Descendants("a");

                var Objs = ds.Where(d => (d.Attributes?["href"].Value?.Contains("/cabinet/areas/detail/")) == true);
                foreach (var v in Objs)
                {
                    
                    var val = v.Attributes?["href"].Value;
                    var id = int.Parse(String.Concat(val.Where(c => char.IsNumber(c))));
                    ActiveGeozones.Add(id);
                }
                //var ds = htmlDoc.DocumentNode.Descendants("tr");
                //var buttons =ds.Where(d=> {
                //if (d.Attributes["data-select"] != null && !string.IsNullOrEmpty(d.Attributes["data-select"].Value))
                //{
                //        try
                //        {
                //            Log.Text($"{d.Attributes["data-select"].Value} - {(d.Attributes["data-id"] != null ? d.Attributes["data-id"].Value : "NULL ???")} ");
                //        }
                //        catch (Exception ex) { Log.Error(ex); }
                //        return d.Attributes["data-select"].Value=="Y";
                //    }
                //    return false; 
                //}) ;

                //    foreach(var v in buttons)
                //    {
                //        if (v.Attributes["data-id"] == null) Log.Text(" ??? "+v.ToString());

                //        ActiveGeozones.Add(int.Parse( v.Attributes["data-id"].Value));
                //    }
                    string TrimText(string txt)
                    {
                        return Regex.Replace(txt, @"\s+", " ").Replace("пнвтсрчтптсбвс", "").Replace("\n\n", "").Trim();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    return false;
                }
            
           

            return true;
        }

        private static string GetLink(string object_id, int page) => $"https://binman.ru/api/1/ajax/areas/?distance=y&object_id={object_id}&template=object&PAGEN_1={page}&is_ajax=y";

        public static bool TryParseObjectType(LoginData ld,string obj_binId,out ObjectType_parse res)
        {

            
            res = new ObjectType_parse();
            try
            {
                HttpRequestMessage hm = new(HttpMethod.Get, URL_edit(obj_binId));
                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    }, 
                     cookie
                    , ld, true, 120);


                string txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(txt);

                var select = htmlDoc.DocumentNode.Descendants("select").Where(d => d.Id is "object_type" or "object_category");

                var Maincat = select.First(d => d.Id is "object_category" );

                var SelMainCat = Maincat.Descendants("option").Where((d) => { var atr = d.Attributes["selected"];
                     if(atr != null)
                    {
                        atr = d.Attributes["value"];
                        if (atr != null && !string.IsNullOrEmpty(atr.Value) && atr.Value!="0") return true;
                    }
                     return false;
                    });
                res.cattegory = (ObjectMainCattegory)(SelMainCat.Count() > 0 ? int.Parse(SelMainCat.First().Attributes["value"].Value) : -1);

            var Sub = select.First(d => d.Id is "object_type");

                var SelSubCat = Sub.Descendants("option").Where((d) => {
                    var atr = d.Attributes["selected"];
                    if (atr != null)
                    {
                        atr = d.Attributes["value"];
                        if (atr != null && !string.IsNullOrEmpty(atr.Value) && atr.Value != "0") return true;
                    }
                    return false;
                });
                res.subCattegory = (ObjectSubCattegory)(SelSubCat.Count() > 0 ? int.Parse(SelSubCat.First().Attributes["value"].Value) : -1);

                return true;

                }
            catch (Exception ex) { Log.Error(ex); return false; }

        }

        public static bool GetAttachedGeozones(LoginData ld,string obj_id, out List<int> ids)
        {
            ids = new List<int>();
            if (TryParseNearestGeozonesPage(ld, obj_id, out var pgs))
            {
                ids.AddRange(pgs);
            }
            else { return false; }

            return true;

        }
        public static bool IsExists(LoginData ld, out bool Exists)
        {
            Exists = false;
            try
            {
                string url = URL_create;
                HttpRequestMessage hm = new(HttpMethod.Get, url);


                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    );

                if (req.StatusCode == HttpStatusCode.OK) { Exists = true; }
                else if (req.StatusCode == HttpStatusCode.Found) { Exists = false; }
                else return false;

                return true;


            }
            catch (Exception e) { Log.Error(e); return false; }
        }
        public static bool DeleteObject(LoginData ld, string binId)
        {


            try
            {
                string url = URL_Delete(binId);
                HttpRequestMessage hm = new(HttpMethod.Get, url);
                //if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;


                var cookie = API.GetDeffaultCookie(ld, "");

                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    , ld, true);
               var resp  = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (resp.Contains("false"))
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
        public static AttachResult SetNewGeozoneList(LoginData ld, List<int> AlreadyAttached2Object, string ObjectId)
        {
            try
            {
                string url = URL_attach2Object(ObjectId);



                var param = new List<KeyValuePair<string, string?>>() { };

                foreach (var v in AlreadyAttached2Object)
                {
                    param.Add(new KeyValuePair<string, string?>("OBJECT[AREAS][]", v.ToString()));
                    Log.Text($"[{ObjectId}] geo: {v.ToString()}");
                }


                param.Add(new KeyValuePair<string, string?>("action", "edit_areas"));



                var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));

                HttpRequestMessage hm = new(HttpMethod.Get, newUrl);
                Log.Text(hm.ToString());
                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] { },
                    cookie, ld
                    );

                return AttachResult.OK;

            }


            catch (Exception e) { Log.Error(e); return AttachResult.Failed; }
        }

    }


}
