using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using DocumentFormat.OpenXml.Spreadsheet;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Newtonsoft.Json;
using Quartz.Impl.Triggers;
using System;
using System.Net.Http;
using System.Text;

namespace AndroidAppServer.Libs.BinMan
{
    public static class BinManDocuments
    {
        public class SimpResponse { 
            public string data { get; set; }
        }

        public struct CreateResponse
        {
            public string id { get; set; }
            public string success { get; set; }
        }
        public class BinManDogData
        {
            public string Db_Guid;
            /// <summary>
            /// Required for edit action
            /// </summary>
            public string bin_id;
            public string Type_BinManCode;
            public string Number;
          //  public string Company_BinManId;
           // public string Organization_BinManId;
            public string Client_BinManid;
            public string Group_BinManCode;
            public DateTime dateFrom;
            public DateTime? dateTo;
            public DateTime dateSign;
            public enum DogType
             {
                /// <summary>
                /// Контракт 
                /// </summary>
               CONTRACT = 170 ,
                /// <summary>
                /// Государственный контракт 
                /// </summary>
                GOS_CONTRACT = 169,
                /// <summary>
                /// Договор с юр. лицом на нежилые объекты
                /// </summary>
                LEGAL = 162,
                /// <summary>
                ///  Договор по 223-ФЗ
                /// </summary>
                LEGAL_FZ = 171,
                /// <summary>
                ///  Договор с УК, ТСЖ, ЖК  
                /// </summary>
                LEGAL_TSJ = 167,
                /// <summary>
                ///   Договор с физ.лицом на нежилые объекты    
                /// </summary>
                PHYSICAL_FACT = 164,
                /// <summary>
                /// Договор с физ.лицом на жилые объекты 
                /// </summary>
                PHYSICAL_NORM = 163,
                /// <summary>
                /// Муниципальный контракт
                /// </summary>
                PHYSICAL_NORM2 = 168

                            
                                                  
					                         
                                                
					                     
                                                    
            }
        }
        public class AttachObjectInfo
        {
            public string Db_Guid;
            public string doc_BinManId;
            public string obj_BinManId;
            public string tarif_BinManCode;
            public string tarif_value;
            public string containerCount;
            public string containerVolume;
            public DateTime activeFrom;
        }
        public class StopDogObject
        {
            public DateTime DateFrom;
            public string comment;
            public string dog_BinId;
            public string object_BinId;
        }
        
           // public static string Url_UpdateSsid(string id) => API.BaseUrl + $"cabinet/clients/add/?id={id}&is_ajax=y";
          //  public const string Url_Update = API.BaseUrl + $"cabinet/clients/add/";
            public const string Url_CreateSsid = API.BaseUrl + $"cabinet/documents/contracts/add/?is_ajax=y";

        public const string Url_StopObject = API.BaseUrl + $"api/1/ajax/status/";

        public static string Url_EditSsid(string bin_id) => API.BaseUrl + $"cabinet/company/contracts/edit/{bin_id}/?is_ajax=y&is_edit=y";
            public const string Url_Create = API.BaseUrl + $"cabinet/documents/contracts/add/";
        public static string Url_Edit(string bin_id) => API.BaseUrl + $"cabinet/company/contracts/edit/{bin_id}/";
        public static string Url_DeleteDog(string bin_id) => API.BaseUrl + $"cabinet/company/contracts/detail/{bin_id}/";
        public static string Url_AttachObject(string bin_id) => API.BaseUrl + $"cabinet/company/contracts/objects/{bin_id}/";
        public static string Url_AttachFile(string bin_id) => API.BaseUrl + $"cabinet/company/contracts/detail/{bin_id}/";
        public static string Url_GetUniqNumber(string TypeCode) => API.BaseUrl + $"api/1/ajax/contract/?company_id=111275&contract_type={TypeCode}";
        public const string Url_GetGroups = API.BaseUrl + $"cabinet/documents/contracts/?action=getGroupOptionList&company={BinManApi.Company_BinId}";

