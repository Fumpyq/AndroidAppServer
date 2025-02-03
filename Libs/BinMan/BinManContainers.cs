using ADCHGKUser4.Controllers.Libs;

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinManParser.Api
{
    public class BinManContainers
    {
       // public string Bin_id;
       public enum BinManResult
        {
            Ok,
            OtherError,
            Failed,
        }
        public enum ContainerStatus
        {
            Not_found = -3,
            Deleted =-2,
            Created=0,
            Failed_To_Create= 2,
            Failed_To_Delete= -4
        }

        public string NAME;
        public Geo_container_type TYPE;
        public string VOLUME;
        public string CLIENT_OWNER="";

        /// <summary>
        /// ??
        /// </summary>
        public string PLACE = "";

        public const string URL_add = API.BaseUrl + "cabinet/containers/add/";
        public static string URL_Delete(string containerBinId) => API.BaseUrl +$"cabinet/containers/detail/{containerBinId}/?action=delete&id={containerBinId}";
        public static string URL_AttachToGeozone(string containerBinId) => API.BaseUrl +$"cabinet/containers/move/{containerBinId}/";
        public static string URL_IsExists(string containerBinId) => API.BaseUrl +$"cabinet/containers/detail/{containerBinId}/";
        

        public Dictionary<string, string> GetCreateFormData(string sessid) =>

        new Dictionary<string, string>()
        { { "sessid",sessid},{ "CONTAINER[PROPERTY_VALUES][COMPANY]", "111275"},
        { "CONTAINER[NAME]", NAME },
        { "CONTAINER[PROPERTY_VALUES][RFID]","" },
        { "CONTAINER[PROPERTY_VALUES][TYPE]", ((int)TYPE).ToString() },
        { "CONTAINER[PROPERTY_VALUES][VOLUME]",VOLUME.Replace(",",".") },
        { "CONTAINER[PROPERTY_VALUES][OWNER_TYPE]","NONE" },
        { "CONTAINER[PROPERTY_VALUES][CLIENT_OWNER]", CLIENT_OWNER },
        { "DOC[0][PROPERTY_VALUES][COMPANY]","111275" },
        { "DOC[0][PROPERTY_VALUES][PLACE]", PLACE},
  };
        public static Dictionary<string, string> GetAttachFormData(string GeozoneBinId) =>

new Dictionary<string, string>()
{ 

        { "status","35" },
        { "date_active_from", DateTime.Now.ToString("dd.MM.yyyy") },
        { "area", GeozoneBinId },
        { "client","" },
        { "contract","" },
};

        public static bool IsContainerExists(LoginData ld,string binid,  out bool exists)
        {
            exists = false;
            try
            {
                HttpRequestMessage hm = new(HttpMethod.Post, URL_IsExists(binid));

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    );

                var res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                exists = !res.Contains("<div class=\"alert alert-danger\">Контейнер не найден</div>");

                return true;
            }
            catch (Exception ex) { Log.Error("IsContainer Exists"); Log.Error(ex); return false; }
        }
        public static bool SendDeleteRequest(LoginData ld, string containerBinId)
        {
            Log.ApiCall($"Delete container [{containerBinId}]");
            try
            {
                string url = URL_Delete(containerBinId) ;
                HttpRequestMessage hm = new(HttpMethod.Get, url);

                //if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;




                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    );

                return true;
            }
            catch (Exception e) { return false; Log.Error(e); }
        }

        public BinManResult SendCreateRequest(LoginData ld,out string binId)
        {
            Log.ApiCall("Insert container");
            binId = "";

            try
            {
                string url = URL_add;
                HttpRequestMessage hm = new(HttpMethod.Post, url);

                if (!API.GetSessIdFrom(ld, url, out string sessId)) return BinManResult.OtherError;

                var data = GetCreateFormData(
                     //ld.PHPSESSID
                     sessId
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

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie,ld, true
                    );
                // Log.Json(req);


#if DEBUG
                var dbText =req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
#endif

               // var uri2ParsebinId = req.Headers.Location;//03.10.2024 Опять обратно поменяли !
                var uri2ParsebinId = req.RequestMessage.RequestUri;//03.10.2024 Опять обратно поменяли !
                var Split1 = uri2ParsebinId.ToString().Split("/");
                binId = Split1[^2].Replace("/","").Trim();
                if (!string.IsNullOrEmpty(binId))
                    return BinManResult.Ok;
                else return BinManResult.Failed;

            }
            catch (Exception e) { Log.Error(e); return BinManResult.OtherError;}
        }
        public static bool SendAttachRequest(LoginData ld, string containerBinId, string geozoneBinId)
        {

            try
            {
                string url = URL_AttachToGeozone(containerBinId);
                HttpRequestMessage hm = new(HttpMethod.Post, url);

                var data = GetAttachFormData(
                    geozoneBinId
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

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie
                    );
              //  Log.Json(req);

               // var t = req.Content.ReadAsStringAsync();
               // t.Wait();
               // var res = t.Result;

              


                return true;

            }
            catch (Exception e) { Log.Error(e);return false; }
        }
    }
}
