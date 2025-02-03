using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using AndroidAppServer.Libs.BinMan;
using BinManParser.Api;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AndroidAppServer.Libs.DadataApi;

namespace BinManParser.Api
{
    public static class AddresStringExtension {
        public static string AppendAddress(this string CurAddress, string ToAdd)
        {
            return string.IsNullOrEmpty(CurAddress) ? ToAdd : CurAddress.Trim() +( string.IsNullOrEmpty(ToAdd) ?"": ", " + ToAdd.Trim());
        }
    }
    internal class BinManGeozones
    {
    }
    public struct CreateGeozoneResponse
    {
        public int area_id { get; set; }
        public int success { get; set; }
    }
    public enum Geo_container_type
    {
        unknown=0,
        evro = 121,             //Евроконтейнер
        shipForPortal = 122,    //"Лодочка" под портал 
        shipForRope = 123,      //"Лодочка" под лебёдку
        Deep = 124,             //Заглубленный
        MultiElev = 125,        //Мультилифт
        SideLoad = 126,         //Для боковой загрузки
        evro_net = 173,         //Евроконтейнер. Сетка
        Frontal = 172           //Фронтальный
    }

    public enum Geo_Group
    {
        mp = 3,     //Муниципальные площадки (МП)
        puk = 4,     //Площадки УК (ПУК)
        pul = 5,     //Площадки ЮЛ (ПЮЛ)
        pfl = 6,    //Площадки ФЛ (ПФЛ)
        ptk = 7,     //Площадки тер. кладбища (ПТК)
        pubdh = 8,   //Площадки Управления БДХ (ПУБДХ)
        pmo = 9,     //Площадки мед. отходов (ПМО)
        sp = 10,     //Смешанные площадки (СП)
    }
    public enum Geo_Lot
    {

        lot_2 = 2,
        lot_3 = 3,
        lot_4 = 5,
        lot_5 = 7,
        lot_6 = 8,
        lot_7 = 9,
        lot_8 = 12,
        lot_9 = 13,
        lot_10 = 10,
    }
    public enum Geo_shipment_type
    {
        handy = 39,          //Ручная отгрузка
        container = 40,      //Контейнерная площадка
    }
    public enum Geo_area_basis// зачем мудрить с названиями :Ъ
    {
        scheben = 153,
        grunt = 152,
        beton = 155,
        asfalt = 154,
    }
    public class Container
    {
        public Container() { }
        public Container(Geo_container_type tYPE, float vOLUME, int count)
        {
            TYPE = tYPE;
            VOLUME = vOLUME;
            this.count = count;
        }

        public Geo_container_type TYPE { get; set; }    //Тип
        public float VOLUME { get; set; }                 //Объем
        public int count { get; set; }                  //Кол-во
    }
}
public class BinManGeozoneTask: BinManGeozone
{
    public BinManTaskType TaskType;
    public List<GeoContainer> ContainerList;
    public LoginData ld;
    public bool isAddressHandmade;
    public bool IsArchive;
    public bool NeedToBeArchived;
}
public class BinManGeozone: BinmanAddresStorage
{
    public string DataBase_Guid;
    public long LAST_AREA { get; set; } // Проще говоря id, эквивалентен номеру в адресе геозоны

    public string COMPANY { get => "111275"; }
    public string NAME { get; set; }
    public bool NO_AUTO_COORDINATES { get; set; }//false = '', true = ???
        
    public Geo_Group? GROUP { get; set; }        //Группа
    public Geo_Lot lot_id { get; set; }         //Лот
    /// <summary>
    /// Тип площадки
    /// </summary>
    public Geo_shipment_type SHIPMENT { get; set; } 
    /// <summary>
    /// Характеристики площадки
    /// </summary>
    public Geo_area_basis AREA_BASIS { get; set; }
    /// <summary>
    /// Навес                  159 - false, 158-true 
    /// </summary>
    public bool AREA_CANOPY { get; set; }
    /// <summary>
    /// Ограждение          157 - false, 156 - true  
    /// </summary>
    public bool AREA_ENCLOSURE { get; set; }    