        public static string URL_DestroyObjectLink(string dog_bin_id, string link_id) => API.BaseUrl + $"cabinet/company/contracts/detail/{dog_bin_id}/?action=delete_story&story_id={link_id}";
        // public const string Url_Parse_id = API.BaseUrl + "cabinet/clients";
        public static Dictionary<string, string> SQLInjectionGetCreateFormData(string sessid, string SQlCode)
        {

            var v = new Dictionary<string, string>()
           {
               {"sessid",sessid},
{"CONTRACT[TYPE]", "13" },
{"CONTRACT[NUMBER]",SQlCode },
{"CONTRACT[COMPANY]", "111275"},
{"CONTRACT[ORGANIZATION]","112790"/*Data.Organization_BinManId*/ },
{"CONTRACT[CLIENT]","5779536"},
{"CONTRACT[GROUP]","1" },
{"CONTRACT[DATE_ACTIVE_FROM]",new DateTime(1973,02,05).ToString("dd.MM.yyyy") }
 };

            
                v.Add("CONTRACT[DATE_ACTIVE_TO]", new DateTime(1973, 02, 05).ToString("dd.MM.yyyy"));
                v.Add("ch", "on");
            
            v.Add("CONTRACT[DATE_SIGNING]", new DateTime(1973, 02, 05).ToString("dd.MM.yyyy"));
            v.Add("CONTRACT[AUTO_NUMBER]",
                //Data.Number 
                "false"
                );
            return v;
        }
        public static Dictionary<string, string> GetCreateFormData(string sessid, BinManDogData Data,string autoNumber)
        {

            var v = new Dictionary<string, string>()
           {
               {"sessid",sessid},
{"CONTRACT[TYPE]",Data.Type_BinManCode },
{"CONTRACT[NUMBER]",autoNumber },
{"CONTRACT[COMPANY]", "111275"},
{"CONTRACT[ORGANIZATION]","112790"/*Data.Organization_BinManId*/ },
{"CONTRACT[CLIENT]",Data.Client_BinManid},
{"CONTRACT[GROUP]",Data.Group_BinManCode },
{"CONTRACT[DATE_ACTIVE_FROM]",Data.dateFrom.ToString("dd.MM.yyyy") }
 };

            if (Data.dateTo != null)
            {
                v.Add("CONTRACT[DATE_ACTIVE_TO]", Data.dateTo.Value.ToString("dd.MM.yyyy"));
                v.Add("ch", "on");
            }
            v.Add("CONTRACT[DATE_SIGNING]", Data.dateSign.ToString("dd.MM.yyyy"));
            v.Add("CONTRACT[AUTO_NUMBER]", 
                //Data.Number 
                autoNumber
                );
            return v;
        }

