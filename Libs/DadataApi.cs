using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Controllers;
using Dadata;
using Dadata.Model;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using static CHGKManager.Libs.ActiveDirectory;
using ClosedXML.Excel;

namespace AndroidAppServer.Libs
{
    public class DadataApi
    {/// <summary>
     /// This class limits the number of requests (method calls, events fired, etc.) that can occur in a given unit of time.
     /// </summary>
        public class RequestLimiter
        {

            #region Constructors
            public RequestLimiter() { }
            /// <summary>
            /// Initializes an instance of the RequestLimiter class.
            /// </summary>
            /// <param name="maxRequests">The maximum number of requests that can be made in a given unit of time.</param>
            /// <param name="timeSpan">The unit of time that the maximum number of requests is limited to.</param>
            /// <exception cref="ArgumentException">maxRequests &lt;= 0</exception>
            /// <exception cref="ArgumentException">timeSpan.TotalMilliseconds &lt;= 0</exception>
            public RequestLimiter(int maxRequests, TimeSpan timeSpan)
            {
                // check parameters
                if (maxRequests <= 0)
                {
                    throw new ArgumentException("maxRequests <= 0", "maxRequests");
                }
                if (timeSpan.TotalMilliseconds <= 0)
                {
                    throw new ArgumentException("timeSpan.TotalMilliseconds <= 0", "timeSpan");
                }

                // initialize instance vars
                _maxRequests = maxRequests;
                _timeSpan = timeSpan;
                _requestTimes = new Queue<DateTime>(maxRequests);

                // sleep for 1/100th timeSpan
                _sleepTimeInMs = Convert.ToInt32(Math.Ceiling(timeSpan.TotalMilliseconds / 100));
            }

            #endregion

            /// <summary>
            /// Waits until an request can be made
            /// </summary>
            public void WaitUntilRequestCanBeMade()
            {
                while (!TryEnqueueRequest())
                {
                    Thread.Sleep(_sleepTimeInMs);
                }
            }

            #region Private Members

            public  Queue<DateTime> _requestTimes { get; set; }
            private readonly object _requestTimesLock = new object();
            private readonly int _maxRequests;
            private readonly TimeSpan _timeSpan;
            private readonly int _sleepTimeInMs;

            /// <summary>
            /// Remove requests that are older than _timeSpan
            /// </summary>
            private void SynchronizeQueue()
            {
                while ((_requestTimes.Count > 0) && (_requestTimes.Peek().Add(_timeSpan) < DateTime.Now))
                {
                    _requestTimes.Dequeue();
                }
            }

            /// <summary>
            /// Attempts to enqueue a request.
            /// </summary>
            /// <returns>
            /// Returns true if the request was successfully enqueued.  False if not.
            /// </returns>
            private bool TryEnqueueRequest()
            {
                lock (_requestTimesLock)
                {
                    SynchronizeQueue();
                    if (_requestTimes.Count < _maxRequests)
                    {
                        _requestTimes.Enqueue(DateTime.Now);
                        return true;
                    }
                    Log.Warning($"Ex Limit, Delaying rerequest on {_sleepTimeInMs}");
                    return false;
                }
            }

            #endregion

        }
        public class DadataAccount
        {
          //  public DateTime FirstRequestTime;
           // public DateTime LastRequestTime;
            public long TotalSendedRequests;
            public string token;
            public string jsonName { get; set; }
            // public object quelock = new object();
            // public ConcurrentQueue<DateTime> Executions = new ConcurrentQueue<DateTime>();
            public RequestLimiter Limiter { get; set; }
            public int ParseRequestLimit = 6500;
            public DadataAccount() { }
            public DadataAccount(string token, string justName,int reqLimit)
            {
                this.token = token;
                this.jsonName = justName;
                Limiter = new RequestLimiter(reqLimit, new TimeSpan(24,00, 00));
                ParseRequestLimit = (int)(0.8f * reqLimit);
            }