    public Container[] containers { get; set; } //Для создания контейнеры
    public string? Db_groupGuid;
    public string? Db_containerGuid;
    public static Dictionary<string, string> Image2TypeGuid = new Dictionary<string, string>()
        {
            { "/local/templates/cabinet/img/report-list-for_side_loading.svg"     ,"BD423E8B-2AC2-4C16-AFD3-20025D4F5B82" },
            { "/local/templates/cabinet/img/report-list-eurocontainer.svg"        ,"B28E1AD5-570D-4271-897F-00005C224FE9" },
            { "/local/templates/cabinet/img/report-list-reclining.svg"            ,"D65ED5D6-05D4-489B-A140-0002201DE98C" }, //Заглубленный
            { "/local/templates/cabinet/img/report-list-boat_under_the_portal.svg","3527C2A0-58D6-44EB-B222-00010160C467" }, //Заглубленный
            { "/local/templates/cabinet/img/report-list-multilift.svg"            ,"FADC87A9-6241-4CFF-8D94-2201CA949C78" }, //Заглубленный
            { "/local/templates/cabinet/img/report-list-boat_under_the_winch.svg" ,"7E050779-A962-4993-9CBE-0001224A0DA2" }, //Заглубленный
        };
    public static Geo_Group? Guid2GROUP(string? Guid) => Guid?.ToUpper() switch
    {
        "43CA6E2B-4A7C-49DD-9725-00DED46158A9" => Geo_Group.pul,
        "222A6E2B-4A7C-49DD-9725-00DED46658A9" => Geo_Group.mp,
        "332A6E2B-4A7C-49DD-9725-00DED46658A9" => Geo_Group.puk,
        "03CA6E2B-4A7C-49DD-9725-00DED46658A9" => Geo_Group.pfl,
        "" or null => null,
        _ => NullCase(Guid)
    };
    public static Geo_Group? NullCase(string? Guid)
    {
        { Log.Warning($"Unknown geozone group {Guid}"); return null; }
    }
    public static Geo_area_basis Guid2Enum(string guid) => guid.ToUpper() switch
    {
        "99900021-C9F6-77AB-CCEF-5900C3854000" => Geo_area_basis.asfalt,
        "99900022-C9F6-77AB-CCEF-5900C3854000" => Geo_area_basis.beton,
        "99900023-C9F6-77AB-CCEF-5900C3854000" => Geo_area_basis.grunt,
        "99900025-C9F6-77AB-CCEF-5900C3854000" => Geo_area_basis.scheben,
        "АСФАЛЬТ" => Geo_area_basis.asfalt,
        "БЕТОН" => Geo_area_basis.beton,
        "ГРУНТ" => Geo_area_basis.grunt,
        "ЩЕБЕНЬ" => Geo_area_basis.scheben,
        _=>throw new Exception("Guid2Enum Не определилось основание геозоны")
    };


    public const string URL_add = API.BaseUrl + "cabinet/areas/add/";
    public static string URL_edit(long id) => API.BaseUrl + "cabinet/areas/edit/" + id + "/";
    public static string URL_attach2Object(string objId) => API.BaseUrl + $"cabinet/objects/detail/{objId}/";
    public static string URL_getContaienrs(string geoId) => API.BaseUrl + $"cabinet/containers/?area={geoId}";
    public static string URL_Archivate(string geoId) => API.BaseUrl + $"cabinet/areas/detail/{geoId}/";
    public static string URL_delete(string geoId) => API.BaseUrl + $"cabinet/areas/detail/{geoId}/?delete_area=1";
    //https://binman.ru/cabinet/areas/detail/5959148/?delete_area=1
    public static string GenerateName(long binId, string? groupGuid,params string[]? ContainerTypeGuid)
    {
        string container = "";
        if (ContainerTypeGuid != null)
        {
            foreach (var type in ContainerTypeGuid.GroupBy(x=> StaticSQl.GetContainer(x).shortName))
            {
                if (type != null)
                {
                    //var cont = ;
                    container += $" {type.Key}";
                }
            }


        }

        return $"{binId}{(groupGuid != null ? " " + GetGroupNameFromGuid(groupGuid) : "")}{container}";
    }
    public static string GenerateName(long binId, string? groupGuid, params GeoContainer[]? Containers)
    {
        string container = "";
        if (Containers != null)
        {
            foreach (var cs in Containers.GroupBy(x=> StaticSQl.GetContainer(x.typeGuid).shortName))
            {
                if (cs != null)
                {
                    //var cont = ;
                    container += $" {cs.Key}";
                }
            }


        }

        return $"{binId}{(groupGuid != null ? " " + GetGroupNameFromGuid(groupGuid) : "")}{container}";
    }

