using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using HtmlAgilityPack;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocumentParser;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public static class BinManGeozoneParser
    {
        public class GeozoneSearchFilter
        {
            public string SearchPrompt;
        }
        public static string GetMainSearchPage_Url(int page) => API.BaseUrl + $"cabinet/areas/?ELEMENTS_COUNT=20&PAGEN_1={page}";


        /// <summary>
        /// filter 09.08.2024 -- NOT USED NOT IMPLEMENTED
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="pageN"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static HtmlDocument GetPage(LoginData ld, int pageN, GeozoneSearchFilter filter = null)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, GetMainSearchPage_Url(pageN));
            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm, new KeyValuePair<string, string>[] {
              new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                         },
                   cookie
                   );

            var txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(txt);
            return htmlDoc;
        }


   public static Dictionary<string, string> ImageRemap = new Dictionary<string, string>()
        {
            {"/local/templates/cabinet/img/modals/icon-dg-priming.svg","Грунт"},
            {"/local/templates/cabinet/img/modals/icon-dg-rubble.svg","Щебень"},
            {"/local/templates/cabinet/img/modals/icon-dg-asphalt.svg","Асфальт"},
            {"/local/templates/cabinet/img/modals/icon-dg-concrete.svg","Бетон"},
        };
            public const string FenceImg = "/local/templates/cabinet/img/modals/icon-dg-fencing.svg";
            public const string RoofImg = "/local/templates/cabinet/img/modals/icon-dg-roof.svg";
            public class GeozonePars
            {
                public string name { get; set; }
                public string binId { get; set; }
                public string Graphic { get; set; }
                public string Container { get; set; }
                public string Objects { get; set; }
                public string Basement { get; set; }
                public string Route { get; set; }
                public bool Roof { get; set; }
                public bool Fence { get; set; }
                public List<GeozoneGraphic> GraphicDetails { get; set; } = new List<GeozoneGraphic>(1);
            }
            public struct GeozoneGraphic
            {
                public string name;
                public List<string> Details;
            }
            public static string GetLink(int page) => API.BaseUrl + $"cabinet/areas/?ELEMENTS_COUNT=20&PAGEN_1={page}";
        public enum SearchCommand
        {
            DoNothing,
            DoAction,
            Break
        }

        public static void ForeachOnMainSearch(LoginData ld,Func<GeozonePars, SearchCommand> TryToParseContainers, Func<GeozonePars, List<GeoContainer>, SearchCommand> OnContainerParsed)
        {
            var FirstPage = GetPage(ld, 1);
            if(!TryParsePage(FirstPage,out var GeozonesParse))
            {
                Log.Error("Something wrong in Geozone parser");
                return;
            }
            OnPageParsed(GeozonesParse);
            if (BinManApi.TryGetPagesCount(FirstPage, out var PageCount))
            {
                for (int i = 2; i <= PageCount; i++)
                {
                    if (!TryParsePage(GetPage(ld, i), out GeozonesParse))
                    {
                        Log.Error("Something wrong in Geozone parser");
                        return;
                    }
                    OnPageParsed(GeozonesParse);
                }
            }
            void OnPageParsed(List<GeozonePars> data)
            {
                foreach(var v in data)
                {
                    var Res = TryToParseContainers(v);
                    if (Res == SearchCommand.DoAction)
                    {
                        if (BinManGeozone.GetGeozoneContainers(ld, v.binId, out var GeoConts))
                            Res = OnContainerParsed(v, GeoConts);
                        if (Res == SearchCommand.Break)
                        {
                            break;
                        }
                    }
                    else if (Res == SearchCommand.Break)
                    {
                        break;
                    }
                }
            }
        }

        //public static int GetPagesCount(LoginData login)
        //{
        //    HttpRequestMessage hm = new(HttpMethod.Get, GetLink(1));
        //    var cookie = API.GetDeffaultCookie(login, "");
        //    var req = API.SendRequest(hm,
        //        new KeyValuePair<string, string>[] {
        // new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
        //        },
        //        cookie
        //        );

        //    var t = req.Content.ReadAsStringAsync();

        //    t.Wait();

        //    string res = t.Result;
        //    //Log.Text(res);
        //    //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
        //    HtmlDocument htmlDoc = new HtmlDocument();
        //    htmlDoc.LoadHtml(res);
        //    //if (!Firste) { htmlDoc.Save("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects_new.html"); Firste = true; }

        //    //Log.Text("----=--=---=-=---=-=----");



        //    int pageCount = 1;
        //    List<DocObject> result = new List<DocObject>(2);
        //    var pagination = htmlDoc.DocumentNode.Descendants("a");
        //    if (pagination != null && pagination.Count() > 0)
        //        pagination = pagination.Where(d => d != null && d.Attributes["class"] != null && d.Attributes["class"].Value != null && d.Attributes["class"].Value.Length > 0 && d.Attributes["class"].Value.Contains("modern-number"));
        //    if (pagination != null && pagination.Count() >= 2)
        //        pageCount = int.Parse(pagination.TakeLast(2).First().InnerText.Trim());

        //    // TryParseMainPage(login, 1);

        //    return pageCount;
        //}
        public static bool TryParsePage(HtmlDocument doc, out List<GeozonePars> gp)
        {

            var htmlDoc = doc;

            gp = new List<GeozonePars>();


            var MainSlice = htmlDoc.DocumentNode.Descendants("tr").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
            int count = MainSlice.Count();
            if (count == 0) return false;
            for (int i = 0; i < count; i++)
            {
                try
                {

                    var CurrentRow = MainSlice.ElementAt(i);


                    var LinksAndNames = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                    var Addres = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                    var AddresText = Addres.First().Descendants("a").Where(d => d.Attributes["class"].Value.Contains("inherit-link"));
                    var container = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("icon-dg-trash"));
                    var Objects = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("icon-dg-build"));
                    var Route = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item param-list__item--disable icon-dg icon-dg-routes"));
                    var Icons = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__icon"));


                    var ImageType = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-type-icon"));
                    var Graphic = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item js-show-schedule b-schedule__activator"));
                    var GraphicDetail = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("js-schedule b-route-schedule__body"));
                    var GraphicSubGraphNames = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-route-schedule__name js-type-schedule"));

                    var GeozoneEl = LinksAndNames.ElementAt(0);
                    var GeozoneLink = GeozoneEl.Descendants("a").First().Attributes["href"].Value;
                    int GeozoneId = int.Parse(GeozoneLink.Split("/")[^2].Replace("/", ""));

                    var GeozoneName = TrimText(GeozoneEl.InnerText);
                    ;
                    //var _GeozoneEl = LinksAndNames.ElementAt(1);
                    //var _GeozoneLink = _GeozoneEl.Descendants("a").First().Attributes["href"].Value;
                    //int _GeozoneId = int.Parse(_GeozoneLink.Split("/")[^2].Replace("/", ""));
                    //var _GeozoneName = TrimText(_GeozoneEl.InnerText);

                    //var innerText = Volume.ElementAt(0).InnerText;

                    // var intextSplit = innerText.Split(" м³ ");

                    // var ContainerVolume = TrimText(intextSplit[0]);


                    //var imgLink = ImageType.ElementAt(0);
                    // var vil = imgLink.Descendants("img").First();
                    //string ImageLink = vil.Attributes["src"].Value;

                    string _Containers = TrimText(container.First().InnerText);
                    string _objects = TrimText(Objects.First().InnerText);
                    string _route = TrimText(Route.First().InnerText);
                    string address = TrimText(AddresText.First().InnerText);

                    var spls = address.Split("(Лот: ");
                    string lot = "";
                    if (spls.Length > 1)
                        lot = TrimText(spls[1].Replace(")", ""));
                    string gaphy = TrimText(Graphic.First().ChildNodes.ElementAt(2).InnerText.Replace("График вывоза", ""));
                    List<string> SubGraphsName = new List<string>();
                    List<List<string>> GraphDetails = new List<List<string>>();
                    int GraphDetailsIndex = 0;
                    List<GeozoneGraphic> Graphics = new List<GeozoneGraphic>();
                    if (GraphicDetail != null && GraphicDetail.Count() > 0)
                    {
                        foreach (var gpage in GraphicDetail)
                        {
                            var GraphicDetailActive = gpage.Descendants("input").Where(d => d.Attributes["checked"] != null);
                            GraphDetails.Add(new List<string>(2));
                            var SubName = TrimText(GraphicSubGraphNames.ElementAt(GraphDetailsIndex).InnerText);
                            SubGraphsName.Add(SubName);
                            foreach (var v in GraphicDetailActive)
                            {
                                GraphDetails[GraphDetailsIndex].Add(TrimText(v.Attributes["value"].Value));
                            }
                            Graphics.Add(new GeozoneGraphic() { name = SubGraphsName[^1], Details = GraphDetails[^1] });
                            GraphDetailsIndex++;
                        }

                    }
                    bool _Fence = false;
                    bool _Roof = false;
                    string _Basement = "";
                    foreach (var ic in Icons)
                    {
                        var icc = ic.Descendants("img");
                        if (icc != null && icc.Count() >= 1)
                        {
                            var img = icc.First();
                            var vl = img.Attributes["src"].Value;
                            switch (vl)
                            {
                                case FenceImg:
                                    _Fence = true;
                                    break;
                                case RoofImg:
                                    _Roof = true;
                                    break;
                                default:
                                    _Basement = ImageRemap[vl];
                                    break;
                            }



                        }
                    }

                    GeozonePars pars = new GeozonePars();
                    pars.name = GeozoneName;
                    pars.binId = GeozoneId.ToString();
                    pars.Graphic = gaphy;
                    pars.GraphicDetails = Graphics;
                    pars.Container = _Containers;
                    pars.Objects = _objects;
                    pars.Roof = _Roof;
                    pars.Fence = _Fence;
                    pars.Basement = _Basement;
                    pars.Route = _route;


                    gp.Add(pars);



                    // var c = new ContainerParse(ImageLink, GeozoneName, GeozoneName, GeozoneId, GeozoneId, ContainerVolume);

                    //  cp.Add(c);
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
                    Log.Error($"ind: {i} " + e.Message);

                }





            }

            return true;
        }

        public static bool TryParsePage(LoginData login, int page, out List<GeozonePars> gp)
            {





                HttpRequestMessage hm = new(HttpMethod.Get, GetLink(page));
                var cookie = API.GetDeffaultCookie(login, "");
                var req = API.SendRequest(hm,
                       new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                       },
                       cookie
                       );

                var t = req.Content.ReadAsStringAsync();

                t.Wait();

                var res = t.Result;
                //Log.Text(res);
                //string res = File.ReadAllText("C:\\Users\\a.m.maltsev\\source\\repos\\BinManParser\\Debug\\req_res_objects.html");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(res);

                gp = new List<GeozonePars>();


                var MainSlice = htmlDoc.DocumentNode.Descendants("tr").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
                int count = MainSlice.Count();
                if (count == 0) return false;
                for (int i = 0; i < count; i++)
                {
                    try
                    {

                        var CurrentRow = MainSlice.ElementAt(i);


                        var LinksAndNames = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
                        var Addres = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
                        var AddresText = Addres.First().Descendants("a").Where(d => d.Attributes["class"].Value.Contains("inherit-link"));
                        var container = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("icon-dg-trash"));
                        var Objects = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("icon-dg-build"));
                        var Route = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item param-list__item--disable icon-dg icon-dg-routes"));
                        var Icons = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__icon"));


                        var ImageType = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-type-icon"));
                        var Graphic = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-list__item js-show-schedule b-schedule__activator"));
                        var GraphicDetail = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("js-schedule b-route-schedule__body"));
                        var GraphicSubGraphNames = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-route-schedule__name js-type-schedule"));

                        var GeozoneEl = LinksAndNames.ElementAt(0);
                        var GeozoneLink = GeozoneEl.Descendants("a").First().Attributes["href"].Value;
                        int GeozoneId = int.Parse(GeozoneLink.Split("/")[^2].Replace("/", ""));

                        var GeozoneName = TrimText(GeozoneEl.InnerText);
                        ;
                        //var _GeozoneEl = LinksAndNames.ElementAt(1);
                        //var _GeozoneLink = _GeozoneEl.Descendants("a").First().Attributes["href"].Value;
                        //int _GeozoneId = int.Parse(_GeozoneLink.Split("/")[^2].Replace("/", ""));
                        //var _GeozoneName = TrimText(_GeozoneEl.InnerText);

                        //var innerText = Volume.ElementAt(0).InnerText;

                        // var intextSplit = innerText.Split(" м³ ");

                        // var ContainerVolume = TrimText(intextSplit[0]);


                        //var imgLink = ImageType.ElementAt(0);
                        // var vil = imgLink.Descendants("img").First();
                        //string ImageLink = vil.Attributes["src"].Value;

                        string _Containers = TrimText(container.First().InnerText);
                        string _objects = TrimText(Objects.First().InnerText);
                        string _route = TrimText(Route.First().InnerText);
                        string address = TrimText(AddresText.First().InnerText);

                        var spls = address.Split("(Лот: ");
                        string lot = "";
                        if (spls.Length > 1)
                            lot = TrimText(spls[1].Replace(")", ""));
                        string gaphy = TrimText(Graphic.First().ChildNodes.ElementAt(2).InnerText.Replace("График вывоза", ""));
                        List<string> SubGraphsName = new List<string>();
                        List<List<string>> GraphDetails = new List<List<string>>();
                        int GraphDetailsIndex = 0;
                        List<GeozoneGraphic> Graphics = new List<GeozoneGraphic>();
                        if (GraphicDetail != null && GraphicDetail.Count() > 0)
                        {
                            foreach (var gpage in GraphicDetail)
                            {
                                var GraphicDetailActive = gpage.Descendants("input").Where(d => d.Attributes["checked"] != null);
                                GraphDetails.Add(new List<string>(2));
                                var SubName = TrimText(GraphicSubGraphNames.ElementAt(GraphDetailsIndex).InnerText);
                                SubGraphsName.Add(SubName);
                                foreach (var v in GraphicDetailActive)
                                {
                                    GraphDetails[GraphDetailsIndex].Add(TrimText(v.Attributes["value"].Value));
                                }
                                Graphics.Add(new GeozoneGraphic() { name = SubGraphsName[^1], Details = GraphDetails[^1] });
                                GraphDetailsIndex++;
                            }

                        }
                        bool _Fence = false;
                        bool _Roof = false;
                        string _Basement = "";
                        foreach (var ic in Icons)
                        {
                            var icc = ic.Descendants("img");
                            if (icc != null && icc.Count() >= 1)
                            {
                                var img = icc.First();
                                var vl = img.Attributes["src"].Value;
                                switch (vl)
                                {
                                    case FenceImg:
                                        _Fence = true;
                                        break;
                                    case RoofImg:
                                        _Roof = true;
                                        break;
                                    default:
                                        _Basement = ImageRemap[vl];
                                        break;
                                }



                            }
                        }

                        GeozonePars pars = new GeozonePars();
                        pars.name = GeozoneName;
                        pars.binId = GeozoneId.ToString();
                        pars.Graphic = gaphy;
                        pars.GraphicDetails = Graphics;
                        pars.Container = _Containers;
                        pars.Objects = _objects;
                        pars.Roof = _Roof;
                        pars.Fence = _Fence;
                        pars.Basement = _Basement;
                        pars.Route = _route;


                        gp.Add(pars);



                        // var c = new ContainerParse(ImageLink, GeozoneName, GeozoneName, GeozoneId, GeozoneId, ContainerVolume);

                        //  cp.Add(c);
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
                        Log.Error($"ind: {i} " + e.Message);

                    }





                }

                return true;
            }

        }
    }