            public void Execute()
            {
                Interlocked.Increment(ref TotalSendedRequests);
                

                Limiter.WaitUntilRequestCanBeMade();
                

                //if (Executions.Count > RequestsLimit)
                //{
                //retry:
                //    lock (quelock)
                //    {
                //        while (!Executions.TryPeek(out var dt))
                //        {
                //            if (DateTime.Now - dt < TimeLimit)
                //            {
                //                Thread.Sleep(DateTime.Now - dt + TimeSpan.FromSeconds(1));
                //                goto retry;
                //            }
                //            else
                //            {
                //                Executions.TryDequeue(out dt);
                //            }
                //        }
                //    }
                //}
                //else
                //if (Executions.Count > 0)
                //{
                //    lock (quelock)
                //    {
                //        while (!Executions.TryPeek(out var dt))
                //        {
                //            if (DateTime.Now - dt > TimeLimit)
                //            {
                //                Executions.TryDequeue(out dt);
                //            }
                //            else { break; }
                //        }
                //    }
                //}


                //Executions.Enqueue(DateTime.Now);
            }

        }

        

        public class BinmanAddresStorage
        {
            public string ADDRESS       { get =>DataStorage.ADDRESS      ; set { DataStorage.ADDRESS       = value; } }///<summary>Город</summary>
            public string CITY          { get =>DataStorage.CITY         ; set { DataStorage.CITY          = value; } }/// <summary>Населенный пункт      ~поселок Плотниково </summary>
            public string SETTLEMENT    { get =>DataStorage.SETTLEMENT   ; set { DataStorage.SETTLEMENT    = value; } }/// <summary>Корпус </summary>
            public string BLOCK         { get =>DataStorage.BLOCK        ; set { DataStorage.BLOCK         = value; } }/// <summary> Дом			        ~дом 23 </summary>
            public string HOUSE         { get =>DataStorage.HOUSE        ; set { DataStorage.HOUSE         = value; } }/// <summary> Район города </summary>
            public string CITY_DISTRICT { get =>DataStorage.CITY_DISTRICT; set { DataStorage.CITY_DISTRICT = value; } }/// <summary>Район   	            ~Промышленновский р-н</summary>
            public string AREA          { get =>DataStorage.AREA         ; set { DataStorage.AREA          = value; } }/// <summary>Регион  	            ~Кемеровская область - Кузбасс</summary>
            public string REGION        { get =>DataStorage.REGION       ; set { DataStorage.REGION        = value; } }/// <summary> Улица 		        ~ул Советская </summary>
            public string STREET        { get =>DataStorage.STREET       ; set { DataStorage.STREET        = value; } }/// <summary>: 55.3787203326879</summary>
            public double  LAT           { get =>DataStorage.LAT          ; set { DataStorage.LAT           = value; } }/// <summary>: 86.81808471679688</summary>
            public double  LON           { get =>DataStorage.LON          ; set { DataStorage.LON           = value; } }
            public string  ROOM         { get =>DataStorage.ROOM         ; set { DataStorage.ROOM          = value; } }
            public string  PostalCode           { get =>DataStorage.POSTAL_CODE          ; set { DataStorage.POSTAL_CODE           = value; } }
            public string? db_partAdressOwnerGuid { get; set; }

            public void CopyAddres(BinmanAddresStorage source)
            {
                ADDRESS = source.ADDRESS;

            }
            public BinManAddressStorageData DataStorage = new BinManAddressStorageData();

        }
        public class BinManAddressStorageData {
            public string ADDRESS { get; set; }
            public string CITY { get; set; }
            public string SETTLEMENT { get; set; }
            public string BLOCK { get; set; }
            public string HOUSE { get; set; }
            public string CITY_DISTRICT { get; set; }
            public string AREA { get; set; }
            public string REGION { get; set; }
            public string STREET { get; set; }
            public string POSTAL_CODE;
            public string ROOM { get; set; }
            public double LAT { get; set; }//: 55.3787203326879
            public double LON { get; set; }//: 86.81808471679688
        };
        //public class 