    public void SetNameFromGeozone(Geozone geo)
    {
        bool isMultiple = false;
     
        if (geo.containers != null && geo.containers.Count > 0)
        {
            if (geo.containers.Count > 1)
            {
                Dictionary<string, List<float>> map = new Dictionary<string, List<float>>();
                foreach (var v in geo.containers)
                {

                    if (map.TryGetValue(v.guid, out var vol))
                    {
                        foreach (var volume in vol)
                        {
                            if (volume == v.volume)
                            {
                                isMultiple = true;
                                break;
                            }
                        }
                        if (isMultiple) break;
                        map.Add(v.guid, vol);
                    }
                }
            }
            else isMultiple = false;
        }
        if (!isMultiple)
        {
            NAME = GenerateName(LAST_AREA, geo.geozoneGroup, geo.containers[0].guid);
        }
        else
        {
            NAME = GenerateName(LAST_AREA, geo.geozoneGroup, geo.containers.ToArray());
        }
    }


    public static string GetGroupNameFromGuid(string guid) => guid.ToUpper() switch
    {
        "43CA6E2B-4A7C-49DD-9725-00DED46158A9" => "ЧКЮЛ",
        "222A6E2B-4A7C-49DD-9725-00DED46658A9" => "МК",
        "332A6E2B-4A7C-49DD-9725-00DED46658A9" => "УК",
        "03CA6E2B-4A7C-49DD-9725-00DED46658A9" => "ЧКФЛ"
    };

    

    public string getGroupNameFromGuid(string guid) => GetGroupNameFromGuid(guid);

    public string BuildAddress()
    {
        return string.Empty
            .AppendAddress(REGION)
            .AppendAddress(AREA)
            .AppendAddress(CITY)
            .AppendAddress(SETTLEMENT)
            .AppendAddress(string.IsNullOrEmpty(SETTLEMENT)? CITY_DISTRICT:string.Empty)
            .AppendAddress(STREET)
            .AppendAddress(BinManObject.formatHome(HOUSE))
            .AppendAddress(BLOCK)
            ;
    }


    public Dictionary<string, string> GetEditFormData(string sessid) =>

     new Dictionary<string, string>()
    {
            { "sessid",sessid},
            { "LAST_AREA" ,LAST_AREA.ToString()},
            { "AREA[PROPS][COMPANY]",COMPANY},
            { "AREA[NAME]",NAME},
            { "AREA[PROPS][NO_AUTO_COORDINATES]","1"},
            { "AREA[PROPS][REGION]", REGION},
            { "AREA[PROPS][AREA]",AREA},
            { "AREA[PROPS][CITY]",CITY},
            { "AREA[PROPS][SETTLEMENT]",SETTLEMENT},
            { "AREA[PROPS][CITY_DISTRICT]",CITY_DISTRICT},
            { "AREA[PROPS][STREET]",STREET},
            { "AREA[PROPS][HOUSE]",BinManObject.formatHome(HOUSE)},
            { "AREA[PROPS][BLOCK]",BLOCK},
            { "AREA[PROPS][LAT]",LAT.ToString("0.00000000000000").Replace(",",".")},
            { "AREA[PROPS][LON]",LON.ToString("0.00000000000000").Replace(",",".")},
            { "lot_id",((int)lot_id).ToString()},
            { "AREA[PROPS][GROUP]",(GROUP==null?"":((int)GROUP.Value).ToString())},
            { "AREA[PROPS][ADDRESS]", BuildAddress() /*ADDRESS*/},
            { "AREA[PROPS][SHIPMENT]",((int)SHIPMENT).ToString()},
            { "AREA[PROPS][AREA_BASIS]",((int)AREA_BASIS).ToString()},
            { "AREA[PROPS][AREA_ENCLOSURE]",AREA_ENCLOSURE  ? "156": "157"},
            { "AREA[PROPS][AREA_CANOPY]",AREA_CANOPY ? "158": "159"},
            { "AREA[PROPS][IMAGE]",""},
    };
    public static Dictionary<string, string> GetArchFormData(bool isArchiving) =>

