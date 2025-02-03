using ADCHGKUser4.Controllers.Libs;
using Dadata.Model;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using System.Web;
using System;
using System.Net.WebSockets;
using Org.BouncyCastle.Utilities;

namespace AndroidAppServer.Libs
{
    public static class Rosreestr
    {


        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

        public static Dictionary<string, KadastrAddInfo> KadInfoMap = new Dictionary<string, KadastrAddInfo>() {
            {"parcel_mzu", new ( "Объект недвижимости", "Многоконтурный земельный участок","25c7e0d1-323d-4522-9d77-f9f4e21848e6","ffd7345c-54cb-40b3-bbea-77fed428c287") },
            {"parcel", new ( "Объект недвижимости", "Земельный участок","e6c8238b-f45c-48a5-b7db-546a6811217e","ffd7345c-54cb-40b3-bbea-77fed428c287") },
            {"parcel_ez", new ( "Объект недвижимости", "Единое землепользование","33dfc5b5-cdf5-4bae-84f7-a09405ae75a1","ffd7345c-54cb-40b3-bbea-77fed428c287") },
        };

        public class RosReestr_Address
        {
            public int object_id { get; set; }
            public int object_level_id { get; set; }
            public int operation_type_id { get; set; }
            public string object_guid { get; set; }
            public int address_type { get; set; }
            public string full_name { get; set; }
            public int region_code { get; set; }
            public bool is_active { get; set; }
            public string path { get; set; }
            public AddressDetails address_details { get; set; }
            public string successor_ref { get; set; }
            public List<Hierarchy> hierarchy { get; set; }
        }
        public enum AddresResponseCode
        {
            AddresNotFound,
            Exception,
            OK
        }
        public class AddressDetails
        {
            public string postal_code { get; set; }
            public string ifns_ul { get; set; }
            public string ifns_fl { get; set; }
            public string ifns_tul { get; set; }
            public string ifns_tfl { get; set; }
            public string okato { get; set; }
            public string oktmo { get; set; }
            public string kladr_code { get; set; }
            public string cadastral_number { get; set; }
            public string apart_building { get; set; }
            public string remove_cadastr { get; set; }
            public string oktmo_budget { get; set; }
        }

        public class Hierarchy
        {
            public string object_type { get; set; }
            public int region_code { get; set; }
            public string name { get; set; }
            public string type_name { get; set; }
            public string type_short_name { get; set; }
            public int object_id { get; set; }
            public int object_level_id { get; set; }
            public string object_guid { get; set; }
            public string full_name { get; set; }
            public string full_name_short { get; set; }
            public string add_number1 { get; set; }
            public string add_type1_name { get; set; }
            public string add_type1_short_name { get; set; }
            public string add_number2 { get; set; }
            public string add_type2_name { get; set; }
            public string add_type2_short_name { get; set; }
            public string number { get; set; }
        }

        public class FiasJsonScheme
        {
            public List<RosReestr_Address> addresses { get; set; }
        }

        public class KadastrAddInfo
        {
            public string type;
            public string kind;
            public string TypeGuid;
            public string KindGuid;

            public KadastrAddInfo(string type, string kind)
            {
                this.type = type;
                this.kind = kind;
            }

            public KadastrAddInfo(string type, string kind, string typeGuid, string kindGuid) : this(type, kind)
            {
                TypeGuid = typeGuid;
                KindGuid = kindGuid;
            }
        }

        public class tryFindByKadastrRespone
        {
            public bool IsSucces;
            public bool IsKnownType;
            public KadastrAddInfo info;
        }

        public class Hint
        {
            public string object_id { get; set; }
            public string full_name { get; set; }
            public string full_name_html { get; set; }
        }

        public class SearchResponse
        {
            public List<Hint> hints { get; set; }
        }


        public const string APIURL = "https://pkk.rosreestr.ru/api/features/1";
        public const string APIURL_FIAS = "https://fias-public-service.nalog.ru/api/spas/v2.0/SearchAddressItems";
        public const string APIURL_FIASbyId = "https://fias-public-service.nalog.ru/api/spas/v2.0/GetAddressItemById";
        public const string APIURL_FIASSearch = "https://fias-public-service.nalog.ru/api/spas/v2.0/GetAddressHint";

