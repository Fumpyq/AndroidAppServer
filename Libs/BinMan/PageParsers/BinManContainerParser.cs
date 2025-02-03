using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public class BinManContainerParser
    {

        public static string GetGeozoneContainers(string geo_BinId) => API.BaseUrl + $"cabinet/containers/?&area={geo_BinId}";
        public static string GetLink(int page) => API.BaseUrl + $"cabinet/containers/?PAGEN_1={page}";
        public static HtmlDocument GetPage(LoginData ld,string Url)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, Url);
            var cookie = API.GetDeffaultCookie(ld, "");
            var req = API.SendRequest(hm,
                new KeyValuePair<string, string>[] {
             new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                },
                cookie
                );

            string res = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);
            return htmlDoc;
        }
        //public static bool TryParsePage(HtmlDocument htmlDoc,out List<>)
        //{

        //    var MainSlice = htmlDoc.DocumentNode.Descendants("div").Where(d => { bool? res = d.Attributes["class"]?.Value.Contains("data-list__row data-row"); return res.HasValue ? res.Value : false; });
        //    int count = MainSlice.Count();
        //    if (count == 0) return false;
        //    for (int i = 0; i < count; i++)
        //    {
        //        try
        //        {

        //            var CurrentRow = MainSlice.ElementAt(i);


        //            var LinksAndNames = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__ttl"));
        //            var Volume = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("param-item__val"));
        //            var ImageType = CurrentRow.Descendants("div").Where(d => d.Attributes["class"].Value.Contains("b-type-icon"));

        //            var ContainerEl = LinksAndNames.ElementAt(0);
        //            var ContainerLink = ContainerEl.Descendants("a").First().Attributes["href"].Value;
        //            int ContainerId = int.Parse(ContainerLink.Split("/")[^2].Replace("/", ""));

        //            var ContainerName = TrimText(ContainerEl.InnerText);



        //            string TrimText(string txt)
        //            {
        //                return Regex.Replace(txt, @"\s+", " ").Replace("пнвтсрчтптсбвс", "").Replace("\n\n", "").Trim();
        //            }


        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e.Message);

        //        }





        //    }

        //    return true;
        //}

        public static bool TryParseMainPage(LoginData login, int page)
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


                    
                    //     var GeozoneEl = LinksAndNames.ElementAt(1);
                    //    var GeozoneLink = GeozoneEl.Descendants("a").First().Attributes["href"].Value;
                    //     int GeozoneId = int.Parse(GeozoneLink.Split("/")[^2].Replace("/", ""));
                    //     var GeozoneName = TrimText(GeozoneEl.InnerText);

                    //   var innerText = Volume.ElementAt(0).InnerText;

                    //  var intextSplit = innerText.Split(" м³ ");

                    //   var ContainerVolume = TrimText(intextSplit[0]);

                    //
                    //    var imgLink = ImageType.ElementAt(0);
                    //    var vil = imgLink.Descendants("img").First();
                    //   string ImageLink = vil.Attributes["src"].Value;


                    // var c = new ContainerParse(ImageLink, ContainerName, GeozoneName, GeozoneId, ContainerId, ContainerVolume);

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
                    Log.Error(e.Message);

                }





            }

            return true;
        }

    }
}