        public static string SavePath = AppContext.BaseDirectory + "//DadataDump.json";
        private static SuggestClientSync api = new SuggestClientSync(Token);
        public static void OnAppClose()
        {
            SaveToJson();
        }
        public static void Init()
        {
            LoadFromJson();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            // do some work

            void CurrentDomain_ProcessExit(object sender, EventArgs e)
            {
                OnAppClose();
            };
          
        }
        public static void SaveToJson()
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(accs,Formatting.Indented));

        }
        public static void LoadFromJson()
        {
            if (File.Exists(SavePath))
            {
                var txt =File.ReadAllText(SavePath);
                try
                {
                   var res = JsonConvert.DeserializeObject<List<DadataAccount>>(txt);
                    foreach(var v in accs)
                    {
                        foreach (var vv in res)
                        {
                            if (v.jsonName == vv.jsonName)
                            {
                                v.Limiter._requestTimes = vv.Limiter._requestTimes;
                            }
                        }
                            
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("Can't serialize json dadata with ex:");
                    Log.Error(ex);
                }
            }
            else
            {
                Log.Warning("Нету файла дампа дадаты");
            }
        }
        public static Dictionary<GeoPoint, Suggestion<Address>> ResultCash = new Dictionary<GeoPoint, Suggestion<Address>>();
     //   public static Dictionary<GeoPoint,SuggestResponse<Suggestion<Address>>> GeolocateCash = new Dictionary<GeoPoint, SuggestResponse<Suggestion<Address>>>();
        public static ConcurrentDictionary<string, Suggestion<Address>> ResultCashByAddres = new ConcurrentDictionary<string, Suggestion<Address>>();

        public static string GetAddress(GeoPoint position)
        {
            var response = api.Geolocate(lat: position.mLatitude, lon: position.mLongitude);


            // foreach (var v in response.suggestions)
            // {
            //     Log.Json(v);
            // }
            if (response.suggestions.Count <= 0) return string.Empty;
            return response.suggestions[0].unrestricted_value;
            
        }
        public static List<Suggestion<Address>> GetAllAddress(GeoPoint position)
        {
        //    if(ResultCash.ContainsKey(position))
        //    {
//
        //    }
            var response = api.Geolocate(lat: position.mLatitude, lon: position.mLongitude,radius_meters:1000);

            var res = new List<Suggestion<Address>>(5);
            foreach (var v in response.suggestions) {
                res.Add(v);
                if(!ResultCashByAddres.ContainsKey(v.unrestricted_value))
                ResultCashByAddres.TryAdd(v.unrestricted_value,v);
            }
                // foreach (var v in response.suggestions)
                // {
                //     Log.Json(v);
                // }

            if(response.suggestions?.Count>0)

                foreach(var v in res)
            {
                DadataController.TryFormatAddresDToDom(v); DadataController.TryFormatAddresDToDom(v);
            }
            else
            {

                List<GeoPoint> AdditionalChecks = new List<GeoPoint>()
                {
                    new GeoPoint(position.mLongitude+0.05, position.mLatitude+0.05),
                    new GeoPoint(position.mLongitude+0.05, position.mLatitude-0.05),
                    new GeoPoint(position.mLongitude-0.05, position.mLatitude+0.05),
                    new GeoPoint(position.mLongitude-0.05, position.mLatitude-0.05),
                };
                foreach (var p in AdditionalChecks)
                {
                    response = api.Geolocate(lat: p.mLatitude, lon: p.mLongitude, radius_meters: 1000);

                    res = new List<Suggestion<Address>>(5);
                    foreach (var v in response.suggestions)
                    {
                        res.Add(v);
                        if (!ResultCashByAddres.ContainsKey(v.unrestricted_value))
                            ResultCashByAddres.TryAdd(v.unrestricted_value, v);
                    }
                    // foreach (var v in response.suggestions)
                    // {
                    //     Log.Json(v);
                    // }

                    if (response.suggestions?.Count > 0)
                    {
                        foreach (var v in res)
                        {
                            DadataController.TryFormatAddresDToDom(v); DadataController.TryFormatAddresDToDom(v);
                        }
                        return res;
                    }
                    Thread.Sleep(50);
                }



                try
                {
                    var closestGeoAddress = SQL.InnerServer_GetClosestGeozoneAddress(position);
                    if (!string.IsNullOrEmpty(closestGeoAddress))
                    {

                        Suggestion<Address> fake = new Suggestion<Address>();
                        fake.value = closestGeoAddress;
                        fake.unrestricted_value = closestGeoAddress;
                        fake.data = new Address();
                        fake.data.region = string.Empty;
                        fake.data.region_with_type = string.Empty;

                        DadataController.TryFormatAddresDToDom(fake);
                        res.Add(fake);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                }
                
            }
                return res;

        }
        public static bool TryFindPoint(string Prompt, out GeoPoint pos, GeoPoint? Priority = null)
        {
            SuggestResponse<Address> response = null;
            if (Priority.HasValue)
            {

                var sg = api.Geolocate(Priority.Value.mLatitude, Priority.Value.mLongitude, count: 1);
                if (sg.suggestions.Count > 0)
                {
                    SuggestAddressRequest srq = new SuggestAddressRequest(Prompt);
                    srq.count = 1;
                    srq.locations_boost = new Address[] { sg.suggestions[0].data };
                    response = api.SuggestAddress(srq);
                }
                else
                {
                    SuggestAddressRequest srq = new SuggestAddressRequest(Prompt);
                    srq.count = 1;
                    srq.locations_geo = new LocationGeo[] { new LocationGeo() { lat = Priority.Value.mLatitude, lon = Priority.Value.mLongitude, radius_meters = 10000 } };
                    response = api.SuggestAddress(srq);
                }

            }
            else {
                response = api.SuggestAddress(Prompt, 1);
            }

            pos = new GeoPoint();
            if(response.suggestions.Count > 0)
            {
                //Log.Json(response);
                if (response.suggestions[0].data.geo_lon != null)
                {
                    pos = new GeoPoint(double.Parse(response.suggestions[0].data.geo_lon.Replace('.', ',')), double.Parse(response.suggestions[0].data.geo_lat.Replace('.', ',')));
                    return true;
                }
                return false;
            }
            return false;

        }

        public static string GetAddress(GeoPoint position, BinmanAddresStorage gd,bool CutNumber=false) {
            var response = api.Geolocate(lat: position.mLatitude, lon: position.mLongitude);
           



      
            var res = response.suggestions[0].data;
            gd.CITY = res.city_with_type;
            gd.SETTLEMENT = res.settlement_with_type;
            gd.BLOCK = res.block;
            gd.HOUSE = res.house_with_type != null ? res.house_with_type : res.house_type_full + " " + res.house;
            gd.CITY_DISTRICT = res.city_district;
            gd.AREA = res.area_with_type;
            gd.REGION = res.region_with_type;
            gd.STREET = res.street_with_type;
            gd.ADDRESS = response.suggestions[0].unrestricted_value;
            gd.PostalCode = res.postal_code;
          //  Log.Json(res);
            string address = response.suggestions[0].unrestricted_value;
            if(CutNumber)
            {
               address= address.Replace($"{res.postal_code}, ","");
            }

            return address;
        }        
        public static string GetAddress(string address, BinmanAddresStorage gd) {

            Suggestion<Address> sug = null;
            Address res = null;
            if (ResultCashByAddres.ContainsKey(address))
            {
                sug = ResultCashByAddres[address];
                res= sug.data;

            }
            else
            {
                var response = api.SuggestAddress(address);
                res = response.suggestions[0].data;
            }





            DadataController.TryFormatAddresDToDom(sug);


            gd.CITY = res.city_with_type;
            gd.SETTLEMENT = res.settlement_with_type;
            gd.BLOCK = res.block;
            gd.HOUSE = res.house_with_type != null ? res.house_with_type : res.house_type_full + " " + res.house;
            gd.CITY_DISTRICT = res.city_district;
            gd.AREA = res.area_with_type;
            gd.REGION = res.region_with_type;
            gd.STREET = res.street_with_type;
            gd.ADDRESS = sug.unrestricted_value;

          

            return sug.unrestricted_value;
        }
        public static bool  TryFindAddressByAddress(string Address,out Suggestion<Address>res )
        {
            if (ResultCashByAddres.TryGetValue(Address, out res))
            {

                if (string.IsNullOrEmpty(res.data.house_with_type)) res.data.house_with_type = res.data.house_type + " " + res.data.house;
                DadataController.TryFormatAddresDToDom(res);
                return true; 
            }
            try
            {
               res= api.SuggestAddress(Address,1).suggestions.First();
                if (string.IsNullOrEmpty(res.data.house_with_type))res.data.house_with_type = res.data.house_type + " " + res.data.house;
                DadataController.TryFormatAddresDToDom(res);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"TryFindAddressByAddress addr: `{Address}`", ex);
                return false;
               
            }


        }

        public static bool TryFindAddressByPrompt_Simple(string Prompt,out Suggestion<Address> addr)
        {
            var res = api.SuggestAddress(Prompt, 1);
            if (res.suggestions.Count > 0)
            {
                addr = res.suggestions.First();
                return true;
            }
            else
            {
                addr = null;
                return false;
            }
        }
        public class GGGGG
        {
            public string lon;
            public string lat;
        }
        public static void LoadSpecialThing11072024(string ExcellFilePath)
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

            //int RegN = 8;
            //int RegN = 9;
            //int RegN = 10;
            //int RegN = 11;
            int asd = 10;

            Dictionary<string, GGGGG> Existing = new Dictionary<string, GGGGG>();
            for (int i = SkipFirstRow ? 2 : 1; i <= l; i++) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! 2= > TEST Потом поменять
            {
                var r1 = es.Cell(i, 8).Value.ToString().Trim();
                var r2 = es.Cell(i, 9).Value.ToString().Trim();
                var r3 = es.Cell(i, 10).Value.ToString().Trim();
                var r4 = es.Cell(i, 11).Value.ToString().Trim();

                var res = "Кемеровская область" + AddComma(r2) + AddComma(r3) + AddComma(r4);
                
              
                try
                {
                    var v1 = es.Cell(i, 12).Value.ToString().Trim();
                    var v2 = es.Cell(i, 13).Value.ToString().Trim();
                    if (string.IsNullOrEmpty(v1)) {
                        if (Existing.TryGetValue(res, out var gg))
                        {
                            es.Cell(i, 12).Value = gg.lat;
                            es.Cell(i, 13).Value = gg.lon;
                        }
                        else
                        {
                            Thread.Sleep(200);
                            if (DadataApi.TryFindAddressByPrompt_Simple(res, out var addr))
                            {
                                es.Cell(i, 12).Value = addr.data.geo_lat;
                                es.Cell(i, 13).Value = addr.data.geo_lon;
                                try
                                {
                                    Existing.TryAdd(res, new GGGGG() { lat = addr.data.geo_lat, lon = addr.data.geo_lon });
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(ex);
                                }
                            }
                            else
                            {
                                es.Cell(i, 12).Value = "-";
                                es.Cell(i, 13).Value = "-";
                            }
                        }
                        asd--;
                        if (asd <= 0) { book.SaveAs(SaveAt); asd = 10; }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                }
            }
            book.SaveAs(SaveAt);
            string AddComma(string ss)
            {
                return string.IsNullOrEmpty(ss)?ss:", "+ss;
            }
        }

        public static bool TryFillAddres(BinmanAddresStorage gd)
        {

            if (!string.IsNullOrEmpty(gd.db_partAdressOwnerGuid))
            {



                Suggestion<Address> sugAddres = SQL.GetOwnedAddressInfo(gd.db_partAdressOwnerGuid, out _);
                Address addr = null;
                GeoPoint gp = new GeoPoint(gd.LON, gd.LAT);
                if (ResultCash.TryGetValue(gp, out var val))
                {
                    sugAddres = val;
                    addr = sugAddres.data;
                }

                BinManAddressStorageData data = gd.DataStorage;
                if (addr == null)
                {
                    addr = sugAddres.data;
                    if (addr == null)
                        return false;
                }

                DadataController.TryFormatAddresDToDom(sugAddres);

                data.CITY = addr.city_with_type;
                data.SETTLEMENT = addr.settlement_with_type;
                data.BLOCK = addr.block;

                if (string.IsNullOrEmpty(data.ROOM) || data.ROOM.ToLower() == "null")
                {
                    var splt = (data.ADDRESS = string.IsNullOrEmpty(gd.ADDRESS) ? sugAddres.unrestricted_value : gd.ADDRESS).Split(",");
                    var kvText = splt[^1];
                    bool FirstNumber = false;
                    string res = "";
                    foreach (var v in kvText)
                    {
                        if (FirstNumber)
                        {
                            res += v;
                        }
                        else if (char.IsNumber(v))
                        {
                            FirstNumber = true;
                            res += v;
                        }
                    }
                    data.ROOM = res;
                }

                data.HOUSE = addr.house_with_type != null ? addr.house_with_type : addr.house_type_full + " " + addr.house;
                data.CITY_DISTRICT = addr.city_district;
                data.AREA = addr.area_with_type;
                data.REGION = addr.region_with_type;
                data.STREET = addr.street_with_type;

                data.ADDRESS = string.IsNullOrEmpty(gd.ADDRESS) ? sugAddres.unrestricted_value : gd.ADDRESS;

                if (!ResultCash.ContainsKey(gp)) ResultCash.Add(gp, sugAddres);


                return true;
            }

            else
            { 
                try
                {
                    Suggestion<Address> sugAddres = null;
                    Address addr = null;
                    GeoPoint gp = new GeoPoint(gd.LON, gd.LAT);
                    if (ResultCash.TryGetValue(gp, out var val))
                    {

                        sugAddres = val;
                        addr = sugAddres.data;

                        //return true;
                    }
                    SuggestResponse<Address> response = null;
                    if (addr == null)
                        response = api.Geolocate(lat: gd.LAT, lon: gd.LON,count:1);


                    //foreach (var v in response.suggestions)
                    //{
                    //    Log.Json(v);
                    //}
                    BinManAddressStorageData data = gd.DataStorage;
                    if (addr == null)
                    {
                        addr = response.suggestions[0].data;
                        sugAddres = response.suggestions[0];
                    }
                    DadataController.TryFormatAddresDToDom(sugAddres);
                    data.CITY = addr.city_with_type;
                    data.SETTLEMENT = addr.settlement_with_type;
                    data.BLOCK = addr.block;

                    if (string.IsNullOrEmpty(data.ROOM) || data.ROOM.ToLower() == "null")
                    {
                        var splt = (data.ADDRESS = string.IsNullOrEmpty(gd.ADDRESS) ? sugAddres.unrestricted_value : gd.ADDRESS).Split(",");
                        var kvText = splt[^1];
                        bool FirstNumber = false;
                        string res = "";
                        foreach (var v in kvText)
                        {
                            if (FirstNumber)
                            {
                                res += v;
                            }
                            else if (char.IsNumber(v))
                            {
                                FirstNumber = true;
                                res += v;
                            }
                        }
                        data.ROOM = res;
                    }

                    data.HOUSE = addr.house_with_type != null ? addr.house_with_type : addr.house_type_full + " " + addr.house;
                    data.CITY_DISTRICT = addr.city_district;
                    data.AREA = addr.area_with_type;
                    data.REGION = addr.region_with_type;
                    data.STREET = addr.street_with_type;

                    data.ADDRESS = string.IsNullOrEmpty(gd.ADDRESS) ? sugAddres.unrestricted_value : gd.ADDRESS;
                    //sugAddres.unrestricted_value;

                    if (!ResultCash.ContainsKey(gp)) ResultCash.Add(gp, sugAddres);


                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return false;
                }
            }
        }       
        public static bool TryFillAddressByBdOrDadata(BinmanAddresStorage gd)
        {
            try
            {
                Suggestion<Address> sugAddres = null;
                if (!string.IsNullOrEmpty(gd.db_partAdressOwnerGuid))
                {
                     sugAddres = SQL.GetOwnedAddressInfo(gd.db_partAdressOwnerGuid, out _);
                }
             
                Address addr=null;
                GeoPoint gp = new GeoPoint(gd.LON, gd.LAT);
                if (sugAddres == null)
                {
                    if (ResultCashByAddres.TryGetValue(gd.ADDRESS, out var val))
                    {

                        sugAddres = val;
                        addr = sugAddres.data;

                        //return true;
                    }
                }
                else
                {
                    addr = sugAddres.data;
                }
                SuggestResponse<Address> response = null;
               // if (addr==null)
               // response = api.Geolocate(lat: gd.LAT, lon: gd.LON);               
                if (addr==null)
                response = api.SuggestAddress(gd.ADDRESS);


                //foreach (var v in response.suggestions)
                //{
                //    Log.Json(v);
                //}
                BinManAddressStorageData data = gd.DataStorage;
                if (addr == null)
                {
                    addr = response.suggestions[0].data;
                    sugAddres = response.suggestions[0];
                }
                DadataController.TryFormatAddresDToDom(sugAddres);
                data.CITY = addr.city_with_type;
                data.SETTLEMENT = addr.settlement_with_type;
                data.BLOCK = addr.block;
                data.HOUSE = addr.house_with_type != null ? addr.house_with_type : addr.house_type_full + " " + addr.house;
                data.CITY_DISTRICT = addr.city_district; 
                data.AREA = addr.area_with_type;
                data.REGION = addr.region_with_type;
               // data.
                data.STREET = addr.street_with_type;

                data.ADDRESS = sugAddres.unrestricted_value;

               if(!ResultCash.ContainsKey(gp)) ResultCash.Add(gp, sugAddres);
               

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }


        public static void AccurateObjectAddreses()
        {
            var ToDadate = SQL.GetObjectsToAccurateAddress();
            foreach (var v in ToDadate)
            {
                if (DadataApi.TryFindAddressByAddress(v.address, out var addr))
                {
                    SQL.InsertObjectPartAddress(addr, v.guid,"OK");
                }
                else
                {
                    SQL.InsertObjectPartAddress(addr, v.guid,"FAILED");
                }
            }
        }

        public static void AccurateDbAddreses()
        {
            int i = 0;
            DateTime dt=DateTime.Now;
            var limitperSec=TimeSpan.FromMilliseconds(1000/25);
            
            object dtlock = new object();
            int _calls=0;
            Stopwatch _sw = Stopwatch.StartNew();

        var Geoz = SQL.GetGeozonesToAccurateAddress();

            var chunks = Geoz.Chunk(Geoz.Count/3+1);

            foreach(var v in chunks)
            {
                var acc = accs[i];
                Task.Run(() =>
                {
                    var api = new SuggestClientSync(acc.token);
                    foreach (var c in v)
                    {
                        lock (dtlock)
                        {
                            var ddif = DateTime.Now - dt;
                            if (ddif < limitperSec)
                            {
                                Thread.Sleep(limitperSec);
                            }
                            dt = DateTime.Now;
                            _calls++;
                            if (_sw.ElapsedMilliseconds > 1000)
                            {
                                _sw.Stop();
                                Console.Title = "CPS: " + _calls +" Ex:"+acc.TotalSendedRequests;

                                //Save or print _calls here before it's zeroed
                                _calls = 0;
                                _sw.Restart();
                            }
                        }
                        acc.Execute();
                        try
                        {
                            Log.Text($"Suggesting address: {c.guid} {c.address}");
                           
                            var response = api.SuggestAddress(c.address,1);
                            if (response.suggestions.Count > 0)
                            {
                                var data = response.suggestions[0];
                                SQL.InsertGeozoneAccurateResult(data, c.guid);
                            }
                            else Log.Warning($"Suggesting address: {c.guid} {c.address} address not found");
                        }
                        catch(Exception ex)
                        {
                            Log.Error(ex);
                            if (ex.ToString().Contains("403) Forbidden"))
                            {
                                Thread.Sleep(new TimeSpan(1, 0, 0));//Ждем условно 1 ч
                                Log.Text("Ну все,(403 Forbidden) (Лимит запросов 10000/д.) я спать на 1 ч");
                            }
                            else
                                Thread.Sleep(60 * 1000);//1m
                        }
                    }
                });
                i++;
                if (i >= accs.Count) i = 0;
            }

        }
    }
}