        public class RosreestrRespons{
            public RosreestrSingleRow[] features { get; set; }
            public string? id { get => features.Length>0? features [0].attrs.cn:null; }
}
            public record struct RosreestrSingleRow(RosreestrAttribute attrs);
             public record struct RosreestrAttribute(string cn,string parcel_type);
        public static bool tryFindByCoords(GeoPoint gp, out RosreestrRespons  res, int TryCount = 25)
        {
             res = null;
            while (TryCount > 0)
            {
                try
                {
                    
                    using (HttpClient client = new HttpClient())
                    {


                        var q = $"?_=1694071795177&text={gp.mLatitude.ToString().Replace('.', ',')}+{gp.mLongitude.ToString().Replace('.', ',')}&limit=40&skip=0&tolerance=32";


                        var url = APIURL + q;
                        Log.Text(url);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                        var resp = client.Send(request);

                        var t = resp.Content.ReadAsStringAsync();
                        t.Wait();
                        var txt = t.Result;

                        try
                        {
                            Log.Message(txt);
                            res = JsonConvert.DeserializeObject<RosreestrRespons>(txt);
                            if (res == null) return false;
                            return true;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                            return false;
                        }
                    }
                    
                }
                catch (Exception e) { TryCount--; }
                Thread.Sleep(2500);
            }
            return false;
        }

        public static bool tryFindByKadastr(string kad, out tryFindByKadastrRespone res, int TryCount = 25)
        {
            res = null;
            while (TryCount > 0)
            {
                try
                {

                    using (HttpClient client = new HttpClient())
                    {


                        var q =kad;


                        var url = APIURL+"/" + q;
                        Log.Text(url);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

                        var resp = client.Send(request);

                        var t = resp.Content.ReadAsStringAsync();
                        t.Wait();
                        var txt = t.Result;

                        try
                        {
                            
                            var tt = JsonConvert.DeserializeObject<RosreestrRespons>(txt);
                            if (tt == null) return false;
                            
                            if (tt.features!=null && tt.features.Length > 0)
                            {
                                if (tt.features[0].attrs.parcel_type != null)
                                {
                                    if (KadInfoMap.TryGetValue(tt.features[0].attrs.parcel_type, out var inf))
                                    {
                                        res = new tryFindByKadastrRespone()
                                        {
                                            info = inf,
                                            IsKnownType = true
                                        };
                                    }
                                    else
                                    {
                                        Log.Warning($"UNKNOWN TYPE: {tt.features[0].attrs.parcel_type}");
                                        Log.Message(txt);
                                    }
                                }
                                else
                                {
                                    Log.Warning($"UNKNOWN TYPE: {tt.features[0].attrs.parcel_type}");
                                    Log.Message(txt);
                                }
                                res.IsSucces = true;
                                return true;
                            }
                            else
                            {
                                res = new tryFindByKadastrRespone()
                                { IsSucces = false };
                                return true;
                            }
                            

                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                            return false;
                        }
                    }

                }
                catch (Exception e) { TryCount--; }
                Thread.Sleep(2500);
            }
            return false;
        }

       public static bool tryNormalizeAddres(string address, out Hint res)
        {
            res = null;
            var uriBuilder = new UriBuilder(APIURL_FIASSearch);
            var parameters = HttpUtility.ParseQueryString(string.Empty);

            parameters["search_string"] = address;
            parameters["address_type"] = "1";

            string txt = string.Empty;
            uriBuilder.Query = parameters.ToString();
            try
            {
                var client = new HttpClient();

                var req = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
                req.Headers.Host = "fias-public-service.nalog.ru";
                req.Headers.Referrer = new Uri("https://fias-public-service.nalog.ru/api/spas/v2.0/swagger/index.html");
                req.Headers.Add("Master-Token", "71ca31a0-9723-4c46-b0e4-f77d21b46f1b");
                //Log.Text(req.ToString());
                var r = client.Send(req);
                var t = r.Content.ReadAsStringAsync();
                t.Wait();
                txt = t.Result;
                // Log.Text(txt);
                var tt = JsonConvert.DeserializeObject<SearchResponse>(txt);
                if(tt!=null && tt.hints!=null && tt.hints.Count>0)
                {
                    res = tt.hints[0];
                    return true;
                }
                // Log.Json(res);
                return false;
            }
            catch (Exception ex) { Log.Error(ex); Log.Error(txt); return false; }

        }
        public static AddresResponseCode tryFindByAddres_FIAS(string prompt, out FiasJsonScheme res)
        {
            res = null;

            if (!tryNormalizeAddres(prompt, out var nrm))
            { return AddresResponseCode.AddresNotFound; }


          
            var uriBuilder = new UriBuilder(APIURL_FIASbyId);
            var parameters = HttpUtility.ParseQueryString(string.Empty);

            parameters["object_id"] =  nrm.object_id;
            parameters["address_type"] = "1";

            string txt =string.Empty;
            uriBuilder.Query = parameters.ToString();
            try
            {
                var client = new HttpClient();
                
                var req = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
                req.Headers.Host = "fias-public-service.nalog.ru";
                req.Headers.Referrer =new Uri("https://fias-public-service.nalog.ru/api/spas/v2.0/swagger/index.html");
                req.Headers.Add("Master-Token", "71ca31a0-9723-4c46-b0e4-f77d21b46f1b");
                //Log.Text(req.ToString());
                var r = client.Send(req);
                var t = r.Content.ReadAsStringAsync();
                t.Wait();
                txt = t.Result;
               // Log.Text(txt);
                res = JsonConvert.DeserializeObject<FiasJsonScheme>(txt);
               // Log.Json(res);
                return AddresResponseCode.OK;
            }
            catch (Exception ex) { Log.Error(ex); Log.Error(txt); return AddresResponseCode.Exception; }

           

        }