 new Dictionary<string, string>()
{
            { "row[date]",DateTime.Now.ToString("dd.MM.yyyy")},
            { "row[status]",isArchiving?"57":"66"},
};
    public Dictionary<string, string> GetCreateFormData(string sessid)
    {

        //AREA[PROPS][RADIUS]: 30
        var td = GetEditFormData(sessid);

        int i = 1;
        if(containers!=null)
        foreach (var v in containers)
        {
            td.Add($"containers[{i}][props][TYPE]", ((int)v.TYPE).ToString());
            td.Add($"containers[{i}][props][VOLUME]", v.VOLUME.ToString());
            td.Add($"containers[{i}][count]", v.count.ToString());
            i++;
        }
        td.Add("AREA[PROPS][RADIUS]", "30");
        return td;
    }
    //public string GetSessIdFrom(LoginData ld, string url)
    //{
    //    HttpRequestMessage hm = new(HttpMethod.Post, url);

    //    var data = GetEditFormData(
    //        //ld.PHPSESSID
    //        "e7ea4754e26971c103c8318702e00a51"
    //        );

    //    Log.Json(data);




    //    var cookie = API.GetDeffaultCookie(ld, "");
    //    var req = API.SendRequest(hm,
    //        null,
    //        cookie
    //        );
    //    Log.Json(req);
    //    var t = req.Content.ReadAsStringAsync();

    //    t.Wait();

    //    var html = t.Result;

    //    var htmlDoc = new HtmlDocument();
    //    htmlDoc.LoadHtml(html);

    //    //sessid

    //    var SesIdParse = htmlDoc.DocumentNode.Descendants("input");
    //    // int l = SesIdParse.Count();

    //    SesIdParse = SesIdParse.Where(d => d != null && d.Attributes["name"] != null && d.Attributes["name"].Value != null && d.Attributes["name"].Value.Length > 0 &&
    //    d.Attributes["name"].Value.Contains("sessid"));
    //    string sesid = SesIdParse.First().Attributes["value"].Value.Trim();
    //    Log.Text(sesid);

    //    // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test2.html");

    //    return sesid;
    //}

    public static bool SendDeleteRequest(LoginData ld, string Idgeo)
    {
        Log.ApiCall($"Delete GEOZONE [{Idgeo}]");
        HttpRequestMessage hm = new(HttpMethod.Get, URL_delete(Idgeo));
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
        if(res.Contains("не является архивной и не может быть удалена"))
        {
            return false;
        }
        return true;
    }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="ld"></param>
   /// <returns>is operation success</returns>
    public bool SendEditRequest(LoginData ld)
    {


        StringBuilder sb = new StringBuilder();
        try
        {
            string url = URL_edit(LAST_AREA);
            HttpRequestMessage hm = new(HttpMethod.Post, url);
            if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;
            var data = GetEditFormData(
                 //ld.PHPSESSID
                 sessId
                );

           // Log.Json(data);

            hm.Content = new FormUrlEncodedContent(data);
            var dd = hm.Content.ReadAsStringAsync();
            dd.Wait();

          

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
            },  cookie , ld, true);
         //   Log.Json(req.Content);
         //   var t = req.Content.ReadAsStringAsync();

          //  t.Wait();
         