        public static Dictionary<string, string> GetEditFormData(string sessid, BinManDogData Data)
            {
            var v = new Dictionary<string, string>()
           {
               {"sessid",sessid},
{"CONTRACT[TYPE]",Data.Type_BinManCode },
{"CONTRACT[NUMBER]",Data.Number},
{"CONTRACT[ORGANIZATION]","112790"/*Data.Organization_BinManId*/ },
{"CONTRACT[GROUP]",Data.Group_BinManCode },
{"CONTRACT[DATE_ACTIVE_FROM]",Data.dateFrom.ToString("dd.MM.yyyy") }
 };

            if (Data.dateTo != null)
            {
                v.Add("CONTRACT[DATE_ACTIVE_TO]", Data.dateTo.Value.ToString("dd.MM.yyyy"));
                v.Add("ch", "on");
            }
            v.Add("CONTRACT[DATE_SIGNING]", Data.dateSign.ToString("dd.MM.yyyy"));
           
            return v;
        }       
        public static Dictionary<string, string> GetAttachObjectFormData(string sessid, AttachObjectInfo Data)
            {
            var v = new Dictionary<string, string>()
           {
               {"sessid",sessid},
{"search_text",""},
{"search","true"},
{"page-number","" },
{"CONTRACT[DATE_ACTIVE_FROM]",Data.activeFrom.ToString("dd.MM.yyyy") },
{"OBJECT[TARIF]",Data.tarif_BinManCode },
{"OBJECT[TARIF_VALUE]",Data.tarif_value },
{"CONTAINER[CONTAINER_AVAL]","N" },
{"CONTAINER[CONTAINER_COUNT]","" },
{"CONTAINER[CONTAINER_VOLUME]","" },
{"OBJECT[TYPE]","not_set" },
{"OBJECT[ID]",Data.obj_BinManId },
 };

            return v;
        }
        public static Dictionary<string, string> GetStopObjectFormData(StopDogObject Data)
        {
            var v = new Dictionary<string, string>()
           {

{"OBJECT[STATUS_DATE_FROM]",Data.DateFrom.ToString("dd.MM.yyyy") },
{"OBJECT[COMMENT]",Data.comment },
{"OBJECT[CONTRACT]",Data.dog_BinId },
{"OBJECT[OBJECT]",Data.object_BinId },
 };

            return v;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="dog_bin_id"></param>
        /// <param name="link_bin_id"></param>
        /// <returns></returns>
        public static bool SendDestroyLinkRequest(LoginData ld,string dog_bin_id,string link_bin_id)
        {

            HttpRequestMessage hm = new(HttpMethod.Get, URL_DestroyObjectLink(dog_bin_id,link_bin_id));



         

            //BinManApi.LogReqContent(hm);

            var cookie = API.GetDeffaultCookie(ld, "");

            Log.ApiCall("RemoveDogObjLink");

            var req = API.SendRequest(hm,
                new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
            },
                cookie, ld, true
                );
            var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if(resp.Contains("\"success\":true}"))
            {
                return true;
            }
            else
            {
                Log.Error("Странный ответ (Удаление объекта от договора (одной ссылки)) ?" + resp);
                return false;
            }
        }
        public static bool SendStopObjectRequest(LoginData ld, StopDogObject di)
        {


                
            try
            {

                string url = Url_StopObject;


                HttpRequestMessage hm = new(HttpMethod.Post, url);


                var data = GetStopObjectFormData(di);



                hm.Content = new FormUrlEncodedContent(data);

                BinManApi.LogReqContent(hm);

                var cookie = API.GetDeffaultCookie(ld, "");

                Log.ApiCall("StopDogObject");

                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );

                //мдее ну и генератор рандома, теперь в ответе  отправляют таймер `0.0005648136138916{ "success":true}`


                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                try
                {
                    //CreateResponse cr = JsonConvert.DeserializeObject<CreateResponse>(resp);
                    if (resp.Length <= 50 && resp.Contains("true"))
                        //      BinId = cr.id;
                        return true;
                    else
                    {
                        Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        Log.Error("[StopDogObject]" + resp);
                        return false;
                    }
                }
                catch (Exception ex) { Log.Error("StopDogObject", ex); }
                //  if (resp.Contains("error"))
                //  {
                Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Log.Error(resp);

                //}
                return true;
            }
            catch (Exception e) { Log.Error(e); return false; }
        }
        public static bool SendEditRequest(LoginData ld, BinManDogData di)
            {



                try
                {

                string url = Url_EditSsid(di.bin_id);
               
               
                HttpRequestMessage hm = new(HttpMethod.Post, Url_Edit(di.bin_id));
                if (!API.GetSessIdFrom(ld, url, out string sessId, HttpMethod.Get, new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    })) return false;

                
                var data = GetEditFormData(
                         //ld.PHPSESSID
                         sessId, di
                        );



                hm.Content = new FormUrlEncodedContent(data);

                BinManApi.LogReqContent(hm);
                
                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );

                //мдее ну и генератор рандома, теперь в ответе  отправляют таймер `0.0005648136138916{ "success":true}`


                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();//14.06.2024 //Здесь реально приходит таймер xd
                try
                {
                    //CreateResponse cr = JsonConvert.DeserializeObject<CreateResponse>(resp);
                    if (resp.Length<=50 && resp.Contains("true"))
                  //      BinId = cr.id;
                    return true;
                    else
                    {
                        Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        Log.Error("[Edit Dog]"+ resp);
                        return false; 
                    }
                }
                catch (Exception ex) { Log.Error("Edit Dog", ex); }
                //  if (resp.Contains("error"))
                //  {
                Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Log.Error(resp);

