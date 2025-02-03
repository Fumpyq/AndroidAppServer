

using AndroidAppServer.Libs;
using AndroidAppServer.Libs.BinMan;
using AndroidAppServer.Libs.BinMan.PageParsers;
using BinManParser.Api;
using ClosedXML.Excel;
using Dadata.Model;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2019.Word.Cid;
using DocumentFormat.OpenXml.Office2021.PowerPoint.Comment;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HeyRed.Mime;
using MailKit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Mozilla;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using PhotoSauce.MagicScaler;
using Quartz.Impl.Triggers;
using SimpleImpersonation;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcpStructs;
using static AndroidAppServer.Controllers.DadataController;
using static AndroidAppServer.Controllers.GeozonesController;
using static AndroidAppServer.Controllers.ServiceController;
using static AndroidAppServer.Controllers.TasksController;
using static AndroidAppServer.Libs.BinMan.BinManDocuments;
using static AndroidAppServer.Libs.BinMan.BinManObject;
using static AndroidAppServer.Libs.BinMan.BinManObjectData;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManClientParser;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocumentParser;
using static AndroidAppServer.Libs.DadataApi;
using static AndroidAppServer.Libs.Rosreestr;
using static BinManParser.Api.BinManContainers;
using static CHGKManager.Libs.ActiveDirectory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DataTable = System.Data.DataTable;
using Path = System.IO.Path;

namespace ADCHGKUser4.Controllers.Libs
{

    public static class SQL
    {
        //public const string SqlconnectionString = "Data Source=CleanIT02;Initial Catalog=TKO;Integrated Security=True;MultipleActiveResultSets=true;User Id=...;Password=...";
        public static string SqlconnectionString = $"Data Source=192.168.10.104;MultipleActiveResultSets=true;Initial Catalog=TKO;Persist Security Info=True;User ID=...;Password=...";
        public static string TracerSqlconnectionString = $"Data Source=192.168.10.104;MultipleActiveResultSets=true;Initial Catalog=Tracer;Persist Security Info=True;User ID=...;Password=...";

        //private static SqlConnection _con = new SqlConnection(SqlconnectionString);
        private static object conLock = new object();

        private static long conUsers = 0;


        public static bool Execute(SqlCommand cmd)
        {
            return Execute(cmd, out _);
        }

        public static Task ExecuteAsync(SqlCommand cmd)
        {
            Log.sql(cmd);
            using (cmd)
            {
                try {
                    // Log.System(cmd.CommandText);
                    //  return Task.Delay(5);
                    return cmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        public static bool Execute(SqlCommand cmd, out Exception ex)
        {
            Log.sql(cmd);
            using (cmd)
            {
                try { cmd.ExecuteNonQuery(); ex = null; return true; }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    ex = e;
                    return false;
                }
            }

        }







        public static void AttachFileToSomething(string somethingGuid, string fileName, FileType type, byte[] data, string UserGuid,string? descr) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_InsertFile";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_owner", somethingGuid);
                cmd.Parameters.AddWithValue("@title", fileName);
                cmd.Parameters.AddWithValue("@idUser", (string.IsNullOrEmpty(UserGuid) ? DBNull.Value : UserGuid));
                cmd.Parameters.AddWithValue("@FileCode", Enum.GetName<FileType>(type));
                cmd.Parameters.AddWithValue("@descr", string.IsNullOrEmpty(descr) ? DBNull.Value :  descr);
                cmd.Parameters.Add("@data", SqlDbType.VarBinary).Value = data;

                SQL.Execute(cmd);
            }
        }

        public static async void InsertNewGeozoneEventAsync(string? geozoneGuid, string userGuid, UniversalEvent ev, GeoPoint UserPosition, GeoPoint? GeozonePosition, IFormFileCollection files)
        {
            InsertNewGeozoneEvent(geozoneGuid, userGuid, ev, UserPosition, GeozonePosition, files);
        }
        public static string InsertNewGeozoneEvent(string? geozoneGuid, string userGuid, UniversalEvent ev, GeoPoint UserPosition, GeoPoint? GeozonePosition, IFormFileCollection files)
        {
            string resGuid = InsertNewGeozoneEvents(geozoneGuid, userGuid, ev, UserPosition, GeozonePosition);
            string tst = "";
            string evName = "";
            try
            {
                tst = GetGeozoneTitle(geozoneGuid);
                evName = GetEventName(ev.type);
            }
            catch(Exception ex)
            {

            }
            SQL.ParseFilesToSomething(resGuid, files, userGuid, $"{DateTime.Now.ToString("yyyy.MM.dd HH-mm")} Событие геозоны {tst} - {evName}", $"{DateTime.Now.ToString("yyyy.MM.dd HH-mm")} Событие геозоны {tst} - {evName}");
            return resGuid;
        }
        public static string? GetEventName(string eventCode)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "select t.Title FROM Type_event_geo t WHERE t.code = @code";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@code", eventCode);

                using (var r = SQL.StartRead(cmd))
                {
                    if (r.Read())
                    {
                        if (r.IsDBNull(0)) return string.Empty;
                        else return r.GetValue(0).ToString();
                    }
                }
                return string.Empty;
            }
        }
        public static List<GeozoneEventType> GetGeozoneEventTypes()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_GetGeozoneEventTypes";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                

                var res = new List<GeozoneEventType>();

                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        GeozoneEventType cc = new GeozoneEventType();

                        cc.guid = r.GetValue(0).ToString();
                        if (!r.IsDBNull(1)) cc.title        = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2)) cc.photoLvl     = int.Parse(r.GetValue(2).ToString());
                        if (!r.IsDBNull(3)) cc.coordsLvl    = int.Parse(r.GetValue(3).ToString());
                        if (!r.IsDBNull(4)) cc.addAction    = r.GetValue(4).ToString();
                        res.Add(cc);
                    }
                }
                return res;

            }
        }
       
        public static List<CommentaryInfo> GetGeozoneCommentaries(string geozoneGuid)
        {
           
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_GetGeozoneHistory";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_geo", geozoneGuid);
                var res = new List<CommentaryInfo>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        CommentaryInfo cc = new CommentaryInfo();
                        if (!r.IsDBNull(0)) cc.icon     = r.GetValue(0).ToString();
                        else cc.icon                    = "-1";
                        if (!r.IsDBNull(1)) cc.title    = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2)) cc.subTitle = r.GetValue(2).ToString();
                        if (!r.IsDBNull(3)) cc.date     = r.GetDateTime(3);
                        if (!r.IsDBNull(4)) cc.user     = r.GetValue(4).ToString();
                        res.Add(cc);
                    }
                }
                return res;
                
            }
        }
        public static string? GetGeozoneTitle(string geozoneGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "select g.title from geozone g WHERE g.id =@id_geo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id_geo", geozoneGuid);

                using (var r = SQL.StartRead(cmd))
                {
                    if (r.Read())
                    {
                        if (r.IsDBNull(0)) return string.Empty;
                        else return r.GetValue(0).ToString();
                    }
                }
                return string.Empty;
            }
        }
        public static string InsertNewGeozoneEvents(string? geozoneGuid, string userGuid, UniversalEvent ev, GeoPoint UserPosition, GeoPoint? GeozonePosition) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_InsertGeoEvent";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                string resGuid = Guid.NewGuid().ToString();

                cmd.Parameters.AddWithValue("@id_geo", geozoneGuid != null ? geozoneGuid : DBNull.Value);
                cmd.Parameters.AddWithValue("@idUser", userGuid);
                cmd.Parameters.AddWithValue("@ID", resGuid);
                cmd.Parameters.AddWithValue("@TypeEvent", ev.type);
                cmd.Parameters.AddWithValue("@Descr", string.IsNullOrEmpty(ev.description) ? "" : ev.description);
                cmd.Parameters.AddWithValue("@lat", UserPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", UserPosition.mLongitude);
                cmd.Parameters.AddWithValue("@geo_lat", GeozonePosition.HasValue ? GeozonePosition.Value.mLatitude : DBNull.Value);
                cmd.Parameters.AddWithValue("@geo_lon", GeozonePosition.HasValue ? GeozonePosition.Value.mLongitude : DBNull.Value);

                SQL.Execute(cmd);
                return resGuid;
            }
        }

        public static async void InsertNewObjectEventAsync(string? geozoneGuid, string userGuid, UniversalEvent ev, GeoPoint UserPosition, GeoPoint? GeozonePosition, IFormFileCollection files)
        {
            InsertNewGeozoneEvent(geozoneGuid, userGuid, ev, UserPosition, GeozonePosition, files);
        }
        public static string InsertNewObjectEvent(string? geozoneGuid, string userGuid, UniversalEvent ev, GeoPoint UserPosition, GeoPoint? GeozonePosition, IFormFileCollection files)
        {
            string resGuid = InsertNewObjectEvents(geozoneGuid, userGuid, ev, UserPosition, GeozonePosition);
            SQL.ParseFilesToSomething(resGuid, files, userGuid,null);
            return resGuid;
        }
        public static string InsertNewObjectEvents(string? geozoneGuid, string userGuid, UniversalEvent ev, GeoPoint UserPosition, GeoPoint? GeozonePosition)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_InsertObjEvent";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                string resGuid = Guid.NewGuid().ToString();

                cmd.Parameters.AddWithValue("@id", resGuid);
                cmd.Parameters.AddWithValue("@id_obj", geozoneGuid != null ? geozoneGuid : DBNull.Value);
                cmd.Parameters.AddWithValue("@idUser", userGuid);

                cmd.Parameters.AddWithValue("@TypeEvent", ev.type);
                cmd.Parameters.AddWithValue("@Descr", string.IsNullOrEmpty(ev.description) ? "" : ev.description);
                cmd.Parameters.AddWithValue("@lat", UserPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", UserPosition.mLongitude);
                cmd.Parameters.AddWithValue("@obj_lat", GeozonePosition.HasValue ? GeozonePosition.Value.mLatitude : DBNull.Value);
                cmd.Parameters.AddWithValue("@obj_lon", GeozonePosition.HasValue ? GeozonePosition.Value.mLongitude : DBNull.Value);

                SQL.Execute(cmd);
                return resGuid;
            }
        }
 public static void InsertTableByProcedure(string procedureName, System.Data.DataTable dt)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = procedureName;
                Log.Text($"SQl {Query}...");
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.CommandTimeout = int.MaxValue;
                SqlParameter tvparam = cmd.Parameters.AddWithValue("@dt", dt);

                tvparam.SqlDbType = SqlDbType.Structured;

                SQL.Execute(cmd);
                Log.Text($"SQl {Query} Execution  Done");
                dt.Clear();
                dt.Dispose();

                //Log.Text($"Gc allocation: {GC.GetTotalAllocatedBytes()}");
                //GC.Collect();
                //Log.Text($"Clear (GC.Collect()), Gc allocation: {GC.GetTotalAllocatedBytes()}");
            }
        }
        public static bool CheckGeoExists(string binId)
        {
         
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();


              

                var Query = "select g.id FROM geozone g WHERE g.ID_binman = @binId";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@binId", binId);

                using (var r = SQL.StartRead(cmd))
                {
                    return r.Read();
                    
                };
            }
            return false;
        }
        public static bool TryGetTarifCodeByName(string txt,out string binCode)
        {
            binCode = string.Empty;
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();


                Dictionary<float, string> VolMap = new Dictionary<float, string>(10);

                var Query = "BinMan_GetTarifCodeByName";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@name", txt);

                using (var r = SQL.StartRead(cmd))
                {
                    if (r.Read())
                    {
                        if (r.IsDBNull(0)) return false; 
                        binCode =r.GetValue(0).ToString();
                        return true;
                    }
                };
            }
            return false;
        }

        public static async Task CreateGeozoneContainers(GeozoneCreate zone, string UserGuid)
        {
            List<Task> tasks = new List<Task>();
            foreach (var v in zone.containers)
            {
                tasks.Add(
                    CreateGeozoneContainer_Async(zone.guid, v, UserGuid)
                    );
            }
            await Task.WhenAll(tasks);
        }


        //public static void SimpleExcelCheck(List<int> ids)
        //{

        //    using var book = new XLWorkbook();
        //    var ws = book.Worksheets.First();

        //    using (SqlConnection _con = new SqlConnection(SqlconnectionString))
        //    {
        //        _con.Open();
        //        int i = 1;
        //        foreach (var id in ids)
        //        {

        //            ws.Cell(i, 1).Value = id.ToString();


        //            var Query = "SELECT g.id FROM geozone g WHERE g.ID_binman =@id";
        //            var cmd = new SqlCommand(Query, _con);
        //            cmd.CommandType = CommandType.Text;

        //            cmd.Parameters.AddWithValue("@id", id);

        //            bool cont = true;

        //            using (var r = SQL.StartRead(cmd))
        //            {
        //                if (!r.HasRows) { ws.Cell(i, 2).Value = "Нету геозоны в бд"; cont = false; }
        //            }

        //            if (!cont) continue;

        //            Query = "SELECT * FROM geo_obj gg WHERE gg.id = @id";
        //            cmd = new SqlCommand(Query, _con);
        //            cmd.CommandType = CommandType.Text;

        //            cmd.Parameters.AddWithValue("@id", id);

        //            bool cont = true;

        //            using (var r = SQL.StartRead(cmd))
        //            {
        //                if (!r.HasRows) { ws.Cell(i, 2).Value = "Нету геозоны в бд"; cont = false; }
        //            }






        //        }


        //    }
        //}

        public static void LoadGeozoneOwnersFromExcel(string FilePath)
        {
            XLWorkbook Excel = XLWorkbook.OpenFromTemplate(FilePath);
            var ws = Excel.Worksheets.First();

            int Count = ws.RowsUsed().Count();




            var ids = Enumerable.Range(6, Count).Chunk(Count / 20 + 1);

            long proceded = 0;
            List<Task> Tasks = new List<Task>();
            foreach (var v in ids)
            {
                var t = Task.Run(() =>
                 {
                     Dictionary<string, string> Kasids = new Dictionary<string, string>();
                     using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                     {
                         _con.Open();
                         foreach (var id in v)
                         {
                             try
                             {
                                 int i = id;
                                 var cur = Interlocked.Increment(ref proceded);
                                 Console.Title = $"Прогресс загрузки из Excel: {cur}/{Count - 5}";

                                 var AddressPart1 = ws.Cell(i, 3).Value.ToString();// Населенный пункт ~ г. Полысаево
                                 var AddressPart2 = ws.Cell(i, 4).Value.ToString();// Улица ~ ул. Бажова
                                 var AddressPart3 = ws.Cell(i, 5).Value.ToString();// Номер дома ~ 5
                                 var Inn = ws.Cell(i, 1).Value.ToString();

                                 var s1 = AddressPart1.Trim().Split('.');
                                 s1[0] = s1[0].Replace(".", "");

                                 AddressPart1 = $"%{string.Join("", s1)}%";

                                 s1 = AddressPart2.Trim().Split('.');
                                 s1[0] = s1[0].Replace(".", "");

                                 AddressPart2 = $"%{string.Join("", s1)}%";

                                 s1 = AddressPart3
                                 //.Replace(" ","")
                                 .Trim().Split('.');
                                 s1[0] = s1[0].Replace(".", "");

                                 AddressPart3 = $"% {string.Join("", s1)},%";

                                 var AddressPart3v2 = $"% {string.Join("", s1)}";

                                 string ClientId = null;

                                 var Query = "SELECT KA.id FROM KA WHERE KA.INN = @inn";
                                 var cmd = new SqlCommand(Query, _con);

                                 if (!Kasids.TryGetValue(Inn, out ClientId))
                                 {
                                     Query = "SELECT KA.id FROM KA WHERE KA.INN = @inn";
                                     cmd = new SqlCommand(Query, _con);
                                     cmd.CommandType = CommandType.Text;

                                     cmd.Parameters.AddWithValue("@inn", Inn);



                                     using (var r = SQL.StartRead(cmd))
                                     {
                                         if (r.Read())
                                         {
                                             ClientId = r.GetValue(0).ToString();
                                             Kasids.Add(Inn, ClientId);
                                         }
                                         else
                                         {
                                             Kasids.Add(Inn, string.Empty);
                                         }
                                     }
                                 }

                                 if (string.IsNullOrEmpty(ClientId))
                                 {
                                     ws.Cell(i, 10).Value = "KA не найден";
                                     continue;
                                 }

                                 Query = @"
SELECT g.ID FROM [Objects] o
            join geo_obj gg on o.Id = gg.id_object
		    join geozone g on g.id = gg.Id_geozone
		    WHERE 1=1
			and  o.adress like @P1 and o.adress like @P2 and (o.adress like @P3 or o.adress like @P4)
			and g.id_owner is null 
            and o.Id_TypeObjects ='313D3353-33F5-49B5-A4A8-0136391F5A2E'
			--and g.Title  ='1287622 УК ПБЗ'
					    group by g.ID";
                                 cmd = new SqlCommand(Query, _con);
                                 cmd.CommandType = CommandType.Text;

                                 //if (!AddressPart2.Contains("Патриотов"))
                                 //{
                                 //    continue;
                                 //}


                                 cmd.Parameters.AddWithValue("@P1", AddressPart1);
                                 cmd.Parameters.AddWithValue("@P2", AddressPart2);
                                 cmd.Parameters.AddWithValue("@P3", AddressPart3);
                                 cmd.Parameters.AddWithValue("@P4", AddressPart3v2);
                                 cmd.CommandTimeout = 234234452;
                                 using (var r = SQL.StartRead(cmd))
                                 {

                                     if (!r.HasRows)
                                     {

                                         ws.Cell(i, 10).Value = "Нету геозон объектов по данному адресу или у них уже указан id_owner";
                                         continue;

                                     }

                                     while (r.Read())
                                     {

                                         var GeoGuid = r.GetValue(0).ToString();

                                         Query = @"UPDATE geozone set Id_owner = @id_owner WHERE geozone.ID = @id_geo";
                                         cmd = new SqlCommand(Query, _con);
                                         cmd.CommandType = CommandType.Text;

                                         cmd.Parameters.AddWithValue("@id_owner", ClientId);
                                         cmd.Parameters.AddWithValue("@id_geo", GeoGuid);
                                         //Log.sql(cmd);
                                         ws.Cell(i, 10).Value = "Успешно";
                                         SQL.Execute(cmd);

                                     }
                                 };

                             }
                             catch (Exception ex) { Log.Error(ex); }
                         }
                     }
                 });
                Tasks.Add(t);
            }
            Task.WaitAll(Tasks.ToArray());


            // "'203276 УК ЕК','1664658 УК ЕК','205080 УК ЕК','3307141 УК ЕК ','3292517 УК ЕК ','232880 УК ПБЗ','210607 УК ЕК','205110 УК ЕК','225239 УК ЕК','204888 УК ЕК','214805 УК ЕК','220978 УК ЕК','204540 УК ЕК','218485 УК ЕК','223405 УК ЕК','223838 УК ЕК','210801 УК ЕК','112145 УК ЕК','216036 УК ЕК','4395324 УК ЕК','4403188 УК ЕК','3308079 УК ЕК ','2033129 УК ПБЗ','222294 УК ЕК','4218783 УК ЕК','112028 УК ЕК','214489 УК ЕК','111318 УК ПБЗ','4387606 УК ПБЗ','112741 УК ЕК','213023 УК ЕК','211081 УК ЕК','204488 УК ЕК','112163 УК ЛПП','112760 УК ЕК','4342189 УК ПБЗ','223109 УК ЕК','213649 УК ЕК','222680 УК ЕК','3398381 УК ЛПП','211411 УК ЕК','210816 УК ЕК','5301152 УК ЕК','112329 УК ЕК','207709 УК ПБЗ','5273284 УК ЕК','225066 УК ЕК','112374 УК ЕК','213045 УК ЕК','2016176 УК ЕК','2895936 УК ЕК','5287356 УК ПБЗ','3286872 УК ЕК ','3043549 УК ПБЗ','212387 УК ЕК','211367 УК ПБЗ\u001aМК ЛПП 20.06.2022','204782 УК ЕК','112335 УК ЕК','211413 УК ПБЗ','215650 УК ЕК','111940 УК ЕК','211914 УК ЕК','210709 УК ЕК','210642 УК ЕК','204472 УК ЕК','112276 УК ЕК','210089 УК ЕК','203027 УК ЕК','1387156 УК ЕК','225326 УК ЕК','204961 УК ПБЗ','224303 УК ЕК','224449 УК ЕК','2704002 УК ЕК','203091 УК ЕК','214405 УК ЕК','204755 УК ЕК','204966 УК ПБЗ','220606 УК ЕК','111577 УК ПБЗ','4217837 УК ЕК','2722117 УК ЕК','2577006 УК ЕК','210619 УК ЕК','3031940 УК ЛПП','214974 УК ЕК','220608 УК ЕК','213138 УК ЕК','218061 УК ЕК','1671162 УК ЕК','111531 УК ЕК','5071785 УК ЕК','2894392 УК ЕК','216159 УК ЕК','204972 УК ЛПП','204816 УК ЕК','111738 УК ЕК','228510 УК ЕК','1669820 УК ЕК','112420 УК ЛПП','211818 УК ПБЗ','111685 УК ЕК','3287391 УК ЕК','214950 УК ЕК','213037 УК ЕК','223389 УК ЕК','213109 УК ЕК','2895424 УК ЕК','210829 УК ЕК','111452 УК ЕК','204725 УК ЕК','204837 УК ЕК','2894806 УК ЕК','203332 УК ПБЗ','112030 УК ЕК','210568 УК ЕК','221975 УК ЕК','204798 УК ЕК','214846 УК ЕК','218349 УК ЕК','4115651 УК ЕК','228898 УК ПБЗ','204850 УК ЕК','213559 УК ЕК','210587 УК ЕК','5084531 УК ЕК','216189 УК ЕК','212801 УК ЕК','3365576 УК ЕК','214597 УК ЕК','204773 УК ЕК','3707972 УК ЕК','211942 УК ЕК','225063 УК ЕК','2477497 УК ЕК','3908445 УК ПБЗ','3452330 УК ЕК','5221308 УК ЕК','3308182 УК ЕК ','1387004 УК ПБЗ','225598 УК ЕК','210592 УК ПБЗ','210697 УК ЕК','3295257 УК ЕК ','223844 УК ЕК','3360833 УК ЕК','203596 УК ПБЗ','204975 УК ПБЗ','214613 УК ЕК','212466 УК ЕК','228632 УК ЕК','213155 УК ЕК','221009 УК ЕК','229505 УК ПБЗ','216518 УК ЕК','213063 УК ЕК','213161 УК ЕК','204855 УК ЕК','215193 УК ЕК','213603 УК ЕК','2122666 УК ЕК','211422 УК ЕК','229056 УК ПБЗ','3080107 УК ЛПП','204928 УК ЕК','4222060 УК ПБЗ','3298451 УК ЕК','230326 УК ЕК','203258 УК ЕК','3307354 УК ЕК ','221970 УК ЕК','1379515 УК ЕК','213370 УК ЕК','3309074 УК ЕК','204849 УК ЕК','214690 УК ЕК'"



            Excel.SaveAs(Path.GetDirectoryName(FilePath) + "//ExpResult.xlsx");
        }


   

        public static void LoadGeozoneOwnersFromExcelDirectDogs(string FilePath, int idCol = 1, int innCol = 12)
        {
            XLWorkbook Excel = XLWorkbook.OpenFromTemplate(FilePath);
            var ws = Excel.Worksheets.First();

            int Count = ws.RowsUsed().Count();


            // var v = Enumerable.Range(2, Count);

            var ids = Enumerable.Range(2, Count).Chunk(Count / 7 + 1);

            long proceded = 0;

            //using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            //{
            //    _con.Open();
            //    foreach (var id in v)
            //    {
            //        try
            //        {
            //            int i = id;
            //            var cur = Interlocked.Increment(ref proceded);

            //            var bin_id = ws.Cell(id, 1).Value.ToString();
            //            var inn = ws.Cell(id, 12).Value.ToString();

            //            Console.Title = $"Прогресс загрузки из Excel: {cur}/{Count - 5}";






            //            var Query = @"UPDATE geozone set Id_owner =(SELECT KA.id FROM KA WHERE ka.INN=@ka_inn) WHERE ID_binman = @bin_id";

            //            var cmd = new SqlCommand(Query, _con);
            //            cmd.CommandType = CommandType.Text;

            //            cmd.Parameters.AddWithValue("@ka_inn", bin_id);
            //            cmd.Parameters.AddWithValue("@bin_id", inn);

            //           if(! SQL.Execute(cmd,out Exception ex)) { Log.Error(ex); }

            //        }
            //        catch (Exception ex) { Log.Error(ex); }
            //    }
            //}

            List<Task> Tasks = new List<Task>();
            foreach (var v in ids)
            {
                var t = Task.Run(() =>
                {
                    //Dictionary<string, string> Kasids = new Dictionary<string, string>();
                    using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                    {
                        _con.Open();
                        foreach (var id in v)
                        {
                            try
                            {
                                int i = id;
                                var cur = Interlocked.Increment(ref proceded);

                                var bin_id = ws.Cell(id, idCol).Value.ToString().Trim().Replace(" ", "");
                                var inn = ws.Cell(id, innCol).Value.ToString().Trim().Replace(" ", "");

                                Console.Title = $"Прогресс загрузки из Excel: {cur}/{Count - 5}";






                                var Query = @"UPDATE geozone set Id_owner =(SELECT top 1 KA.id FROM KA WHERE ka.INN=@ka_inn) WHERE ID_binman = @bin_id";

                                var cmd = new SqlCommand(Query, _con);
                                cmd.CommandType = CommandType.Text;

                                cmd.Parameters.AddWithValue("@ka_inn", inn);
                                cmd.Parameters.AddWithValue("@bin_id", bin_id);

                                if (!SQL.Execute(cmd, out Exception ex)) { Log.Error(ex); }

                            }
                            catch (Exception ex) { Log.Error(ex); }
                        }
                    }
                });
                Tasks.Add(t);
            }
            Task.WaitAll(Tasks.ToArray());


            // "'203276 УК ЕК','1664658 УК ЕК','205080 УК ЕК','3307141 УК ЕК ','3292517 УК ЕК ','232880 УК ПБЗ','210607 УК ЕК','205110 УК ЕК','225239 УК ЕК','204888 УК ЕК','214805 УК ЕК','220978 УК ЕК','204540 УК ЕК','218485 УК ЕК','223405 УК ЕК','223838 УК ЕК','210801 УК ЕК','112145 УК ЕК','216036 УК ЕК','4395324 УК ЕК','4403188 УК ЕК','3308079 УК ЕК ','2033129 УК ПБЗ','222294 УК ЕК','4218783 УК ЕК','112028 УК ЕК','214489 УК ЕК','111318 УК ПБЗ','4387606 УК ПБЗ','112741 УК ЕК','213023 УК ЕК','211081 УК ЕК','204488 УК ЕК','112163 УК ЛПП','112760 УК ЕК','4342189 УК ПБЗ','223109 УК ЕК','213649 УК ЕК','222680 УК ЕК','3398381 УК ЛПП','211411 УК ЕК','210816 УК ЕК','5301152 УК ЕК','112329 УК ЕК','207709 УК ПБЗ','5273284 УК ЕК','225066 УК ЕК','112374 УК ЕК','213045 УК ЕК','2016176 УК ЕК','2895936 УК ЕК','5287356 УК ПБЗ','3286872 УК ЕК ','3043549 УК ПБЗ','212387 УК ЕК','211367 УК ПБЗ\u001aМК ЛПП 20.06.2022','204782 УК ЕК','112335 УК ЕК','211413 УК ПБЗ','215650 УК ЕК','111940 УК ЕК','211914 УК ЕК','210709 УК ЕК','210642 УК ЕК','204472 УК ЕК','112276 УК ЕК','210089 УК ЕК','203027 УК ЕК','1387156 УК ЕК','225326 УК ЕК','204961 УК ПБЗ','224303 УК ЕК','224449 УК ЕК','2704002 УК ЕК','203091 УК ЕК','214405 УК ЕК','204755 УК ЕК','204966 УК ПБЗ','220606 УК ЕК','111577 УК ПБЗ','4217837 УК ЕК','2722117 УК ЕК','2577006 УК ЕК','210619 УК ЕК','3031940 УК ЛПП','214974 УК ЕК','220608 УК ЕК','213138 УК ЕК','218061 УК ЕК','1671162 УК ЕК','111531 УК ЕК','5071785 УК ЕК','2894392 УК ЕК','216159 УК ЕК','204972 УК ЛПП','204816 УК ЕК','111738 УК ЕК','228510 УК ЕК','1669820 УК ЕК','112420 УК ЛПП','211818 УК ПБЗ','111685 УК ЕК','3287391 УК ЕК','214950 УК ЕК','213037 УК ЕК','223389 УК ЕК','213109 УК ЕК','2895424 УК ЕК','210829 УК ЕК','111452 УК ЕК','204725 УК ЕК','204837 УК ЕК','2894806 УК ЕК','203332 УК ПБЗ','112030 УК ЕК','210568 УК ЕК','221975 УК ЕК','204798 УК ЕК','214846 УК ЕК','218349 УК ЕК','4115651 УК ЕК','228898 УК ПБЗ','204850 УК ЕК','213559 УК ЕК','210587 УК ЕК','5084531 УК ЕК','216189 УК ЕК','212801 УК ЕК','3365576 УК ЕК','214597 УК ЕК','204773 УК ЕК','3707972 УК ЕК','211942 УК ЕК','225063 УК ЕК','2477497 УК ЕК','3908445 УК ПБЗ','3452330 УК ЕК','5221308 УК ЕК','3308182 УК ЕК ','1387004 УК ПБЗ','225598 УК ЕК','210592 УК ПБЗ','210697 УК ЕК','3295257 УК ЕК ','223844 УК ЕК','3360833 УК ЕК','203596 УК ПБЗ','204975 УК ПБЗ','214613 УК ЕК','212466 УК ЕК','228632 УК ЕК','213155 УК ЕК','221009 УК ЕК','229505 УК ПБЗ','216518 УК ЕК','213063 УК ЕК','213161 УК ЕК','204855 УК ЕК','215193 УК ЕК','213603 УК ЕК','2122666 УК ЕК','211422 УК ЕК','229056 УК ПБЗ','3080107 УК ЛПП','204928 УК ЕК','4222060 УК ПБЗ','3298451 УК ЕК','230326 УК ЕК','203258 УК ЕК','3307354 УК ЕК ','221970 УК ЕК','1379515 УК ЕК','213370 УК ЕК','3309074 УК ЕК','204849 УК ЕК','214690 УК ЕК'"



            //  Excel.SaveAs(Path.GetDirectoryName(FilePath) + "//ExpResult.xlsx");
        }
        //public static void LoadGeozonePartAddresses()
        //{
        //    using (SqlConnection _con = new SqlConnection(SqlconnectionString))
        //    {
        //        _con.Open();

        //        var Query = @"select";
        //        var cmd = new SqlCommand(Query, _con);

        //        cmd.CommandType = CommandType.StoredProcedure;
        //        var res = new List<BinManAccrual>();
        //        var sw = Stopwatch.StartNew();
        //        using (var r = SQL.StartRead(cmd))
        //        {
        //            while (r.Read())
        //            {
        //                BinManAccrual a = new BinManAccrual();
        //                a.doc_BinId = r.GetValue(0).ToString();
        //                if (!r.IsDBNull(1))
        //                    a.dateFrom = r.GetDateTime(1);
        //                if (!r.IsDBNull(2))
        //                    a.dateTo = r.GetDateTime(2);
        //                if (!r.IsDBNull(2))
        //                    a.date = r.GetDateTime(3);
        //                if (!r.IsDBNull(4))
        //                    a.summ = r.GetValue(4).ToString();
        //                a.db_guid = r.GetValue(5).ToString();
        //                if (!r.IsDBNull(6))
        //                    a.parentBinId = r.GetValue(6).ToString();

        //                res.Add(a);


        //            }

        //        }
        //        return res;
        //    }
        //}

        public static List<BinManAccrual> BinMan_GetAccruals2Create()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = @"BinMan_GetAccruals2Update";
                var cmd = new SqlCommand(Query, _con);

                cmd.CommandType = CommandType.StoredProcedure;
                var res = new List<BinManAccrual>();
                var sw = Stopwatch.StartNew();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        BinManAccrual a = new BinManAccrual();
                        a.doc_BinId = r.GetValue(0).ToString();
                        if(!r.IsDBNull(1))
                        a.dateFrom = r.GetDateTime(1);
                        if (!r.IsDBNull(2))
                            a.dateTo = r.GetDateTime(2);
                       // if (!r.IsDBNull(2))
                            a.date = r.GetDateTime(3);
                      //  if (!r.IsDBNull(4))
                            a.summ = r.GetValue(4).ToString();
                        a.db_guid = r.GetValue(5).ToString();
                        if (!r.IsDBNull(6))
                            a.comment = r.GetValue(6).ToString();
                        if (!r.IsDBNull(7))
                        a.parentBinId = r.GetValue(7).ToString();
                        a.typeRaw = r.GetValue(8).ToString();
                        a.volume = "0";
                        

                        res.Add(a);


                    }

                }
                return res;
            }
        }
        public static void UpdateAccruaBinManStatus(AccrualsCreationResult res, string AccrGUID,string Accr_bin_id)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = @"BinMan_UpdateAccrualsStatus";
                var cmd = new SqlCommand(Query, _con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_accr",AccrGUID);
                cmd.Parameters.AddWithValue("@status", Enum.GetName(res));
                cmd.Parameters.AddWithValue("@binid", string.IsNullOrEmpty(Accr_bin_id)?DBNull.Value: Accr_bin_id);

                SQL.Execute(cmd);
            }
        }


        public record struct ObjParsePair(string binid, string docGuid);
        private static HashSet<string> DogBanList = new HashSet<string>();
        public static void BinManLoad_LoadObjectFromBinManByCustomSql()
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = @"1";
                var cmd = new SqlCommand(Query, _con);

                //                Query = @"SELECT d.id_Dogovor,d.id FROM [Objects] o 
                //join Dog_obj do on do.id_object = o .Id
                //join Dog d on d.id = do.id_dogovor
                //WHERE not EXISTS(SELECT * FROM Dog_tariff dt WHERE dt.id_object = o.Id)
                //and not EXISTS (SELECT * FROM temp_BinMan_ObjectParse tmo WHERE tmo.binid =o.ID_Binman and tmo.people = '0')
                //and o .active <>0
                //GROUP BY d.id_Dogovor,d.id";//old