         //   Log.Text(t.Result);
         //   Log.Json(t.Result);
          //  var response = JsonConvert.DeserializeObject<CreateGeozoneResponse>(t.Result);
         //   if (response.success != 1)
          //  {
          //      return false;
          //  }


            return true;

            //  var html = t.Result;

            // var htmlDoc = new HtmlDocument();
            // htmlDoc.LoadHtml(html);

            // htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test.html");


        }
        catch (Exception e) { Log.Error(e); Log.Error("Request object:\n" + sb.ToString()); Log.Text("+++ " + sb.ToString()); return false; }
    }
    public static bool SendSetArchiveRequest(LoginData ld,string bin_id,bool IsArchiving)
    {
        Log.ApiCall($"Set Arch [{bin_id}:{IsArchiving}]");
        try
        {
            string url = URL_Archivate(bin_id);
            HttpRequestMessage hm = new(HttpMethod.Post, url);
          
            var data = GetArchFormData(IsArchiving);


            hm.Content = new FormUrlEncodedContent(data);

            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm,
                new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")  }, cookie , ld, false);


            return true;
        }
        catch (Exception e) { Log.Error("Failed to set geozone archive status", e); return false; }
    }
    /// <summary>
    /// BinMan: Обычно приходит нормальный json ответ
    /// </summary>
    /// <param name="ld"></param>
    /// <param name="bin_id"></param>
    /// <returns></returns>
    public bool SendCreateRequest(LoginData ld , out int bin_id)
    {
        Log.Text("SendCreateRequest");
        Log.Json(ld);
        StringBuilder sb = new StringBuilder();
        bin_id = -1;
        try
        {
            string url = URL_add;
            HttpRequestMessage hm = new(HttpMethod.Post, url);
            if (!API.GetSessIdFrom(ld, url, out string sessId)) return false;
            var data = GetCreateFormData(
                //ld.PHPSESSID
                sessId
                );

            Log.Json(data);

            hm.Content = new FormUrlEncodedContent(data);
            var dd = hm.Content.ReadAsStringAsync();
            dd.Wait();

          

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
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest") },cookie , ld, true);
            Log.Json(req.Content);
            var t = req.Content.ReadAsStringAsync();
            
            t.Wait();
          
        
            Log.Text(t.Result);
            Log.Json(t.Result);
            var response = JsonConvert.DeserializeObject<CreateGeozoneResponse>(t.Result);
            bin_id = response.area_id;
            if (response.success != 1)
            {
                return false;
            }

            LAST_AREA = bin_id;
           
                 
            return true;

            //   var html = t.Result;

            //    var htmlDoc = new HtmlDocument();
            //  htmlDoc.LoadHtml(html);

            //htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\geozones\\test.html");


        }
        catch (Exception e) { Log.Error(e); Log.Error("Request object:\n" + sb.ToString()); return false; }
    }
    /// <summary>
    /// Container guid it's binid
    /// </summary>
    /// <param name="ld"></param>
    /// <param name="geo_id"></param>
    /// <param name="containers"></param>
    /// <returns></returns>
    public static bool GetGeozoneContainers(LoginData ld,string geo_id, out List<GeoContainer> containers)
    {
        Log.ApiCall($"Get geozone containers [{geo_id}]");
        HttpRequestMessage hm = new(HttpMethod.Get, URL_getContaienrs(geo_id));
        var cookie = API.GetDeffaultCookie(ld, "");
        var req = API.SendRequest(hm,
               new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
               },cookie,ld
               );

        var t = req.Content.ReadAsStringAsync();

        t.Wait();

        var res = t.Result;
        //Log.Text(res);
        //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(res);

        containers = new List<GeoContainer>();


        var MainSlice = htmlDoc.DocumentNode.Descendants("div").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
        int count = MainSlice.Count();
        if (count == 0) return false;
        for (int i = 0; i < count; i++)
        {
            try
            {

                var CurrentRow = MainSlice.ElementAt(i);


                var LinksAndNames = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                var Volume = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                var ImageType = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-type-icon"));

                var ContainerEl = LinksAndNames.ElementAt(0);
                var ContainerLink = ContainerEl.Descendants("a").First().Attributes["href"].Value;
                int ContainerId = int.Parse(ContainerLink.Split("/")[^2].Replace("/", ""));

                var ContainerName = TrimText(ContainerEl.InnerText);
                ;
                var GeozoneEl = LinksAndNames.ElementAt(1);
                var GeozoneLink = GeozoneEl.Descendants("a").First().Attributes["href"].Value;
                int GeozoneId = int.Parse(GeozoneLink.Split("/")[^2].Replace("/", ""));
                var GeozoneName = TrimText(GeozoneEl.InnerText);

                var innerText = Volume.ElementAt(0).InnerText;

                var intextSplit = innerText.Split(" м³ ");

                var ContainerVolume = TrimText(intextSplit[0]);


                var imgLink = ImageType.ElementAt(0);
                var vil = imgLink.Descendants("img").First();
                string ImageLink = vil.Attributes["src"].Value;


                var c = new GeoContainer();
                try
                {
                    c.volume = float.Parse(ContainerVolume.Replace(".",","));
                }
                catch (Exception ex)
                {
                    Log.Error("Cant pasrse container volume");
                    Log.Error(ex);
                }
                if (!Image2TypeGuid.TryGetValue(ImageLink, out var TypeGuid))
                    TypeGuid = "NULL";

                c.typeGuid = TypeGuid;
                c.guid = ContainerId.ToString();

                containers.Add(c);
                //if (HandlersMap.TryGetValue(cp.geozoneBinId, out var hh))
                //{
                //    hh.Add(cp);
                //}
                //else
                //{
                //    hh = new GeozoneHandler();
                //    hh.Add(cp);
                //    HandlersMap.Add(cp.geozoneBinId, hh);
                //}










                string TrimText(string txt)
                {
                    return Regex.Replace(txt, @"\s+", " ").Replace("пнвтсрчтптсбвс", "").Replace("\n\n", "").Trim();
                }


            }
            catch (Exception e)
            {
                Log.Error(e.Message);

            }





        }
        return true;

        
    }

   public enum AttachResult
    {
        NotAttachedInBinman,
        OK,
        Failed,
    }
    public static AttachResult AttachToObject(LoginData ld,List<int> AlreadyAttached2Object,string geoId, string ObjectId,bool IsAttach=true)
    {
        try
        {
            string url = URL_attach2Object(ObjectId);
           

           
            var param = new List<KeyValuePair<string, string?>>() { };
            Log.Text($"Will insert geozones to obj {ObjectId}");
            if (IsAttach)
            {
                param.Add(new KeyValuePair<string, string?>("OBJECT[AREAS][]", geoId));
                Log.Text($"[{ObjectId}] geo: {geoId}");
                foreach (var v in AlreadyAttached2Object)
                {
                    param.Add(new KeyValuePair<string, string?>("OBJECT[AREAS][]", v.ToString()));
                    Log.Text($"[{ObjectId}] geo: {v.ToString()}");
                }
            }
            else
            {
                var tt = int.Parse(geoId);
                if (AlreadyAttached2Object.Contains(tt))
                    AlreadyAttached2Object.Remove(tt);
                else {
                    Log.Warning("Геозона в BinMan не привязана, невозможно отвязать то, что не привязанно");
                    return AttachResult.NotAttachedInBinman;
                }
                foreach (var v in AlreadyAttached2Object)
                {
                    param.Add(new KeyValuePair<string, string?>("OBJECT[AREAS][]", v.ToString()));
                    Log.Text($"[{ObjectId}] geo: {v.ToString()}");
                }
            }

            param.Add(new KeyValuePair<string, string?>("action", "edit_areas"));
          


            var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));

            HttpRequestMessage hm = new(HttpMethod.Get, newUrl);
            Log.Text(hm.ToString());
            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm,
                new KeyValuePair<string, string>[] {},
                cookie,ld
                );

            return AttachResult.OK;

        }


        catch (Exception e) { Log.Error(e); return AttachResult.Failed; }
    }

}


