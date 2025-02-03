using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using System.Xml.Linq;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public static class BinManTariffParser
    {
        public static string Url_Get() => API.BaseUrl + $"cabinet/company/tarif/plan/";
        public class BinManTariff
        {
            public string binId;
            public string name;
            public string price;
            public string measure; 
            public string trashVolumeInM3; 
            public string trashInTons;
            public string dateStart;
            public string dateEnd;
            public string group;
            public string status;
        }


   

        public static List<BinManTariff> TryParseTarifs(LoginData login)
        {
            HttpRequestMessage hm = new(HttpMethod.Get, Url_Get());
            var cookie = API.GetDeffaultCookie(login, "");
            var req = API.SendRequest(hm,
                   new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                   },
                   cookie
                   );

            var txt = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();



            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(txt);

            var res = new List<BinManTariff>();

            ParsePage(htmlDoc);

            using var book = new XLWorkbook();
            var SaveAt = "C:\\Users\\a.m.maltsev\\Downloads\\BinManTarifs.xlsx";

            var es = book.Worksheets.Add();

            int x = 0;
            int y=1;
            es.Cell(y,++x).Value = "binId";
            es.Cell(y,++x).Value = "name";
            es.Cell(y,++x).Value = "price";
            es.Cell(y,++x).Value = "measure";
            es.Cell(y,++x).Value = "trashVolumeInM3";
            es.Cell(y,++x).Value = "trashInTons";
            es.Cell(y,++x).Value = "dateStart";
            es.Cell(y,++x).Value = "dateEnd";
            es.Cell(y,++x).Value = "group";
            es.Cell(y,++x).Value = "status";
            y++;
            foreach (var v in res)
            {
                x = 0;
               es.Cell(y,++x).Value = v.binId;
               es.Cell(y,++x).Value = v.name;
               es.Cell(y,++x).Value = v.price;
               es.Cell(y,++x).Value = v.measure;
               es.Cell(y,++x).Value = v.trashVolumeInM3;
               es.Cell(y,++x).Value = v.trashInTons;
               es.Cell(y,++x).Value = v.dateStart;
               es.Cell(y,++x).Value = v.dateEnd;
               es.Cell(y,++x).Value = v.group;
               es.Cell(y,++x).Value = v.status;
                y++;
            }
            book.SaveAs(SaveAt);       
            return res;

            void ParsePage(HtmlDocument htmlDoc)
            {

                var tarifs = htmlDoc.DocumentNode.Descendants("div").Where(x => x.Attributes?["class"]?.Value == "car-row js-car-row car-list-row car-list-body-row");
                foreach (var c in tarifs)
                {
                    //var name_ns = c.Descendants("a").Where(x => x.Attributes?["target"]?.Value == "_blank");
                    //var Res = new BinManTariff();// TODO DO do do do 
                    //res.Add(Res);

                    var values = c.Descendants("div").Where (x => x.Attributes?["class"]?.Value == "company");
                    var i = 0;
                    var CbinId = Get(i++);
                    var Cname = Get(i++);
                    var Cprice = Get(i++);
                    var Cmeasure = Get(i++);
                    var CtrashVolumeInM3 = Get(i++);
                    var CtrashInTons = Get(i++);
                    var CdateStart = Get(i++);
                    var CdateEnd = Get(i++);
                    var Cgroup = Get(i++);
                    var Cstatus = Get(i++);


                    BinManTariff bmt = new BinManTariff();

                    bmt.binId = CbinId;
                    bmt.name = Cname;
                    bmt.price = Cprice;
                    bmt.measure = Cmeasure;
                    bmt.trashVolumeInM3 = CtrashVolumeInM3;
                    bmt.trashInTons = CtrashInTons;
                    bmt.dateStart = CdateStart;
                    bmt.dateEnd = CdateEnd;
                    bmt.group = Cgroup;
                    bmt.status = Cstatus;

                    res.Add(bmt);

                    string Get(int ind)
                    {
                        return values.ElementAt(ind).Descendants("div").First().InnerText.Trim();
                    }
                }


            }
        }
    }
}