//                Query = @"  SELECT d.id_Dogovor,d.id FROM [Objects] o 
//join Dog_obj do on do.id_object = o .Id
//join Dog d on d.id = do.id_dogovor
//WHERE not EXISTS (SELECT * FROM temp_BinMan_ObjectParse tmo WHERE tmo.binid =o.ID_Binman and tmo.id_dog =d.id)
//and ISNULL(o.active,1) <>0
//GROUP BY d.id_Dogovor,d.id";
                Query = @"select d.id_Dogovor,d.id FROM Dog d WHERE d.WaitToLoadObjFromBinman =1 and
 d.id_dogovor is not null";
                cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                var res = new List<ObjParsePair>();
                var sw = Stopwatch.StartNew();
                using (var r = SQL.StartRead(cmd))
                {


                    while (r.Read())
                    {
                        
                        var id = r.GetValue(0).ToString();
                        if (DogBanList.Contains(id)) continue;
                           var guid = r.GetValue(1).ToString();


                        res.Add(new ObjParsePair(id, guid));


                    }

                }
                long Proceded = 1;
                var Splt = res.Chunk(res.Count / 8 + 1);
                var tasks = new List<Task>();
                var Count = res.Count;
                foreach (var v in Splt)
                {
                    Task t = Task.Run(() =>
                    {
                        foreach (var rec in v)
                        {
                            try
                            {
                                long proc = Interlocked.Increment(ref Proceded);
                                try
                                {
#if DEBUG
                                    Console.Title = $"{Proceded}/{Count} ETA: {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds * ((float)Count/(float)Proceded)).ToReadableString()} , Elapsed: {sw.Elapsed.ToString()}";
#endif
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("Timer Error!");
                                    Log.Error(ex);
                                }


                                if (!BinManHelper.ParseToDbDocumentObjects(rec.binid, rec.docGuid))
                                {
                                    DogBanList.Add(rec.binid);
                                    Log.Error($"WAA! ошибки на договоре `{rec.binid}`");
                                }
                                else
                                {

                                }

                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                            }
                        }
                    });
                    tasks.Add(t);
                }
                Task.WaitAll(tasks.ToArray());
#if DEBUG
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
                Log.Text("ГОТОВО!");
