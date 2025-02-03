using BinManParser.Api;
using CHGKManager.Libs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using static AndroidAppServer.Controllers.DadataController;
using static AndroidAppServer.Libs.DadataApi;
using static CHGKManager.Libs.ActiveDirectory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ADCHGKUser4.Controllers.Libs
{
    public enum EventsList
    {
        insert,
        update,
        need_clean,
        clean_start,
        clean_stop,
        clean_finish,
        no_find,
        damage,
        visit,
        move,
        need_insert
    }

    public record struct GeoPoint(double mLongitude, double mLatitude);
    public enum BasementType
    {
        scheben=0,
        asfalt=1,
        grunt=2,
        beton=3,
    }
    public enum FileQuality
    {
        /// <summary>
        /// 128
        /// </summary>
        preview,
        /// <summary>
        /// 512
        /// </summary>
        med,
        /// <summary>
        /// no scaling applyed
        /// </summary>
        original,
    }
    public class GeoObjectEditRequest
    {
        public string guid { get; set; }
        public string name { get; set; }
        public string typeGuid { get; set; }
        public string subTypeGuid { get; set; }
        public bool isRentor { get; set; }
        public float area { get; set; }
        

        public string commantary { get; set; }
        public GeoPoint newPosition { get; set; }
        public GeoPoint originalPosition { get; set; }


    }
    public enum FileType
    {
        foto, sign
    }
    public enum BinManSyncStatus
    {

        update,
        ok

    }
    public class User
    {
        public string guid { get; set; }
        public string fio { get; set; }
        public string fio_short { get; set; }
        public string Division_guid;
    }
    public class Division
    {
        public string title { get; set; }
        public string guid { get; set; }
        public List<User> users { get; set; } = new List<User>();
    }

    public class CustomUserComment
       
    {
        public string guid { get; set; }
        public string comment { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? dateUpdated { get; set; }

        public string form { get; set; }
    }
    public class TrashPileUpdateRequest: IllegalTrashPileFullData
    {
        public GeoPoint newPosition { get; set; }
    }
    public class CloseTrashPileRequest
    {
        public string guid { get; set; }
        public string desc { get; set; }
        public DateTime date { get; set; }
        public float volume { get; set; }
        public GeoPoint? userPosition { get; set; }
    }
    public class SearchResults<T>
    {
        public bool isDataFinded { get; set; }
        public GeoPoint point { get; set; }
        public List<T> results { get; set; }
    }

    public class ObjectType
    {
        public string guid  { get; set; }
        public string name  { get; set; }
        public string icon  { get; set; }
        public int binId    { get; set; }
   

    }
    public class PrimaryObjectType: ObjectType
    {
        public List<ObjectType> subTypes { get; set; } = new List<ObjectType>();
    }
    public class UserAccount {
        public string nickName;
        public bool isGpsTrackingEnabled;
        public TimeOnly WorkTimeStart;
        public TimeOnly WorkTimeEnd;
        public List<GeozoneTask> tasks;
        public bool isDebugEnabled;
    }
    public class ContainerEnum
    {
        public string guid;
        public string name;
        public string? volume;
        public string shortName;
    }
    public class GeoObjectLinkTask
    {
        public string DbGuid { get; set; }
        public string GeoBinId{ get; set; }
        public string ObjectBinId{ get; set; }
        public BinManTaskType TaskType{ get; set; }
    }
    public enum ContainerEditAction
    {
        delete,
        insert,
    }
    public class GeoContainerEdit {
        public ContainerEditAction action { get; set; }
        public GeoContainer container { get; set; }
    }
    
    public class GeoContainer {
        /// <summary>
        /// If it is from BinmanGeozones.GetGeozoneContainers()
        /// When it is BinId _-_
        /// </summary>
        public string? guid { get; set; }
        public string? type { get; set; }
        public string typeGuid { get; set; }
        public string? title { get; set; }
        public string? typeShort { get; set; }
        public string? clientName { get; set; }
        public string? clientInn { get; set; }
        public int count { get; set; }
        public int negcount { get; set; }
        public float volume { get; set; }
        public int? containerNumber { get; set; }

        public Geo_container_type GetBinManType() => typeGuid.ToUpper() switch
        {

            "B28E1AD5-570D-4271-897F-00005C224FE9" => Geo_container_type.evro,
            "3527C2A0-58D6-44EB-B222-00010160C467" => Geo_container_type.shipForPortal,
            "7E050779-A962-4993-9CBE-0001224A0DA2" => Geo_container_type.shipForRope,
            "D65ED5D6-05D4-489B-A140-0002201DE98C" => Geo_container_type.Deep,
            "73AB6C70-1E8D-4D3B-999D-10122F4696B3" => Geo_container_type.evro_net,
            "BD423E8B-2AC2-4C16-AFD3-20025D4F5B82" => Geo_container_type.SideLoad,
            "FADC87A9-6241-4CFF-8D94-2201CA949C78" => Geo_container_type.MultiElev,
            "250D87C0-D994-4593-93C0-C0BF2ED73450" => Geo_container_type.Frontal,
            _ => Geo_container_type.unknown,
        };

    }
    public class GeozoneTask: IHaveId
    {
        public string guid { get; set; }
        public string geozoneGuid { get; set; }

        //  public Geozone geozone { get; set; }
        public GeozoneMarker marker { get; set; }
        public string status { get; set; }
        public string statusCode { get; set; }
        public string statusGuid { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public int order { get; set; }
        public string description { get; set; }
        public DateTime? deadPlan { get; set; }
        public DateTime? dateFact { get; set; }
        public DateTime? dateCreate { get; set; }

        public List<SQlFileInfo> rawImages { get; set; }
        public bool canIExecute { get; set; }
        /// <summary>
        /// FIO 
        /// </summary>
        public string executor { get; set; }
        public string executor_guid { get; set; }
        public bool isImportant { get; set; }
        public bool isPhotoRequired { get; set; }
        public string number { get; set; }

        //[JsonIgnore]
        public string GetId() => geozoneGuid;
        public override string ToString()
        {
            return guid.Substring(0, 5) + "..." + guid.Substring(guid.Length - 5) + " Geo:" + (geozoneGuid == null ? "NULL" : "OK");
        }

    }
    public class GeozoneCreateCallBack {
        public string? guid { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
    }
    public class GeoObjectMarker
    {
        public GeoPoint position { get; set; }
        public string guid { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string objectIcon { get; set; }
        public bool needView { get; set; }
        public int chained { get; set; }
        public Client client { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public string document { get; set; }
        public bool isDogActive { get; set; }
        public bool isArch { get; set; }
    }
    public class GeoObjectCreateRequest
    {
        public GeoPoint position { get; set; }
        public ObjectType objectType{ get; set; }
        public ObjectType subObjectType { get; set; }
        public bool isRentor { get; set; }
        public float area { get; set; }
        public string? userTitle { get; set; }
      //  public string? commentary { get; set; }

    }
    public class GeoObject: GeoObjectMarker
    {
     

        public string objectType { get; set; }
        public string subObjctType { get; set; }
        public string binId  { get; set; }
        public bool isRentor { get; set; }
        public float area { get; set; }
        public string? kadastr { get; set; }

    }

    public class Client
    {
        public string guid { get; set; }
        public string name { get; set; }
        public string inn { get; set; }
        public string ogrn { get; set; }
    }
    public class UniversalEvent {
        /// <summary>
        /// only when from DB
        /// </summary>
        public string? guid { get; set; }
        public string type { get; set; }
        public bool isCanBeDeleted { get; set; }
        /// <summary>
        /// not all events have description
        /// </summary>
        public string? description { get; set; }
        public DateTime dateTime { get; set; }
        public const string GEO_EVENT_Move="move";
        public const string GEO_EVENT_InsertRequest= "need_insert";
    }
    public class AnyEventType
    {
        public string guid { get; set; }
        public string title { get; set; }
        public int photoLvl { get; set; }
        public int coordsLvl { get; set; }
        public string addAction { get; set; }
    }
    public class GeozoneEventType : AnyEventType { }
    public class ObjectEventType : AnyEventType
    {
        public string name { get; set; }
        public string code { get; set; }
    }

    public class CommentaryInfo
    {
        public string icon { get; set; }
        public string? title { get; set; }
        public string? subTitle { get; set; }
        public string? user { get; set; }

        public DateTime? date { get; set; }

    }
    public interface IOriginalUserCoordinates
    {
        public GeoPoint userPosition { get; set; }
    }
    public class TrackPoint
    {
        public DateTime time { get; set; }
        public GeoPoint point { get; set; }
    }
    public class SingleUserTrackPoint: TrackPoint { 
        public string userGuid { get; set; } 
        public string userName { get; set; }
    }

    public class GeozoneEdit
    {
        public string guid { get; set; }   
        public GeoPoint? newPosition { get; set; }
        public GeoPoint initialPosition { get; set; }
        public GeoPoint userPosition { get; set; }
        public string commentary { get; set; }

        public string? lot { get; set; }
        public string? subDistrict { get; set; }
        public string? subDistrictZone { get; set; }

        public string? title { get; set; }
        public string? address { get; set; }
        public float area { get; set; }
        public int basement { get; set; }
        public bool fence { get; set; }
        public bool roof { get; set; }
        public bool gate { get; set; }
        public bool? isCustomAddress { get; set; }
        public DetailedAddress? detailAddress { get; set; }
        public GeoContainerEdit[] containersEditActions { get; set; }
        /// <summary>
        /// guid
        /// </summary>
        public string geozoneGroup { get; set; }

        public string GetBasementGuid() => basement switch
        {
            (int)BasementType.scheben => "99900025-C9F6-77AB-CCEF-5900C3854000",
            (int)BasementType.grunt => "99900023-C9F6-77AB-CCEF-5900C3854000",
            (int)BasementType.beton => "99900022-C9F6-77AB-CCEF-5900C3854000",
            (int)BasementType.asfalt => "99900021-C9F6-77AB-CCEF-5900C3854000",
        };

    }
    public interface IPhotoOwner
    {
        public int? photos { get; set; }
    }
    public class IllegalTrashPileMarker: IPhotoOwner
    {
        public string? guid { get; set; }
        public string title { get; set; }
        public float volume { get; set; }
        public GeoPoint position { get; set; }
        public int? photos { get ; set ; }
        public bool isArch { get; set; }

    }
    public class LandFillMarker
    {
        public string guid { get; set; }
        public string title { get; set; }
        public GeoPoint pos { get; set; }
        public int rad { get; set; }
    }

    public class IllegalTrashPileCreate : IllegalTrashPile, IOriginalUserCoordinates
    {
        public GeoPoint userPosition { get; set; }
    }
    public class IllegalTrashPile: IllegalTrashPileMarker
    {
       
        public DateTime regDate { get; set; }
     
        public string description { get; set; }
       
    }
    public class IllegalTrashPileFullData : IllegalTrashPile
    {

        public float realVolume      { get; set; }
        public string kadastr { get; set; }
        public DateTime clearDate    { get; set; }
        public DateTime creationDate { get; set; }

    }

    public class GeozoneMove: IOriginalUserCoordinates
    { 
        public string guid { get; set; }    
        public string commentary { get; set; }
        public GeoPoint newPosition { get; set; }
        public GeoPoint oldPosition { get; set; }
        public GeoPoint userPosition { get ; set; }
    }
    public class GeozoneCreate : Geozone, IOriginalUserCoordinates
    {
        public GeoPoint userPosition { get ; set ; }
        public DetailedAddress? detailAddress { get; set; }
        public BinmanAddresStorage AddressDetails;
    }
    public class OnGeozoneClickData
    {
        public List<GeoObject> objects { get; set; }
        public Geozone geozone { get; set; }
    }
    public class Status
    {
        public string guid;
        public string code;
        public string title;
    }

    public class SQlFileInfo
    {
        public string guid { get; set; }
        public string fileName { get; set; }
        public DateTime creationDate { get; set; }
        public string userName { get; set; }
        public string title { get; set; }
        public bool allowDelete { get; set; }
    }
    public class ExtendedFileInfo: SQlFileInfo
    {
        public enum FileTypeIcon
        {
            deffault,
            picture,
            just_audio,
            video,

        }
        public enum AddInfoType
        {
            none
        }
        public string ext { get; set; }
        public string buityFileName { get; set; }
        public FileTypeIcon icon { get; set; }
        public AddInfoType ai_type { get; set; }
        public object addInfo { get; set; }
    


    }


     public class MigGeozoneVisit
    {
        public string id { get; set; }
        public string car { get; set; }
        public DateTime? lastUpdate { get; set; }
        public DateTime? visitDate{ get; set; }
        public string comment { get; set; }
        public string status {  get; set; }
    }



    public class FeedBackMessage
    {
        public string title { get; set; }
        public string descr { get; set; }
        public string? logs { get; set; }


    }
    public class GeozoneMarker : IPhotoOwner
    {
        public string? guid { get; set; }
        public string? typeGuid { get; set; }
        public string? postfix { get; set; }
        public bool needWatch { get; set; }
        public bool isArch { get; set; }

        public int color { get; set; }
        public int containersCount { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public GeoPoint position { get; set; }
        public int chainObjCount { get; set; }
        public int? photos { get; set; }

    }
    public class Geozone : GeozoneMarker
    {


        public string? name { get; set; }

        public string? address { get; set; }
        public bool? isAddressCustom { get; set; }
        public DateTime? creationDate { get; set; }
        public string? creator { get; set; }
        public string clientName { get; set; }
        public string clientInn { get; set; }
        public string groundType { get; set; }
        public string status { get; set; }
        public string statusCode { get; set; }
        public string binId { get; set; }
        public bool gate { get; set; }
        public bool barrier { get; set; }
        public bool haveregime { get; set; }


        public bool roof { get; set; }
        public bool fence { get; set; }

        public float area { get; set; }
        public string archDescr { get; set; }

        public string commentary { get; set; }
        /// <summary>
        /// guid
        /// </summary>
        public string geozoneGroup { get; set; }
        /// <summary>
        /// guid
        /// </summary>
        public string lot { get; set; }
        /// <summary>
        /// guid
        /// </summary>
        public string subDistr { get; set; }
        /// <summary>
        /// guid
        /// </summary>
        public string subDistrZone { get; set; }
        public DateTime? lastEvent { get; set; }

        public List<UniversalEvent>? events { get; set; }

        public BasementType groundCode { get; set; }
        public List<GeoContainer> containers { get; set; }
        public string? GetContainerTypeGuidFromContainers()
        {
            return GetContainerTypeGuidFromContainers(containers);
        }
        public static string? GetContainerTypeGuidFromContainers(List<GeoContainer> containers)
        {
            if (containers.Count == 0) return string.Empty;
            if (containers.Count == 1) return containers[0].typeGuid;
            var c = containers[0];
            foreach (var v in containers)
            {
                if (v.typeGuid != c.typeGuid) return null;
                c = v;
            }
            return null;
        }
        public string GetBasementGuid() => groundCode switch
        {
           BasementType.scheben => "99900025-C9F6-77AB-CCEF-5900C3854000",
           BasementType.grunt => "99900023-C9F6-77AB-CCEF-5900C3854000",
           BasementType.beton => "99900022-C9F6-77AB-CCEF-5900C3854000",
           BasementType.asfalt => "99900021-C9F6-77AB-CCEF-5900C3854000",
        };
        

    }
    public class ContainerType
    {
        public string guid { get; set; }
        public string name { get; set; }
        public int icon { get; set; }
        public float? area { get; set; }
        public string? shortname { get; set; }
    }
    public class SimpleGeozone
    {
        public string guid { get; set; }
        public string name { get; set; }    
        public string address { get; set; }
        public int color { get; set; }
        public GeoPoint position { get;set; }

    }
    public class SubDistrict
    {
        public string guid { get; set; }
        public string title { get; set; }
        public List<SubDisrictZone> zones { get; set; } = new List<SubDisrictZone>();
    }
    public class SubDisrictZone
    {
        public string guid { get; set; }
        public string title { get; set; }
    }
    public  class Districts
    {
        public string guid { get; set; }
        public string title { get; set; }
        public string analyticGroupId { get; set; }
        public List<SubDistrict> subDistricts { get; set; } = new List<SubDistrict>();
        
    }
    public  class GeozoneType
    {
        public string guid { get; set; }
        public string title { get; set; }
        public long color { get; set; }
        public string descr { get; set; }
    }


    public record struct UserLogin(string login, Role[] roles);
    public static class RoleExt
    {
        public static bool ContainRole(this Role[] roles, string role)
        {
            foreach (var r in roles)
            {
                if (r.name == role) return true;
            }
            return false;
        }
        public static bool ContainAnyRole(this Role[] roles, User_Roles[] role)
        {
            foreach (var r in roles)
            {
                foreach (var v in role)
                {
                    if (r.id == (int)v) return true;
                }
            }
            return false;
        }
        public static bool ContainAnyRole(this Role[] roles, Role[] role)
        {
            foreach (var r in roles)
            {
                foreach (var v in role)
                {
                    if (r.id == v.id) return true;
                }
            }
            return false;
        }
        public static bool ContainAnyRole(this Role[] roles, int[] role)
        {
            foreach (var r in roles)
            {
                foreach (var v in role)
                {
                    if (r.id == v) return true;
                }
            }
            return false;
        }
        public static bool ContainAnyRole(this Role[] roles, string[] role)
        {
            foreach (var r in roles)
            {
                foreach (var v in role)
                {
                    if (r.name == v) return true;
                }
            }
            return false;
        }
        public static bool ContainRole(this Role[] roles, int role_id)
        {
            foreach (var r in roles)
            {
                if (r.id == role_id) return true;
            }
            return false;
        }
        public static bool ContainRole(this Role[] roles, User_Roles role_id)
        {
            foreach (var r in roles)
            {
                if (r.id == (int)role_id) return true;
            }
            return false;
        }
    }


    public interface IHaveId
    {
        public string GetId();
    }
    public static class SQLExtentions
    {
        public static SqlParameter AddWithArrayValue<T>(this SqlParameterCollection command, string name, IEnumerable<T> ids) where T : IHaveId
        {
            var parameter = new SqlParameter();

            parameter.ParameterName = name;
            //parameter.TypeName = typeof(T).Name.ToLowerInvariant() + "_id_list";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Direction = ParameterDirection.Input;

            parameter.Value = ids.ToGuidList();

            command.Add(parameter);
            return parameter;
        }
        public static SqlParameter AddWithArrayValue(this SqlParameterCollection command, string name, IEnumerable<string> ids)
        {
            var parameter = new SqlParameter();

            parameter.ParameterName = name;
            //parameter.TypeName = typeof(T).Name.ToLowerInvariant() + "_id_list";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Direction = ParameterDirection.Input;

            parameter.Value = ids.ToGuidList();

            command.Add(parameter);
            return parameter;
        }

        public static DataTable ToGuidList<T>(this IEnumerable<T> arr) where T : IHaveId 
        {
            var res = new DataTable();
            res.Columns.Add("id",typeof(Guid));
            foreach (var item in arr)
            {

                res.Rows.Add(Guid.Parse(item.GetId()));
            }
            return res;
        }
        public static DataTable ToGuidList(this IEnumerable<string> arr) 
        {
            var res = new DataTable();
            res.Columns.Add("id", typeof(Guid));
            foreach (var item in arr)
            {
                if (string.IsNullOrEmpty(item)) continue;
                //Log.Text($"GuidItem: {item}");
                res.Rows.Add(Guid.Parse(item));
            }
            return res;
        }
    }


  
  
  
  
  
  
  
  

    public static class TimeOnlyExtensions
    {
        public static TimeOnly AddSeconds(this TimeOnly time, int seconds)
        {
            var ticks = (long)(seconds * 10000000 );
            return AddTicks(time, ticks);
        }
        public static TimeOnly RandomRange(TimeOnly from, TimeOnly to)
        {
             Random ran = new Random();
            var diff = Math.Abs( (int)(from.ToTimeSpan() - to.ToTimeSpan()).TotalSeconds);

            return TimeOnly.FromTimeSpan( from.ToTimeSpan() + new TimeSpan(0,0,ran.Next(Math.Min(diff,0),Math.Max(diff,0))));
        }
        public static TimeOnly AddMilliseconds(this TimeOnly time, int milliseconds)
        {
            var ticks = (long)(milliseconds * 10000 + (milliseconds >= 0 ? 0.5 : -0.5));
            return AddTicks(time, ticks);
        }
        public static DateTime FromTimeADate(TimeOnly time, DateOnly date)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }
        public static TimeOnly AddTicks(this TimeOnly time, long ticks)
        {
            return new TimeOnly(time.Ticks + ticks);
        }
    }

    public static class LengExtensions {
        public static string GetSecOrMinEnd(int SecOrMin)
        {
            if (SecOrMin <= 20) return "";
            switch (SecOrMin%10)
            {
                case 1:return "a";
                case int x when x >= 1 && x <= 4:return "ы";
                default:  return "";
            }
        }
        public static string GetHour(int H)
        {
            if (H >= 10 && H<=20) return "в";
            switch (H%10)
            {
                case 1: return "";
                case int x when x >= 1 && x <= 4: return "а";
                default: return "в";
            }
        }
    }
    public static class BoolExtensions
    {
        public static bool SuperParse(string val) => val.ToLower() switch
        {
       
            "1" => true,
            "true" => true,
            "positive" => true,
            "yes" => true,
            "да" => true,
            null => false,
            "no" => false,
            "нет" => false,
            "nope" => false,
            "negative" => false,
            "false" => false,
            "0" => false,
            _ => false,
        };
        
        
      
        
    }
    public static class TcpExt {
        //public static TcpState GetState(this TcpClient tcpClient)
        //{
        //    var foo = IPGlobalProperties.GetIPGlobalProperties()
        //      .GetActiveTcpConnections()
        //      .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
        //    return foo != null ? foo.State : TcpState.Unknown;
        //}
    }
}