        public static void ParseKadastrsByAddreses()
        {
            var objects = SQL.GetObjectList2KadastrParse();

            int errorsInRow = 0;
            long number = 0;
            foreach (var v in objects)
            {
                try
                {
                   // Console.Title = $"Запись {number}/{objects.Count}";
                    number++;
                    var status = Rosreestr.tryFindByAddres_FIAS(v.address, out var res);
                    if (status == AddresResponseCode.OK)
                    {
                        string kadastr = string.Empty;
                        foreach (var k in res.addresses)
                        {
                            if (k.address_details.cadastral_number != null) { kadastr = k.address_details.cadastral_number; break; };

                            break;// Только первый наиболее верный иначе совсем мимо
                        }
                        errorsInRow = 0;
                        if (!string.IsNullOrEmpty(kadastr))
                        {
                            //Log.Text($"{v.guid}, {kadastr} Insert");
                            SQL.InsertFIASKadastrParse(v.guid, kadastr);
                        }
                        else
                        {
                           // Log.Warning($"{v.guid}, kadastr Null");
                            SQL.InsertFIASKadastrParse(v.guid, "Не найден");
                        }


                    }
                    else if (status == AddresResponseCode.AddresNotFound)
                    {
                        Log.Warning($"{v.guid}, kadastr Null");
                        SQL.InsertFIASKadastrParse(v.guid, "Не найден");
                    }
                    else if (status == AddresResponseCode.Exception)
                    {
                        if (errorsInRow >= 10) break;
                        errorsInRow++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        public static void ParseKadTypesByKadastr()
        {
            var objects = SQL.GetObjectList2KadTypeParse();

            int errorsInRow = 0;
            long number = 0;
            int chunksize = objects.Count / 8 +1;
            var splt = objects.Chunk(chunksize);
            List<Task> wait = new List<Task>(8);
            foreach (var ch in splt)
            {
                Task t = Task.Run(() =>
                {
                    foreach (var v in ch)
                    {
                        try
                        {
                            Console.Title = $"Запись {number}/{objects.Count}";
                            number++;

                            if (Rosreestr.tryFindByKadastr(v.kadastr, out var res))
                            {
                                if (res.IsSucces)
                                {
                                    if (res.IsKnownType)
                                    {
                                        Log.Text($"INSERT for `{v.guid}` - T: `{res.info.type}` L: `{res.info.kind}`");
                                        SQL.InsertKadastrTypeParse(v.guid, res.info.TypeGuid, res.info.KindGuid);
                                    }

                                }
                                else
                                {
                                    Log.Text($"INSERT for `{v.guid}` - T: `null` L: `null`");
                                    SQL.InsertKadastrTypeParse(v.guid, null, null);
                                }
                            }
                            else
                            {
                                if (errorsInRow >= 10) break;
                                errorsInRow++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }
                });
               wait.Add(t);
            }

            Task.WaitAll(wait.ToArray());
        }
    
        public static void StartWorker()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Rosreestr.ParseKadastrsByAddreses();
                        Thread.Sleep(1000 * 60 * 60); // - 1 H
                    }
                    catch (Exception ex)
                    {
                        Log.Error("KadParser", ex);
                        Thread.Sleep(1000 * 60 * 10); // - 10 min
                    }
                }
            });
        }
    }
}