#endif
            }

        }

        public static void SimpleContainersCountFix()
        {
            using var book = new XLWorkbook();
            var ws = book.Worksheets.Add();

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();


                Dictionary<float, string> VolMap = new Dictionary<float, string>(10);

                var Query = "SELECT tc.id_type_cont,volume FROM Type_cont_volume tc";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var type = r.GetValue(0).ToString();
                        var vol = MathF.Round(float.Parse(r.GetValue(1).ToString()), 2);

                        VolMap.Add(vol, type);
                    }
                };






                Query = "SELECT g.ID,count(c.volume) as cont_count,tg.[Кол-во контейнеров,шт],round(sum (c.volume),2)as cont_volume,tg.[Общий объём контейнеров, м3] FROM temp_geo tg\r\njoin geozone g on g.ID_binman = tg.Ид\r\nleft join Containers c on c.id_geo = g.ID\r\n--WHERE sum (c.volume) <> tg.[Общий объём контейнеров, м3]\r\nGROUP BY c.id_geo,tg.Ид,g.ID,tg.[Общий объём контейнеров, м3],[Кол-во контейнеров,шт]\r\nHAVING (round(sum (c.volume),2) <> TRY_CONVERT (float, tg.[Общий объём контейнеров, м3])\r\nor count(c.volume)<>TRY_CONVERT (int,tg.[Кол-во контейнеров,шт]))\r\n";
                cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                int i = 0;
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var id = r.GetValue(0).ToString();
                        var Count = float.Parse(r.GetValue(2).ToString());
                        var DbCount = float.Parse(r.GetValue(1).ToString());
                        var Volume = float.Parse(r.GetValue(4).ToString());



                        //  float? Db_Count = null;
                        //     float? db_Volume = ;

                        float coof = Volume / Count;
                        coof = MathF.Round(coof, 2);
                        //"B28E1AD5-570D-4271-897F-00005C224FE9" // Евро
                        //"3527C2A0-58D6-44EB-B222-00010160C467" // Лодочка под портал
                        //"BD423E8B-2AC2-4C16-AFD3-20025D4F5B82" // Под боковую загрузку
                        if (VolMap.TryGetValue(coof, out var ContTypeGuid) && Count != 0)
                        {
                            delete();
                            for (int j = 0; j < Count; j++) Insert(coof, ContTypeGuid);
                        }
                        else
                        {
                            i++;
                            ws.Cell(i, 1).Value = $"{id}";
                            ws.Cell(i, 2).Value = $"Не известный результат {Volume}/{Count}={coof}";
                        }

                        //switch (coof)
                        //{
                        //    case 1.1f:
                        //        {
                        //            delete();
                        //            Insert(1.1f, "B28E1AD5-570D-4271-897F-00005C224FE9");
                        //            break;
                        //        }
                        //    case 0.75f:
                        //        {
                        //            delete();
                        //            Insert(0.75f, "BD423E8B-2AC2-4C16-AFD3-20025D4F5B82");
                        //            break;
                        //        }
                        //    case 7.6f:
                        //        {
                        //            delete();
                        //            Insert(7.6f, "3527C2A0-58D6-44EB-B222-00010160C467");
                        //            break;
                        //        }
                        //        default:
                        //        {
                        //            i++;
                        //            ws.Cell(i, 1).Value = $"{id}";
                        //            ws.Cell(i, 2).Value = $"Не известный результат {Volume}/{Count}={coof}";
                        //            break;
                        //        }
                        //}

                        void Insert(float Volume, string Type)
                        {
                            Query = "INSERT Containers(ID,volume,id_typeContainer,id_type_ContainerStatus,id_geo,DateCreate) VALUES(newid(),@vol,@idType,10,@id_geo,GETDATE())";
                            cmd = new SqlCommand(Query, _con);
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.AddWithValue("@vol", Volume);
                            cmd.Parameters.AddWithValue("@idType", Type);
                            cmd.Parameters.AddWithValue("@id_geo", id);

                            SQL.Execute(cmd);
                        }
                        void delete()
                        {

                            Query = "DELETE FROM Containers WHERE Containers.id_geo = @id_geo";
                            cmd = new SqlCommand(Query, _con);
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.AddWithValue("@id_geo", id);

                            SQL.Execute(cmd);
                        }




                    }

                    book.SaveAs("C:\\Users\\a.m.maltsev\\source\\repos\\FilePile\\Res5.xlsx");


                }
            }
        }
        public static async void CreateGeozoneContainer(string geozoneId, GeoContainer container, string UserGuid, bool IsEdit = false)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_InsertNewContainers";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_Geozone", geozoneId);
                cmd.Parameters.AddWithValue("@TypeID", container.typeGuid);
                cmd.Parameters.AddWithValue("@binmanID", DBNull.Value);
                cmd.Parameters.AddWithValue("@volume", container.volume);
                cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@id_user", UserGuid);
                cmd.Parameters.AddWithValue("@isEditAction", IsEdit);

                 SQL.Execute(cmd);
            }

        }
        public static async Task CreateGeozoneContainer_Async(string geozoneId, GeoContainer container, string UserGuid, bool IsEdit = false)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_InsertNewContainers";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_Geozone", geozoneId);
                cmd.Parameters.AddWithValue("@TypeID", container.typeGuid);
                cmd.Parameters.AddWithValue("@binmanID", DBNull.Value);
                cmd.Parameters.AddWithValue("@volume", container.volume);
                cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@id_user", UserGuid);
                cmd.Parameters.AddWithValue("@isEditAction", IsEdit);

                await SQL.ExecuteAsync(cmd);
            }

        }
        public static async Task CreateIllegalTrashPile(IllegalTrashPile req, string UserGuid, IFormFileCollection files)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                string guid = Guid.NewGuid().ToString();

                var Query = "CrateMate_InsertTrashHeap";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", guid);
                cmd.Parameters.AddWithValue("@Title", req.title);
                cmd.Parameters.AddWithValue("@Descr", req.description);
                cmd.Parameters.AddWithValue("@Volume_Plan", req.volume);
                cmd.Parameters.AddWithValue("@DateFind", req.regDate);
                cmd.Parameters.AddWithValue("@lat", req.position.mLatitude);
                cmd.Parameters.AddWithValue("@lon", req.position.mLongitude);
                cmd.Parameters.AddWithValue("@id_userCreate", UserGuid);

                Task.Run(() => {
                    if (Rosreestr.tryFindByCoords(req.position, out var res))
                        SQL.UpdateIllegalTrashKadastr(guid, res.id);
                });

                await SQL.ExecuteAsync(cmd);

                SQL.ParseFilesToSomething(guid, files, UserGuid);


            }
        }
        public static bool CloseIllegalTrashPile(CloseTrashPileRequest req, string userGuid, IFormFileCollection files)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_CloseTrashHeap";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                string guid = Guid.NewGuid().ToString();
                cmd.Parameters.AddWithValue("@id", req.guid);
                cmd.Parameters.AddWithValue("@id_event", guid);
                cmd.Parameters.AddWithValue("@descr", req.desc);
                cmd.Parameters.AddWithValue("@dateClean", req.date);
                cmd.Parameters.AddWithValue("@Volume_Real", req.volume);
                cmd.Parameters.AddWithValue("@Id_userClose", userGuid);


                if (!SQL.Execute(cmd)) { return false; }

                SQL.ParseFilesToSomething(guid, files, userGuid);
                return true;

            }
        }
        public static bool Update_TrashHeap(TrashPileUpdateRequest req, string userGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_Update_TrashHeap";
                var cmd = new SqlCommand(Query, _con);

                if (req.newPosition.mLatitude == 0 || req.newPosition.mLongitude == 0) req.newPosition = req.position;

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", req.guid);
                cmd.Parameters.AddWithValue("@Title", req.title);
                cmd.Parameters.AddWithValue("@Descr", req.description);
                cmd.Parameters.AddWithValue("@Volume_Plan", req.volume);
                cmd.Parameters.AddWithValue("@lat", req.newPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", req.newPosition.mLongitude);


                Task.Run(() => {
                    if (Rosreestr.tryFindByCoords(req.newPosition, out var res))
                        SQL.UpdateIllegalTrashKadastr(req.guid, res.id);
                });


                if (!SQL.Execute(cmd)) { return false; }

                return true;

            }
        }

        public static List<IllegalTrashPileFullData> GetAllIllegalTrashPiles()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_ShowAllActiveTrashHeap";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                var res = new List<IllegalTrashPileFullData>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        IllegalTrashPileFullData v = new IllegalTrashPileFullData();

                        v.guid = r.GetValue(0).ToString();
                        v.title = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2)) v.description = r.GetValue(2).ToString();
                        v.volume = float.Parse(r.GetValue(3).ToString());
                        if (!r.IsDBNull(4)) v.realVolume = float.Parse(r.GetValue(4).ToString());
                        if (!r.IsDBNull(5)) v.regDate = r.GetDateTime(5);
                        if (!r.IsDBNull(6)) v.clearDate = r.GetDateTime(6);
                        if (!r.IsDBNull(7)) v.isArch = r.GetBoolean(7);
                        v.position = new GeoPoint(float.Parse(r.GetValue(9).ToString()), float.Parse(r.GetValue(8).ToString()));
                        if (!r.IsDBNull(11)) v.creationDate = r.GetDateTime(11);
                        if (!r.IsDBNull(14)) v.photos = int.Parse(r.GetValue(14).ToString());
                        if (!r.IsDBNull(15)) v.kadastr = r.GetValue(15).ToString();
                        res.Add(v);
                    }
                }
                return res;
            }
        }
        public static List<GeoObjectMarker> GetObjectsFromSearchPrompt(string searchPrompt, GeoPoint UserScreenPosition)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_ObjectsSearch";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@lat", UserScreenPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", UserScreenPosition.mLongitude);
                cmd.Parameters.AddWithValue("@prompt", searchPrompt);

                var res = DefaultObjectsRead(cmd);


                return res;
            }
        }
        public static Dictionary<string, Func<string, SqlCommand, string>> geozoneSearchCommands = new Dictionary<string, Func<string, SqlCommand, string>>() {
            {"ф1", (string line,SqlCommand cmd)=>{return "ddsd";}}
        };
        public static List<GeozoneMarker> GetGeozonesFromSearchPrompt(string searchPrompt, GeoPoint UserScreenPosition)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {

                //try
                //{

                //    if(searchPrompt.Length>1)
                //    if (searchPrompt[0] == '@')
                //        {
                //            var args = searchPrompt.Split(" ");

                //            foreach(var v in args)
                //            {
                //                if(v.Contains)
                //            }
                //        }


                //}catch(Exception ex)
                //{
                //    Log.Error("Search line CMD", ex);
                //}


                _con.Open();

                var Query = "CrateMate_GeozoneSearch";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@lat", UserScreenPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", UserScreenPosition.mLongitude);
                cmd.Parameters.AddWithValue("@prompt", searchPrompt);

                var res = DefaultGeozoneMarkersRead(cmd);


                return res;
            }
        }
        public static string GetDistrictGuidByAddress(string address)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_DetectDistrictByAddress";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;




                cmd.Parameters.AddWithValue("@address", address);



                using (var r = SQL.StartRead(cmd))
                {
                    if (r.Read())
                    {
                        var distr_id = r.GetValue(0).ToString();
                        return distr_id;
                    }
                }

                return string.Empty;
            }
        }
        public static async Task CreateGeozone(GeozoneCreate zone, string UserGuid, IFormFileCollection files)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_InsertGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                string? containerType = zone.GetContainerTypeGuidFromContainers();
                var adrGuid = string.Empty; 
                var addr = string.Empty; 
                if (zone.guid==null) zone.guid = Guid.NewGuid().ToString();
                if (zone.detailAddress != null)
                {
                    adrGuid =  Guid.NewGuid().ToString();
                    addr =  zone.detailAddress.info.value;
                    Log.Warning("Создается геозона без адреса !?",Log.LogLevel.DangerLikeBug);
                    zone.detailAddress.guid = adrGuid;
                    SQL.InsertNewAddress(zone.detailAddress.info, adrGuid, zone.guid);
                    if (string.IsNullOrEmpty(addr)) addr = zone.address;
                }


                cmd.Parameters.AddWithValue("@id", zone.guid);
                cmd.Parameters.AddWithValue("@lat", zone.position.mLatitude);
                cmd.Parameters.AddWithValue("@lon", zone.position.mLongitude);
                cmd.Parameters.AddWithValue("@fence", zone.fence);
                cmd.Parameters.AddWithValue("@roof ", zone.roof);
                cmd.Parameters.AddWithValue("@Descr", zone.commentary);
                cmd.Parameters.AddWithValue("@idGroup", zone.geozoneGroup);
                cmd.Parameters.AddWithValue("@id_District", zone.lot);
                cmd.Parameters.AddWithValue("@id_UserCreate", UserGuid);
                cmd.Parameters.AddWithValue("@area", zone.area);



                cmd.Parameters.AddWithValue("@adress", addr);
                cmd.Parameters.AddWithValue("@id_typeGround", zone.GetBasementGuid());
                cmd.Parameters.AddWithValue("@have_gate", zone.gate);
                cmd.Parameters.AddWithValue("@container_type", string.IsNullOrEmpty(containerType) ? DBNull.Value : containerType);
                cmd.Parameters.AddWithValue("@adress_Detail", string.IsNullOrEmpty(adrGuid) ? DBNull.Value : adrGuid);
                cmd.Parameters.AddWithValue("@id_subDistrict", string.IsNullOrEmpty(zone.subDistr) ? DBNull.Value : zone.subDistr);
                cmd.Parameters.AddWithValue("@id_subDistrictZone", string.IsNullOrEmpty(zone.subDistrZone) ? DBNull.Value : zone.subDistrZone);

                //5866963
                SQL.ParseFilesToSomething(zone.guid,files,UserGuid, $"Создание геозоны {DateTime.Now.ToString("yyyy.MM.dd HH:mm")}", $"Создание геозоны {DateTime.Now.ToString("yyyy.MM.dd HH:mm")}");

             //   Task.УдалитьВсеНе0Начисления(() => { SQL.InsertNewGeozoneEvent(zone.guid, UserGuid, new UniversalEvent() { type = Enum.GetName(EventsList.insert), description = zone.commentary }, zone.userPosition, null, files); });

                await SQL.ExecuteAsync(cmd);
            }
        }
        public static async Task FullGeozoneCreation(GeozoneCreate zone, string UserGuid, IFormFileCollection files, BinManGeozone? binman = null)
        {

            zone.guid = Guid.NewGuid().ToString();

            List<GeoContainer> result = new List<GeoContainer>();
            foreach (var v in zone.containers)
            {
                for (int i = 0; i < v.count; i++) result.Add(new GeoContainer()
                {
                    volume = v.volume,
                    typeGuid = v.typeGuid,
                    guid = Guid.NewGuid().ToString()
                });
            }
            zone.containers = result;
            Task GeoContainers = CreateGeozoneContainers(zone, UserGuid);
            await Task.WhenAll(GeoContainers);
            Task GeoCreate = CreateGeozone(zone, UserGuid, files);
            // GeoCreate.Start();
          
            // GeoContainers.Start();
           // if (BinManApi.IsApiEnabled) Task.УдалитьВсеНе0Начисления(() => { CreateGeozoneInBinManWithContainers(zone, binman, GeoCreate, GeoContainers); });

            await Task.WhenAll(GeoCreate);

        }

        public static async Task CreateGeozoneInBinManWithContainers(GeozoneCreate zone, BinManGeozone binman, Task WaitToInsertGeozone, Task WaitToInsertContainers)
        {
            try
            {

                //asd.bin_ID                     := Sheet.Cells[I + 1, 18].Value;
                //asd.Geozone_Title              := Sheet.Cells[I + 1, 18].Value;
                //asd.DistrictText               := Sheet.Cells[I + 1, 18].Value;
                //asd.Address                    := Sheet.Cells[I + 1, 18].Value;
                //asd.Coords                     := Sheet.Cells[I + 1, 18].Value;
                //asd.Area                       := Sheet.Cells[I + 1, 18].Value;
                //asd.Ground_Text                := Sheet.Cells[I + 1, 18].Value;
                //asd.Fence_Text                 := Sheet.Cells[I + 1, 18].Value;
                //asd.ContainerType_Text         := Sheet.Cells[I + 1, 18].Value;
                //asd.ContainerVolumes_Text      := Sheet.Cells[I + 1, 18].Value;
                //asd.ContainerCount_Text        := Sheet.Cells[I + 1, 18].Value;
                //asd.ContainerVolume_Total_Text := Sheet.Cells[I + 1, 18].Value;
                //asd.ContainerCount_Total_Text  := Sheet.Cells[I + 1, 18].Value;

                //asd.bin_ID                     := str;
                //asd.bin_ID                     := Sheet.Cells[I + 1, 18].Value;
                //asd.Geozone_Title              := Sheet.Cells[I + 1, 19].Value;
                //asd.DistrictText               := Sheet.Cells[I + 1, 1].Value;
                //asd.Address                    := Sheet.Cells[I + 1, 2].Value;
                //asd.Coords                     := Sheet.Cells[I + 1, 3].Value;
                //asd.Area                       := Sheet.Cells[I + 1, 4].Value;
                //asd.Ground_Text                := Sheet.Cells[I + 1, 5].Value;
                //asd.Fence_Text                 := Sheet.Cells[I + 1, 6].Value;
                //asd.ContainerType_Text         := Sheet.Cells[I + 1, 7].Value;
                //asd.ContainerVolumes_Text      := Sheet.Cells[I + 1, 8].Value;
                //asd.ContainerCount_Text        := Sheet.Cells[I + 1, 9].Value;
                //asd.ContainerVolume_Total_Text := Sheet.Cells[I + 1, 12].Value;
                //asd.ContainerCount_Total_Text  := Sheet.Cells[I + 1, 13].Value;
                //asd.Ka_Text                    := Sheet.Cells[I + 1, 14].Value;
                //asd.Objects_Text               := Sheet.Cells[I + 1, 15].Value;
                //asd.Alive_Text                 := Sheet.Cells[I + 1, 16].Value;


                // (Self.bin_ID                     = Other.bin_ID                    )
                //and (Self.bin_ID                     = Other.bin_ID                    )
                //and (Self.Geozone_Title              = Other.Geozone_Title             )
                //and (Self.DistrictText               = Other.DistrictText              )
                //and (Self.Address                    = Other.Address                   )
                //and (Self.Coords                     = Other.Coords                    )
                //and (Self.Area                       = Other.Area                      )
                //and (Self.Ground_Text                = Other.Ground_Text               )
                //and (Self.Fence_Text                 = Other.Fence_Text                )
                //and (Self.ContainerType_Text         = Other.ContainerType_Text        )
                //and (Self.ContainerVolumes_Text      = Other.ContainerVolumes_Text     )
                //and (Self.ContainerCount_Text        = Other.ContainerCount_Text       )
                //and (Self.ContainerVolume_Total_Text = Other.ContainerVolume_Total_Text)
                //and (Self.ContainerCount_Total_Text  = Other.ContainerCount_Total_Text )
                //and (Self.Ka_Text                    = Other.Ka_Text                   )
                //and (Self.Objects_Text               = Other.Objects_Text              )
                //and (Self.Alive_Text                 = Other.Alive_Text)


                //arrData[I, 18] := dsa.bin_ID                    ;
                //arrData[I, 19] := dsa.Geozone_Title             ;
                //arrData[I, 1] := dsa.DistrictText              ;
                //arrData[I, 2] := dsa.Address                   ;
                //arrData[I, 3] := dsa.Coords                    ;
                //arrData[I, 4] := dsa.Area                      ;
                //arrData[I, 5] := dsa.Ground_Text               ;
                //arrData[I, 6] := dsa.Fence_Text                ;
                //arrData[I, 7] := dsa.ContainerType_Text        ;
                //arrData[I, 8] := dsa.ContainerVolumes_Text     ;
                //arrData[I, 9] := dsa.ContainerCount_Text       ;
                //arrData[I, 12] := dsa.ContainerVolume_Total_Text;
                //arrData[I, 13] := dsa.ContainerCount_Total_Text ;
                //arrData[I, 14] := dsa.Ka_Text                   ;
                //arrData[I, 15] := dsa.Objects_Text              ;
                //arrData[I, 16] := dsa.Alive_Text;







                await Task.Delay(1);
                BinManGeozone bg = binman;

                LoginData ld = BinManApi.GetNextAccount();

                bg.AREA_CANOPY = zone.roof;
                bg.AREA_ENCLOSURE = zone.fence;

                bg.NAME = "Геозона...";  //TODO
                bg.AREA_BASIS = (zone.groundCode switch
                {
                    BasementType.asfalt => Geo_area_basis.asfalt,
                    BasementType.scheben => Geo_area_basis.scheben,
                    BasementType.beton => Geo_area_basis.beton,
                    BasementType.grunt => Geo_area_basis.grunt,
                });
                int geozoneBin_Id = 0;
                if (
                    //true 
                    bg.SendCreateRequest(ld, out geozoneBin_Id)
                )
                {
                    foreach (var v in zone.containers)
                    {

                        BinManContainers binContainer = new BinManContainers();



                        binContainer.NAME = "Контейнер геозоны " + geozoneBin_Id;
                        binContainer.VOLUME = v.volume.ToString();
                        binContainer.TYPE = v.GetBinManType();


                        if (binContainer.SendCreateRequest(ld, out var containerBin_Id) == BinManResult.Ok)
                        {
                            //TODO
                            Task.Run(() =>
                            {
                                try
                                {
                                    Task.WaitAll(WaitToInsertContainers);
                                    SQL.ContainerAddBinId(v.guid, containerBin_Id);
                                    Task.WaitAll(WaitToInsertGeozone);
                                    BinManContainers.SendAttachRequest(ld, containerBin_Id, geozoneBin_Id.ToString());
                                }

                                catch (Exception ex) { Log.Error(ex); }
                            });


                        }


                    }
                    //TODO
                    Task.Run(() =>
                    {
                        try
                        {
                            Task.WaitAll(WaitToInsertGeozone);
                            bg.LAST_AREA = geozoneBin_Id;
                            bg.NAME = $"{bg.LAST_AREA} {bg.getGroupNameFromGuid(zone.geozoneGroup)}";
                            bg.SendEditRequest(ld);
                            SQL.GeozoneAddBinId(zone.guid, bg.NAME, geozoneBin_Id);
                        }

                        catch (Exception ex) { Log.Error(ex); }
                    });

                }
            }
            catch (Exception ex) { Log.Error(ex); }
        }
        private static HashSet<GeoPoint> IgnoreList = new HashSet<GeoPoint>();
        public static Dictionary<string,int> GuidIgnoreList = new Dictionary<string, int>();
     

        public static List<BinManGeozoneTask> GetBinmanGeozonesUpdateList()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "Binman_UpdateGEO";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;





                List<BinManGeozoneTask> res = new List<BinManGeozoneTask>();



                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {

                        var gd = new BinManGeozoneTask()
                        {
                            DataBase_Guid = r.GetValue(0).ToString(),
                            LAST_AREA = !r.IsDBNull(4) ? long.Parse(r.GetValue(4).ToString()) : -1,
                            LAT = float.Parse(r.GetValue(1).ToString()),
                            LON = float.Parse(r.GetValue(2).ToString()),
                            NAME = r.GetValue(3).ToString(),
                            AREA_CANOPY = r.GetBoolean(6),
                            AREA_ENCLOSURE = r.GetBoolean(7),
                            AREA_BASIS = (r.IsDBNull(8) ? Geo_area_basis.grunt : BinManGeozone.Guid2Enum(r.GetValue(8).ToString())),
                            TaskType = Enum.Parse<BinManTaskType>(r.GetValue(11).ToString()),
                            ADDRESS = r.GetValue(12).ToString(),
                            NeedToBeArchived = r.GetBoolean(17),
                            IsArchive = r.GetBoolean(16),


                        };
                        if (GuidIgnoreList.TryGetValue(gd.DataBase_Guid, out var FailCount))
                        {
                            if (FailCount>=3)
                            continue;
                        }

                        if (!r.IsDBNull(18)) gd.db_partAdressOwnerGuid = r.GetValue(18).ToString();

                        var gp = new GeoPoint(gd.LAT, gd.LON);
                        //if (IgnoreList.Contains(gp)) continue;

                        if (!r.IsDBNull(15)) gd.isAddressHandmade = r.GetBoolean(15);

                        if (r.IsDBNull(9)) { Log.Error($"Геозона {gd.DataBase_Guid}, {gd.ADDRESS} имеет Id_TypeGeozone = null"); }
                        /*
                            //GetValue(gd.DataBase_Guid,out var Cc);
                            if (GuidIgnoreList.ContainsKey(gd.DataBase_Guid))
                            {
                                GuidIgnoreList[gd.DataBase_Guid] += 1; // Не потоко безопасно !!
                            }
                            else
                                GuidIgnoreList.TryAdd(gd.DataBase_Guid,0);
                            continue;
                        */
                        
                        else { gd.Db_groupGuid = r.GetValue(9).ToString(); }
                        if (!r.IsDBNull(10)) gd.Db_containerGuid = r.GetValue(10).ToString();

                        gd.GROUP = BinManGeozone.Guid2GROUP(gd.Db_groupGuid);

                 

                        //  if (GuidIgnoreList.Contains(gd.DataBase_Guid)) { }
                        //   else
                        if (!IgnoreList.Contains(gp))
                        {
                            // if (!gd.isAddressHandmade)
                            // {
                            if (DadataApi.TryFillAddressByBdOrDadata(gd))
                            {

                                gd.CITY_DISTRICT = "";
                                if (!r.IsDBNull(19))
                                {
                                    gd.CITY_DISTRICT = r.GetValue(19).ToString();
                                    if (!r.IsDBNull(20))
                                    {
                                        gd.CITY_DISTRICT += $" ({r.GetValue(20).ToString()})";
                                    }
                                }



                                res.Add(gd);
                            }
                            else
                            {
                                Query = "CrateMate_WriteBadAddress";
                                cmd = new SqlCommand(Query, _con);
                                cmd.Parameters.AddWithValue("@id_owner", gd.DataBase_Guid);
                                cmd.Parameters.AddWithValue("@address", gd.ADDRESS);
                                cmd.CommandType = CommandType.StoredProcedure;

                                Log.Warning($" [BinMan Sync] Ignoring {gd.DataBase_Guid}, ({gd.NAME}) - Can't get address from dadata"); res.Add(gd);// IgnoreList.Add(gp);// continue;
                                SQL.Execute(cmd);
                            }


                            // }
                            //  else
                            //  {
                            //      res.Add(gd);
                            //gd.ADDRESS=
                            // }
                        }
                        else
                        {
                            continue;
                        }
                        Log.Warning($"Геозона {gd.DataBase_Guid}, {gd.ADDRESS} имеет Id_Type_container = null");
                        if (!r.IsDBNull(13) && !r.IsDBNull(14))
                    {
                            var log = r.GetValue(13).ToString();
                            var pass = r.GetValue(14).ToString();
                            if(log != "altunin@sibtko.ru")// Волшебный пользователь, который не работает ни в 1-й системе
                                gd.ld = BinManApi.GetCustomAccount(log, pass);
                    }

                    Query = "BinMan_GetGeozoneContainers";
                        cmd = new SqlCommand(Query, _con);
                        cmd.Parameters.AddWithValue("@geoGuid", gd.DataBase_Guid);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var r2 = SQL.StartRead(cmd))
                        {
                            var conts = new List<GeoContainer>();
                            try
                            {
                                while (r2.Read())
                                {
                                    var cc = new GeoContainer();
                                    cc.guid = r2.GetValue(0).ToString();

                                    cc.volume = float.Parse(r2.GetValue(1).ToString());
                                    cc.typeGuid = r2.GetValue(2).ToString();

                                    if (cc.typeGuid.Contains("b7daeb"))
                                    {

                                    }

                                    conts.Add(cc);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Битый контейнер в базе ?",ex);
                            }
                            List<BinManParser.Api.Container> bconts = new List<BinManParser.Api.Container>();
                            Dictionary<Tuple<Geo_container_type, float>, BinManParser.Api.Container> map = 
                                new Dictionary<Tuple<Geo_container_type, float>, BinManParser.Api.Container>();
                            foreach (var cc in conts)
                            {
                                var tt = new Tuple<Geo_container_type, float>(cc.GetBinManType(), cc.volume);
                                if (map.TryGetValue(tt, out var asd))
                                {
                                    asd.count++;
                                }
                                else {
                                    BinManParser.Api.Container c = new BinManParser.Api.Container();
                                    c.TYPE = cc.GetBinManType();
                                    c.VOLUME = cc.volume;
                                    c.count = 1;

                                    bconts.Add(c);
                                    map.Add(tt, c);
                                }




                            }

                            if (gd.TaskType == BinManTaskType.insert) gd.NAME = BinManGeozoneTask.GenerateName(gd.LAST_AREA, gd.Db_groupGuid, conts.ToArray());

                            gd.ContainerList = conts;
                            gd.containers = bconts.ToArray();
                        }
                    }
                }

                return res;

            }
        }
        public static BinManGeozoneTask GetBinmanGeozonesUpdate_SingleGeo(string geoGuid)
        {
           using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "Binman_UpdateGEO_GetSingleGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.Parameters.AddWithValue("@idGeo", geoGuid);
                cmd.CommandType = CommandType.StoredProcedure;





                List<BinManGeozoneTask> res = new List<BinManGeozoneTask>();



                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {

                        var gd = new BinManGeozoneTask()
                        {
                            DataBase_Guid = r.GetValue(0).ToString(),
                            LAST_AREA = !r.IsDBNull(4) ? long.Parse(r.GetValue(4).ToString()) : -1,
                            LAT = double.Parse(r.GetValue(1).ToString()),
                            LON = double.Parse(r.GetValue(2).ToString()),
                            NAME = r.GetValue(3).ToString(),
                            AREA_CANOPY = r.GetBoolean(6),
                            AREA_ENCLOSURE = r.GetBoolean(7),
                            AREA_BASIS = (r.IsDBNull(8) ? Geo_area_basis.grunt : BinManGeozone.Guid2Enum(r.GetValue(8).ToString())),
                            //TaskType = Enum.Parse<BinManTaskType>(r.GetValue(11).ToString()),
                            ADDRESS = r.GetValue(12).ToString(),
                            NeedToBeArchived = r.GetBoolean(17),
                            IsArchive = r.GetBoolean(16),


                        };
                        if (GuidIgnoreList.TryGetValue(gd.DataBase_Guid, out var FailCount))
                        {
                            if (FailCount >= 3)
                                continue;
                        }

                        if (!r.IsDBNull(18)) gd.db_partAdressOwnerGuid = r.GetValue(18).ToString();

                        var gp = new GeoPoint(gd.LAT, gd.LON);
                        //if (IgnoreList.Contains(gp)) continue;

                        if (!r.IsDBNull(15)) gd.isAddressHandmade = r.GetBoolean(15);

                        if (r.IsDBNull(9)) { Log.Error($"Геозона {gd.DataBase_Guid}, {gd.ADDRESS} имеет Id_TypeGeozone = null"); }
                        /*
                            //GetValue(gd.DataBase_Guid,out var Cc);
                            if (GuidIgnoreList.ContainsKey(gd.DataBase_Guid))
                            {
                                GuidIgnoreList[gd.DataBase_Guid] += 1; // Не потоко безопасно !!
                            }
                            else
                                GuidIgnoreList.TryAdd(gd.DataBase_Guid,0);
                            continue;
                        */

                        else { gd.Db_groupGuid = r.GetValue(9).ToString(); }
                        if (!r.IsDBNull(10)) gd.Db_containerGuid = r.GetValue(10).ToString();

                        gd.GROUP = BinManGeozone.Guid2GROUP(gd.Db_groupGuid);



                        //  if (GuidIgnoreList.Contains(gd.DataBase_Guid)) { }
                        //   else
                        if (!IgnoreList.Contains(gp))
                        {
                            // if (!gd.isAddressHandmade)
                            // {
                            if (DadataApi.TryFillAddressByBdOrDadata(gd))
                            {

                                gd.CITY_DISTRICT = "";
                                if (!r.IsDBNull(19))
                                {
                                    gd.CITY_DISTRICT = r.GetValue(19).ToString();
                                    if (!r.IsDBNull(20))
                                    {
                                        gd.CITY_DISTRICT += $" ({r.GetValue(20).ToString()})";
                                    }
                                }



                                res.Add(gd);
                            }
                            else
                            {
                                Query = "CrateMate_WriteBadAddress";
                                cmd = new SqlCommand(Query, _con);
                                cmd.Parameters.AddWithValue("@id_owner", gd.DataBase_Guid);
                                cmd.Parameters.AddWithValue("@address", gd.ADDRESS);
                                cmd.CommandType = CommandType.StoredProcedure;

                                Log.Warning($" [BinMan Sync] Ignoring {gd.DataBase_Guid}, ({gd.NAME}) - Can't get address from dadata"); res.Add(gd);// IgnoreList.Add(gp);// continue;
                                SQL.Execute(cmd);
                            }


                            // }
                            //  else
                            //  {
                            //      res.Add(gd);
                            //gd.ADDRESS=
                            // }
                        }
                        else
                        {
                            continue;
                        }
                        Log.Warning($"Геозона {gd.DataBase_Guid}, {gd.ADDRESS} имеет Id_Type_container = null");
                        if (!r.IsDBNull(13) && !r.IsDBNull(14))
                        {
                            var log = r.GetValue(13).ToString();
                            var pass = r.GetValue(14).ToString();
                            if (log != "altunin@sibtko.ru")// Волшебный пользователь, который не работает ни в 1-й системе
                                gd.ld = BinManApi.GetCustomAccount(log, pass);
                        }

                        Query = "BinMan_GetGeozoneContainers";
                        cmd = new SqlCommand(Query, _con);
                        cmd.Parameters.AddWithValue("@geoGuid", gd.DataBase_Guid);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var r2 = SQL.StartRead(cmd))
                        {
                            var conts = new List<GeoContainer>();
                            try
                            {
                                while (r2.Read())
                                {
                                    var cc = new GeoContainer();
                                    cc.guid = r2.GetValue(0).ToString();

                                    cc.volume = float.Parse(r2.GetValue(1).ToString());
                                    cc.typeGuid = r2.GetValue(2).ToString();

                                    if (cc.typeGuid.Contains("b7daeb"))
                                    {

                                    }

                                    conts.Add(cc);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Битый контейнер в базе ?", ex);
                            }
                            List<BinManParser.Api.Container> bconts = new List<BinManParser.Api.Container>();
                            Dictionary<Tuple<Geo_container_type, float>, BinManParser.Api.Container> map =
                                new Dictionary<Tuple<Geo_container_type, float>, BinManParser.Api.Container>();
                            foreach (var cc in conts)
                            {
                                var tt = new Tuple<Geo_container_type, float>(cc.GetBinManType(), cc.volume);
                                if (map.TryGetValue(tt, out var asd))
                                {
                                    asd.count++;
                                }
                                else
                                {
                                    BinManParser.Api.Container c = new BinManParser.Api.Container();
                                    c.TYPE = cc.GetBinManType();
                                    c.VOLUME = cc.volume;
                                    c.count = 1;

                                    bconts.Add(c);
                                    map.Add(tt, c);
                                }




                            }

                            if (gd.TaskType == BinManTaskType.insert) gd.NAME = BinManGeozoneTask.GenerateName(gd.LAST_AREA, gd.Db_groupGuid, conts.ToArray());

                            gd.ContainerList = conts;
                            gd.containers = bconts.ToArray();
                        }
                    }
                }

                return res.Count>0 ? res[0]:null;

            }
        }



        public static HashSet<string> ContainersGuidIgnoreList = new HashSet<string>();
        public class BinManContainerTask :GeoContainer
        {
            public BinManTaskType type;
            public string geo_binid;
            public LoginData ld;
        }

        public static List<BinManContainerTask> GetBinmanContainersUpdateList()
        {
           using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "BinMan_GetContainers2Sync";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<BinManContainerTask> res = new List<BinManContainerTask>();


                try
                {
                    using (var r = SQL.StartRead(cmd))
                    {
                        while (r.Read())
                        {

                            var cc = new BinManContainerTask();

                            cc.guid = r.GetValue(0).ToString();
                            if (ContainersGuidIgnoreList.Contains(cc.guid)) continue;
                            cc.geo_binid = r.GetValue(1).ToString();
                            cc.volume = float.Parse(r.GetValue(2).ToString());
                            cc.typeGuid = r.GetValue(3).ToString();
                            cc.type = r.GetInt32(4) == 1 ? BinManTaskType.insert : BinManTaskType.delete;

                            if (!r.IsDBNull(5) && !r.IsDBNull(6))
                            {
                                var log = r.GetValue(5).ToString();
                                var pass = r.GetValue(6).ToString();

                                cc.ld = BinManApi.GetCustomAccount(log, pass);
                            }

                            res.Add(cc);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                }
                return res;

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns> string.empty if not found</returns>
        public static string NDoc2BinId(this string t)
        {
            try
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {



                    _con.Open();

                    var Query = "SELECT Top(1) d.id_Dogovor FROM DOG d WHERE @ndoc= d.NDoc";
                    var cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@ndoc", t);


                    using (var r= SQL.StartRead(cmd))
                    {
                        
                        if (r.Read())
                        {
                            if (!r.IsDBNull(0)) return r.GetValue(0).ToString();
                            else return string.Empty;
                        }
                    }

                    return string.Empty;
                       
                }
            }
            catch(Exception ex)
            {
                return string.Empty; 
                Log.Error("NDoc2BinId", ex);
            }
        }
        //public static void DeleteDogNachsInMounth(DateTime mounth)
        //{
        //    using (SqlConnection _con = new SqlConnection(SqlconnectionString))
        //    {
        //        _con.Open();
        //        var Query = @"Delete from accruals";
        //        var cmd = new SqlCommand(Query, _con);
        //        cmd.Parameters.AddWithValue("@dateFrom", DateFrom);
        //        cmd.Parameters.AddWithValue("@dateTo", DateTo);

        //        var res = new List<string>();
        //        cmd.CommandType = CommandType.Text;
        //        using (var r = SQL.StartRead(cmd))
        //        {

        //            while (r.Read())
        //            {
        //                var bin_id = r.GetValue(0).ToString();

        //                if (!res.Contains(bin_id))
        //                    res.Add(bin_id);
        //            }


        //        }
        //        return res;
        //    }
        //}

        /// <summary>
        /// Example: 
        /// new DateTime(2024,05,01),
        /// new DateTime(2024,06,01)
        /// </summary>
        /// <returns>List of Dog_binId</returns>
        public static List<string> GetAccrualsListToCreateAutomaticly(DateTime DateFrom,DateTime DateTo)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = @"
;with cte as( select * FROM Load_accruals_list lol
WHERE lol.is_marked_as_load = 1
and lol.date > cast('2024.01.01' as date))

select d.id_Dogovor FROM Dog d
join cte c on c.id_dog = d.id_Dogovor
WHERE
--d.id_Dogovor = '939628' and

--select * FROM accruals a WHERE a.id_dog = 'BCB32114-82E9-4C5D-AA7A-C70436D76A93'

not exists(
		select * FROM accruals a 
		WHERE 
		a.id_dog =  d.id and 
		a.vid_usl 
		 = 1 and
		  (a.id_Head_nach is null or a.id_Head_nach ='') and a.Date_period between cast(@dateFrom as date) and  cast(@dateTo as date) 
  )
  and d.id_TypeDog = '111AECCB-89EF-4A7E-AE42-08D5CEBC2110'
  and 
  (d.DateEnd is null or d.DateEnd >=cast(@dateFrom as date))";
                var cmd = new SqlCommand(Query, _con);
                cmd.Parameters.AddWithValue("@dateFrom", DateFrom );
                cmd.Parameters.AddWithValue("@dateTo",DateTo);

                var res = new List<string>();
                cmd.CommandType = CommandType.Text;
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var bin_id = r.GetValue(0).ToString();
                        
                        if(!res.Contains(bin_id))
                            res.Add(bin_id);
                    }


                }
                return res;
            }
        }

        public struct ReopenSettings
        {
            bool CreateAccrual;
        }// TODO ?
        //public static void GetDogObjectsBinIds()
        //{ }
        public static void DogReopeningFromExcel(params string[] ExcellFilePaths)
        {
            // if (BinManApi.Accounts == null || BinManApi.Accounts.Length == 0)
            //   BinManApi.Init();
            foreach (var ExcellFilePath in ExcellFilePaths)
            {
                using var book = new XLWorkbook(ExcellFilePath);
                var SaveAt = Path.GetDirectoryName(ExcellFilePath) + Path.GetFileNameWithoutExtension(ExcellFilePath) + ".res" + Path.GetExtension(ExcellFilePath);

                var es = book.Worksheets.First();

                var c = es.Column(1);
                var cc = c.LastCellUsed();
                int l = 0;
                try
                {
                    l = cc.Address.RowNumber;
                }
                catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
                var SkipFirstRow = true;

                var IdDog_Col = 1;
                var TarifCount_Col = 2;
                var DateStart_Col = 3;
                var DateEnd_Col = 4;

                for (int i = SkipFirstRow ? 2 : 1; i <= l; i++) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! 2= > TEST Потом поменять
                {
                    var Ndoc =  es.Cell(i, IdDog_Col).Value.ToString().Trim();
                    var TarifCount = es.Cell(i, TarifCount_Col).Value.ToString().Trim();
                    var DateStart = es.Cell(i, DateStart_Col).Value.ToString().Trim();
                    var DateEnd = es.Cell(i, DateEnd_Col).Value.ToString().Trim();
                    var IdDog = string.Empty;
                    var idKa = string.Empty;

                    var DateStart_dt = DateTime.Parse(DateStart);
                    var DateEnd_dt = DateTime.Parse(DateEnd);
                    var DateSign = DateStart_dt;

                    

                    es.Cell(i, 6).Value = "Начата обработка...";

                    using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                    {

                        _con.Open();
                        var DogGuid = string.Empty;
                        var Query = "select top(1) d.id_Dogovor, d.id_KA,d.DateAccept,d.id FROM Dog d WHERE d.NDoc = @ndoc";
                        var cmd = new SqlCommand(Query, _con);
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@ndoc", Ndoc);


                        using (var r = SQL.StartRead(cmd))
                        {

                            if (r.Read())
                            {
                                if (!r.IsDBNull(0)) IdDog = r.GetValue(0).ToString();
                                if (!r.IsDBNull(1)) idKa = r.GetValue(1).ToString();
                                if (!r.IsDBNull(2)) DateSign = r.GetDateTime(2);
                                DogGuid = r.GetValue(3).ToString();
                                //else Ndoc = string.Empty;
                            }

                        }


                    }
                    if (!string.IsNullOrEmpty(Ndoc) && !string.IsNullOrEmpty(idKa))
                    {
                        var ld = BinManApi.GetNextAccount();
                        if (BinManDocumentParser.TryParseObjects(ld, IdDog, out var resobj))
                        {
                            // с 01.07 Приостановка, с 15.07 новый тариф, начисление за июль после
                            if (resobj.Count > 0)

                            {
                                es.Cell(i, 7).Value = "Get object... - Успешно";
                             
                                
                                
                                if (BinManDocuments.SendEditRequest(ld, new BinManDogData()
                                {
                                    bin_id = IdDog,
                                    dateFrom = DateStart_dt,
                                    dateTo = DateEnd_dt,
                                    //dateSign = DateStart_dt,//13.06.2024 -- Аня сазала не трогать
                                    dateSign = DateSign,
                                    Number = Ndoc,
                                    Client_BinManid = idKa,

                                    Type_BinManCode = ((int)BinManDogData.DogType.PHYSICAL_NORM).ToString(),
                                    Group_BinManCode = "8"

                                }));
                                
                                {

                                    es.Cell(i, 8).Value = "Edit DOG... - Успешно";
                                    foreach (var o in resobj)
                                    {
                                        if (o.changes?.First().status != "Приостановленный")
                                        {
                                            if(o.DT_PeriodTo != default)
                                            if (BinManDocuments.SendStopObjectRequest(ld, new BinManDocuments.StopDogObject()
                                            {
                                                dog_BinId = IdDog,
                                                object_BinId = o.binid,
                                                DateFrom = o.DT_PeriodTo.AddDays(1),
                                                comment = "Не обслуживался"
                                            }))
                                            { }
                                        }
                                        if (string.IsNullOrEmpty(TarifCount))
                                        {

                                            TarifCount = o.tarif_volume;
                                            if (TarifCount == "Нет данных" || TarifCount == "0")
                                               // int step = 0;
                                                foreach(var h in o.changes)
                                                {
                                                    TarifCount = h.tarif_volume;
                                                    if (!(string.IsNullOrEmpty(TarifCount)|| TarifCount == "Нет данных" || TarifCount == "0")) break;
                                                }


                                            //    TarifCount = SQL.GetObjectTariff(o.binid,IdDog);
                                            
                                        }
                                        if (BinManDocuments.SendAttachObjectRequest(ld, new BinManDocuments.AttachObjectInfo()
                                        {
                                            doc_BinManId = IdDog,
                                            activeFrom = DateStart_dt,
                                            obj_BinManId = o.binid,
                                            tarif_BinManCode = "237", // TARIF
                                                                      // select * FROM Tariff_body t WHERE GETDATE() between t.DateBegin and t.DateEnd
                                            tarif_value = TarifCount//ACTUAL VALUE

                                        }))
                                        {
                                            es.Cell(i, 9).Value = "Edit OBJECT... - Успешно";
                                        };
                                    }

                                    var dateFrom    = new DateTime(2024, 07, 01);
                                    var dateTo      = new DateTime(2024, 07, 31);

                                    //СОЗДАТЬ НАЧИСЛЕНИЯ
                                     if(false)  if (BinManDocAccruals.TryGetAccrualSumm(ld, IdDog, dateFrom, dateTo, out var Summa))
                                    {
                                        BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
                                        {
                                            doc_BinId = IdDog,
                                            comment = "",
                                            summ = Summa.ToString(),
                                            date = new DateTime(2024, 07, 31),
                                            typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
                                            dateFrom = dateFrom,
                                            dateTo = dateTo

                                        }, out var BinIdd);
                                    }
                                    else
                                    {
                                        Log.Error($"ASDASDASD!!!! : {IdDog}");
                                        //throw new Exception();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        es.Cell(i, 7).Value = "Get Dog (NDoc)... - FAILED! ";
                    }
                }
                book.SaveAs(SaveAt);
            }
        }
        public static string GetObjectTariff(string BinidObj,string BinidDog)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {

                _con.Open();
                var Query = @"
select top 1 replace(tt.people,' человек','') FROM temp_BinMan_ObjectParse tt WHERE tt.id_dog  
= (select top 1 d.id FROM dog d WHERE d.id_Dogovor = @Binid_dog) and tt.binid = @Binid_obj
and tt.people <> 'Нет данных' and tt.people <> '0'
";
                var cmd = new SqlCommand(Query, _con);
                 cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@Binid_dog", BinidDog);
                cmd.Parameters.AddWithValue("@Binid_obj", BinidObj);

                using (var r = SQL.StartRead(cmd))
                {

                    if (r.Read())
                    {

                        if (!r.IsDBNull(0)) return r.GetValue(0).ToString();

                    }
                    else { return string.Empty; }

                }
                return string.Empty;
            }
        }
        public static void LoadDataFromBinmanFormatExcel(string ExcellFilePath,DateTime AntiMultiLoad)
        {
            if (AntiMultiLoad < DateTime.Now)
            {
                Log.Error("ПОВТОРНАЯ ЗАГРУЗКА ФАЙЛА !!!???");
                return;
            }
                using var book = new XLWorkbook(ExcellFilePath);
                var es = book.Worksheets.First();
            
                var c = es.Column(1);
                var cc = c.LastCellUsed();
                int l = 0;
                try
                {
                    l = cc.Address.RowNumber;
                }
                catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
            var SkipFirstRow = true;

            var Type_map = BinMan_GetKaTypeMap();
            var Dog_Group_map = BinMan_GetDogGroupTypeMap();
            var Obj_Type_map = BinMan_GetObjTypeMap();
            var Tarif_Type_map = BinMan_GetTarifTypeMap();

                for (int i = SkipFirstRow ? 2 : 1; i <= l; i++)
                {
                #region KA
                string CreateKaError = string.Empty;
                var Ka_Type = es.Cell(i, 1).Value.ToString().Trim();
                    var Ka_Title_short = es.Cell(i, 2).Value.ToString().Trim();

                    var Ka_F = es.Cell(i, 3).Value.ToString().Trim();
                    var Ka_I = es.Cell(i, 4).Value.ToString().Trim();
                    var Ka_O = es.Cell(i, 5).Value.ToString().Trim();

#endregion
#region Dog

                var CreateDogError = string.Empty;

                var dog_Start = DateTime.MinValue;
                if (!DateTime.TryParse(es.Cell(i, 6).Value.ToString().Trim(), out dog_Start)) { CreateDogError += "Err dog_Start;"; }

                var dog_End = DateTime.MinValue;
                if (!DateTime.TryParse(es.Cell(i, 7).Value.ToString().Trim(), out dog_End)) { CreateDogError += "Err dog_End;"; }
                #endregion

                #region Tarif
                var CreateTarifError = string.Empty;

                var volume = es.Cell(i, 8).Value.ToString().Trim();
                var tarif_Type = es.Cell(i, 9).Value.ToString().Trim();
                if (string.IsNullOrEmpty(tarif_Type))
                {
                    tarif_Type = "FL";
                }

                #endregion

                #region Obj
                var CreateObjError = string.Empty;
                var obj_address = es.Cell(i, 10).Value.ToString().Trim();
                var obj_type = es.Cell(i, 11).Value.ToString().Trim();
                //12 obl
                //13 Район
                // 14 Горов
                // Насел пункт 15
                // Улица 16
                // Дом 17
                // Корпус 18
                // Пом 19
                var obj_Korp = es.Cell(i, 18).Value.ToString().Trim();
                var obj_Kvart_Flat = es.Cell(i, 19).Value.ToString().Trim();
                // x4 - YeS 20-24
                var obj_name = es.Cell(i, 24).Value.ToString().Trim();
                #endregion


                var dog_group = es.Cell(i, 25).Value.ToString().Trim();
                var id_geozone = es.Cell(i, 26).Value.ToString().Trim();



                
                if (!Type_map.TryGetValue(Ka_Type, out var Ka_Type_Guid)) CreateKaError += "Тип не известный;";

                if (!Dog_Group_map.TryGetValue(dog_group, out var Dog_Group_Guid)) CreateDogError = "Тип не известный;";

                if (!Obj_Type_map.TryGetValue(obj_type, out var Obj_Type_Guid)) CreateObjError = "Тип не известный;";

                if (!Tarif_Type_map.TryGetValue(tarif_Type, out var Tarif_Type_Guid)) CreateTarifError = "Тип не известный;";

                if (!DadataApi.TryFindAddressByAddress(obj_address, out var addr))
                {

                    //CreateObjError += "Dadata не нашла адрес ";
                    var ind = obj_address.IndexOf(", кв.");
                    var CutAddress = obj_address.Substring(0, ind);
                    var flat = obj_address.Substring(ind);
                    if (!DadataApi.TryFindAddressByAddress(CutAddress, out addr))
                    {
                        CreateObjError += "Dadata вообще не нашла адрес  ";
                    }
                    else
                    {
                        addr.data.flat = flat;
                        addr.data.flat_type_full = "квартира";
                    }
                }
                var addr_guid = Guid.NewGuid().ToString();

                var Kaa = new BinManKALoad()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Ka_F = Ka_F,
                    Ka_I = Ka_I,
                    Ka_O = Ka_O,
                    Ka_Title_short = Ka_Title_short,
                    Ka_Type_guid = Ka_Type_Guid,
                    address_id = addr_guid,
                    address = addr ==null? obj_address : addr.unrestricted_value 
                };
                var dog = new BinManDogLoad()
                {
                    Guid = Guid.NewGuid().ToString(),
                    DateEnd = dog_End,
                    DateStart = dog_Start,
                    dog_group_Guid = Dog_Group_Guid,
                    ka_guid = Kaa.Guid
                };








                bool SomeError = false;

                if (!string.IsNullOrEmpty(CreateKaError)) { es.Cell(i, 22).Value = "Клиент: " + CreateKaError; SomeError = true; } // Ошибки по Ka
               
                if (!string.IsNullOrEmpty(CreateDogError)) { es.Cell(i, 23).Value = "Дог: " + CreateDogError; SomeError = true; } // Ошибки по Ka
            
                if (!string.IsNullOrEmpty(CreateObjError)) { es.Cell(i, 24).Value = "Тариф: " + CreateObjError; SomeError = true; } // Ошибки по Ka
                
                if (!string.IsNullOrEmpty(CreateTarifError)) { es.Cell(i, 25).Value = "Объект: " + CreateTarifError; SomeError = true; } // Ошибки по Ka

                if (!SomeError) {

                    var obj = new BinManObjectLoad()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        addres =  addr,
                        title = obj_name,
                        type_guid = Obj_Type_Guid.PrimaryType_Guid,
                        subtype_guid = Obj_Type_Guid.SubType_Guid,
                        geoObj_geozone_binId = id_geozone,
                        address_Guid = addr_guid

                    };
                    var dt = new BinManDogTarifLoad()
                    {
                        dog_guid = dog.Guid,
                        tarif_guid = Tarif_Type_Guid,
                        obj_guid = obj.Guid,
                        volume = volume
                    };

                 SQL.LoadBinManKaInBd(Kaa); 
                 SQL.LoadBinManKaInBd(dog); 
                 SQL.LoadBinManKaInBd(obj); 
                 //SQL.LoadBinManKaInBd(Kaa); 
                 SQL.LoadBinManKaInBd(dt);

                  //  book.SaveAs();

                }
            }


        }
        public class BinManObjectLoadType
        {
            public string PrimaryType_Guid;
            public string SubType_Guid;

            public BinManObjectLoadType(string primaryType_Guid, string subType_Guid)
            {
                PrimaryType_Guid = primaryType_Guid;
                SubType_Guid = subType_Guid;
            }
        }
        public class BinManKALoad
        {
            public string Guid;
            public string Ka_Type_guid;
            public string Ka_Title_short;
            public string Ka_F ;
            public string Ka_I ;
            public string Ka_O ;
            public string address;
            public string address_id;
        }
        public class BinManDogLoad
        {
            public string Guid;
            public DateTime DateStart;
            public DateTime DateEnd;
            public string dog_group_Guid;
            public string ka_guid;
        }
        public class BinManObjectLoad
        {
            public string Guid;
            public string address_Guid;
            public Suggestion<Address> addres;
            public string type_guid;
            public string subtype_guid;
            public string geoObj_geozone_binId;
            public string title;
        }
        public class BinManDogTarifLoad
        {
            public string obj_guid;
            public string dog_guid;
            public string tarif_guid;
            public string volume;
        }
        public static void LoadBinManKaInBd(BinManKALoad ka)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsertKaInBd";
                var cmd = new SqlCommand(Query, _con);

                cmd.Parameters.AddWithValue("@id", ka.Guid);
                cmd.Parameters.AddWithValue("@f", ka.Ka_F);
                cmd.Parameters.AddWithValue("@i", ka.Ka_I);
                cmd.Parameters.AddWithValue("@o", ka.Ka_O);
                cmd.Parameters.AddWithValue("@title", ka.Ka_Title_short);
                cmd.Parameters.AddWithValue("@id_type", ka.Ka_Type_guid);
                cmd.Parameters.AddWithValue("@address", ka.address);
                cmd.Parameters.AddWithValue("@id_address", ka.address_id);


                cmd.CommandType = CommandType.StoredProcedure;

                SQL.Execute(cmd);

                
            }
        }       
        public static void LoadBinManKaInBd(BinManDogTarifLoad dt)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsertTarifInBD";
                var cmd = new SqlCommand(Query, _con);

                cmd.Parameters.AddWithValue("@id_dog", dt.dog_guid);
                cmd.Parameters.AddWithValue("@id_obj", dt.obj_guid);
                cmd.Parameters.AddWithValue("@id_tarif", dt.tarif_guid);
                cmd.Parameters.AddWithValue("@volume", dt.volume);



                cmd.CommandType = CommandType.StoredProcedure;

                SQL.Execute(cmd);

                
            }
        }
        public static void LoadBinManKaInBd(BinManDogLoad d)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsertDogInBD";
                var cmd = new SqlCommand(Query, _con);

                cmd.Parameters.AddWithValue("@id", d.Guid);
                cmd.Parameters.AddWithValue("@dateStart", d.DateStart);
                cmd.Parameters.AddWithValue("@dateEnd", d.DateEnd);
                cmd.Parameters.AddWithValue("@id_ka", d.ka_guid);
                cmd.Parameters.AddWithValue("@id_type_group", d.dog_group_Guid);


                cmd.CommandType = CommandType.StoredProcedure;

                SQL.Execute(cmd);


            }
        }
        public static void LoadBinManKaInBd(BinManObjectLoad d)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsertObjectInBd";
                var cmd = new SqlCommand(Query, _con);

                string guid = d.address_Guid;

                SQL.InsertNewAddress(d.addres, guid, d.Guid,true);
               // var lat = ;
               // d.addres = SQL.GetOwnedAddressInfo(guid);


                cmd.Parameters.AddWithValue("@id", d.Guid);
                cmd.Parameters.AddWithValue("@address", d.addres.value);
                cmd.Parameters.AddWithValue("@id_type", d.type_guid);
                cmd.Parameters.AddWithValue("@id_subtype", d.subtype_guid);
                cmd.Parameters.AddWithValue("@binid_geozone_to_geo_obj", d.geoObj_geozone_binId);
                cmd.Parameters.AddWithValue("@id_part_address", guid);
                cmd.Parameters.AddWithValue("@lat", d.addres.data.geo_lat);
                cmd.Parameters.AddWithValue("@lon", d.addres.data.geo_lon);
                cmd.Parameters.AddWithValue("@title", d.title);




                cmd.CommandType = CommandType.StoredProcedure;

                SQL.Execute(cmd);


            }
        }
        public static bool SimpleTextExecute(string Command, Dictionary<string, object> Params = null)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = Command;
                var cmd = new SqlCommand(Query, _con);

                

                foreach (var p in Params)
                    cmd.Parameters.AddWithValue(p.Key, p.Value);





                cmd.CommandType = CommandType.Text;

                return SQL.Execute(cmd);


            }
        }
        public static Dictionary<string, string> BinMan_GetTarifTypeMap()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "SELECT  t.id,t.code FROM Tariff t";
                var cmd = new SqlCommand(Query, _con);


                var res = new Dictionary<string, string>();
                cmd.CommandType = CommandType.Text;
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var bin_key = r.GetValue(1).ToString();
                        var our_id = r.GetValue(0).ToString();

                        res.Add(bin_key, our_id);
                    }


                }
                return res;
            }
            
        }        
        public class InfoForFullKaDelition
        {
            public List<DbWithBinId> kaIds = new List<DbWithBinId>();
            public List<DbWithBinId> dogsIds = new List<DbWithBinId>();
            public List<DbWithBinId> objsIds = new List<DbWithBinId>();
            public class DbWithBinId
            {
                public string dbId;
                public string binId;

                public DbWithBinId(string dbId, string binId)
                {
                    this.dbId = dbId;
                    this.binId = binId;
                }
            }
        }
        public static InfoForFullKaDelition GetKaListForDeletion()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "HandMade_KADestroyerList_ORder_Obj_Dog";
                var cmd = new SqlCommand(Query, _con);


              
                cmd.CommandType = CommandType.StoredProcedure;
                var res = new InfoForFullKaDelition();
                using (var r = SQL.StartRead(cmd))
                {
                    
                    while (r.Read())
                    {
                       
                        var our_id = r.GetValue(0).ToString();
                        var bin_key = r.GetValue(1).ToString();

                        res.objsIds.Add(new InfoForFullKaDelition.DbWithBinId(our_id, bin_key));
                    }
                    r.NextResult();
                    while (r.Read())
                    {

                        var our_id = r.GetValue(0).ToString();
                        var bin_key = r.GetValue(1).ToString();

                        res.dogsIds.Add(new InfoForFullKaDelition.DbWithBinId(our_id, bin_key));
                    }
                    r.NextResult();
                    while (r.Read())
                    {

                        var our_id = r.GetValue(0).ToString();
                        var bin_key = r.GetValue(1).ToString();

                        res.kaIds.Add(new InfoForFullKaDelition.DbWithBinId(our_id, bin_key));
                    }

                }
                return res;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns> bin_id, (primary type, subtype) </returns>
        public static Dictionary<string, BinManObjectLoadType> BinMan_GetObjTypeMap()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "SELECT t.id,t.Title,t.id_Type_Object FROM SubType_Object t";
                var cmd = new SqlCommand(Query, _con);


                var res = new Dictionary<string, BinManObjectLoadType>();
                cmd.CommandType = CommandType.Text;
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var bin_key = r.GetValue(1).ToString();
                        var our_id = r.GetValue(0).ToString();
                        var our_id_2 = r.GetValue(2).ToString();
                        res.TryAdd(bin_key, new BinManObjectLoadType( our_id_2, our_id ));
                    }


                }
                return res;
            }

        }
        public static Dictionary <string,string> BinMan_GetKaTypeMap()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "SELECT t.id,t.TypeBinman FROM Type_KA t";
                var cmd = new SqlCommand(Query, _con);


                var res = new Dictionary<string, string>();
                cmd.CommandType = CommandType.Text;
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var bin_key = r.GetValue(1).ToString();
                        var our_id = r.GetValue(0).ToString();

                        res.Add(bin_key, our_id);
                    }

                    
                }
                return res;
            }
            
        }
        public static Dictionary<string, string> BinMan_GetDogGroupTypeMap()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "SELECT ga.id,ga.Title FROM GroupAnalytic ga";
                var cmd = new SqlCommand(Query, _con);


                var res = new Dictionary<string, string>();
                cmd.CommandType = CommandType.Text;
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var bin_key = r.GetValue(1).ToString();
                        var our_id = r.GetValue(0).ToString();

                        res.Add(bin_key, our_id);
                    }

                }
                return res;
            }
         
        }
        public static Suggestion<Address> GetAddressInfo(string AddressGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "GetAddressInfo";
                var cmd = new SqlCommand(Query, _con);

                cmd.Parameters.AddWithValue("@addressGuid", AddressGuid);

                cmd.CommandType = CommandType.StoredProcedure;
                using (var r2 = SQL.StartRead(cmd))
                {
                    
                    r2.Read();
                    
                    if (!r2.HasRows) return null;

                    return ReadAddress(r2, out _, out _);
                }

            }
        }

        public static Suggestion<Address> GetOwnedAddressInfo(  string OwnerGuid, out string guid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "GetOwnerAddressInfo";
                var cmd = new SqlCommand(Query, _con);
                cmd.Parameters.AddWithValue("@id", OwnerGuid);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var r2 = SQL.StartRead(cmd))
                {
                    var conts = new List<GeoContainer>();
                    r2.Read();
                    guid = null;
                    if (!r2.HasRows) return null;
                    
                    return ReadAddress(r2,out guid, out _);
                }

            }
        }
        public record struct AddressAccurateTask(string guid,string address);
        public static List<AddressAccurateTask> GetGeozonesToAccurateAddress()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "Dadata_GetGeozonesToAccurateAddreses";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                var res = new List<AddressAccurateTask>(15000);
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        AddressAccurateTask ga = new AddressAccurateTask();

                        ga.guid = r.GetValue(0).ToString();
                        ga.address = r.GetValue(1).ToString();

                        res.Add(ga);
                    }
                }
                return res;
            }
        }
        public static List<AddressAccurateTask> GetObjectsToAccurateAddress()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "Dadata_GetobjectsToAccurateAddreses";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                var res = new List<AddressAccurateTask>(60000);
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        AddressAccurateTask ga = new AddressAccurateTask();

                        ga.guid = r.GetValue(0).ToString();
                        ga.address = r.GetValue(1).ToString();

                        res.Add(ga);
                    }
                }
                return res;
            }
        }
        public static void InsertGeozoneAccurateResult(Suggestion<Address> address,string geozoneGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var addressGuid = Guid.NewGuid().ToString();

                InsertNewAddress(address,addressGuid,geozoneGuid);

                var Query = "UPDATE geozone set id_adress_Detail = @addresDetailGuid  WHERE id = @id";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", geozoneGuid);
                cmd.Parameters.AddWithValue("@addresDetailGuid", addressGuid);


                


                SQL.Execute(cmd);
            }
        }


        //public static void InsertGeozoneCommentary()
        //{
        //    using (SqlConnection _con = new SqlConnection(SqlconnectionString))
        //    {
        //        _con.Open();
        //        var Query = "InsertPartitionalAddress";
        //        var cmd = new SqlCommand(Query, _con);

        //        cmd.CommandType = CommandType.StoredProcedure;

        //        if (guid == null) guid = Guid.NewGuid().ToString();

        //        AddAddressCmdParams(cmd, address, guid, ownerGuid, ignoreHandMadeProcessing);

        //        SQL.Execute(cmd);
        //    }
        //}


        public static void InsertNewAddress(Suggestion<Address> address,string guid=null, string ownerGuid=null, bool ignoreHandMadeProcessing = false)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "InsertPartitionalAddress";
                var cmd = new SqlCommand(Query, _con);
              
                cmd.CommandType = CommandType.StoredProcedure;

                if (guid == null) guid = Guid.NewGuid().ToString();

                AddAddressCmdParams(cmd,address,guid,ownerGuid,ignoreHandMadeProcessing);

                SQL.Execute(cmd);
            }
        }

        public static string GetGeozoneGuidByBinId(string BinId)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "select g.ID FROM geozone g WHERE g.ID_binman = @BinId";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@BinId", BinId);
                
                using (var r = SQL.StartRead(cmd))
                {
                    if (r.Read())
                    {
                        return r.GetValue(0).ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                
            }
        }

        public static string InnerServer_GetClosestGeozoneAddress(GeoPoint pos)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetClosestGeozone";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@lat", pos.mLatitude);
                cmd.Parameters.AddWithValue("@lon", pos.mLongitude);

                using (var r = SQL.StartRead(cmd))
                {

                    if (r.Read())
                    {
                        return r.GetValue(0).ToString();
                    }
                }
                return string.Empty;
            }
            
        }

        public static bool UpdateGeozoneAddressWithPartAddress(string geoGuid,Suggestion<Address> address)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "update geozone set Adress = @address WHERE geozone.ID=  @id";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", geoGuid);
                cmd.Parameters.AddWithValue("@address", address.value);
                if (SQL.Execute(cmd))
                {
                    SQL.UpdateAddress(address, string.Empty, geoGuid);
                    return true;
                }
                else
                {
                    return false;
                }
               

            }
        }
        /// <summary>
        /// Guid можно оставить пустым, тогда он сгенерится рандомный, а на уровне бд будет проверка если у этого владельца уже есть адрес, тогда не будет дупликатов, только обновление существующей записи
        /// </summary>
        /// <param name="address"></param>
        /// <param name="guid"></param>
        /// <param name="ownerGuid"></param>
        public static void UpdateAddress(Suggestion<Address> address, string guid,string ownerGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "UpdatePartitionalAddress";
                var cmd = new SqlCommand(Query, _con);

                cmd.CommandType = CommandType.StoredProcedure;

                if (string.IsNullOrEmpty(guid)) guid = Guid.NewGuid().ToString();

                AddAddressCmdParams(cmd, address, guid, ownerGuid);

                SQL.Execute(cmd);
            }
        }
        public static void DoneHandMadeAdressProcessing(Suggestion<Address> address, bool ignoreHandMadeProcessing = false)
        {
            try
            {
                if (!ignoreHandMadeProcessing)
                {
                    address.data.house_type_full = address.data.house_type_full?.Replace("null ", "");
                    address.data.house_with_type = address.data.house_with_type?.Replace("null ", "");

                    if (!string.IsNullOrEmpty(address.data.house_with_type))
                    {
                        var splt = address.data.house_with_type.Split(", ");
                        try
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(address.data.region_with_type) && (!string.IsNullOrEmpty(address.value) || address.value.ToLower().Contains("null")) && address.unrestricted_value != null && address.unrestricted_value.Length > 5) address.value = address.unrestricted_value.Replace(address.data.region_with_type + ", ", "");
                                if (string.IsNullOrEmpty(address.value)) address.value = address.unrestricted_value.Replace("Кемеровская область - Кузбасс, ", "");
                            }
                            catch (Exception ex) { Log.Error("address dumb error :)", ex); }

                            var h = splt[0];
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
                            // address.data.house = Regex.Match(splt[0], @"\d+").Value;

                            //if (!address.data.house_type.Contains("дом"))
                            //    address.data.house_type = "дом";
                            try
                            {
                                address.data.house_with_type = address.data.house_with_type.ToLower().Replace("д ", "дом ").Replace("д.", "дом");
                                address.data.house_with_type = splt[0].Replace(",", "");
                            }
                            catch (Exception ex) { Log.Error("address dumb error v2 :)", ex); }
                            if (address.data.house_with_type == new System.String(address.data.house_with_type.Where(Char.IsDigit).ToArray()))
                            {
                                address.data.house_with_type =

                                    address.data.house_type + " " + address.data.house_with_type;
                            }
                            if (splt.Length > 1)
                            {
                                if (splt[1].Contains("к"))
                                {
                                    address.data.flat = Regex.Match(splt[1], @"\d+").Value;
                                    address.data.flat_type_full = splt[1];
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(address.unrestricted_value))
                        {
                            if (!address.unrestricted_value.Contains(address.value))
                            {
                                var splt2 = address.value.Split(",");
                                var splt1 = address.unrestricted_value.Split(",");
                                if (splt1.Length >= 2 && splt2.Length >= 2)
                                {
                                    splt1[^1] = splt2[^1];
                                    splt1[^2] = splt2[^2];
                                    if (
                                        splt1.Contains("к ")
                                        || splt1.Contains("кв ")
                                        || splt1.Contains("кв. ")
                                        || splt1.Contains("квартира ")
                                        || splt1.Contains("квартира")
                                        )
                                        if (splt1.Length >= 3 && splt2.Length >= 3)
                                        {
                                            splt1[^3] = splt2[^3];
                                        }
                                }
                                address.unrestricted_value = string.Join(",", splt1).Trim();
                                address.value = GetDetailedTrimmedAdress(address).Trim();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                    try
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
                        if (!string.IsNullOrEmpty(h2) && !string.IsNullOrEmpty(address.data.house_with_type))
                        {
                            Log.Text($"H2: `{h2}` to `{address.data.house_with_type}` address.unrestricted_value : `{address.unrestricted_value}`");

                            var r = " " + address.value.Replace(h2, " " + address.data.house_with_type)
                                 // .Replace(" " + h2.Trim(), " " + address.data.house_with_type.Trim())
                                 // .Replace(h2.Trim(), " " + address.data.house_with_type.Trim())
                                 .Trim();//Почти осмысленные действия xd
                            address.value = r;
                            r = " " + address.unrestricted_value.Replace(h2, " " + address.data.house_with_type)
                                 // .Replace(" " + h2.Trim(), " " + address.data.house_with_type.Trim())
                                 // .Replace(h2.Trim(), " " + address.data.house_with_type.Trim())
                                 .Trim();//Почти осмысленные действия xd
                            address.unrestricted_value = r;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                    try//Здесь вероятно Короткий адрес синхронизирован с полным, заполнены дома/ квартиры
                    {
                        if (address.data.street_with_type == null || !address.unrestricted_value.Contains(address.data.street_with_type))
                        {
                            var ChekList = new List<string>() {
                            "пр-кт ",
                            " пер",
                            "ул ",
                            "кв-л ",
                            "б-р ",
                            "мкр ",
                            "пер ",
                            " переулок",
                            " шоссе",
                            " проезд",
                            " спуск",
                            " ал",
                            "сад ",
                            "пом. ",
                            "пом ",
                            "наб ",
                            "промплощадка ",
                            "пл-ка ",
                        };
                            var spl = address.unrestricted_value.Split(", ");
                            var s = spl.Where(ss => ChekList.Any(c => ss.Contains(c))).FirstOrDefault();
                            if (!string.IsNullOrEmpty(s))
                            {
                                address.data.street_with_type = s.Trim();
                            }
                        }

                        if (address.data.street_with_type!=null && address.data.street !=null && !address.data.street_with_type.Contains(address.data.street))
                        {
                            bool antiInf = false;
                        gttgt:
                            var sps = address.data.street_with_type.Split(" ");
                            if (sps.Length > 1)
                            {
                                //Зачастую в конце названия а не в начале
                                if (sps[^1].Contains("пер")
                                    || sps[^1].Contains("пер.")
                                    || sps[^1].Contains("переулок")
                                    || sps[^1].Contains("шоссе")
                                    || sps[^1].Contains("проезд")
                                    || sps[^1].Contains("спуск")
                                    || sps[^1].Contains("ал")
                                    || sps[^1].Contains("тупик")
                                    || sps[^1].Contains("б-р")
                                    || sps[^1].Contains("кв-л")
                                    || sps[^1].Contains("наб")
                                    || sps[^1].Contains("ал")
                                    )
                                {
                                    address.data.street = string.Join(" ", sps.Take(sps.Length - 1));
                                }
                                else
                                {
                                    address.data.street = string.Join(" ", sps.Skip(1));
                                }
                            }
                            else if (!antiInf)
                            {

                                antiInf = true;
                                var ChekList = new List<string>() {
                            "пр-кт",
                            "ул",
                            "кв-л",
                            "б-р",
                            "мкр",
                            "пер",
                            "переулок",
                            "шоссе",
                            "проезд",
                            "спуск",
                            "ал",
                            "сад",
                            "пом.",
                            "пом",
                            "наб",
                            "промплощадка",
                            "пл-ка",
                        };
                                foreach (var c in ChekList)
                                {
                                    if (address.data.street_with_type.Contains(c))
                                    {
                                        if (address.data.street_with_type.IndexOf(c) > 3)
                                            address.data.street_with_type = address.data.street_with_type.Replace(c, c + " ");
                                        else
                                            address.data.street_with_type = address.data.street_with_type.Replace(c, " " + c);
                                        goto gttgt;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
                try
                {
                    if (address.data.house_type == "дом") address.data.house_type_full = "дом";

                    address.data.house_type_full = address.data.house_type_full.Replace("д д ", "дом ")
                                .Replace("д. д ", "дом ")
                                .Replace("д  д. ", "дом ")
                                .Replace("д. д. ", "дом ")
                                .Replace("дом д ", "дом ")
                                .Replace("дом д. ", "дом ")
                                .Replace("д дом ", "дом ")
                                .Replace("д. дом ", "дом ")
                                .Replace("дом дом ", "дом ");

                    //address.data.house_with_type.Replace("д ","дом").Replace("д. ", "дом");
                    //  address.data.house_type = address.data.house_type.Replace("д ", "дом ").Replace("д. ", "дом ");
                    // address.data.house;
                    // address.data.house_with_type = address.data.house_type + (string.IsNullOrEmpty(address.data.house)? "" : " "+ address.data.house);

                    address.data.house_type_full = address.data.house_type_full.Replace("null ", "");
                    address.data.house_with_type = address.data.house_with_type.Replace("null ", "");



                }
                catch (Exception ex)
                {

                }
                if (string.IsNullOrEmpty(address.data.house_with_type))
                {
                    if (string.IsNullOrEmpty(address.data.house_type)) {
                        if (!string.IsNullOrEmpty(address.data.house))
                            address.data.house_with_type = $"дом {address.data.house}";
                         }
                    else
                    {
                        if (!string.IsNullOrEmpty(address.data.house))
                            address.data.house_with_type = $"{(address.data.house_type.Trim() == "д" ? "дом":address.data.house_type.Trim())} {address.data.house}";
                    }
                }
                
            }
            catch (Exception ex)
            {
                Log.Warning(ex.ToString());
            }
        }
        private static void AddAddressCmdParams(SqlCommand cmd, Suggestion<Address> address, string guid, string Owner_Guid,bool ignoreHandMadeProcessing=false)
        {
            DoneHandMadeAdressProcessing(address,ignoreHandMadeProcessing);


             cmd.Parameters.AddWithValue("@id"                     , string.IsNullOrEmpty(guid        ) ? DBNull.Value :guid   );                                                               
            cmd.Parameters.AddWithValue("@id_owner"               , string.IsNullOrEmpty(Owner_Guid  )?DBNull.Value:Owner_Guid);     
            cmd.Parameters.AddWithValue("@shortaddress"           , address.value                                             );
            cmd.Parameters.AddWithValue("@fulladdress"            , address.unrestricted_value                                );
            cmd.Parameters.AddWithValue("@postal_code"            , address.data.postal_code                                  );
            cmd.Parameters.AddWithValue("@region_fias_id"         , address.data.region_fias_id                               );
            cmd.Parameters.AddWithValue("@region_kladr_id"        , address.data.region_kladr_id                              );
            cmd.Parameters.AddWithValue("@region"                 , address.data.region                                       );
            cmd.Parameters.AddWithValue("@region_with_type"       , address.data.region_with_type                             );
            cmd.Parameters.AddWithValue("@area_fias_id"           , address.data.area_fias_id                                 );
            cmd.Parameters.AddWithValue("@area_kladr_id"          , address.data.area_kladr_id                                );
            cmd.Parameters.AddWithValue("@area"                   , address.data.area                                         );
            cmd.Parameters.AddWithValue("@area_with_type"         , address.data.area_with_type                               );
            cmd.Parameters.AddWithValue("@sub_area_fias_id"       , address.data.sub_area_fias_id                             );
            cmd.Parameters.AddWithValue("@sub_area_kladr_id"      , address.data.sub_area_kladr_id                            );
            cmd.Parameters.AddWithValue("@sub_area_with_type"     , address.data.sub_area_with_type                           );
            cmd.Parameters.AddWithValue("@sub_area"               , address.data.sub_area                                     );
            cmd.Parameters.AddWithValue("@city_fias_id"           , address.data.city_fias_id                                 );
            cmd.Parameters.AddWithValue("@city_kladr_id"          , address.data.city_kladr_id                                );
            cmd.Parameters.AddWithValue("@city"                   , address.data.city                                         );
            cmd.Parameters.AddWithValue("@city_with_type"         , address.data.city_with_type                               );

            //cmd.Parameters.AddWithValue("@city_district_fias_id"  , address.data.city_district_fias_id                        );//Добавлено 04.03.2024 Удалено тогдаже xd
            //cmd.Parameters.AddWithValue("@city_district_kladr_id" , address.data.city_district_kladr_id                       );
            //cmd.Parameters.AddWithValue("@city_district_with_type", address.data.city_district_with_type                      );
            //cmd.Parameters.AddWithValue("@city_district_type"     , address.data.city_district_type                           );
            //cmd.Parameters.AddWithValue("@city_district_type_full", address.data.city_district_type_full                      );
            //cmd.Parameters.AddWithValue("@city_district"          , address.data.city_district                                );


             


            cmd.Parameters.AddWithValue("@settlement_fias_id"     , address.data.settlement_fias_id                           );
            cmd.Parameters.AddWithValue("@settlement_kladr_id"    , address.data.settlement_kladr_id                          );
            cmd.Parameters.AddWithValue("@settlement_with_type"   , address.data.settlement_with_type                         );
            cmd.Parameters.AddWithValue("@settlement"             , address.data.settlement                                   );
            cmd.Parameters.AddWithValue("@street_fias_id"         , address.data.street_fias_id                               );
            cmd.Parameters.AddWithValue("@street_kladr_id"        , address.data.street_kladr_id                              );
            cmd.Parameters.AddWithValue("@street_with_type"       , address.data.street_with_type                             );
            cmd.Parameters.AddWithValue("@street"                 , address.data.street                                       );
            cmd.Parameters.AddWithValue("@house_fias_id"          , address.data.house_fias_id                                );
            cmd.Parameters.AddWithValue("@house_kladr_id"         , address.data.house_kladr_id                               );
            cmd.Parameters.AddWithValue("@house_with_type"        , address.data.house_with_type                              );
            cmd.Parameters.AddWithValue("@house_type"             , address.data.house_type                                   );
            cmd.Parameters.AddWithValue("@house"                  , address.data.house                                        );
            cmd.Parameters.AddWithValue("@block_type_full"        , address.data.block_type_full                              );
            cmd.Parameters.AddWithValue("@block"                  , address.data.block                                        );
            cmd.Parameters.AddWithValue("@flat_fias_id"           , address.data.flat_fias_id                                 );
            cmd.Parameters.AddWithValue("@flat_type_full"         , address.data.flat_type_full                               );
            cmd.Parameters.AddWithValue("@flat"                   , address.data.flat                                         );
            cmd.Parameters.AddWithValue("@geo_lat"                , address.data.geo_lat                                      );
            cmd.Parameters.AddWithValue("@geo_lon"                , address.data.geo_lon                                      );
            cmd.Parameters.AddWithValue("@fias_id"                , address.data.fias_id                                      );
            cmd.Parameters.AddWithValue("@fias_level"             , address.data.fias_level                                   );
            cmd.Parameters.AddWithValue("@qc_geo"                 , address.data.qc_geo                                       );
            cmd.Parameters.AddWithValue("@fias_actuality_state"   , address.data.fias_actuality_state                         );
            cmd.Parameters.AddWithValue("@oktmo"                  , address.data.oktmo                                        );
            cmd.Parameters.AddWithValue("@okato"                  , address.data.okato                                        );
            cmd.Parameters.AddWithValue("@division"               , address.data.region                                       );

        
        
        }






        public static Suggestion<Address> ReadAddress(SqlDataReader r,out string guid,out string Owner_guid)
        {
            var address =new Suggestion<Address>();
            int i                             = 0;
            address.data = new Address();
           guid                              = r.GetValue(i++).ToString();
           Owner_guid                        = r.GetValue(i++).ToString();
            i--;
           if(!r.IsDBNull(++i)) address.value                              = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.unrestricted_value                 = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.postal_code                   = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.region_fias_id                = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.region_kladr_id               = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.region                        = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.region_with_type              = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.area_fias_id                  = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.area_kladr_id                 = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.area                          = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.area_with_type                = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.sub_area_fias_id              = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.sub_area_kladr_id             = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.sub_area_with_type            = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.sub_area                      = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.city_fias_id                  = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.city_kladr_id                 = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.city                          = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.city_with_type                = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.settlement_fias_id            = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.settlement_kladr_id           = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.settlement_with_type          = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.settlement                    = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.street_fias_id                = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.street_kladr_id               = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.street_with_type              = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.street                        = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.house_fias_id                 = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.house_kladr_id                = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.house_type                    = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.house                         = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.block_type_full               = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.block                         = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.flat_fias_id                  = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.flat_type_full                = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.flat                          = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.geo_lat                       = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.geo_lon                       = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.fias_id                       = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.fias_level                    = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.qc_geo                        = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.fias_actuality_state          = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.oktmo                         = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.okato                         = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.region                        = r.GetValue(i).ToString();
           if(!r.IsDBNull(++i)) address.data.house_with_type               = r.GetValue(i).ToString();

           //if(!r.IsDBNull(++i)) address.data.city_district_fias_id         = r.GetValue(i).ToString();//Добавлено 04.03.2024 Удалено тогдаже xd
           //if (!r.IsDBNull(++i)) address.data.city_district_kladr_id        = r.GetValue(i).ToString();
           //if(!r.IsDBNull(++i)) address.data.city_district_with_type       = r.GetValue(i).ToString();
           //if(!r.IsDBNull(++i)) address.data.city_district_type            = r.GetValue(i).ToString();
           //if(!r.IsDBNull(++i)) address.data.city_district_type_full       = r.GetValue(i).ToString();
           //if(!r.IsDBNull(++i)) address.data.city_district                 = r.GetValue(i).ToString();


            Log.Json(address);
            return address;

        }
        public static string CreateTableScript(string tableName, DataTable table)
        {
            string sqlsc;
            sqlsc = "CREATE TABLE [dbo].[" + tableName + "](";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            var res = sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";

            return res;
        }
        public static void SetKaPartAddressIds(Suggestion<Address> addr, string kaGuid)
        {
            //TODO
        }
        public static ConcurrentDictionary<string,string> KaIgnoreList = new ConcurrentDictionary<string,string>();
        public class BinManClientTask : ClientData
        {
            public string KA_DbGuid;
            public BinManTaskType Type;
            public string AddressGuid;
            public string FactAddressGuid;
            public string AddressString;
            public string FactAddressString;
            
        }
        /// <summary>
        /// it Also call DADATA for address if it wasn't present, and fill it(addres) in db accordingly address type (fact, address)
        /// </summary>
        /// <returns>Complete data, also with filled addresses</returns>
        public static List<BinManClientTask> GetBinManClientsTaskList()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_GetClientsSyncList";
                var cmd = new SqlCommand(Query, _con);
                
                cmd.CommandType = CommandType.StoredProcedure;
                var res = new List<BinManClientTask>();
                using (var r = SQL.StartRead(cmd))
                {
                    
                    while (r.Read())
                    {
                        
                        BinManClientTask cd = new BinManClientTask();

                        cd.KA_DbGuid = r.GetValue(0).ToString();
                        if (KaIgnoreList.ContainsKey(cd.KA_DbGuid)) continue;
                        cd.Form = Enum.Parse <ClientType> ( r.GetValue(1).ToString());
                        cd.type_Code = r.GetValue(2).ToString();

                        switch (cd.Form)
                        {
                            case ClientType.INDIVIDUAL:

                                if (r.IsDBNull(4)) { Log.Error($"!!! Client `{cd.KA_DbGuid}` of form-type INDIVIDUAL has null F "); KaIgnoreList.TryAdd(cd.KA_DbGuid,"F is null"); continue; }
                                cd.F_SURNAME = r.GetValue(4).ToString();

                                if (r.IsDBNull(5)) { Log.Error($"!!! Client `{cd.KA_DbGuid}` of form-type INDIVIDUAL has null I "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "I is null"); continue; }
                                cd.F_NAME = r.GetValue(5).ToString();

                                if (!r.IsDBNull(6)) cd.F_PATRONYMIC = r.GetValue(6).ToString();
                                if (!r.IsDBNull(12)) cd.PASSPORT_CODE = r.GetValue(12).ToString();
                                if (!r.IsDBNull(13)) cd.PASSPORT_CODE = r.GetValue(13).ToString();

                                break;
                            case ClientType.U:
                            case ClientType.MANAGEMENT_COMPANY:

                                if (r.IsDBNull(7)) { Log.Error($"!!! Client `{cd.KA_DbGuid}` of form-type {Enum.GetName(cd.Form)} has null Title and Title_small "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "Title and Title_small is null"); continue; }
                                cd.UR_NAME = r.GetValue(7).ToString();

                                if (!r.IsDBNull(8)) cd.UR_FULLNAME = r.GetValue(8).ToString();
                                if (!r.IsDBNull(9)) cd.UR_OGRN = r.GetValue(9).ToString();
                                if (!r.IsDBNull(10)) cd.UR_KPP = r.GetValue(10).ToString();
                                if (!r.IsDBNull(11)) cd.UR_REG_DATE = r.GetValue(11).ToString();

                                break;
                            default:
                                break;
                        }


                        if (r.IsDBNull(14))
                        {
                            if (cd.Form != ClientType.INDIVIDUAL)
                            {
                                Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null INN ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "INN is null"); continue;
                            }
                        }
                        else
                        {
                            cd.INN = r.GetValue(14).ToString();
                        }

                        if (!r.IsDBNull(15))
                        {
                            cd.AddressGuid = r.GetValue(15).ToString();
                            cd.address = SQL.GetAddressInfo(cd.AddressGuid);
                        }

                        if (r.IsDBNull(17))
                        {
                            if (string.IsNullOrEmpty(cd.AddressGuid)) {
                                Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id address and address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "Address Fully  null"); continue;
                            }
                        }
                        else
                        {
                            cd.AddressString = r.GetValue(17).ToString();
                            
                            if (cd.address == null)
                            {
                                if (DadataApi.TryFindAddressByAddress(cd.AddressString, out cd.address))
                                {
                                    //var Query3 = "CrateMate_SetKaPartAddress";
                                    //var cmd3 = new SqlCommand(Query3, _con);
                                    //var guid = Guid.NewGuid().ToString();

                                    //SQL.InsertNewAddress(cd.address, guid, cd.KA_DbGuid,true);

                                    //cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                                    //cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                    //cmd3.Parameters.AddWithValue("@isItLiveAddress", true);

                                    //cmd3.CommandType = CommandType.StoredProcedure;

                                    //SQL.Execute(cmd3);
                                }
                                else
                                {
                                    
                                        Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id address and address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "Address Fully  null"); continue;
                                    
                                }
                            }

                        }
                        if (!r.IsDBNull(16))
                        {
                            cd.FactAddressGuid = r.GetValue(16).ToString();
                            cd.factAddress = SQL.GetAddressInfo(cd.FactAddressGuid);
                        }

                        if (r.IsDBNull(18))
                        {
                           // if (string.IsNullOrEmpty(cd.FactAddressGuid))
                            //    { Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id live_address and live_address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "live_Address Fully null"); continue; }
                           // else
                           // {
                           //if(cd.FactAddressGuid !=null)
                           //     cd.factAddress = SQL.GetOwnedAddressInfo(cd.FactAddressGuid);
                            //else
                            //{
                            //    cd.factAddress = cd.address;
                            //}
                           // }
                        }
                        else
                        {
                            cd.FactAddressString = r.GetValue(18).ToString();
                            if(cd.factAddress == null)
                            { 
                                if(DadataApi.TryFindAddressByAddress(cd.FactAddressString, out cd.factAddress))
                                {
                                    //var Query3 = "CrateMate_SetKaPartAddress";
                                    //var cmd3 = new SqlCommand(Query3, _con);
                                    //var guid = Guid.NewGuid().ToString();
                                    //SQL.InsertNewAddress(cd.factAddress, guid, cd.KA_DbGuid,true);

                                    //cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                                    //cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                    //cmd3.Parameters.AddWithValue("@isItLiveAddress", true);

                                    //cmd3.CommandType = CommandType.StoredProcedure;

                                    //SQL.Execute(cmd3);
                                }
                                else
                                {

                                    //Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id address and address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "FactAddress Fully  null"); continue;

                                }
                            }
                        }

                        bool Contacts = false;
                        if (r.IsDBNull(19)) { Log.Error("Тааак, кто -то пошаманил над процедурой BinMan_GecClientsSyncList .-. ");  } else
                           Contacts = r.GetBoolean(19);

                        if (Contacts )
                        {
                            var Query2 = "BinMan_GetKaContacts";
                            var cmd2 = new SqlCommand(Query2, _con);
                            cmd2.Parameters.AddWithValue("@KaGuid", cd.KA_DbGuid);
                            cmd2.CommandType = CommandType.StoredProcedure;

                            using (var r2= SQL.StartRead(cmd2))
                            {
                                var Phones = new List<string>();
                                var Emails = new List<string>();
                                while (r2.Read())
                                {
                                    var type = r2.GetValue(0).ToString();
                                    switch (type)
                                    {
                                        case "email":
                                            { if (!r2.IsDBNull(1)) Emails.Add(r2.GetValue(1).ToString()); break; }
                                        case "phone":
                                            { if (!r2.IsDBNull(1)) Phones.Add(r2.GetValue(1).ToString()); break; }
                                        default : { break; }
                                    }
                                }
                                cd.EMAIL = Emails.ToArray();
                                cd.PHONE = Phones.ToArray();
                            }

                        }
                        if(!r.IsDBNull(20))
                        cd.ID = r.GetValue(20).ToString();





                        res.Add(cd);
                        //if (res.Count >= 20)
                        //{
                        //    Log.Warning("Подозрательно много клиентов на обработку (>=20) , система приостановлена на 30 минут ");
                        //    Thread.Sleep(1000 * 60 * 30);
                        //}
                    }

                    
                }
                foreach(var cd in res)
                {

                        if (cd.address != null && string.IsNullOrEmpty(cd.AddressGuid))
                        {

                                var Query3 = "CrateMate_SetKaPartAddress";
                                var cmd3 = new SqlCommand(Query3, _con);
                                var guid = Guid.NewGuid().ToString();

                                SQL.InsertNewAddress(cd.address, guid, cd.KA_DbGuid, true);

                                cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                                cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                cmd3.Parameters.AddWithValue("@isItLiveAddress", false);

                                cmd3.CommandType = CommandType.StoredProcedure;

                                SQL.Execute(cmd3);
                            
                        }

                        if (cd.factAddress != null && string.IsNullOrEmpty(cd.FactAddressGuid))
                        {

                                var Query3 = "CrateMate_SetKaPartAddress";
                                var cmd3 = new SqlCommand(Query3, _con);
                                var guid = Guid.NewGuid().ToString();
                                SQL.InsertNewAddress(cd.factAddress, guid, cd.KA_DbGuid, true);

                                cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                                cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                cmd3.Parameters.AddWithValue("@isItLiveAddress", true);

                                cmd3.CommandType = CommandType.StoredProcedure;

                                SQL.Execute(cmd3);
                            
                        }
                    
                }
                return res;
            }
        }
      public enum BinManOperationStatusString
        {
            OK,
            Failed
        }
        public static void BinManMarkClientSucces(string ka_guid,string ka_binid, BinManOperationStatusString status)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_MarkKaSuccesInserted";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;





                cmd.Parameters.AddWithValue("@db_guid", ka_guid);
                cmd.Parameters.AddWithValue("@binman_id", ka_binid);
                cmd.Parameters.AddWithValue("@status", Enum.GetName(status));


                SQL.Execute(cmd);

            }
        }
        public static List<GeoContainer> GetGeozoneContainers(string geoGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_GetGeozoneContainers";
                var cmd = new SqlCommand(Query, _con);
                cmd.Parameters.AddWithValue("@geoGuid", geoGuid);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var r2 = SQL.StartRead(cmd))
                {
                    var conts = new List<GeoContainer>();
                    while (r2.Read())
                    {
                        var cc = new GeoContainer();
                        cc.guid = r2.GetValue(0).ToString();
                        cc.volume = float.Parse(r2.GetValue(1).ToString());
                        cc.typeGuid = r2.GetValue(2).ToString();
                        conts.Add(cc);
                    }
                    return conts;
                }

            }
        }
        public static void InsertBinManDocParse(BinDocDetails det,string client_guid, string docGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsertDogParse";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;





                cmd.Parameters.AddWithValue("@id", docGuid);
                cmd.Parameters.AddWithValue("@bin_id", det.binid);
                cmd.Parameters.AddWithValue("@client_id", client_guid);
                cmd.Parameters.AddWithValue("@ndoc", det.ndoc);
                cmd.Parameters.AddWithValue("@dateBegin", det.dateBegin);
                cmd.Parameters.AddWithValue("@dateEnd", det.dateEnd);
                cmd.Parameters.AddWithValue("@dateAccept", det.dateAccept);
                cmd.Parameters.AddWithValue("@Type", det.Type);
                cmd.Parameters.AddWithValue("@Group", det.Group);

                SQL.Execute(cmd);

            }
        }       
        public static void InsertBinManClientParse(BinClientInfo clientInfo,string guid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsertClientParse";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;



                cmd.Parameters.AddWithValue("@Id               ", guid);
                cmd.Parameters.AddWithValue("@binId            ", clientInfo.BinId     );
                cmd.Parameters.AddWithValue("@name             ", clientInfo.name      );
                cmd.Parameters.AddWithValue("@fullname         ", clientInfo.fullname );
                cmd.Parameters.AddWithValue("@inn              ", clientInfo.inn   );
                cmd.Parameters.AddWithValue("@ogrn             ", clientInfo.ogrn);
                cmd.Parameters.AddWithValue("@kpp              ", clientInfo.kpp      );
                cmd.Parameters.AddWithValue("@regDate          ", clientInfo.regDate     );
                cmd.Parameters.AddWithValue("@UrAddress        ", clientInfo.UrAddress     );
                cmd.Parameters.AddWithValue("@UrAddresIndex    ", clientInfo.UrAddressIndex     );
                cmd.Parameters.AddWithValue("@UrAddressFias    ", clientInfo.UrFiasCode     );
                cmd.Parameters.AddWithValue("@FactAddress      ", clientInfo.factAddress     );
                cmd.Parameters.AddWithValue("@FactAddressIndex ", clientInfo.factAddressIndex     );
                cmd.Parameters.AddWithValue("@FactAddressFias  ", clientInfo.FacFiasCode     );
                cmd.Parameters.AddWithValue("@phone            ", clientInfo.Phone     );
                cmd.Parameters.AddWithValue("@Email            ", clientInfo.Email     );
                cmd.Parameters.AddWithValue("@Bik              ", clientInfo.BIK     );
                cmd.Parameters.AddWithValue("@BankName         ", clientInfo.bankName     );
                cmd.Parameters.AddWithValue("@RasSchet         ", clientInfo.rasSchet     );
                cmd.Parameters.AddWithValue("@KorSchet         ", clientInfo.corSchet     );
                cmd.Parameters.AddWithValue("@Ruk              ", clientInfo.rukov     );
                cmd.Parameters.AddWithValue("@RukPosition      ", clientInfo.rukovPosition     );
                cmd.Parameters.AddWithValue("@OKPO             ", clientInfo.OKPO     );
                cmd.Parameters.AddWithValue("@OKVED"            , clientInfo.OKVED     );
                cmd.Parameters.AddWithValue("@Passport"         , clientInfo.Passport     );
                cmd.Parameters.AddWithValue("@Fio"              , clientInfo.Fio     );

                



                SQL.Execute(cmd);

            }
        }
        public static void UpdateDogBinManObjectLoadStatus(string docGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "Update dog set [WaitToLoadObjFromBinman]=0 where [id]=@id";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", docGuid);
                SQL.Execute(cmd);

            }
        }
        public static void InsertBinManObjectsParse(DocObject doc, string docGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_InsetObjectParse";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                string guid = Guid.NewGuid().ToString();

                cmd.Parameters.AddWithValue("@id", guid);
                cmd.Parameters.AddWithValue("@dog_id", docGuid);
                cmd.Parameters.AddWithValue("@binid", doc.binid);
                cmd.Parameters.AddWithValue("@name", doc.name);
                cmd.Parameters.AddWithValue("@addres", doc.address);
                cmd.Parameters.AddWithValue("@lot", doc.lot);
                cmd.Parameters.AddWithValue("@people", doc.people);
                cmd.Parameters.AddWithValue("@tax", doc.tax);
                cmd.Parameters.AddWithValue("@taxSumm", doc.taxSumm);
                cmd.Parameters.AddWithValue("@Graphic", doc.Graphic);
                cmd.Parameters.AddWithValue("@Container", doc.Container);
                cmd.Parameters.AddWithValue("@PerioudFrom", doc.PeriodFrom);
                cmd.Parameters.AddWithValue("@PerioudTo", doc.PeriodTo);
                if (doc.type != null) {
                    cmd.Parameters.AddWithValue("@objectType", doc.type.cattegory.ToString()); ;
                    cmd.Parameters.AddWithValue("@objectSubType", doc.type.subCattegory.ToString());
                }


                Log.sql(cmd);
                SQL.Execute(cmd);

                foreach (var v in doc.changes)
                {


                    Query = "BinMan_InsertObjectParseHistory";
                    cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // @id_Dogovor

                    cmd.Parameters.AddWithValue("@head_Id", guid);
                    cmd.Parameters.AddWithValue("@date_insert", v.date);
                    cmd.Parameters.AddWithValue("@status", v.status);
                    cmd.Parameters.AddWithValue("@tax", v.tarif_full_text);
                    cmd.Parameters.AddWithValue("@tax_info", v.tarif_info);
                    cmd.Parameters.AddWithValue("@people", v.people);
                    cmd.Parameters.AddWithValue("@people_detail", v.people_reason);
                    cmd.Parameters.AddWithValue("@summ", v.summ);
                    cmd.Parameters.AddWithValue("@graphy", v.graphy);
                    cmd.Parameters.AddWithValue("@container", v.container);
                    cmd.Parameters.AddWithValue("@PeriodFrom", v.PeriodFrom);
                    cmd.Parameters.AddWithValue("@PeriodTo", v.PeriodTo);
                    Log.sql(cmd);
                    SQL.Execute(cmd);
                }

            }
        
        }

        public static void CompleteBinParse(string DocGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_EndDogParse";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;





                cmd.Parameters.AddWithValue("@id_temp_dog", DocGuid);


                SQL.Execute(cmd);

            }
        }
        public static List<User> GetUsersList()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetUserList";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;




                var res = new List<User>(125);
               // cmd.Parameters.AddWithValue("@Ndoc", NDoc);

                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var u = new User();
                        u.guid = r.GetValue(0).ToString();
                        if (!r.IsDBNull(1)) u.fio = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2)) u.fio_short = r.GetValue(2).ToString();
                        if (!r.IsDBNull(3)) u.Division_guid = r.GetValue(3).ToString(); 
                        res.Add(u);
                    }
                    return res;
                    
                }


            }
        }
        public static List<Division> GetDivisionList()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetDivisionList";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;




                var res = new List<Division>(10);
                // cmd.Parameters.AddWithValue("@Ndoc", NDoc);

                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var d = new Division();
                        d.guid = r.GetValue(0).ToString();
                        if (!r.IsDBNull(1)) d.title = r.GetValue(1).ToString();
                        res.Add(d);
                    }
                    return res;

                }


            }
        }
        public static string GetDocumentBinId(string NDoc)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "SELECT d.id_Dogovor FROM Dog d WHERE d.NDoc=@Ndoc";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;





                cmd.Parameters.AddWithValue("@Ndoc", NDoc);

                using (var r = SQL.StartRead(cmd))
                {
                    r.Read();
                    if (!r.HasRows) return string.Empty;
                    return r.GetValue(0).ToString(); 
                }
                

            }
        }
        public static void DocParseRequestJ(string request,string UserGuid,string docGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_DogUpdateRequestJ";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;





                cmd.Parameters.AddWithValue("@user", UserGuid);
                cmd.Parameters.AddWithValue("@request", request);
                cmd.Parameters.AddWithValue("@doc_guid", docGuid);


                SQL.Execute(cmd);

            }
        }

        public static string LoadFullBinmanObject(DocObject doc, string docId)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "LoadBinmanHeadObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                string guid = Guid.NewGuid().ToString();

                cmd.Parameters.AddWithValue("@id", guid);
                cmd.Parameters.AddWithValue("@name", doc.name);
                cmd.Parameters.AddWithValue("@addres", doc.address);
                cmd.Parameters.AddWithValue("@lot", doc.lot);
                cmd.Parameters.AddWithValue("@people", doc.people);
                cmd.Parameters.AddWithValue("@tax", doc.tax);
                cmd.Parameters.AddWithValue("@taxSumm", doc.taxSumm);
                cmd.Parameters.AddWithValue("@Graphic", doc.Graphic);
                cmd.Parameters.AddWithValue("@Container", doc.Container);
                cmd.Parameters.AddWithValue("@PerioudFrom", doc.PeriodFrom);
                cmd.Parameters.AddWithValue("@PerioudTo", doc.PeriodTo);
                cmd.Parameters.AddWithValue("@id_Dogovor", docId);

                Log.sql(cmd);
                SQL.Execute(cmd);

                foreach (var v in doc.changes)
                {


                    Query = "LoadBinmanHistoryObj";
                    cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // @id_Dogovor

                    cmd.Parameters.AddWithValue("@id_head_obj", guid);
                    cmd.Parameters.AddWithValue("@date_insert", v.date);
                    cmd.Parameters.AddWithValue("@status", v.status);
                    cmd.Parameters.AddWithValue("@tax", v.tarif_full_text);
                    cmd.Parameters.AddWithValue("@tax_info", v.tarif_info);
                    cmd.Parameters.AddWithValue("@people", v.people);
                    cmd.Parameters.AddWithValue("@people_reason", v.people_reason);
                    cmd.Parameters.AddWithValue("@summ", v.summ);
                    cmd.Parameters.AddWithValue("@graphy", v.graphy);
                    cmd.Parameters.AddWithValue("@container", v.container);
                    cmd.Parameters.AddWithValue("@PeriodFrom", v.PeriodFrom);
                    cmd.Parameters.AddWithValue("@PeriodTo", v.PeriodTo);
                    Log.sql(cmd);
                    SQL.Execute(cmd);
                }

                return guid;
            }
        }
        public static ConcurrentDictionary<string,string> LinkIgnoreList = new ConcurrentDictionary<string, string>();
        public static List<GeoObjectLinkTask> GetBinManObjectGeozonesLinks()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "BinMan_GetObject2GeoLinksToSync";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<GeoObjectLinkTask> res = new List<GeoObjectLinkTask>();



                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {

                        var t = new GeoObjectLinkTask();
                        t.DbGuid = r.GetValue(0).ToString();
                        if (LinkIgnoreList.ContainsKey(t.DbGuid)) continue;
                        t.ObjectBinId = r.GetValue(1).ToString();
                        t.GeoBinId = r.GetValue(2).ToString();
                        t.TaskType = Enum.Parse<BinManTaskType>(r.GetValue(3).ToString());
                        //   t.DataBase_Guid = r.GetValue(0).ToString();
                        res.Add(t);
                    }
                }

                return res;

            }
        }

        public static void MarkDeletedBinManObjectGeozonesLink(GeoObjectLinkTask task, BinManGeozone.AttachResult status)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "BinMan_SyncDeleteObjGeoLinkStatus";

                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", task.DbGuid);
                cmd.Parameters.AddWithValue("@status", status switch
                {
                    BinManGeozone.AttachResult.OK => "Ok ?",
                    BinManGeozone.AttachResult.Failed => "Что-то пошло не так",
                    BinManGeozone.AttachResult.NotAttachedInBinman => "А она и не была прикреплена в binMan :P"
                });
                SQL.Execute(cmd);



            }
        }
        public static void MarkInsertedBinManObjectGeozonesLink(GeoObjectLinkTask task)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query ="BinMan_MarkObject2GeozoneLinkInserted";

                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@linkGuid", task.DbGuid);
                SQL.Execute(cmd);



            }
        }
        public static void InsertObjectPartAddress(Suggestion<Address> address, string ObjDbGuid,string Status)
        {
            using (SqlConnection _con2 = new SqlConnection(SqlconnectionString))
            {
                _con2.Open();

                var Query3 = "CrateMate_SetObjPartAddress";
                var cmd3 = new SqlCommand(Query3, _con2);
                var guid = Guid.NewGuid().ToString();
                if(address !=null)
                SQL.InsertNewAddress(address, guid, ObjDbGuid, true);

                cmd3.Parameters.AddWithValue("@obj_Guid", ObjDbGuid);
                cmd3.Parameters.AddWithValue("@partAddr_id",(address==null?DBNull.Value: guid));
                cmd3.Parameters.AddWithValue("@status", Status);

                cmd3.CommandType = CommandType.StoredProcedure;

                SQL.Execute(cmd3);
            }
        }
        public static List<BinManClientTask> GetClientsFailedListToFixIfTheyCreated()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "BinMan_GetClientsFailedListToFixIfTheyCreated";
                var cmd = new SqlCommand(Query, _con);

                cmd.CommandType = CommandType.StoredProcedure;
                var res = new List<BinManClientTask>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {

                        BinManClientTask cd = new BinManClientTask();

                        cd.KA_DbGuid = r.GetValue(0).ToString();
                        if (KaIgnoreList.ContainsKey(cd.KA_DbGuid)) continue;
                        cd.Form = Enum.Parse<ClientType>(r.GetValue(1).ToString());
                        cd.type_Code = r.GetValue(2).ToString();

                        switch (cd.Form)
                        {
                            case ClientType.INDIVIDUAL:

                                if (r.IsDBNull(4)) { Log.Error($"!!! Client `{cd.KA_DbGuid}` of form-type INDIVIDUAL has null F "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "F is null"); continue; }
                                cd.F_SURNAME = r.GetValue(4).ToString();

                                if (r.IsDBNull(5)) { Log.Error($"!!! Client `{cd.KA_DbGuid}` of form-type INDIVIDUAL has null I "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "I is null"); continue; }
                                cd.F_NAME = r.GetValue(5).ToString();

                                if (!r.IsDBNull(6)) cd.F_PATRONYMIC = r.GetValue(6).ToString();
                                if (!r.IsDBNull(12)) cd.PASSPORT_CODE = r.GetValue(12).ToString();
                                if (!r.IsDBNull(13)) cd.PASSPORT_CODE = r.GetValue(13).ToString();

                                break;
                            case ClientType.U:
                            case ClientType.MANAGEMENT_COMPANY:

                                if (r.IsDBNull(7)) { Log.Error($"!!! Client `{cd.KA_DbGuid}` of form-type {Enum.GetName(cd.Form)} has null Title and Title_small "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "Title and Title_small is null"); continue; }
                                cd.UR_NAME = r.GetValue(7).ToString();

                                if (!r.IsDBNull(8)) cd.UR_FULLNAME = r.GetValue(8).ToString();
                                if (!r.IsDBNull(9)) cd.UR_OGRN = r.GetValue(9).ToString();
                                if (!r.IsDBNull(10)) cd.UR_KPP = r.GetValue(10).ToString();
                                if (!r.IsDBNull(11)) cd.UR_REG_DATE = r.GetValue(11).ToString();

                                break;
                            default:
                                break;
                        }


                        if (r.IsDBNull(14))
                        {
                            if (cd.Form != ClientType.INDIVIDUAL)
                            {
                                Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null INN ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "INN is null"); continue;
                            }
                        }
                        else
                        {
                            cd.INN = r.GetValue(14).ToString();
                        }

                        if (!r.IsDBNull(15))
                        {
                            cd.AddressGuid = r.GetValue(15).ToString();
                            cd.address = SQL.GetAddressInfo(cd.AddressGuid);
                        }

                        if (r.IsDBNull(17))
                        {
                            if (string.IsNullOrEmpty(cd.AddressGuid))
                            {
                                Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id address and address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "Address Fully  null"); continue;
                            }
                        }
                        else
                        {
                            cd.AddressString = r.GetValue(17).ToString();

                            if (cd.address == null)
                            {
                                if (DadataApi.TryFindAddressByAddress(cd.AddressString, out cd.address))
                                {
                                    //var Query3 = "CrateMate_SetKaPartAddress";
                                    //var cmd3 = new SqlCommand(Query3, _con);
                                    //var guid = Guid.NewGuid().ToString();

                                    //SQL.InsertNewAddress(cd.address, guid, cd.KA_DbGuid,true);

                                    //cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                                    //cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                    //cmd3.Parameters.AddWithValue("@isItLiveAddress", true);

                                    //cmd3.CommandType = CommandType.StoredProcedure;

                                    //SQL.Execute(cmd3);
                                }
                                else
                                {

                                    Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id address and address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "Address Fully  null"); continue;

                                }
                            }

                        }
                        if (!r.IsDBNull(16))
                        {
                            cd.FactAddressGuid = r.GetValue(16).ToString();
                            cd.factAddress = SQL.GetAddressInfo(cd.FactAddressGuid);
                        }

                        if (r.IsDBNull(18))
                        {
                            // if (string.IsNullOrEmpty(cd.FactAddressGuid))
                            //    { Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id live_address and live_address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "live_Address Fully null"); continue; }
                            // else
                            // {
                            //if(cd.FactAddressGuid !=null)
                            //     cd.factAddress = SQL.GetOwnedAddressInfo(cd.FactAddressGuid);
                            //else
                            //{
                            //    cd.factAddress = cd.address;
                            //}
                            // }
                        }
                        else
                        {
                            cd.FactAddressString = r.GetValue(18).ToString();
                            if (cd.factAddress == null)
                            {
                                if (DadataApi.TryFindAddressByAddress(cd.FactAddressString, out cd.factAddress))
                                {
                                    //var Query3 = "CrateMate_SetKaPartAddress";
                                    //var cmd3 = new SqlCommand(Query3, _con);
                                    //var guid = Guid.NewGuid().ToString();
                                    //SQL.InsertNewAddress(cd.factAddress, guid, cd.KA_DbGuid,true);

                                    //cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                                    //cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                    //cmd3.Parameters.AddWithValue("@isItLiveAddress", true);

                                    //cmd3.CommandType = CommandType.StoredProcedure;

                                    //SQL.Execute(cmd3);
                                }
                                else
                                {

                                    //Log.Error($"!!! Client `{cd.KA_DbGuid}`  has null id address and address ! "); KaIgnoreList.TryAdd(cd.KA_DbGuid, "FactAddress Fully  null"); continue;

                                }
                            }
                        }

                        bool Contacts = false;
                        if (r.IsDBNull(19)) { Log.Error("Тааак, кто -то пошаманил над процедурой BinMan_GecClientsSyncList .-. "); }
                        else
                            Contacts = r.GetBoolean(19);

                        if (Contacts)
                        {
                            var Query2 = "BinMan_GetKaContacts";
                            var cmd2 = new SqlCommand(Query2, _con);
                            cmd2.Parameters.AddWithValue("@KaGuid", cd.KA_DbGuid);
                            cmd2.CommandType = CommandType.StoredProcedure;

                            using (var r2 = SQL.StartRead(cmd2))
                            {
                                var Phones = new List<string>();
                                var Emails = new List<string>();
                                while (r2.Read())
                                {
                                    var type = r2.GetValue(0).ToString();
                                    switch (type)
                                    {
                                        case "email":
                                            { if (!r2.IsDBNull(1)) Emails.Add(r2.GetValue(1).ToString()); break; }
                                        case "phone":
                                            { if (!r2.IsDBNull(1)) Phones.Add(r2.GetValue(1).ToString()); break; }
                                        default: { break; }
                                    }
                                }
                                cd.EMAIL = Emails.ToArray();
                                cd.PHONE = Phones.ToArray();
                            }

                        }
                        if (!r.IsDBNull(20))
                            cd.ID = r.GetValue(20).ToString();





                        res.Add(cd);
                        //if (res.Count >= 20)
                        //{
                        //    Log.Warning("Подозрательно много клиентов на обработку (>=20) , система приостановлена на 30 минут ");
                        //    Thread.Sleep(1000 * 60 * 30);
                        //}
                    }


                }
                foreach (var cd in res)
                {

                    if (cd.address != null && string.IsNullOrEmpty(cd.AddressGuid))
                    {

                        var Query3 = "CrateMate_SetKaPartAddress";
                        var cmd3 = new SqlCommand(Query3, _con);
                        var guid = Guid.NewGuid().ToString();

                        SQL.InsertNewAddress(cd.address, guid, cd.KA_DbGuid, true);

                        cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                        cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                        cmd3.Parameters.AddWithValue("@isItLiveAddress", false);

                        cmd3.CommandType = CommandType.StoredProcedure;

                        SQL.Execute(cmd3);

                    }

                    if (cd.factAddress != null && string.IsNullOrEmpty(cd.FactAddressGuid))
                    {

                        var Query3 = "CrateMate_SetKaPartAddress";
                        var cmd3 = new SqlCommand(Query3, _con);
                        var guid = Guid.NewGuid().ToString();
                        SQL.InsertNewAddress(cd.factAddress, guid, cd.KA_DbGuid, true);

                        cmd3.Parameters.AddWithValue("@ka_Guid", cd.KA_DbGuid);
                        cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                        cmd3.Parameters.AddWithValue("@isItLiveAddress", true);

                        cmd3.CommandType = CommandType.StoredProcedure;

                        SQL.Execute(cmd3);

                    }

                }
                return res;
            }
        }
        public static ConcurrentDictionary<string,string> IgnoredObjectsId = new ConcurrentDictionary<string, string>();

        public static List<BinManObjectData> GetObjectListToSyncBinMan(bool ПофигНаАдрес = false)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "BinMan_GetObjectListToSync";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<BinManObjectData> res = new List<BinManObjectData>();



                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var t = new BinManObjectData();

                        if (r.IsDBNull(9)) continue;
                        t.DataBase_Guid = r.GetValue(0).ToString();
                        if (IgnoredObjectsId.ContainsKey(t.DataBase_Guid)) continue;
                        

                        t.BinId = r.GetValue(1).ToString();
                        t.NAME = r.GetValue(2).ToString();
                        t.CATEGORY = (ObjectMainCattegory)int.Parse(r.GetValue(5).ToString());
                        t.SUBCATTEGORY = (ObjectSubCattegory)int.Parse(r.GetValue(6).ToString());
                        t.lon = (float)r.GetDouble(4);
                        t.lat = (float)r.GetDouble(3);
                        
                        if (!r.IsDBNull(8) && !r.IsDBNull(7))
                        {
                            var log = r.GetValue(7).ToString();
                            var pass = r.GetValue(8).ToString();

                            t.ld = BinManApi.GetCustomAccount(log,pass);
                        }
                       
                        t.CLIENTID = r.GetValue(9).ToString();
                        var pa_guid = string.Empty;
                        var addr = "";
                        if (!r.IsDBNull(10))
                            pa_guid = r.GetValue(10).ToString();
                        if (!r.IsDBNull(11))
                            addr = r.GetValue(11).ToString();

                        t.RawAddress = addr;
                        t.Address_dbGuid = pa_guid;

                        res.Add(t);
              
                    }
                }
                for (int i = res.Count-1; i >=0 ; i--)
                {
                    var v = res[i];
                  //  foreach (var v in res)
                   // {
                        if (string.IsNullOrEmpty(v.Address_dbGuid))
                        {
                            if (DadataApi.TryFindAddressByAddress(v.RawAddress, out v.address))
                            {
                                //Th
                                //var tsk =   Task.УдалитьВсеНе0Начисления(() =>
                                // {
                                using (SqlConnection _con2 = new SqlConnection(SqlconnectionString))
                                {
                                    _con2.Open();

                                    var Query3 = "CrateMate_SetObjPartAddress";
                                    var cmd3 = new SqlCommand(Query3, _con2);
                                    var guid = Guid.NewGuid().ToString();
                                    SQL.InsertNewAddress(v.address, guid, v.DataBase_Guid, true);

                                    cmd3.Parameters.AddWithValue("@obj_Guid", v.DataBase_Guid);
                                    cmd3.Parameters.AddWithValue("@partAddr_id", guid);
                                    cmd3.Parameters.AddWithValue("@status", "OK");

                                    cmd3.CommandType = CommandType.StoredProcedure;

                                    SQL.Execute(cmd3);
                                }
                                //  });
                                ///  tsk.Wait();
                            }
                        }
                        else
                        {
                            v.address = SQL.GetAddressInfo(v.Address_dbGuid);
                        }

                        if (!ПофигНаАдрес && (v.address == null)) { Log.Error($"!!! Object `{v.DataBase_Guid}`  has null id address and address ! "); KaIgnoreList.TryAdd(v.DataBase_Guid, "Address Fully  null"); res.Remove(v); continue; }

                   // }
                }
                //foreach(var t in res)
                //{

                //}
                return res;

            }
        }
        public static void MarkGeozoneBinmanArchived(BinManGeozoneTask gd)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "BinMan_SetGeozoneIsArch";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;



                cmd.Parameters.AddWithValue("@id_geo", gd.DataBase_Guid);
                cmd.Parameters.AddWithValue("@is_arch", gd.IsArchive);


                SQL.Execute(cmd);

            }
        }
        public static void MarkGeozoneBinmanArchived(string LogFilePath)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var txt = File.ReadAllText(LogFilePath);
                var splts = txt.Split("[StoredProcedure] CrateMate_UpdateGeo");

                for(int i = 1; i < splts.Length; i++)
                {
                    var DatePart = splts[i-1].Substring(splts[i - 1].Length- "[29-08 16:22:33]".Length-2).Replace("[","").Trim();
                    if (DatePart.Split(":")[0].Length != 8) DatePart=DatePart.Replace(" ", "0");
                    DatePart = new System.String(DatePart.Where(Char.IsDigit).ToArray());
                    var resDate = new DateTime(2023, int.Parse(DatePart.Substring(2, 2)), int.Parse(DatePart.Substring(0, 2)), int.Parse(DatePart.Substring(4, 2)), int.Parse(DatePart.Substring(6, 2)), int.Parse(DatePart.Substring(8)));


                 //  var resDate = DateTime.ParseExact("2023" + new System.String(DatePart.Where(Char.IsDigit).ToArray()),"yyyyddMMHHmmSS", CultureInfo.InvariantCulture);

                    var comment = splts[i].Split("\n");
                    if (comment.Length >= 10)
                    {
                        var commentREs = comment[10];
                        commentREs = commentREs.Replace("@descr = ", "").Replace("\r","").Trim();
                        var geo_id = comment[1].Replace("@id = ","").Replace("\r", "").Trim();


                        var Query = "UPDATE j_Geozone set descr = @descr WHERE j_Geozone.DateOperation between DATEADD(SECOND,-2,@date) and DATEADD(SECOND,2,@date) and j_Geozone.id_geozone = @id_geo";
                        var cmd = new SqlCommand(Query, _con);
                        cmd.CommandType = CommandType.Text;



                        cmd.Parameters.AddWithValue("@descr", commentREs);
                        cmd.Parameters.AddWithValue("@date", resDate);
                        cmd.Parameters.AddWithValue("@id_geo", geo_id);


                        SQL.Execute(cmd);
                    };
                }


              


                //var descr = "asd";
                //var date = "daste";
                //var id = "@id_geo";



            }
        }
        public class Db_Object
        {
            public string guid;
            public double lon;
            public double lat;
            public string address;
        }
        public static Dictionary<string, Db_Object> SelectAllObjects()
        {
            
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "select o.Id,o.Adress,o.lon,o.lat From [Objects] o ";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;

                var res = new Dictionary<string, Db_Object>();
                int loading = 0;
                using(var r = SQL.StartRead(cmd))
                {
                    while (
                       
                        r.Read())
                    {
                        var obj = new Db_Object();

                        obj.guid = r.GetValue(0).ToString();
                        if (!r.IsDBNull(1)) obj.address = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2)) obj.lon = r.GetDouble(2);
                        if (!r.IsDBNull(3)) obj.lat = r.GetDouble(3);

                        res.Add(obj.guid, obj);
                        loading += 1;
                        Console.Title = $"Загрузка объектов {loading}";
                    }
                }
                return res;

               

            }
        }
        public static ConcurrentDictionary<string, string> DogIgnoreList = new ConcurrentDictionary<string,string>();
        public static List<BinManDogData> GetDogListToSincBinMan()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "CrateMate_GetDogBinManList";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                var res = new List<BinManDogData>();
                int loading = 0;
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var d = new BinManDogData();
                        d.Db_Guid =r.GetValue(0).ToString();

                        if (DogIgnoreList.ContainsKey(d.Db_Guid)) continue;

                        if (r.IsDBNull(1)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetDogBinManList .-. (Ka_binid (ClientID) is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid,"Ka_binid (ClientID) is null ?!");continue; } 
                        d.Client_BinManid = r.GetValue(1).ToString();
                        if (r.IsDBNull(2)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetDogBinManList .-. (Группа договора is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "Группа договора is null ?!"); continue; }
                        d.Group_BinManCode = r.GetValue(2).ToString();
                        if (r.IsDBNull(3)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetDogBinManList .-. (Тип договора is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "Тип договора is null ?!"); continue; }
                        d.Type_BinManCode = r.GetValue(3).ToString();

                        if (r.IsDBNull(4)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetDogBinManList .-. (DateBegin is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "DateBegin is null ?!"); continue; }
                        d.dateFrom = r.GetDateTime(4);
                        if (!r.IsDBNull(5)) d.dateTo = r.GetDateTime(5);
                        if (r.IsDBNull(6)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetDogBinManList .-. (Дата подписи is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "Дата подписи is null ?!"); continue; }
                        d.dateSign = r.GetDateTime(6);
                        if (!r.IsDBNull(7)) d.Number = r.GetValue(7).ToString();
                        if (!r.IsDBNull(8)) d.bin_id = r.GetValue(8).ToString();
                        res.Add(d);
                    }
                }
                return res;



            }
        }
        public static ConcurrentDictionary<string, string> IdDogObjLinksIgnoreList = new ConcurrentDictionary<string, string>();
        public static List<AttachObjectInfo> GetDogObjLinksListToSincBinMan()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "CrateMate_GetBinManDogObjList";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                var res = new List<AttachObjectInfo>();
                int loading = 0;
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var d = new AttachObjectInfo();
                        d.Db_Guid = r.GetValue(0).ToString();

                        if (DogIgnoreList.ContainsKey(d.Db_Guid)) continue;

                        if (r.IsDBNull(1)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetBinManDogObjList .-. (doc_BinManId is null ?!)"); IdDogObjLinksIgnoreList.TryAdd(d.Db_Guid, "Ka_binid (ClientID) is null ?!"); continue; }
                        d.doc_BinManId = r.GetValue(1).ToString();
                        if (r.IsDBNull(2)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetBinManDogObjList .-. (obj_BinManId is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "ObjBinId is null ?!"); continue; }
                        d.obj_BinManId = r.GetValue(2).ToString();
                        if (r.IsDBNull(3)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetBinManDogObjList .-. (tarif volume is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "tarif volume is null ?!"); continue; }
                        d.tarif_value = r.GetValue(3).ToString();

                        if (r.IsDBNull(4)) { Log.Error("Тааак, кто -то пошаманил над процедурой CrateMate_GetBinManDogObjList .-. (Tarif BinManCode is null ?!)"); DogIgnoreList.TryAdd(d.Db_Guid, "Tarif BinManCode  is null ?!"); continue; }
                        d.tarif_BinManCode = r.GetValue(4).ToString();
                        if (!r.IsDBNull(5)) d.activeFrom= r.GetDateTime(5);

                        res.Add(d);
                    }
                }
                return res;

                //https://binman.ru/cabinet/company/contracts/detail/5938134/?action=delete_story&story_id=4986649

            }
        }
        public static void MarkDogObjUpdated(string link_Guid,BinManOperationStatusString status)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "CrateMate_BinManUpdateDogObjLinkStatus";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<BinManGeozone> res = new List<BinManGeozone>();


                cmd.Parameters.AddWithValue("@status", Enum.GetName(status));
                cmd.Parameters.AddWithValue("@link_Id", link_Guid);



                SQL.Execute(cmd);

            }
        }
        public static void MarkDogUpdated (string dog_Guid, BinManOperationStatusString status, string bin_id,string nDoc)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "CrateMate_MarkDogInserted";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<BinManGeozone> res = new List<BinManGeozone>();

                cmd.Parameters.AddWithValue("@dog_Guid", dog_Guid);
                cmd.Parameters.AddWithValue("@bin_id", string.IsNullOrEmpty(bin_id)?DBNull.Value:bin_id);
                cmd.Parameters.AddWithValue("@status", Enum.GetName(status));
                cmd.Parameters.AddWithValue("@ndoc", string.IsNullOrEmpty(nDoc) ? DBNull.Value :nDoc);


                SQL.Execute(cmd);

            }
        }
        public static void MarkGeozoneBinmanUpdated(BinManGeozone gd)
        {
           using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

               // BuidAddress()

                var Query = "Cratemate_SuccessGeoUpdate";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<BinManGeozone> res = new List<BinManGeozone>();

                cmd.Parameters.AddWithValue("@id_geo", gd.DataBase_Guid);
                cmd.Parameters.AddWithValue("@BinAddress", gd.BuildAddress());


                SQL.Execute(cmd);

            }
        }
        public static List<BinManGeozone> GetBinmanGeozonesInsertList()
        {
           using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();



                var Query = "Binman_insertList";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<BinManGeozone> res = new List<BinManGeozone>();



                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var gd = new BinManGeozone()
                        {
                            DataBase_Guid = r.GetValue(0).ToString(),
                            LAST_AREA = long.Parse(r.GetValue(4).ToString()),
                            LAT = float.Parse(r.GetValue(1).ToString()),
                            LON = float.Parse(r.GetValue(2).ToString()),
                            NAME = r.GetValue(3).ToString(),
                            AREA_CANOPY = r.GetBoolean(6),
                            AREA_ENCLOSURE = r.GetBoolean(7),
                            AREA_BASIS = (r.IsDBNull(8) ? Geo_area_basis.grunt : BinManGeozone.Guid2Enum(r.GetValue(8).ToString()))
                        };
                        var gp = new GeoPoint(gd.LAT, gd.LON);
                        if (!IgnoreList.Contains(gp))
                        {
                            if (DadataApi.TryFillAddres(gd)) { res.Add(gd); }
                            else IgnoreList.Add(gp);
                        }
                    }
                }

                return res;

            }
        }
        public static void GeozoneAddBinId(string? guid, string genericName, int geozoneBin_Id)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "Binman_UpdateInsertedGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ID", guid);
                cmd.Parameters.AddWithValue("@binmanID", geozoneBin_Id);
                cmd.Parameters.AddWithValue("@Title", genericName);
                SQL.Execute(cmd);
            }
        }

        public static async void ContainerAddBinId(string guid, string containerBin_Id)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_UpdateContainer";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", guid);
                cmd.Parameters.AddWithValue("@binmanID", containerBin_Id);
                SQL.ExecuteAsync(cmd);
            }
        }
        public static void ContainerBinManResult(string guid, string containerBin_Id, ContainerStatus statusCode)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "BinMan_BinManContainerResult";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_container", guid);
                cmd.Parameters.AddWithValue("@binid", containerBin_Id);
                cmd.Parameters.AddWithValue("@statusCode", (int)statusCode);
                SQL.Execute(cmd);
            }
        }
        public static List<ContainerEnum> GetContainers()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                string Query = "CrateMate_GetTypeContainers";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;


                using (var r = StartRead(cmd))
                {
                    var res = new List<ContainerEnum>();
                    while (r.Read())
                    {
                        ContainerEnum ct = new ContainerEnum();

                        ct.name = r.GetString(1);
                        ct.guid = r.GetValue(0).ToString();
                        ct.shortName = r.GetString(3);

                        res.Add(ct);
                    }
                    return res;
                }

            }
        }
        public static void MoveGeozone(GeozoneMove move, string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "Cratemate_ChangeGeoPos";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@geo", move.guid);
                cmd.Parameters.AddWithValue("@lat", move.newPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", move.newPosition.mLongitude);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);



                SQL.Execute(cmd);
                SQL.InsertNewGeozoneEvents(move.guid, UserGuid, new UniversalEvent() { type = Enum.GetName(EventsList.update), description = move.commentary }, move.userPosition, move.oldPosition);
            }
        }
        /// <summary>
        /// All neded events generated on SQL Side
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="UserGuid"></param>
        public static void EditGeozone(GeozoneEdit edit, string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                if (!edit.newPosition.HasValue) edit.newPosition = edit.initialPosition;

                var Query = "CrateMate_UpdateGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                if(edit.detailAddress !=null)
                SQL.UpdateAddress(edit.detailAddress.info,null, edit.guid);

                //Geozone.GetContainerTypeGuidFromContainers(edit.co);

                var ToDelete = edit.containersEditActions.Where(x => x.action == ContainerEditAction.delete);

                var toInsert = edit.containersEditActions.Where(x => x.action == ContainerEditAction.insert);

                foreach (var v in ToDelete)
                {
                   // for (int i = 0; i < v.container.count - v.container.negcount; i++)
                        SQL.DeleteGeozoneContainers(edit.guid, v.container.negcount, v.container.volume, v.container.typeGuid, UserGuid);
                }
                foreach (var v in toInsert)
                {
                    v.container.guid = Guid.NewGuid().ToString();
                    for (int i = 0; i < v.container.count - v.container.negcount; i++)
                    {
                        
                        SQL.CreateGeozoneContainer(edit.guid, v.container, UserGuid, true);
                    }
                }

                //foreach (var v in edit.containersEditActions)
                //{
                //    switch (v.action)
                //    {
                //        case ContainerEditAction.delete:
                //            SQL.DeleteGeozoneContainers(edit.guid, v.container.negcount, v.container.volume, v.container.typeGuid,UserGuid);
                //            break;
                //        case ContainerEditAction.insert:
                //            v.container.guid = Guid.NewGuid().ToString();
                //            for(int i = 0; i < v.container.count-v.container.negcount;i++)
                //            SQL.CreateGeozoneContainer(edit.guid, v.container,UserGuid,true).Wait();
                //            break;
                //        default:
                //            break;
                //    }
                //}

                


                // Geozone gz = SQL.GetGeozone(edit.guid, _con);

                cmd.Parameters.AddWithValue("@id", edit.guid);
                //  cmd.Parameters.AddWithValue("@geo", edit.guid);
                cmd.Parameters.AddWithValue("@lat", edit.newPosition.Value.mLatitude);
                cmd.Parameters.AddWithValue("@lon", edit.newPosition.Value.mLongitude);
                cmd.Parameters.AddWithValue("@id_TypeGround", edit.GetBasementGuid());
                cmd.Parameters.AddWithValue("@roof", edit.roof);
                cmd.Parameters.AddWithValue("@fence", edit.fence);
                cmd.Parameters.AddWithValue("@gate", edit.gate);
                cmd.Parameters.AddWithValue("@id_typeGeozone", edit.geozoneGroup);
                cmd.Parameters.AddWithValue("@area", edit.area);
                cmd.Parameters.AddWithValue("@lot_id_District", string.IsNullOrEmpty(edit.lot) ? DBNull.Value : edit.lot);
                cmd.Parameters.AddWithValue("@descr", edit.commentary);
                cmd.Parameters.AddWithValue("@title", string.IsNullOrEmpty(edit.title) ? DBNull.Value : edit.title);
                cmd.Parameters.AddWithValue("@idUser", UserGuid);
                cmd.Parameters.AddWithValue("@id_subDistrict", string.IsNullOrEmpty(edit.subDistrict) ? DBNull.Value : edit.subDistrict);
                cmd.Parameters.AddWithValue("@id_subDistrictZone", string.IsNullOrEmpty(edit.subDistrictZone) ? DBNull.Value : edit.subDistrictZone);

                var addr = edit.detailAddress?.info.value;
                if (string.IsNullOrEmpty(addr)) addr = edit.address; 

                cmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(addr) ? DBNull.Value : addr);





                //TODO

                SQL.Execute(cmd);
                //SQL.InsertNewGeozoneEvents(move.guid, UserGuid, new UniversalEvent() { type = Enum.GetName(EventsList.update), description = move.commentary }, move.userPosition, move.oldPosition);
            }
        }
        public static string? GetDetailedTrimmedAdress(Suggestion<Address> da)
        {
            return da.unrestricted_value?
                .Replace(da.data.postal_code + ", ", "")
                .Replace(da.data.postal_code + ",", "")
                .Replace(da.data.region + ", ", "")
                .Replace(da.data.region + ",", "")
                .Trim();
        }
        public static bool TryGetEmailFromParse(string address,out MailboxAddress res,out string TAO)
        {
            res = null;
            TAO = string.Empty;
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "Mail_ParseTAO";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@address", address);


                using (var r = SQL.StartRead(cmd))
                {
                    r.Read();
                    if (!r.HasRows) return false;
                    
                    if (r.IsDBNull(0) || r.IsDBNull(1)) return false;
                    res = new MailboxAddress("", r.GetValue(0).ToString());
                    TAO = r.GetValue(1).ToString();
                    return true;
                }

            }
            
        }
        public static void UpdateIllegalTrashKadastr(string heapGuid, string kadastr)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();


                var Query = "CrateMate_UpdateTrashHeapHadastr";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@trashHeapGuid", heapGuid);
                cmd.Parameters.AddWithValue("@kadastr", kadastr);
                SQL.Execute(cmd);
            }
        }

        public static void InsertUserTrackingPoint(GeoPoint pos, string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_SetPos";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.AddWithValue("@lat", pos.mLatitude);
                cmd.Parameters.AddWithValue("@lon", pos.mLongitude);
                cmd.Parameters.AddWithValue("@id", UserGuid);
                // cmd.Parameters.AddWithValue("@id_user", UserGuid);



                SQL.Execute(cmd);

            }
        }
        public static List<BinManAccrual> GetAllDocAnySummAccruals(string docBinid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                List<BinManAccrual> res = new List<BinManAccrual>();
                var Query = "BinMan_GetAllDoc19Accruals";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@docBinid", docBinid);

                long recs = 0;

                using (var r = StartRead(cmd))
                {
                    while (r.Read())
                    {
                        BinManAccrual a = new BinManAccrual();
                        a.typeRaw = "19";
                        a.date =r.GetDateTime(0);
                        a.summ = r.GetValue(1).ToString();
                        a.comment = r.GetValue(2).ToString();
                        a.dateFrom = (r.GetDateTime(3));
                        a.dateTo = r.GetDateTime(4);
                       // DateOnly.FromDateTime();
                        res.Add(a);
                    }
                }
                return res;
            }
        }
        private record class AccrualsNode
        {
            public string id_parent;
            public string id;
            public AccrualsNode child;
            public AccrualsNode parent;
        }//HANDMADE !
        public static void ProceedAccruals()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                // var Query = "select a.NDoc FROM accruals a GROUP BY (a.NDoc)";
                //Только не проставленные
                var Query = "select a.NDoc FROM accruals a GROUP BY (a.NDoc)";
                // select a.NDoc FROM accruals a GROUP BY (a.NDoc)
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;
                // cmd.Parameters.AddWithValue("@guid", guid);
                //Log.sql(cmd);

                int prog = 0;

                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var Ndoc = r.GetValue(0);

                        Console.Title = "Прогресс: " + prog;
                        prog++;
                        Query = "select a.id_nach,a.id_Head_nach FROM accruals a WHERE a.NDoc=@Ndoc";
                        cmd = new SqlCommand(Query, _con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@Ndoc", Ndoc);

                        using (var rr = SQL.StartRead(cmd))
                        {
                            List<AccrualsNode> Nodes = new List<AccrualsNode>();

                            while (rr.Read())
                            {

                                var id = rr.GetValue(0).ToString();
                                var id_head = rr.IsDBNull(1) ? string.Empty : rr.GetValue(1).ToString();

                                Nodes.Add(new AccrualsNode() { id = id, id_parent = id_head });
                            }

                            var c = Nodes.Count;
                            for (int i = 0; i < c; i++)
                            {
                                var node = Nodes[i];
                                if (node.id_parent != null)
                                {
                                    var nd = Nodes.FirstOrDefault(n => n.id == node.id_parent);

                                    if (nd != null)
                                    {
                                        node.parent = nd;
                                        nd.child = node;
                                    }
                                    else
                                    {
                                        //Log.Error("Владелец (Начисление) не найдено");
                                    }
                                }
                            }
                            //   for (int i = 0; i < c; i++)
                            // {
                            Nodes = Nodes.Where(n => (n.parent == null && n.child != null)).ToList();
                            // }

                            foreach (var node in Nodes)
                            {

                                var nd = node.child;
                                while (nd != null)
                                {
                                    Query = "INSERT INTO [dbo].[temp_accruals_stack] VALUES(@head, @id)";
                                    cmd = new SqlCommand(Query, _con);
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.AddWithValue("@head", node.id);
                                    cmd.Parameters.AddWithValue("@id", nd.id);
                                    nd = nd.child;
                                    SQL.Execute(cmd);
                                }
                                // Log.Text($"Node Done: {node.id}");
                            }

                        }

                        // AddressAccurateTask ga = new AddressAccurateTask();
                        // ga.guid = guid;
                        // ga.address = r.GetValue(0).ToString();





                        // return ga;
                    }
                }
                // return null;
            }
        }
        public static List<GeoObject> GetObjectList2KadastrParse()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                List<GeoObject> res = new List<GeoObject>();
                var Query = "TEMP_getObject2KadastrParse";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                long recs = 0;

                using (var r = StartRead(cmd))
                {
                    while (r.Read())
                    {
                        Console.Title = $"Загружено из бд: {recs}";
                        recs++;
                        GeoObject obj = new GeoObject();
                        obj.guid = r.GetValue(0).ToString();
                        obj.address = r.GetValue(1).ToString();
                        res.Add(obj);
                    }
                }
                return res;
            }
        }
        public static List<GeoObject> GetObjectList2KadTypeParse()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                List<GeoObject> res = new List<GeoObject>();
                var Query = "TEMP_GetAllObectKadastrs";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                long recs = 0;

                using (var r = StartRead(cmd))
                {
                    while (r.Read())
                    {
                        Console.Title = $"Загружено из бд: {recs}";
                        recs++;
                        GeoObject obj = new GeoObject();
                        obj.guid = r.GetValue(0).ToString();
                        obj.kadastr = r.GetValue(1).ToString();
                        res.Add(obj);
                    }
                }
                return res;
            }
        }
        public class Tmp_obj
        {
            public string id;
            public string? Title;
            public string? INN;
            public string? KPP;
            public string? OGRN;
        }
        public static IEnumerable<Tmp_obj> TEST()
        {
           // Stopwatch sw = Stopwatch.StartNew();
           
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                
                var Query = "SELECT  [id],[INN],[OGRN],[Title] FROM [TKO].[dbo].[KA_old]";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.Text;

                long recs = 0;
                
                using (var r = StartRead(cmd))
                {
                    var q = r.AsParallel();
                        
                    while (r.Read())
                    {
                        if (r.IsDBNull(0)) continue;
                        var tt = new Tmp_obj();
                        if (!r.IsDBNull(1)) tt.INN = r.GetValue(1).ToString();
                       // if(string.IsNullOrEmpty(tt.INN)) continue;
                       tt.id = r.GetValue(0).ToString();

                        if (!r.IsDBNull(2)) tt.OGRN = r.GetValue(2).ToString();
                        if (!r.IsDBNull(3))tt.Title = r.GetValue(3).ToString();
                        yield return tt;
                       // ress.Add(tt.id, tt);
                      //  ress.Add(r.GetValue(0).ToString());
                    }
                }
               // Log.Text(sw.Elapsed.ToString());
               // return ress;
            }
        }
        public static void InsertFIASKadastrParse(string objGuid, string? kadastr)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "TEMP_ParseKadastrFromFIAS_Objects";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@objGuid", objGuid);
                cmd.Parameters.AddWithValue("@kadastr", string.IsNullOrEmpty(kadastr) ? DBNull.Value : kadastr);

                Execute(cmd);
            }
        }
        public static void InsertKadastrTypeParse(string objGuid, string? typeGuid, string? kindGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                if (string.IsNullOrEmpty(typeGuid)) typeGuid = "5b04769d-218b-4c13-9415-408c18e39ae8";
                if (string.IsNullOrEmpty(kindGuid)) kindGuid = "a8254c1d-e48b-48ac-8b44-790dbd3e2711";


                var Query = "TEMP_ParseKadType";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@objGuid", objGuid);
                cmd.Parameters.AddWithValue("@type", typeGuid);
                cmd.Parameters.AddWithValue("@kind", kindGuid);


                Execute(cmd);
            }
        }

        public static bool CheckLogAPass(string Login, string? Password, bool IsAdAuthorized, out string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "CrateMate_CheckLogin";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@login", string.IsNullOrEmpty(Login) || string.IsNullOrWhiteSpace(Login) ? DBNull.Value : Login);
                cmd.Parameters.AddWithValue("@pass", string.IsNullOrEmpty(Password) || string.IsNullOrWhiteSpace(Password) ? DBNull.Value : Password);
                cmd.Parameters.AddWithValue("@AD", IsAdAuthorized);

                using (var r = StartRead(cmd)) {
                    if (r.Read())
                    {
                        UserGuid = r.GetValue(0).ToString();
                        if (UserGuid.Length <= 1) return false;
                        return true;
                    }
                    else
                    {
                        UserGuid = string.Empty;
                        return false;
                    }
                }
            }
        }


        public static Role[] GetUserRoles(string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetUserRoles";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", UserGuid);
                List<Role> roles = new List<Role>();
                using (var r = StartRead(cmd))
                {
                    while (r.Read())
                    {
                        Role rl = new Role();
                        rl.name = r.GetValue(0).ToString();
                        roles.Add(rl);
                    }
                }
                return roles.ToArray();
            }
        }
        public static UserAccount GetUserAdditionLoginData(string UserGuid) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_UserConf";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", UserGuid);

                using (var r = StartRead(cmd))
                {
                   
                    r.Read();
                    


                    var res=  new UserAccount() {
                        nickName = r.GetValue(0).ToString() + " " + r.GetValue(1).ToString() + " " + r.GetValue(2).ToString(),

                        isGpsTrackingEnabled = r.IsDBNull(3) ? false : r.GetBoolean(3),
                    };
                    if (!r.IsDBNull(4) && !r.IsDBNull(5))
                    {
                         res.WorkTimeStart  = TimeOnly.FromDateTime(r.GetDateTime(4));
                         res.WorkTimeEnd = TimeOnly.FromDateTime(r.GetDateTime(5));
                    }
                    if (!r.IsDBNull(6)) res.isDebugEnabled = r.GetBoolean(6);
                   

                    return res;
                }
            }

        }
        public static bool CreateNewObject(GeoObjectCreateRequest req,string userGuid, out string guid, IFormFileCollection files)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMAte_InsertObj";
                var cmd = new SqlCommand(Query, _con);

                guid = Guid.NewGuid().ToString();
                string guide = guid;
                string address = DadataApi.GetAddress(req.position).Replace(" 650036, Кемеровская область - Кузбасс,", "").Replace("Кемеровская область - Кузбасс,", "").Replace("650036,", "").Trim();
                string Title = req.userTitle != null ? (req.userTitle.Length > 0 ? req.userTitle : string.Empty) : string.Empty;
                if (string.IsNullOrEmpty(Title)) Title = req.subObjectType.name + ", " + address;
                req.userTitle = Title;

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", guid);
                cmd.Parameters.AddWithValue("@lon", req.position.mLongitude);
                ;
                cmd.Parameters.AddWithValue("@lat", req.position.mLatitude);
                cmd.Parameters.AddWithValue("@title", Title);
                cmd.Parameters.AddWithValue("@Id_TypeObjects", req.objectType.guid);
                cmd.Parameters.AddWithValue("@Id_SubTypeObjects", req.subObjectType.guid);

                cmd.Parameters.AddWithValue("@adress", address);
                cmd.Parameters.AddWithValue("@area", req.area);
                cmd.Parameters.AddWithValue("@isRentObj", req.isRentor);
                cmd.Parameters.AddWithValue("@IdUser", userGuid);
                //  cmd.Parameters.AddWithValue("@commentary", req.commentary!= null  ? req.commentary: DBNull.Value);



                if (
                SQL.Execute(cmd)
                //true
                )
                {
                    SQL.ParseFilesToSomething(guid, files,userGuid);
                    //Task.УдалитьВсеНе0Начисления(() =>
                    //{
                    //    if (SQL.CreateObjectInBinman(req, out var binId))
                    //    {
                    //        SQL.UpdateObjectBinId(guide, binId);
                    //        SQL.SetObjectStatus(guide,BinManSyncStatus.ok);
                    //    }
                    //});
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void ArchiveGeozone(string GeozoneGuid, string userId, string Commentary, bool inArchive)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_SetGeoArch";
                var cmd = new SqlCommand(Query, _con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_geo", GeozoneGuid);
                cmd.Parameters.AddWithValue("@id_user", userId);
                cmd.Parameters.AddWithValue("@descr", Commentary);
                cmd.Parameters.AddWithValue("@isArchiving", inArchive);


                SQL.Execute(cmd);


            }
        }
        //public static bool CreateObjectInBinman(GeoObjectCreateRequest req, out long binId)
        //{
        //    // b.REGION = "Кемеровская область - Кузбасс";
        //    //b.CITY = "г Кемерово";
        //    //b.STREET = "ул Пушкина";
        //    //b.ADDRESS = "Кемеровская область - Кузбасс, г Кемерово, ул Пушкина";
        //    //b.LAT = 55.358542f;
        //    //b.LON = 86.08729f;
        //    //b.lot_id = Geo_Lot.lot_2;
        //    //b.CATEGORY = BinManObject.ObjectMainCattegory.Trade;
        //    //b.SUBCATTEGORY = BinManObject.ObjectSubCattegory.Supermarket;
        //    //b.NAME = "Продовольственный магазин, г Кемерово, ул Пушкина, TEST";
        //    binId = -1;
        //    if (!BinManApi.IsApiEnabled) return false;
        //    BinManObject obj = new BinManObject();
        //    obj.NAME = req.userTitle;
        //    // obj.lot_id = Geo_Lot.lot_2;
        //    obj.LAT = (float)req.position.mLatitude;
        //    obj.LON = (float)req.position.mLongitude;
        //    try
        //    {
        //        obj.CATEGORY = (BinManObject.ObjectMainCattegory)req.objectType.binId;
        //    }
        //    catch (Exception e) { Log.Error($"CATTEGORY: dbBinId: {req.objectType.binId}"); Log.Error(e); }
        //    try
        //    {
        //        obj.SUBCATTEGORY = (BinManObject.ObjectSubCattegory)req.subObjectType.binId;
        //    }
        //    catch (Exception e) { Log.Error($"SUBCATTEGORY: dbBinId: {req.subObjectType.binId}"); Log.Error(e); }


        //    DadataApi.TryFillAddres(obj);
        //    Log.Json(obj);

        //    return obj.SendCreateRequest(BinManApi.GetNextAccount(), out binId);


        //}
        public static void UnlinkGeozoneFromObject(string geoGuid, string objectGuid, string userGuid,string comment = null)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "Cratemate_DeleteChainGeoObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_geo", geoGuid);
                cmd.Parameters.AddWithValue("@id_obj", objectGuid);
                cmd.Parameters.AddWithValue("@id_user", userGuid);
                cmd.Parameters.AddWithValue("@comment", string.IsNullOrEmpty(comment) ? DBNull.Value : comment);
                SQL.Execute(cmd);
            }
        }
        public static void SetObjectStatus(string objectGuid, BinManSyncStatus status)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "BinMan_SetObjectBinStatus";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@objectId", objectGuid);
                cmd.Parameters.AddWithValue("@status", Enum.GetName(status));
                SQL.Execute(cmd);
            }
        }
        public static void UpdateObjectBinId(string objGuid, long binId)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Query = "BinMan_UpdateObjectId";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ObjectGuid", objGuid);
                cmd.Parameters.AddWithValue("@binmanID", binId);
                SQL.Execute(cmd);
            }
        }
        public static OnGeozoneClickData GetOnClickData(string GeoGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var Result = new OnGeozoneClickData();
                Result.geozone = SQL.GetGeozone(GeoGuid, _con);
                Result.objects = SQL.GetGeozoneObjects(GeoGuid, _con);
                Result.geozone.containers = SQL.GetGeozoneContainers(GeoGuid, _con);
                return Result;
            }
        }
        public static List<GeoObject> GetGeozoneObjects(string GeoGuid, SqlConnection con = null)
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            List<GeoObject> result(SqlConnection _con)
            {
                var Query = "CrateMate_getObjGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idGeo", GeoGuid);


                List<GeoObject> res = new List<GeoObject>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        GeoObject Sg = new GeoObject();

                        if (!r.IsDBNull(5) && !r.IsDBNull(6))
                        {

                            Sg.guid = r.GetValue(0).ToString();
                            Sg.name = r.GetValue(1).ToString();
                            Sg.address = r.GetValue(2).ToString();

                            GeoPoint gp = new GeoPoint(float.Parse(r.GetValue(6).ToString().Replace(".", ",")), float.Parse(r.GetValue(5).ToString().Replace(".", ",")));
                            Sg.position = gp;
                            Sg.needView = r.IsDBNull(7) ? false : r.GetBoolean(7);
                            if (!r.IsDBNull(8)) Sg.objectIcon = r.GetValue(8).ToString();
                            if (!r.IsDBNull(10)) Sg.chained = int.Parse(r.GetValue(10).ToString());
                            if (!r.IsDBNull(11)) Sg.dateStart = r.GetDateTime(11);
                            if (!r.IsDBNull(12)) Sg.dateEnd = r.GetDateTime(12);
                            Sg.isDogActive = r.GetInt32(13) == 1;
                           
                            Sg.client = new Client();
                            if (!r.IsDBNull(14)) Sg.client.name = r.GetValue(14).ToString();
                            if (!r.IsDBNull(15)) Sg.client.inn = r.GetValue(15).ToString();
                            if (!r.IsDBNull(16)) Sg.client.ogrn = r.GetValue(16).ToString();
                            if (!r.IsDBNull(17)) Sg.document = r.GetValue(17).ToString();
                            if (!r.IsDBNull(18)) Sg.isArch = r.GetBoolean(18);

                           res.Add(Sg);
                        }

                        // TrashManl<%>pf6d!TrashMan123l<%>pf6d!0.05.1
                        // 6DBCB676-B7E6-4390-8A49-46DEC577966D
                    }
                }
                return res;
            }
        }
        public static void DeleteEvent(string evGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_DeleteEvent";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_Event", evGuid);
                //Log.Text("Delete: "+evGuid);
                SQL.Execute(cmd);
            }
        }
        public static List<GeozoneMarker> GetClosestGeozones(GeoPoint pos,float SearchRange=650)
        {
            return GetClosestGeozones(pos.mLatitude, pos.mLongitude,SearchRange);
        }
        public static List<GeozoneMarker> GetClosestGeozones(double lat, double lon, float SearchRange = 650) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetRoundGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@lon", lon);
                cmd.Parameters.AddWithValue("@search_range_metre", SearchRange);

                var res = DefaultGeozoneMarkersRead(cmd);






                return res;
            }
        }
        public static bool TryGetRawFileData(string FileGuid, out FileData data)
        {
            data = new FileData();
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_DownloadFile";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fileGuid", FileGuid);

                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        data = new FileData();
                        data.FileType = MimeTypesMap.GetMimeType(r.GetValue(0).ToString());
                        //data.data = r.GetStream(1);
                        var RelativeFileSystemPath = r.GetValue(3).ToString();
                        Stream stream = null;
                        if (!string.IsNullOrEmpty(RelativeFileSystemPath))
                        {
                            var FileSystemRoot = r.GetValue(2).ToString();
#pragma warning disable CA1416 // Проверка совместимости платформы
                            WindowsIdentity.RunImpersonated(userHandle, () =>
                            {
                                stream = new MemoryStream( File.ReadAllBytes(FileSystemRoot + "\\" + RelativeFileSystemPath));
                            });
#pragma warning restore CA1416 // Проверка совместимости платформы
                        }
                        else
                        {
                            stream = r.GetStream(1);
                        }
                        data.data = stream;
                        if (data.data == null) return false;
                        return true;
                    }
                }

            }
            return false;
        }
        public record struct FileData(string FileType, Stream data);
        private static UserCredentials credentials = new UserCredentials("CHGK", "a.m.maltsev", "b6b2mizz");
        private static SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.Interactive);
        public static bool TryGetFileData(string FileGuid, out FileData data, FileQuality? fq = FileQuality.med)
        {
            data = new FileData();
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_DownloadFile";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fileGuid", FileGuid);

                using (var r = SQL.StartRead(cmd))
                {

                    if (r.Read())
                    {
                        data = new FileData();
                        data.FileType = MimeTypesMap.GetMimeType(r.GetValue(0).ToString());

                        var RelativeFileSystemPath = r.GetValue(3).ToString();
                        Stream stream = null;
                        if (!string.IsNullOrEmpty(RelativeFileSystemPath))
                        {
                            var FileSystemRoot = r.GetValue(2).ToString();
#pragma warning disable CA1416 // Проверка совместимости платформы
                            WindowsIdentity.RunImpersonated(userHandle, () =>
                            {
                                stream = File.OpenRead(FileSystemRoot + "\\" + RelativeFileSystemPath);
                            });
#pragma warning restore CA1416 // Проверка совместимости платформы
                        }
                        else
                        {
                            stream = r.GetStream(1);
                        }
                        // MimeTypesMap.
                        // fileFormat
                        switch (fq)
                        {
                            case FileQuality.preview:
                                data.data = ImageProcesser.ResizeImage(stream, 128);
                                break;
                            case FileQuality.med:
                                data.data = ImageProcesser.ResizeImage(stream, 512);
                                break;
                            case FileQuality.original:
                                data.data = stream;
                                break;
                            default:
                                data.data = ImageProcesser.ResizeImage(stream, 512);
                                break;
                        }

                        if (data.data == null) return false;
                        return true;
                    }
                }

            }
            return false;
        }
        public static List<SQlFileInfo> GetSomethingFilesInfo(string guid,string? userGuid)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetOwnerFilesInfo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ownerGuid", guid);
                cmd.Parameters.AddWithValue("@id_user",string.IsNullOrEmpty(userGuid)?DBNull.Value: userGuid);
                List<SQlFileInfo> res = new List<SQlFileInfo>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        SQlFileInfo ff = new SQlFileInfo();
                        if (r.IsDBNull(0)) continue;
                        ff.guid = r.GetValue(0).ToString();
                        ff.fileName = r.GetValue(1).ToString();
                        ff.creationDate = r.GetDateTime(2);
                        ff.userName = r.GetValue(3).ToString();
                        ff.title = r.GetValue(4).ToString();
                        ff.allowDelete = r.GetBoolean(5);

                        res.Add(ff);
                    }
                }
                return res;
            }

        }
        public static List<ExtendedFileInfo> GetSomethingFilesInfo_Ext(string guid, string? userGuid)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetOwnerFilesInfo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ownerGuid", guid);
                cmd.Parameters.AddWithValue("@id_user", string.IsNullOrEmpty(userGuid) ? DBNull.Value : userGuid);
                List<ExtendedFileInfo> res = new List<ExtendedFileInfo>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        ExtendedFileInfo ff = new ExtendedFileInfo();
                        if (r.IsDBNull(0)) continue;
                        ff.guid = r.GetValue(0).ToString();
                        if(!r.IsDBNull(1))
                        ff.fileName = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2))
                            ff.creationDate = r.GetDateTime(2);
                        if (!r.IsDBNull(3))
                            ff.userName = r.GetValue(3).ToString();
                        ff.title = r.GetValue(4).ToString();
                        ff.allowDelete = r.GetBoolean(5);


                        ff.ext = Path.GetExtension(ff.fileName);
                        ff.ai_type = ExtendedFileInfo.AddInfoType.none;

                        if (Guid.TryParse(Path.GetFileNameWithoutExtension(ff.fileName), out var vg)){
                            ff.buityFileName = "Без имени" + Path.GetExtension(ff.fileName);
                        }
                        else
                        {
                            ff.buityFileName = ff.fileName;
                        }

                        if (ff.ext !=null)
                        switch (ff.ext.ToLower())
                        {
                            case ".jpg" or ".png" or ".jpeg":
                                ff.icon = ExtendedFileInfo.FileTypeIcon.picture;
                                break;
                            case ".mp3" or ".mp4":
                                ff.icon = ExtendedFileInfo.FileTypeIcon.video;
                                break;
                        }
                        Log.Text("Icone:: "+ff.icon.ToString());
                        res.Add(ff);
                    }
                }
                return res;
            }


        }
        public static void UpdateOrderUserCustomCommentary(List<string> CommentGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                for (int i = 0; i < CommentGuid.Count; i++)
                {
                    var Query = "CrateMate_ReorderCustomUserComment";
                    var cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;



                    cmd.Parameters.AddWithValue("@id", CommentGuid[i]);
                    cmd.Parameters.AddWithValue("@order", i);


                    SQL.Execute(cmd);
                }


            }
        }
        public static void RemoveUserCustomCommentary(string CommentGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_RemoveCustomUserComment";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;



                cmd.Parameters.AddWithValue("@id", CommentGuid);


                SQL.Execute(cmd);


            }
        }

        public static void SaveUserCustomCommentary(CreateCustomCommentRequest comment,string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_InsertNewCustomUserComment";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                if (string.IsNullOrEmpty(comment.guid)) comment.guid = Guid.NewGuid().ToString();

                cmd.Parameters.AddWithValue("@id_user", UserGuid);
                cmd.Parameters.AddWithValue("@id", comment.guid);
                cmd.Parameters.AddWithValue("@comment", comment.comment);
                cmd.Parameters.AddWithValue("@form", comment.form);

                SQL.Execute(cmd);
                

            }
        }


        public static List<CustomUserComment> GetUserCustomComments(string UserGuid,string form)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetCustomUserComments";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_user", UserGuid);
                cmd.Parameters.AddWithValue("@form_requesting", form);
                // cmd.Parameters.AddWithValue("@id_user", UserGuid);


                List<CustomUserComment> res = new List<CustomUserComment>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var c= new CustomUserComment();
                        c.comment = r.GetValue(0).ToString();
                        c.dateCreated = r.GetDateTime(1);
                        if (!r.IsDBNull(2))
                            c.dateUpdated = r.GetDateTime(2);
                        c.form = r.GetValue(3).ToString();
                        c.guid = r.GetValue(4).ToString();
                        res.Add(c);
                    }
                }
                //res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;

            }
        }

        public static List<MigGeozoneVisit> GetMigGeozoneVisits(string geo_guid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetGeozoneMigVisits";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", geo_guid);
                // cmd.Parameters.AddWithValue("@id_user", UserGuid);


                List<MigGeozoneVisit> res = new List<MigGeozoneVisit>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        MigGeozoneVisit v = new MigGeozoneVisit();
                        v.id = r.GetValue(0).ToString();
                        if (!r.IsDBNull(1))
                        v.car = r.GetValue(1).ToString();
                        if(!r.IsDBNull(2))
                        v.lastUpdate = r.GetDateTime(2);
                        if(!r.IsDBNull(3))
                        v.comment = r.GetValue(3).ToString();
                        v.status = r.GetValue(4).ToString();
                        if (!r.IsDBNull(5))
                            v.visitDate = r.GetDateTime(5);
                        res .Add(v);
                    }
                }
                //res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
               
            }
        }

        public static List<GeozoneMarker> DefaultGeozoneMarkersRead2(SqlCommand cmd)
        {
            List<GeozoneMarker> res = new List<GeozoneMarker>();

            SqlDataAdapter ds = new SqlDataAdapter(cmd);
            System.Data.DataTable dt = new();
            ds.Fill(dt);

            foreach (DataRow r in dt.Rows)
            {

                GeozoneMarker Sg = new GeozoneMarker();

                // if (!r.IsDBNull(4) && r.GetBoolean(4)) continue;

                Sg.guid = r[0].ToString();
                if (r[4]!=DBNull.Value) Sg.isArch = (bool)r[4];
                //  Sg.name = r.GetValue(3).ToString();
                //  Sg.address = r.GetValue(5).ToString();
                Sg.color = int.Parse(r[8].ToString());
                Sg.containersCount = int.Parse(r[11].ToString());
                Sg.position = new GeoPoint(float.Parse(r[2].ToString()), float.Parse(r[1].ToString()));
                if (r[12] != DBNull.Value) Sg.needWatch = (bool)r[12];
                else Sg.needWatch = false;
                if (r[13] != DBNull.Value)
                    Sg.typeGuid = r[13].ToString();
                if (r[3] != DBNull.Value) Sg.name = r[3].ToString();
                if (r[5] != DBNull.Value) Sg.address = r[5].ToString();
                //if (!r.IsDBNull(5)) Sg.address = r.GetValue(5).ToString();
                if (r[14] != DBNull.Value) Sg.chainObjCount = int.Parse(r[14].ToString());
                if (r[15] != DBNull.Value) Sg.postfix = r[15].ToString();
                if (r[16] != DBNull.Value) Sg.photos = int.Parse(r[16].ToString());

                //if (!r.IsDBNull(10))
                //     Sg.lastEvent = r.GetDateTime(10);
                res.Add(Sg);

                //LoadGeozoneEvents(Sg);
            }
            return res;
        }


    public static List<GeozoneMarker> DefaultGeozoneMarkersRead(SqlCommand cmd)
        {
            List<GeozoneMarker> res = new List<GeozoneMarker>();

            using (var r = SQL.StartRead(cmd))
            {
                if (r == null) throw new Exception("[Кастомное исключение] DefaultGeozoneMarkersRead - null reader");
                while (r.Read())
                {
                    GeozoneMarker Sg = new GeozoneMarker();

                    // if (!r.IsDBNull(4) && r.GetBoolean(4)) continue;

                    Sg.guid = r.GetValue(0).ToString();
                    if (!r.IsDBNull(4)) Sg.isArch = r.GetBoolean(4);
                    //  Sg.name = r.GetValue(3).ToString();
                    //  Sg.address = r.GetValue(5).ToString();
                    Sg.color = int.Parse(r.GetValue(8).ToString());
                    Sg.containersCount = int.Parse(r.GetValue(11).ToString());
                    Sg.position = new GeoPoint(float.Parse(r.GetValue(2).ToString()), float.Parse(r.GetValue(1).ToString()));
                    if (!r.IsDBNull(12)) Sg.needWatch = r.GetBoolean(12);
                    else Sg.needWatch = false;
                    if (!r.IsDBNull(13))
                        Sg.typeGuid = r.GetValue(13).ToString();
                    if (!r.IsDBNull(3)) Sg.name = r.GetValue(3).ToString();
                    if (!r.IsDBNull(5)) Sg.address = r.GetValue(5).ToString();
                    if (!r.IsDBNull(5)) Sg.address = r.GetValue(5).ToString();
                    if (!r.IsDBNull(14)) Sg.chainObjCount = int.Parse(r.GetValue(14).ToString());
                    if (!r.IsDBNull(15)) Sg.postfix = r.GetValue(15).ToString();
                    if (!r.IsDBNull(16)) Sg.photos = int.Parse(r.GetValue(16).ToString());
                    

                    //if (!r.IsDBNull(10))
                    //     Sg.lastEvent = r.GetDateTime(10);
                    res.Add(Sg);

                    //LoadGeozoneEvents(Sg);
                }
            }
            return res;
        }

        public static List<UniversalEvent> GetIllegalHistory(string pileGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetHeapEvents";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", pileGuid);
               // cmd.Parameters.AddWithValue("@id_user", UserGuid);


                List<UniversalEvent> res = new List<UniversalEvent>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        UniversalEvent Ge = new UniversalEvent();
                        Ge.guid = r.GetValue(0).ToString();
                        Ge.type = r.GetValue(1).ToString();
                        Ge.dateTime = r.GetDateTime(2);
                        Ge.description = r.GetValue(4).ToString();
                        Ge.isCanBeDeleted = r.GetInt32(5) == 1;
                        res.Add(Ge);
                    }
                }
                res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static bool ReopenIllegal(CloseTrashPileRequest req, string UserGuid, IFormFileCollection files) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_OpenHeap";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                string guid = Guid.NewGuid().ToString();
                cmd.Parameters.AddWithValue("@id_event", guid);
                cmd.Parameters.AddWithValue("@id_heap", req.guid);
                cmd.Parameters.AddWithValue("@idUser", UserGuid);
                cmd.Parameters.AddWithValue("@volume", req.volume);
                cmd.Parameters.AddWithValue("@Descr", req.desc);
                cmd.Parameters.AddWithValue("@user_lat", (req.userPosition !=null? req.userPosition.Value.mLatitude : DBNull.Value));
                cmd.Parameters.AddWithValue("@user_lon", (req.userPosition != null ? req.userPosition.Value.mLongitude : DBNull.Value));

                Task.Run(() => { ParseFilesToSomething(guid, files,UserGuid); });

                return SQL.Execute(cmd);

                
            }
            return false;
        }
        public static List<LandFillMarker> GetAllLandFillds()
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetAllLandFillds";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                List<LandFillMarker> res = new List<LandFillMarker>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var lf = new LandFillMarker();

                        lf.guid = r.GetValue(0).ToString();
                        if (!r.IsDBNull(1)) lf.title = r.GetValue(1).ToString();
                        lf.pos = new GeoPoint(float.Parse(r.GetValue(3).ToString()), float.Parse(r.GetValue(2).ToString()));
                        lf.rad = int.Parse(r.GetValue(4).ToString());
                        res.Add(lf);
                    }
                }

                return res;
            }

        }
        public static List<GeozoneMarker> GetObjectGeozones(string objectGuid)
        {

            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetGeoFromObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", objectGuid);

                List<GeozoneMarker> res = new List<GeozoneMarker>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        GeozoneMarker Sg = new GeozoneMarker();

                        if (!r.IsDBNull(4) && r.GetBoolean(4)) continue;

                        Sg.guid = r.GetValue(0).ToString();
                        //  Sg.name = r.GetValue(3).ToString();
                        //  Sg.address = r.GetValue(5).ToString();
                        Sg.color = int.Parse(r.GetValue(8).ToString());
                        Sg.containersCount = int.Parse(r.GetValue(11).ToString());
                        Sg.position = new GeoPoint(float.Parse(r.GetValue(2).ToString()), float.Parse(r.GetValue(1).ToString()));
                        if (!r.IsDBNull(12)) Sg.needWatch = r.GetBoolean(12);
                        else Sg.needWatch = false;
                        if (!r.IsDBNull(13))
                            Sg.typeGuid = r.GetValue(13).ToString();
                        if (!r.IsDBNull(3)) Sg.name = r.GetValue(3).ToString();
                        if (!r.IsDBNull(5)) Sg.address = r.GetValue(5).ToString();
                        if (!r.IsDBNull(14)) Sg.chainObjCount = int.Parse(r.GetValue(14).ToString());

                        //if (!r.IsDBNull(10))
                        //     Sg.lastEvent = r.GetDateTime(10);
                        res.Add(Sg);

                        //LoadGeozoneEvents(Sg);
                    }
                }






                return res;
            }


        }

        public static bool LinkGeozone2Object(string UserGuid, string ObjectGuid,string GeozoneGuid,string comment = null)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_CreateChainGeoObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_obj", ObjectGuid);
                cmd.Parameters.AddWithValue("@id_geo", GeozoneGuid);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);
                cmd.Parameters.AddWithValue("@comment", string.IsNullOrEmpty(comment) ? DBNull.Value: comment);
                //Log.Text("Delete: "+evGuid);
                 return SQL.Execute(cmd);
            }
        }
        public static List<GeoObjectMarker> GetClosestObjects(GeoPoint pos)
        {
            return GetClosestObjects(pos.mLatitude, pos.mLongitude);
        }
        public static List<GeoObjectMarker> GetClosestObjects(double lat, double lon)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetRoundObjects";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@lon", lon);

                var res = DefaultObjectsRead(cmd);

                return res;
            }
        }
        public static List<GeoObjectMarker> DefaultObjectsRead(SqlCommand cmd) {

            List<GeoObjectMarker> res = new List<GeoObjectMarker>();
            using (var r = SQL.StartRead(cmd))
            {

                while (r.Read())
                {
                    GeoObjectMarker Sg = new GeoObjectMarker();

                    if (!r.IsDBNull(6) && !r.IsDBNull(7))
                    {

                        Sg.guid = r.GetValue(0).ToString();
                        Sg.name = r.GetValue(1).ToString();
                        Sg.address = r.GetValue(2).ToString();
                        //    if (!r.IsDBNull(3))
                        //      Sg.objectType = r.GetValue(3).ToString();
                        //   if (!r.IsDBNull(4))
                        //       Sg.subObjctType = r.GetValue(4).ToString();
                        //  Log.Text($"1: {r.GetValue(5).ToString()} 2:{r.GetValue(6).ToString()}");
                        //  Sg.binId = r.GetValue(5).ToString();

                        GeoPoint gp = new GeoPoint(float.Parse(r.GetValue(7).ToString().Replace(".", ",")), float.Parse(r.GetValue(6).ToString().Replace(".", ",")));
                        Sg.position = gp;
                        Sg.needView = r.IsDBNull(8) ? false : r.GetBoolean(8);
                        if (!r.IsDBNull(9)) Sg.objectIcon = r.GetValue(9).ToString();
                        if (!r.IsDBNull(11)) Sg.chained = int.Parse(r.GetValue(11).ToString());
                        if (!r.IsDBNull(12)) Sg.dateStart = r.GetDateTime(12);
                        if (!r.IsDBNull(13)) Sg.dateEnd = r.GetDateTime(13);
                        Sg.isDogActive = r.GetInt32(14) == 1;
                        Sg.client = new Client();
                        if (!r.IsDBNull(15))  Sg.client.name = r.GetValue(15).ToString();
                        if (!r.IsDBNull(16))  Sg.client.inn = r.GetValue(16).ToString();
                        if (!r.IsDBNull(17))  Sg.client.ogrn = r.GetValue(17).ToString();
                        if (!r.IsDBNull(18)) Sg.document = r.GetValue(18).ToString();
                        if (!r.IsDBNull(19)) Sg.isArch =! r.GetBoolean(19);

                        //if (r.FieldCount > 10)
                        //    Sg.objectType = r.GetValue(10).ToString();
                        //if (r.FieldCount > 11)
                        //    Sg.subObjctType = r.GetValue(11).ToString();
                        //if (r.FieldCount > 12)
                        //{
                        //    Sg.client = new Client();
                        //    Sg.client.name = r.GetValue(12).ToString();
                        //    if (r.FieldCount > 13)
                        //        if (long.TryParse(r.GetValue(13).ToString(), out long inn1))
                        //            Sg.client.inn = inn1;
                        //    if (r.FieldCount > 14)
                        //        if (long.TryParse(r.GetValue(14).ToString(), out long ogrn1))
                        //            Sg.client.ogrn = ogrn1;
                        //}

                        res.Add(Sg);
                    }

                    // TrashManl<%>pf6d!TrashMan123l<%>pf6d!0.05.1
                    // 6DBCB676-B7E6-4390-8A49-46DEC577966D
                }
            }
            int CutInt(string str)
            {
                return int.Parse(new System.String(str.Where(Char.IsDigit).ToArray())??"-1");
            }

            var test = res;
            //new List<GeoObjectMarker>();
            //test.Add(new GeoObjectMarker() { address = "ул. Гагарина, дом 13, кв. 4"});
            //test.Add(new GeoObjectMarker() { address = "ул. Гагарина, дом 13, кв. 6"});
            //test.Add(new GeoObjectMarker() { address = "ул. Кумарика, дом 21, кв. 18" });
            //test.Add(new GeoObjectMarker() { address = "ул. Кумарика, дом 21, кв. 11" });
            //test.Add(new GeoObjectMarker() { address = "ул. Гагарина, дом 13, кв. 1"});
            //test.Add(new GeoObjectMarker() { address = "ул. Гагарина, дом 13, кв. 17"});

            //test.Add(new GeoObjectMarker() { address = "ул. Кумарика, дом 21, кв. 14" });
            //test.Add(new GeoObjectMarker() { address = "ул. Кумарика, дом 21, кв. 21" });

            var resbkp = new List<GeoObjectMarker>(test);
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                test.Sort((GeoObjectMarker m1, GeoObjectMarker m2) =>
                {

                    try
                    {
                        if (m1.objectIcon == "1" || string.IsNullOrEmpty(m1.objectIcon))
                        {
                            if (m2.objectIcon != "1")
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            if (m2.objectIcon == "1" || string.IsNullOrEmpty(m2.objectIcon))
                            {
                                return -1;
                            }
                        }


                        int icon1 = (string.IsNullOrEmpty(m1.objectIcon) ? 1 : int.Parse(m1.objectIcon));
                        int icon2 = (string.IsNullOrEmpty(m2.objectIcon) ? 1 : int.Parse(m2.objectIcon));


                        if (icon1 != icon2) return icon1 > icon2 ? -1 : 1;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }



                    if (m1.address == null || m2.address == null) return 0;
                    try
                    {


                        if (m1.address.ToLower().Contains("дом"))
                        {
                            if (m2.address.ToLower().Contains("дом"))
                            {

                                var m1spl = m1.address.Split(',');
                                var m1home = m1spl.FirstOrDefault((string el) => { string str = el.ToLower(); return str.Contains("дом") || str.Contains("д ") || str.Contains("д."); },
                                    string.Empty);

                                var m2spl = m2.address.Split(',');
                                var m2home = m2spl.FirstOrDefault((string el) => { string str = el.ToLower(); return str.Contains("дом") || str.Contains("д ") || str.Contains("д."); },
                                    string.Empty);

                                if (string.IsNullOrEmpty(m1home))
                                {
                                    if (string.IsNullOrEmpty(m2home))
                                    {
                                        return m1.address.Length >= m2.address.Length ? 1 : -1;
                                    }
                                    return -1;
                                }
                                if (string.IsNullOrEmpty(m2home))
                                {
                                    return 1;
                                }

                                int m1hnumber = CutInt(m1home);
                                int m2hnumber = CutInt(m2home);


                                if (m1hnumber != m2hnumber) return (m1hnumber > m2hnumber ? 1 : -1);

                                var m1apar = m1spl.FirstOrDefault((string el) => { string str = el.ToLower(); return str.Contains("кв.") || str.Contains("кв ") || str.Contains("квартира"); }, string.Empty);

                                var m2apar = m2spl.FirstOrDefault((string el) => { string str = el.ToLower(); return str.Contains("кв.") || str.Contains("кв ") || str.Contains("квартира"); }, string.Empty);

                                if (string.IsNullOrEmpty(m1apar))
                                {
                                    if (string.IsNullOrEmpty(m2apar))
                                    {
                                        return m1.address.Length >= m2.address.Length ? 1 : -1;
                                    }
                                    return -1;
                                }
                                if (string.IsNullOrEmpty(m2apar))
                                {
                                    return 1;
                                }

                                int m1anumber = CutInt(m1apar);
                                int m2anumber = CutInt(m2apar);

                                if (m1anumber == m2anumber) return 0;
                                return (m1anumber > m2anumber ? 1 : -1);

                                return 0;
                            }
                            return -1;
                        }
                        if (m2.address.ToLower().Contains("дом"))
                        {
                            return 1;
                        }
                        if (m1.address.Length != m2.address.Length)
                            return m1.address.Length > m2.address.Length ? 1 : -1;
                        return 0;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        return 0;
                    }



                    return 0;
                });
                sw.Stop();
                Log.System("Sort Time: " + TimeOnly.FromTimeSpan(sw.Elapsed).ToString("H:mm:ss:fff"));
            }
            catch (Exception e)
            {
                Log.Error(e);
                res = resbkp;
            }

            return res;


        }
        public static void DeleteGeozoneContainers(string geo_guid, int contCount,float volume,string type_guid,string userGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                
                var Query = "CrateMate_RemoveFirstGeozoneContainer";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
              


                cmd.Parameters.AddWithValue("@geozone", geo_guid);
                cmd.Parameters.AddWithValue("@volume", volume);
                cmd.Parameters.AddWithValue("@type", type_guid);
                cmd.Parameters.AddWithValue("@id_user", userGuid);

                for (int i = 0;i<contCount ; i++)
                    SQL.Execute(cmd);

            }
        }
        public static List<GeoContainer> GetGeozoneContainers(string GeozoneGuid, SqlConnection con = null)
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            List<GeoContainer> result(SqlConnection _con)
            {
                var Query = "CrateMate_GetContainers";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_geo", GeozoneGuid);

                List<GeoContainer> res = new List<GeoContainer>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        GeoContainer c = new GeoContainer();
                        c.guid = r.GetValue(0).ToString();
                        c.typeGuid = r.GetValue(8).ToString();
                        c.containerNumber = int.Parse(r.GetValue(1).ToString());
                        c.title = r.GetValue(2).ToString();
                        c.volume = float.Parse(r.GetValue(5).ToString().Replace(".", ","));
                        c.type = r.GetValue(3).ToString();
                        c.typeShort = r.GetValue(4).ToString();
                        c.clientName = r.GetValue(6).ToString();
                        c.clientInn = r.GetValue(7).ToString();
                        c.count = 1;
                        bool IsMerged = false;
                        foreach (var v in res)
                        {
                            if (v.volume == c.volume && v.type == c.type)
                            {
                                v.count++;
                                IsMerged = true;
                                break;
                            }
                        }
                        if(!IsMerged) res.Add(c);
                    }
                    return res;
                }
                return null;
            }
        }
        public static Dictionary<string, ObjectType> LastKnownObjectTypes = new Dictionary<string, ObjectType>(10);
        public static Dictionary<string, ObjectType> LastKnownObjectSubTypes = new Dictionary<string, ObjectType>(110);
        public static List<PrimaryObjectType>  GetObjectTypes()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                LastKnownObjectTypes.Clear();
                LastKnownObjectSubTypes.Clear();
                Dictionary<string, PrimaryObjectType> types = new Dictionary<string, PrimaryObjectType>(10);
                var res = new List<PrimaryObjectType>();
                _con.Open();
                var Query = "CrateMate_GetObjectsTypes";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

            
                using (var r = SQL.StartRead(cmd))
                {
                  
                    while (r.Read())
                    {

                        var v = new PrimaryObjectType();
                        v.guid = r.GetValue(0).ToString();
                        v.name = r.GetValue(1).ToString();
                        v.icon = r.GetValue(2).ToString();
                        v.binId = int.Parse(r.GetValue(3).ToString());
                       // Log.Text(v.guid+" " + v.name);
                        res.Add(v);
                        types.Add(v.guid, v);
                        LastKnownObjectTypes.Add(v.guid, v);
                    }
                }

                Query = "CrateMate_GetObjectsSubTypes";
                cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
               
        

                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {

                        var v = new ObjectType();
                        v.guid = r.GetValue(0).ToString();
                        v.name = r.GetValue(1).ToString();
                        v.icon = r.GetValue(2).ToString();
                       
                        string PrimType = r.GetValue(3).ToString();
                        if(!r.IsDBNull(4))v.binId = int.Parse(r.GetValue(4).ToString());
                        if (types.TryGetValue(PrimType,out var pt))
                        {
                            if (pt.subTypes == null) pt.subTypes = new List<ObjectType>();

                            pt.subTypes.Add(v);
                            LastKnownObjectSubTypes.Add(v.guid, v);
                        }

                        

                    }
                }
                return res;
            }
        }
        public static void EditGeoObject(GeoObjectEditRequest req,string UserGuid) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_UpdateObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;


                if (req.newPosition==null || req.newPosition.mLatitude == 0) req.newPosition = req.originalPosition;

                
                cmd.Parameters.AddWithValue("@id", req.guid);
                cmd.Parameters.AddWithValue("@Code_type", req.typeGuid);
                cmd.Parameters.AddWithValue("@code_Subtype", req.subTypeGuid);
                cmd.Parameters.AddWithValue("@Title", req.name);
                cmd.Parameters.AddWithValue("@lat", req.newPosition.mLatitude);
                cmd.Parameters.AddWithValue("@lon", req.newPosition.mLongitude);
                cmd.Parameters.AddWithValue("@area", req.area);
                cmd.Parameters.AddWithValue("@isRentObj", req.isRentor);
                cmd.Parameters.AddWithValue("@idUser", UserGuid);
                cmd.Parameters.AddWithValue("@comment", req.commantary);

                SQL.Execute(cmd);

            }
        }
        public static List<SingleUserTrackPoint> GetLastUsersPoint4Date(DateTime date,string WatcherUserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetLastPosUsers4day";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@id_userWatcher", WatcherUserGuid);
                //8c84d447-76c0-429d-b29d-44a1dce61767;
                var res = new List<SingleUserTrackPoint>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        SingleUserTrackPoint tt = new SingleUserTrackPoint();
                        tt.userGuid = r.GetValue(0).ToString();
                        tt.time = r.GetDateTime(1);
                        tt.point = new GeoPoint(double.Parse(r.GetValue(3).ToString()), double.Parse(r.GetValue(2).ToString()));
                        tt.userName = r.GetValue(4).ToString();
                        res.Add(tt);
                    }
                }
                return res;
            }

        }
        public static List<SingleUserTrackPoint> GetLastCarsPoints4Day(DateTime date)
        {
            using (SqlConnection _con = new SqlConnection(TracerSqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetLastAvtoPosition";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@date", date);
                //8c84d447-76c0-429d-b29d-44a1dce61767;
                var res = new List<SingleUserTrackPoint>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        SingleUserTrackPoint tt = new SingleUserTrackPoint();
                        tt.userGuid = r.GetValue(0).ToString();
                        tt.time = r.GetDateTime(1).AddHours(7);
                        tt.point = new GeoPoint(double.Parse(r.GetValue(3).ToString()), double.Parse(r.GetValue(2).ToString()));
                        tt.userName = r.GetValue(4).ToString();
                        res.Add(tt);
                    }
                }
                return res;
            }

        }
        public static List<TrackPoint> GetCarTrack_ForMig(string id_car, DateTime dateFrom,DateTime dateTo)
        {
            using (SqlConnection _con = new SqlConnection(TracerSqlconnectionString))
            {
                _con.Open();

                
                var Query = "GetTrackForCar";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_car", id_car);
                cmd.Parameters.AddWithValue("@dateFrom", dateFrom);
                cmd.Parameters.AddWithValue("@dateTo", dateTo);

          
                var res = new List<TrackPoint>(30000);
               // Log.sql(cmd);
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        // Log.Text("Read");
                        TrackPoint tp = new TrackPoint();
                        tp.time = r.GetDateTime(0).AddHours(7);
                        tp.point = new GeoPoint(double.Parse(r.GetValue(1).ToString()), double.Parse(r.GetValue(2).ToString()));
                        res.Add(tp);
                    }
                }
                return res;
            }
        }
        public static List<TrackPoint> GetCarTrack(string userGuid, DateTime date)
        {
            using (SqlConnection _con = new SqlConnection(TracerSqlconnectionString))
            {
                _con.Open();

                var TodayStart = date;

                TodayStart = new DateTime(TodayStart.Year, TodayStart.Month, TodayStart.Day, 0, 0, 0);
                var TodayEnd = TodayStart.AddHours(24);

                var Query = "GetTrackForCar";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@car_guid", userGuid);
                cmd.Parameters.AddWithValue("@date", TodayStart);
                cmd.Parameters.AddWithValue("@time_from",new TimeOnly(0,0,0).ToString());
                cmd.Parameters.AddWithValue("@time_to", new TimeOnly(23, 59, 59).ToString() );

                var res = new List<TrackPoint>(50);
                Log.sql(cmd);
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        // Log.Text("Read");
                        TrackPoint tp = new TrackPoint();
                        tp.time = r.GetDateTime(0).AddHours(7);
                        tp.point = new GeoPoint(double.Parse(r.GetValue(1).ToString()), double.Parse(r.GetValue(2).ToString()));
                        res.Add(tp);
                    }
                }
                return res;
            }
        }
        public static List<SingleUserTrackPoint> GetLastUsersPoint()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetLastPosUsersToday";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                //8c84d447-76c0-429d-b29d-44a1dce61767;
                var res = new List<SingleUserTrackPoint>();
                using (var r = SQL.StartRead(cmd))
                {
                    
                    while (r.Read())
                    {
                        SingleUserTrackPoint tt = new SingleUserTrackPoint();
                        tt.userGuid = r.GetValue(0).ToString();
                        tt.time = r.GetDateTime(1);
                        tt.point = new GeoPoint(double.Parse(r.GetValue(3).ToString()), double.Parse(r.GetValue(2).ToString()));
                        tt.userName = r.GetValue(4).ToString();
                        res.Add(tt);
                    }
                }
                return res;
            }

        }
        public static List<TrackPoint> GetUserTrack(string userGuid, DateTime date)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                var TodayStart = date;

                TodayStart = new DateTime(TodayStart.Year, TodayStart.Month, TodayStart.Day,0,0,0);
                var TodayEnd = TodayStart.AddHours(24);

                var Query = "CleanIT_PosUser";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_user", userGuid);
                cmd.Parameters.AddWithValue("@dateBegin", TodayStart);
                cmd.Parameters.AddWithValue("@dateEnd", TodayEnd);

                var res = new List<TrackPoint>(50);
                Log.sql(cmd);
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                       // Log.Text("Read");
                        TrackPoint tp = new TrackPoint();
                        tp.time = r.GetDateTime(0);
                        tp.point = new GeoPoint(double.Parse(r.GetValue(2).ToString()), double.Parse(r.GetValue(1).ToString()));
                        res.Add(tp);
                    }
                }
                return res;
            }
        }
      
        public static GeoObject GetGeoObject(string objGuid, SqlConnection con = null)
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            GeoObject result(SqlConnection _con)
            {

                var Query = "CrateMate_GetObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_Obj", objGuid);

                var res = new GeoObject();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        res.name = r.GetValue(0).ToString();
                        res.address = r.GetValue(1).ToString();
                        res.document = r.GetValue(2).ToString();
                        if (!r.IsDBNull(3)) res.dateStart = r.GetDateTime(3);
                        if (!r.IsDBNull(4)) res.dateEnd = r.GetDateTime(4);
                        res.isDogActive = r.GetInt32(5) == 1;
                        res.client = new Client();
                        if (!r.IsDBNull(6)) res.client.name = r.GetValue(6).ToString();
                        if (!r.IsDBNull(7)) res.client.inn = r.GetValue(7).ToString();
                        if (!r.IsDBNull(8)) res.client.ogrn = r.GetValue(8).ToString();
                        if (!r.IsDBNull(9)) res.objectType = r.GetValue(9).ToString();
                        if (!r.IsDBNull(10)) res.subObjctType = r.GetValue(10).ToString();
                        if (!r.IsDBNull(11)) res.chained = int.Parse(r.GetValue(11).ToString());
                        if (!r.IsDBNull(12)) res.binId = r.GetValue(12).ToString();
                        if (!r.IsDBNull(13)) res.area = float.Parse(r.GetValue(13).ToString());
                        if (!r.IsDBNull(14)) res.isRentor = r.GetBoolean(14);
                        

                    }
                }
                return res;
            }
        }
        public static List<ObjectEventType> GetObjectEventsInfos()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetTypeEventObj";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;


                var res = new List<ObjectEventType>();
                using (var r = SQL.StartRead(cmd))
                {

                    while (r.Read())
                    {
                        var t = new ObjectEventType();
                        if (r.IsDBNull(1)) continue;
                        t.guid  = r.GetValue(0).ToString();
                        t.name  = r.GetValue(1).ToString();
                        t.code  = r.GetValue(2).ToString();
                        t.title = t.name;
                        t.photoLvl = int.Parse(r.GetValue(5).ToString());
                        t.coordsLvl = int.Parse(r.GetValue(6).ToString());
                        t.addAction = r.GetValue(7).ToString();

                        // t.value = r.GetValue(3).ToString();
                        // t.posUI = r.IsDBNull(4) ? BoolExtensions.SuperParse(r.GetValue(4).ToString()) : false;
                        res.Add(t);
                    }
                    
                }
                return res;
            }
        }
        public class InnerServer_Geozone
        {
            public string title;
            public string address;
            public double lat;
            public double lon;
        }
        public static InnerServer_Geozone InnerServer_GetGeozone(string GeozoneGuid, SqlConnection con = null)
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            InnerServer_Geozone result(SqlConnection _con)
            {
                var Query = "CrateMate_InnerServer_GetGeozone";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", GeozoneGuid);

                //List<Geozone> res = new List<Geozone>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        InnerServer_Geozone gg = new InnerServer_Geozone();
                        if (!r.IsDBNull(0)) gg.title = r.GetValue(0).ToString();
                        gg.address = gg.address.Replace("Кемеровская область -Кузбасс, ", "");
                        if (!r.IsDBNull(1)) gg.address = r.GetValue(1).ToString();
                        if (!r.IsDBNull(2)) gg.lat = double.Parse(r.GetValue(2).ToString());
                        if (!r.IsDBNull(3)) gg.lon = double.Parse(r.GetValue(3).ToString());
                        return gg;
                    }
                }
                return null;
            }
        }

        public static Geozone GetGeozone(string GeozoneGuid, SqlConnection con = null)
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            Geozone result(SqlConnection _con)
            {
                var Query = "CrateMate_GetGeo";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", GeozoneGuid);

                //List<Geozone> res = new List<Geozone>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        Geozone Sg = new Geozone();
                        Sg.guid = r.GetValue(0).ToString();
                        Sg.name = r.GetValue(3).ToString();
                        Sg.address = r.GetValue(5).ToString();
                        if (!r.IsDBNull(6)) Sg.binId = r.GetValue(6).ToString();
                        Sg.color = int.Parse(r.GetValue(8).ToString());
                        Sg.position = new GeoPoint(float.Parse(r.GetValue(2).ToString()), float.Parse(r.GetValue(1).ToString()));

                        if (!r.IsDBNull(7))
                            Sg.creationDate = r.GetDateTime(7);
                        Sg.commentary = r.GetValue(9).ToString();
                        if(!r.IsDBNull (10))
                        Sg.creator = r.GetValue(10).ToString();
                        Sg.groundType = r.GetValue(11).ToString();
                        Sg.status = r.GetValue(12).ToString();
                        Sg.statusCode = r.GetValue(13).ToString();
                        if (!r.IsDBNull(14)) Sg.geozoneGroup   = r.GetValue(14).ToString();
                        if (!r.IsDBNull(16)) Sg.roof           = r.GetBoolean(16);
                        if (!r.IsDBNull(17)) Sg.fence          = r.GetBoolean(17);
                        if (!r.IsDBNull(18)) Sg.gate           = r.GetBoolean(18);
                        if (!r.IsDBNull(19)) Sg.barrier        = r.GetBoolean(19);
                        if (!r.IsDBNull(20)) Sg.haveregime     = r.GetBoolean(20);
                        if (!r.IsDBNull(21)) Sg.lastEvent      = r.GetDateTime(21);
                        if (!r.IsDBNull(22)) Sg.clientName     = r.GetValue(22).ToString();
                        if (!r.IsDBNull(23)) Sg.clientInn      = r.GetValue(23).ToString();
                        if (!r.IsDBNull(25)) Sg.area           = float.Parse(r.GetValue(25).ToString());
                        if (!r.IsDBNull(26)) Sg.geozoneGroup   = r.GetValue(26).ToString();
                        if (!r.IsDBNull(28)) Sg.isAddressCustom= (r.GetInt32(28) ==1);
                        if (!r.IsDBNull(29)) Sg.lot            = r.GetValue(29).ToString();
                        if (!r.IsDBNull(30)) Sg.subDistr       = r.GetValue(30).ToString();
                        if (!r.IsDBNull(31)) Sg.subDistrZone   = r.GetValue(31).ToString();
                        if (!r.IsDBNull(32)) Sg.archDescr = r.GetValue(32).ToString();
                        //  Sg.creator = r.GetValue(11).ToString();
                        //   Sg.creationDate = r.GetDateTime(12);
                        // LoadGeozoneEvents(Sg);

                        return Sg;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// type - 0 - start, 1 - finish 2- pause
        /// </summary>
        /// <param name="type"></param>
        /// <param name="UserGuid"></param>
        /// <param name="position"></param>
        /// <param name="GeozoneGuid"></param>
        /// <param name="TaskGuid"></param>
        public static void RegisterClearEvents(int type,string UserGuid,GeoPoint position,string GeozoneGuid, string TaskGuid,IFormFileCollection files)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "";
               EventsList ev = EventsList.clean_start;
                switch (type)
                {
                    case 0: ev = EventsList.clean_start; Query = "CrateMate_CleanBegin"; break;
                    case 1: ev = EventsList.clean_finish; Query = "CrateMate_CleanFinish"; break;
                    case 2: ev = EventsList.clean_stop; Query = "CrateMate_CleanStop"; break;
                }
        
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", TaskGuid);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);

                //EventsList ev = EventsList.clean_start;
                //switch (type)
                //{
                //    case 0:ev = EventsList.clean_start;break;
                //    case 1:ev = EventsList.clean_finish;break;
                //    case 2:ev = EventsList.clean_stop;break;
                //}

                SQL.Execute(cmd);
                SQL.InsertNewGeozoneEvents(GeozoneGuid, UserGuid, new UniversalEvent() { type = Enum.GetName(ev) },position,null);
                InnerServer_Geozone ggg = null;
                try
                {
                    ggg = SQL.InnerServer_GetGeozone(GeozoneGuid, _con);
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                }

                if (ev==EventsList.clean_start)SQL.ParseFilesToSomething(TaskGuid,files,UserGuid,"Фото до",$"{DateTime.Now.ToString("dd.MM.yyyy")}_{ggg?.title}_{ggg?.address}");
                if (ev == EventsList.clean_finish) SQL.ParseFilesToSomething(TaskGuid,files,UserGuid,"Фото после", $"{DateTime.Now.ToString("dd.MM.yyyy")}_{ggg?.title}_{ggg?.address}");

            }
        }
        public static void DeleteFile(string guid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_DeleteFile";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_file", guid);
             
                SQL.Execute(cmd);
            }
            
        }
        public static void ParseFilesToSomething(string SomethingGuid, IFormFileCollection files, string UserGuid, string? AllFilesDescr =null, string? fileName = null)
        {
            ParseFilesToSomething(SomethingGuid, files.ToArray(),UserGuid, AllFilesDescr,fileName);
        }
        public static void ParseNamedFilesToSomething(string SomethingGuid, IFormFile[] files, string UserGuid, string? AllFilesDescr = null)
        {
            foreach (var f in files)
            {
               // string ex = Path.GetExtension(f.FileName);
                using (MemoryStream stream = new MemoryStream())
                {
                    f.CopyTo(stream);
                    SQL.AttachFileToSomething(SomethingGuid, f.FileName,FileType.foto, stream.GetBuffer(), UserGuid, AllFilesDescr);

                }
            }
        }
        public static void ParseFilesToSomething(string SomethingGuid, IFormFile[] files,string UserGuid, string? AllFilesDescr = null, string? fileName = null)
        {
            int i = 0;
            foreach (var f in files)
            {
                string ex = Path.GetExtension(f.FileName);
                using (MemoryStream stream = new MemoryStream())
                {
                    f.CopyTo(stream);
                    
                    SQL.AttachFileToSomething(SomethingGuid, $"{fileName} {(i==0?"":$" ({i})")}{ex}", FileType.foto, stream.GetBuffer(), UserGuid, AllFilesDescr);
                    i++;

                }
            }
        }
        public static void FinishAnyTask(string taskGuid,string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_TaskFinish";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", taskGuid);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);

                SQL.Execute(cmd);
            }
        }
        public static List<GeozoneMarker> GetGeozoneMarkersByList(List<string> geoz_ids, SqlConnection con = null)
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            List<GeozoneMarker> result(SqlConnection _con)
            {
                var Query = "CrateMate_GetGeozoneMarkers";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithArrayValue("@Guid_List", geoz_ids);

                var res = DefaultGeozoneMarkersRead(cmd);

                return res;
            }
        }
        public static List<GeozoneMarker> GetGeozoneMarkersByList<T>(List<T> geoz_ids, SqlConnection con = null) where T : IHaveId
        {
            if (con == null)
            {
                using (SqlConnection _con = new SqlConnection(SqlconnectionString))
                {
                    _con.Open();
                    return result(_con);
                }
            }
            else return result(con);
            List<GeozoneMarker> result(SqlConnection _con)
            {
                var Query = "CrateMate_GetGeozoneMarkers";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Guid_List", geoz_ids.ToGuidList());

                var res  = DefaultGeozoneMarkersRead(cmd);

                return res;
            }
        }
        public class GeozoneVisitRecord
        {
            /// <summary>
            /// Car _title - Name
            /// </summary>
            [JsonPropertyName("c")]
            public string CarTitle { get; set; }
            /// <summary>
            ///  Visit Enter point time
            /// </summary>
            [JsonPropertyName("e")]
            public DateTime VisitDate { get; set; }
            /// <summary>
            /// Is duration
            /// </summary>
          //  [JsonPropertyName("d")]
         //   public string Duration { get; set; }

            [JsonPropertyName("s")]
            public string Status { get; set; }
            [JsonPropertyName("v")]
            public TimeSpan DurationValue { get; set; }
        }
        public static List<GeozoneVisitRecord> GetGeozoneVisits(string geozoneGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetAllGeozoneVisits";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_geo", geozoneGuid);


                List<GeozoneVisitRecord> res = new List<GeozoneVisitRecord>();
                
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        var v = new GeozoneVisitRecord();
                        if (!r.IsDBNull(0))
                            v.CarTitle = r.GetValue(0).ToString();
                        if (r.IsDBNull(1)) continue;
                        v.VisitDate = r.GetDateTime(1);
                       // if (r.IsDBNull(2)) continue;
                       // v.Duration = r.GetValue(2).ToString();
                        if (!r.IsDBNull(3)) 
                        v.Status = r.GetValue(3).ToString();
                    if (!r.IsDBNull(4))
                        v.DurationValue = r.GetTimeSpan(4);
                    res.Add(v);
                    }
                }
                return res;
            }
            }
        public static List<GeozoneTask> GetUserTasks(string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetTasks";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", UserGuid);

                

                List<GeozoneTask> res = new List<GeozoneTask>();
                Dictionary<string, GeozoneTask> idMap = new Dictionary<string, GeozoneTask>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        GeozoneTask task = new GeozoneTask();
                        string GeoGuid = r.GetValue(0).ToString();
                        task.geozoneGuid = GeoGuid;
                        //task.geozone = GetGeozone(GeoGuid);
                       // if (task.geozone == null) { Log.Warning("Задача с несуществующей геозоной !"); continue; }
                        task.guid = r.GetValue(1).ToString(); 

                        task.description = r.GetValue(3).ToString();
                        task.statusCode = r.GetValue(4).ToString();
                        task.status = r.GetValue(5).ToString();
                        task.title = r.GetValue(7).ToString();
                        task.type = r.GetValue(8).ToString();
                        if (!r.IsDBNull(9))
                        task.deadPlan = r.GetDateTime(9);

                        if (!r.IsDBNull(10))
                            task.dateFact = r.GetDateTime(10);
                        if (!r.IsDBNull(11)) task.isPhotoRequired = r.GetBoolean(11);
                        else task.isPhotoRequired = true;
                      idMap.TryAdd(task.geozoneGuid, task);
                        res.Add(task);
                    }
                }

                var mrkrs = SQL.GetGeozoneMarkersByList(res, _con);

                foreach (var v in mrkrs)
                {
                    if (idMap.TryGetValue(v.guid, out var task))
                    {
                        task.marker = v;
                    }
                    else
                    {
                        Log.Warning("GetUserTasks => Маркер не был найден для задачи. вероятно геозона указанная в задаче более не существует");
                    }
                    var rese = res.Where(x => (x.marker != null)).ToList();
                }
               

                return res.Where(x=> (x.marker != null)).ToList();
                //user.tasks = res;
               // geozone.events = res;
            }
        }
        public static void TableBulkInsert(ref DataTable dt, string TargetTableName, Dictionary<string, string> FieldsMapping = null)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(SqlconnectionString))
            {
                bulkCopy.BulkCopyTimeout = 600; // in seconds
                bulkCopy.DestinationTableName = "dbo." + TargetTableName;
                if (FieldsMapping != null)
                {
                    dt.CaseSensitive = false;
                    foreach (var v in FieldsMapping)
                    {
                        bulkCopy.ColumnMappings.Add(v.Key, v.Value);
                    }
                }
                else
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dt.CaseSensitive = false;
                        bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                    }
                }
                Log.sql($"BULK insert into {TargetTableName} With {dt.Rows.Count} rows");
                //bulkCopy.
                bulkCopy.WriteToServer(dt);
            }
        }
        public static void LoadBinManCarsInTemp(List<BinManCarParser.BinManCar> Cars)
        {
            DataTable dt = new DataTable();

            var LoadDate = DateTime.Now;

            dt.Columns.Add("id",typeof(Guid));
            dt.Columns.Add("title");
            dt.Columns.Add("gos_num");
            dt.Columns.Add("IMEI");
            dt.Columns.Add("cont_count");
            dt.Columns.Add("dateLoadInDb");
            foreach (var c in Cars)
            {
                c.dbGuid = Guid.NewGuid();
                dt.Rows.Add(c.dbGuid, c.name, c.gosNum, c.IMEI, c.ConateinerTypes.Count(), LoadDate);
            }
            SQL.TableBulkInsert(ref dt, "temp_BinMan_Cars", new Dictionary<string, string>()
            {
               { "id", "id" },
               { "title", "title"},
               { "gos_num", "gos_num"},
               { "IMEI", "IMEI"},
               { "cont_count", "cont_count"},
               { "dateLoadInDb", "dateLoadInDb"},
            });
            dt.Dispose();
            dt = new DataTable();

            

            dt.Columns.Add("id_temp_car", typeof(Guid));
            dt.Columns.Add("id_type_cont", typeof(Guid));

            foreach (var c in Cars)
            {
                foreach (var Cont in c.ConateinerTypes)
                {
                    dt.Rows.Add(c.dbGuid,new Guid(Cont));
                }
            }
            SQL.TableBulkInsert(ref dt, "temp_BinMan_CarContainers", new Dictionary<string, string>()
            {
               { "id_temp_car", "id_temp_car"},
               { "id_type_cont", "id_type_cont"}
            });
            dt.Dispose();
        }
        public static void LoadGeozoneEvents(Geozone geozone,string UserGuid)
        {

                geozone.events = GetGeozoneEvents(geozone.guid,UserGuid);
      
        }
        public static List<UniversalEvent> GetGeozoneEvents(string GeozoneGuid,string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetGeoEvents";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", GeozoneGuid);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);


                List<UniversalEvent> res = new List<UniversalEvent>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        UniversalEvent Ge = new UniversalEvent();
                        Ge.guid = r.GetValue(0).ToString();
                        Ge.type = r.GetValue(1).ToString();
                        Ge.dateTime = r.GetDateTime(2);
                        Ge.description = r.GetValue(4).ToString();
                        Ge.isCanBeDeleted = r.GetInt32(5)==1;
                        res.Add(Ge);
                    }
                }
                res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static List<Status> GetPossibleTaskStatuses()
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {

                _con.Open();
                var Query = "CrateMate_GetGeozoneTasks";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;


                List<Status> res = new List<Status>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        try
                        {
                            Status st = new Status();

                            st.guid = r.GetValue(0).ToString();
                            if(!r.IsDBNull(1))  st.code = r.GetValue(1).ToString();
                            if(!r.IsDBNull(2))  st.title = r.GetValue(2).ToString();
                            else { continue; }
                            res.Add(st);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Some task invalid !", ex);
                        }
                    }
                }
                // res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static List<GeozoneTask> GetSourceTasks_Deprecated(string SourceGuid, string? UserGuid, bool? withHistory, string source_type)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {

                _con.Open();
                var Query = "CrateMate_GetGeozoneTasks_Deprecated";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@id_geo",id_geo);
                cmd.Parameters.AddWithValue("@id_user", string.IsNullOrEmpty(UserGuid) ? DBNull.Value : UserGuid);
                cmd.Parameters.AddWithValue("@withHistory", withHistory.HasValue ? withHistory.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@source_type", source_type);
                cmd.Parameters.AddWithValue("@id_source", SourceGuid);

                List<GeozoneTask> res = new List<GeozoneTask>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        try
                        {
                            GeozoneTask task = new GeozoneTask();
                            string GeoGuid = r.GetValue(0).ToString();
                            task.geozoneGuid = GeoGuid;
                            // task.geozone = GetGeozone(GeoGuid);
                            // if (task.geozone == null) { Log.Warning("Задача с несуществующей геозоной !"); continue; }
                            task.guid = r.GetValue(1).ToString();

                            task.description = r.GetValue(3).ToString();
                            task.statusCode = r.GetValue(4).ToString();
                            task.status = r.GetValue(5).ToString();
                            task.title = r.GetValue(7).ToString();
                            task.type = r.GetValue(8).ToString();
                            if (!r.IsDBNull(9))
                                task.deadPlan = r.GetDateTime(9);

                            if (!r.IsDBNull(10))
                                task.dateFact = r.GetDateTime(10);

                            task.canIExecute = r.GetBoolean(11);

                            task.rawImages = SQL.GetSomethingFilesInfo(task.guid, null);
                            task.executor = r.GetValue(12).ToString();
                            task.isImportant = r.GetBoolean(13);
                            task.statusGuid = r.GetValue(14).ToString();
                            //  idMap.TryAdd(task.geozone.guid, task);
                            res.Add(task);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Some task invalid !", ex);
                        }
                    }
                }
                // res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static List<User> GetUserDivisionUsers(string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {

                _con.Open();
                var Query = "CrateMate_GetUsersFromDivisionOfUser";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@id_geo",id_geo);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);


                List<User> res = new List<User>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                      
                        User u = new User();

                        u.guid = r.GetValue(0).ToString();
                        u.fio = r.GetValue(1).ToString();
                        res.Add(u);
                    }
                }
                // res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static void SetTaskExecutor (string UserGuid ,SetTaskExecutorRequest req)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();

                foreach (var tg in req.taskGuid) { 
                var Query = "CrateMate_SetTaskExecutor";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_user", UserGuid);
                cmd.Parameters.AddWithValue("@id_executor", req.executorGuid);
                cmd.Parameters.AddWithValue("@id_task", tg);
                cmd.Parameters.AddWithValue("@datePlan", req.datePlan.HasValue ? req.datePlan.Value:DBNull.Value);


                SQL.Execute(cmd);
                }
            }
        }
        public static List<GeozoneTask> GetSourceTasks(string SourceGuid, string? UserGuid,TaskFilters filters, string source_type,string? FilterPreset )
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
               
                _con.Open();
                var Query = "CrateMate_GetGeozoneTasks";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@id_geo",id_geo);
                cmd.Parameters.AddWithValue("@id_user", string.IsNullOrEmpty(UserGuid) ? DBNull.Value : UserGuid);
                cmd.Parameters.AddWithValue("@source_type", source_type);
                cmd.Parameters.AddWithValue("@id_source", SourceGuid);
                cmd.Parameters.AddWithValue("@date_planFrom", filters.planDateFrom.HasValue ? filters.planDateFrom.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@date_planTo", filters.planDateTo.HasValue ? filters.planDateTo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@OnlyWithNullDatePlan", filters.onlyNullDatePlan);
    
                cmd.Parameters.AddWithArrayValue("@status", filters.statusGuids);
                cmd.Parameters.AddWithValue("@filterPreset", string.IsNullOrEmpty(FilterPreset) ? DBNull.Value : FilterPreset);
                Dictionary<string, List<GeozoneTask>> Geozone2TaskMap = new Dictionary<string, List<GeozoneTask>>();
                List<string> GeozonesIds = new List<string>();
                List<GeozoneTask> res = new List<GeozoneTask>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        try
                        {
                            GeozoneTask task = new GeozoneTask();
                            string GeoGuid = r.GetValue(0).ToString();
                            task.geozoneGuid = GeoGuid;
                            // task.geozone = GetGeozone(GeoGuid);
                            // if (task.geozone == null) { Log.Warning("Задача с несуществующей геозоной !"); continue; }
                            task.guid = r.GetValue(1).ToString();

                            task.description = r.GetValue(3).ToString();
                            task.statusCode = r.GetValue(4).ToString();
                            task.status = r.GetValue(5).ToString();
                            task.title = r.GetValue(7).ToString();
                            task.type = r.GetValue(8).ToString();
                            if (!r.IsDBNull(9))
                                task.deadPlan = r.GetDateTime(9);

                            if (!r.IsDBNull(10))
                                task.dateFact = r.GetDateTime(10);

                            task.canIExecute = r.GetBoolean(11);

                            task.rawImages = SQL.GetSomethingFilesInfo(task.guid, null);
                            if (!r.IsDBNull(12)) task.executor = r.GetValue(12).ToString();
                            task.isImportant = r.GetBoolean(13);
                            if (!r.IsDBNull(14)) task.statusGuid = r.GetValue(14).ToString();
                            if (!r.IsDBNull(15)) task.executor_guid = r.GetValue(15).ToString();
                            task.number = r.GetValue(16).ToString();
                            if (!r.IsDBNull(17)) task.dateCreate = r.GetDateTime(17);
                            if (!r.IsDBNull(18)) task.isPhotoRequired = r.GetBoolean(18);
                            else { task.isPhotoRequired = true; }
                            //  idMap.TryAdd(task.geozone.guid, task);
                            if (Geozone2TaskMap.TryGetValue(task.geozoneGuid,out var tl))
                            {
                                tl.Add(task);
                            }
                            else
                            Geozone2TaskMap.Add(task.geozoneGuid, new List<GeozoneTask>() { task});
                            GeozonesIds.Add(task.geozoneGuid);
                            res.Add(task);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Some task invalid !", ex);
                        }
                    }
                }

                var Geozones = GetGeozoneMarkersByList(GeozonesIds,_con);
                foreach(var g in Geozones)
                {
                    if(Geozone2TaskMap.TryGetValue(g.guid,out var tsk))
                    {
                        foreach(var t in tsk)
                            t.marker = g;
                    }
                }
                // res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static List<UniversalEvent> GetObjectEvents(string ObjectGuid,string UserGuid)
        {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                _con.Open();
                var Query = "CrateMate_GetObjEvents";
                var cmd = new SqlCommand(Query, _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", ObjectGuid);
                cmd.Parameters.AddWithValue("@id_user", UserGuid);


                List<UniversalEvent> res = new List<UniversalEvent>();
                using (var r = SQL.StartRead(cmd))
                {
                    while (r.Read())
                    {
                        UniversalEvent Ge = new UniversalEvent();
                        Ge.guid = r.GetValue(0).ToString();
                        Ge.type = r.GetValue(1).ToString();
                        Ge.dateTime = r.GetDateTime(2);
                        Ge.description = r.GetValue(4).ToString();
                        Ge.isCanBeDeleted = r.GetInt32(5)==1;
                        res.Add(Ge);
                    }
                }
                res.Sort((x, y) => DateTime.Compare(y.dateTime, x.dateTime));
                return res;
            }
        }
        public static bool ReadBaseData(out List<Districts> dists , out List<ContainerType> containers, out List<GeozoneType> geoTypes) {
            using (SqlConnection _con = new SqlConnection(SqlconnectionString))
            {
                dists = new List<Districts>(25);
                containers = new List<ContainerType>(10);
                geoTypes = new List<GeozoneType>(6);
                try
                {
                    _con.Open();


                    var Query = "CrateMate_GetDistricts";
                    var cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    var DistMap = new Dictionary<string, Districts>();
                    using (var r = StartRead(cmd))
                    {
                        while (r.Read())
                        {
                            Districts dist = new Districts();
                            dist.title = r.GetString(1);
                            dist.guid = r.GetValue(0).ToString();
                            dist.analyticGroupId = r.GetValue(2).ToString();
                            DistMap.TryAdd(dist.guid, dist);
                            dists.Add(dist);
                        }
                    }


                    Query = "CrateMate_GetSubDistricts";
                    cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    var SubDisMap = new Dictionary<string, SubDistrict >();
                    using (var r = StartRead(cmd))
                    {
                        while (r.Read())
                        {
                            SubDistrict sub = new SubDistrict();
                            sub.title = r.GetString(1);
                            sub.guid = r.GetValue(0).ToString();

                            var disGuid = r.GetValue(2).ToString();
                            if (!DistMap.TryGetValue(disGuid, out var ds))
                            {
                                if (sub.guid.ToUpper() == "A3FE2EBC-1373-45D4-8846-D18ABCAF2BBC")
                                {
                                    foreach (var v in DistMap)
                                    {
                                        v.Value.subDistricts.Insert(0, sub);
                                    }
                                }
                                else
                                    Log.Warning($"Под территоря не принадлежащая никому ! ({sub.title}) {{{sub.guid}}}"); continue;
                            }
                            else
                            {
                                SubDisMap.Add(sub.guid, sub);
                                ds.subDistricts.Add(sub);
                            }
                        }
                    }
                    Query = "CrateMate_GetSubDistrictsZones";
                    cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var r = StartRead(cmd))
                    {
                        while (r.Read())
                        {
                            SubDisrictZone zone = new SubDisrictZone();
                            zone.title = r.GetString(1);
                            zone.guid = r.GetValue(0).ToString();

                            var disGuid = r.GetValue(2).ToString();
                            if (!SubDisMap.TryGetValue(disGuid, out var ds))
                            {
                                if (zone.guid.ToUpper() == "A3FE2EBC-1373-45D4-8846-D18ABCAF2BBC")
                                {
                                    foreach (var v in SubDisMap)
                                    {
                                        v.Value.zones.Insert(0,zone);
                                    }
                                }
                                else
                                    Log.Warning($"Под под территоря не принадлежащая никому ! ({zone.title}) {{{zone.guid}}}"); continue;
                            }
                            else
                            {
                                ds.zones.Add(zone);
                            }
                        }
                    }



                    Query = "CrateMate_GetTypeContainers";
                    cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var r = StartRead(cmd))
                    {
                        while (r.Read())
                        {
                            ContainerType ct = new ContainerType();

                            ct.name = r.GetString(1);
                            ct.guid = r.GetValue(0).ToString();
                            ct.icon = int.Parse( r.GetValue(2).ToString() );
                            ct.shortname = r.GetValue(3).ToString();
                            ct.area = -1;
                                //float.Parse(r.GetValue(3).ToString());

                            containers.Add(ct);
                        }
                    }
                    Query = "CrateMate_GetTypeGeozone";
                    cmd = new SqlCommand(Query, _con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var r = StartRead(cmd))
                    {
                        while (r.Read())
                        {
                            GeozoneType gt = new GeozoneType();

                            gt.guid = r.GetValue(0).ToString();
                            gt.title = r.GetString(1);
                            gt.color = long.Parse(r.GetValue(2).ToString());
                            gt.descr = r.GetValue(3).ToString();

                            geoTypes.Add(gt);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return false;
                }

                //cmd.Parameters.AddWithValue("@task_guid");
            }
        }
        public static SqlDataReader StartRead(SqlCommand cmd)
        {
            //Console.WriteLine(sqlCommand);
            try
            {
                SqlDataReader sqldr = cmd.ExecuteReader();
                return sqldr;
            }
            catch (Exception e)
            {
                Log.sql(cmd);
                Log.Error(e.Message);
                return null;
            }
        }


    }

}