                //}
                return true;
            }
            catch (Exception e) { Log.Error(e); return false; }
        }
        public class FileAddResponse {

            public string hashName { get; set; }
            public string id { get; set; }
            public string loadedBy { get; set; }
            public string loadedDate { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string size { get; set; }
        }
        public struct AttachFileRequestData
        {
            public string doc_binId;
            public byte[] file;
            public string fileName;
        }
        public const long byteIn15Mb = 15_728_640;
        public static bool SendAttachFile(LoginData ld, AttachFileRequestData data, out FileAddResponse res)
        {
            res = null;
            try
            {
               if(string.IsNullOrEmpty(data.fileName))
                {
                    Log.Error("Documents.SendAttachFile: data.fileName is null or empty !");
                    return false;
                }
               if(data.file.Length > byteIn15Mb)
                {
                    Log.Warning("Documents.SendAttachFile: Бинман обычно запрещает загрузку файлов > 15 mb, вероятно файл будет успешно загружен, но так делать не хорошо");
                }
                string url = Url_AttachFile(data.doc_binId);


                HttpRequestMessage hm = new(HttpMethod.Post, url);


                HttpContent stringContent = new StringContent("docs_file_upload");
                HttpContent bytesContent = new ByteArrayContent(data.file);

                using var fd = new MultipartFormDataContent("----WebKitFormBoundary1XidrgGyEbvErMP3");
                fd.Add(bytesContent, "file",data.fileName);
                fd.Add(stringContent,"action");

                hm.Content = fd;
                //hm.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
                //hm.Content.Headers.ContentType.Parameters
                //    .Add(new System.Net.Http.Headers.NameValueHeaderValue("boundary", "----WebKitFormBoundary1XidrgGyEbvErMP3"));

               // BinManApi.LogReqContent(hm);

                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    null,
                    cookie, ld, true
                    );

                //мдее ну и генератор рандома, теперь в ответе  отправляют таймер `0.0005648136138916{ "success":true}`


                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                try
                {
                    res = JsonConvert.DeserializeObject<FileAddResponse>(resp);
                    
                    return res !=null;
                }catch(Exception ex)
                {
                    Log.Error("SendAttachFile",ex);
                    Log.Error(resp);
                    return false;
                }
            }
            catch (Exception ex) { Log.Error("Edit Dog", ex); return false; }
        }

            public static bool SendCreateRequest(LoginData ld, BinManDogData di, out string BinId)
            {

                BinId = string.Empty;
            string DocNumber = "0";
            Task t = Task.Run(() =>
            {
                string url = Url_GetUniqNumber(di.Type_BinManCode);
                HttpRequestMessage hm2 = new(HttpMethod.Get, url);
                string content = "";
                var cookie = API.GetDeffaultCookie(ld, "");
                var res = API.SendRequest(hm2, new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    },
                    cookie, skipLogin: true);
                try
                {
                    content = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var resp = JsonConvert.DeserializeObject<SimpResponse>(content);
                    DocNumber = resp.data;
                }
                catch (Exception e) { Log.Warning(content); Log.Error("Get AutoNDoc BINMAN", e); }
                hm2 = new(HttpMethod.Get, Url_GetGroups);
                cookie = API.GetDeffaultCookie(ld, "");
                res = API.SendRequest(hm2, new KeyValuePair<string, string>[] {
                       new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    },
                    cookie, skipLogin: true);
                content = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            });
            try
                {
                    string url = Url_CreateSsid;
                t.Wait();
                Log.Text("SESID: "+ ld.PHPSESSID);
                HttpRequestMessage hm = new(HttpMethod.Post, Url_Create);
                    if (!API.GetSessIdFrom(ld, url, out string sessId,HttpMethod.Get, new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    })) return false;

                if (DocNumber == "0") { return false; }
                var data = GetCreateFormData(
                         //ld.PHPSESSID
                         sessId,di, DocNumber
                        );

                   

                    hm.Content = new FormUrlEncodedContent(data);

                    BinManApi.LogReqContent(hm);
                Log.Text("SESID: " + ld.PHPSESSID+" sss: "+sessId);
                var cookie = API.GetDeffaultCookie(ld, "");
                    var req = API.SendRequest(hm,
                        new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    },
                        cookie, ld, true
                        );

                di.Number = DocNumber;

                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();//Приходит CreateResponse, даже при ошибках ~ Success:, binId:
                try
                {
                    CreateResponse cr = JsonConvert.DeserializeObject<CreateResponse>(resp);
                    if(cr.success=="true")
                    BinId = cr.id;
                    return true;
                }
                catch (Exception ex) { Log.Error("Create Dog", ex); }
              //  if (resp.Contains("error"))
              //  {
                    Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    Log.Error(resp);
                   
                //}
                return true; 
                }
                catch (Exception e) { Log.Error(e); return false; }
            }
        public static bool SQLInjectionSendCreateRequest(LoginData ld, BinManDogData di, out string BinId)
        {

            BinId = string.Empty;
            string DocNumber = di.Number;

            DocNumber = @" 1as and=
--"; 
            //SQl injection code in clause
            //WHERE
            //f = InjectionCode AND
            //f2 = ... 



            try
            {
                string url = Url_CreateSsid;
               // t.Wait();
                Log.Text("SESID: " + ld.PHPSESSID);
                HttpRequestMessage hm = new(HttpMethod.Post, Url_Create);
                if (!API.GetSessIdFrom(ld, url, out string sessId, HttpMethod.Get, new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    })) return false;

                if (DocNumber == "0") { return false; }
                var data = SQLInjectionGetCreateFormData(
                         //ld.PHPSESSID
                         sessId, DocNumber
                        );



                hm.Content = new FormUrlEncodedContent(data);

                BinManApi.LogReqContent(hm);
                Log.Text("SESID: " + ld.PHPSESSID + " sss: " + sessId);
                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );

                di.Number = DocNumber;

                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();//Приходит CreateResponse, даже при ошибках ~ Success:, binId:
                try
                {
                    CreateResponse cr = JsonConvert.DeserializeObject<CreateResponse>(resp);
                    if (cr.success == "true")
                        BinId = cr.id;
                    return true;
                }
                catch (Exception ex) { Log.Error("Create Dog", ex); }
                //  if (resp.Contains("error"))
                //  {
                Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Log.Error(resp);

                //}
                return true;
            }
            catch (Exception e) { Log.Error(e); return false; }
        }


        public static bool SendAttachObjectRequest(LoginData ld, AttachObjectInfo ai)
        {

            

            try
            {
                string url = Url_CreateSsid;
           
                HttpRequestMessage hm = new(HttpMethod.Post, Url_AttachObject(ai.doc_BinManId));
                if (!API.GetSessIdFrom(ld, url, out string sessId, HttpMethod.Get, new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                    })) return false;

              
                var data = GetAttachObjectFormData(
                         //ld.PHPSESSID
                         sessId, ai
                        );



                hm.Content = new FormUrlEncodedContent(data);

                BinManApi.LogReqContent(hm);
           
                var cookie = API.GetDeffaultCookie(ld, "");
                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );



                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();// В случае ошибки приходит в ответ сайт
               // Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
               // Log.Error(resp);
                try
                {
                    if (resp.Length <= 50 && resp.Contains("true"))
                    {
                        CreateResponse cr = JsonConvert.DeserializeObject<CreateResponse>(resp);
                        if (cr.success == "true")
                        return true;
                    }
                    else
                    {
                       Log.Text(hm.ToString()); Log.Text(hm.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        Log.Error(resp);
                        return false;
                    }
                        
                }
                catch (Exception ex) { Log.Error("Create Dog-obj link", ex); }
                //  if (resp.Contains("error"))
                //  {
              

                //}
                return true;
            }
            catch (Exception e) { Log.Error(e); return false; }
        }
        public static bool SendDeleteDogRequest(LoginData ld, string idDog)
        {



            try
            {

                string url = Url_DeleteDog(idDog);


                HttpRequestMessage hm = new(HttpMethod.Post, url);


                var data = new Dictionary<string,string>();
                data.Add("action", "delete");
                data.Add("id", idDog);



                hm.Content = new FormUrlEncodedContent(data);

                BinManApi.LogReqContent(hm);

                var cookie = API.GetDeffaultCookie(ld, "");

                Log.ApiCall($"DeleteDog {idDog}");

                var req = API.SendRequest(hm,
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                    cookie, ld, true
                    );

                //Тут на момент 21.11.2024 - в ответ возвращается ошибка
                // [Error]
                //Cannot use object of type Bitrix\Main\DB\MysqliResult as array (0)
                //...
                // Но при этом договор удаляется успешно!, так что хз как тут классифицировать успешный запрос

                var resp = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
              
                //Log.Error(resp);

                //}
                return true;
            }
            catch (Exception e) { Log.Error(e); return false; }
        }
    }
}
