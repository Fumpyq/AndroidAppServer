using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using AndroidAppServer.Libs.BinMan;
using AndroidAppServer.Libs.BinMan.PageParsers;
using BinManParser.Api;
using ClosedXML.Excel;
using Dadata.Model;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using Irony.Parsing;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Web;
using static AndroidAppServer.Libs.DadataApi;
using static AndroidAppServer.Libs.MailService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HandMadeController : ControllerBase
    {
#if DEBUG

        private static Dictionary<string, AccrualsType> NamingMap = new Dictionary<string, AccrualsType> {
            {"1" ,AccrualsType.accr_by_doc},//Начисление по договору
            {"2" ,AccrualsType.accr_by_req_or_add},//Начисление по заявке или за дополнительный вывоз
            {"4" ,AccrualsType.penny},//Пеня
            {"18" ,AccrualsType.accr_based_on_req_or_add},//Начисление на основании дополнительных вывозов и заявок
            {"19" ,AccrualsType.accr_any_summ},//Произвольная сумма
            {"Произвольная сумма" ,AccrualsType.accr_any_summ},//Произвольная сумма
            {"20" ,AccrualsType.STORNO},//СТОРНО
        };

        [HttpGet("samePercentage")]
        public IActionResult getSamePercentage([FromQuery] string s1, [FromQuery] string s2)
        {
            var res = ComputeSimilarity.CalculateSimilarity(s1, s2);

            return Ok($"{(res * 100).ToString("F1")} %");
        }

        
        [HttpPost("kadastByAddress")]
        public IActionResult GetKadastr([FromQuery]int SourceColumn, [FromQuery] int TargetColumn, [FromQuery] bool SkipFirstRow, IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using var book = new XLWorkbook(stream);
                var es = book.Worksheets.First();
                var c = es.Column(SourceColumn);
                var cc = c.LastCellUsed();
                int l = 0;
                try
                {
                    l= cc.Address.RowNumber;
                }
                catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }

                for (int i = SkipFirstRow ? 2 : 1; i <= l; i++)
                {
                    string val = es.Cell(i, SourceColumn).Value.GetText();
                    if (!string.IsNullOrEmpty(val) && val.Length >= 5)
                    {
                        string?[] spl = val.Split(',');
                        if (spl[^1].Contains("к")) { val = String.Join(separator:",",value: spl, startIndex: 0,count: spl.Length - 1); };
                        Log.Text($"[HM] Search: {val}");
                        var code = Rosreestr.tryFindByAddres_FIAS(val, out var res);
                        if (code != Rosreestr.AddresResponseCode.OK)
                        {
                            es.Cell(i, TargetColumn).Value = "-";
                        }
                        else
                        {
                            es.Cell(i, TargetColumn).Value = string.IsNullOrEmpty(
                                res.addresses[0].address_details.cadastral_number) ?
                                "-" :
                                res.addresses[0].address_details.cadastral_number;
                        }

                    }
                }
                using (var streamres = new MemoryStream())
                {
                    book.SaveAs(streamres);
                    var content = streamres.ToArray();
                        book.Dispose();
                    return File(content, file.ContentType, file.Name);
                }
            }

        }       
        [HttpPost("ParseBinDocumentObjects")]
        public IActionResult ParseInBdDocObjects([FromQuery] int? ColumnLimit, [FromQuery] int IdColumn, [FromQuery] bool SkipFirstRow, IFormFile file)
        {

            const int idcol = 1;
            const int typecol = 3;



            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using var book = new XLWorkbook(stream);
                var es = book.Worksheets.First();
                // var c = es.Column(SourceColumn);
                var cc = es.Column(1).LastCellUsed();
                int l = 0;
                try
                {
                    l = cc.Address.RowNumber;
                }
                catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }

                for (int i = SkipFirstRow ? 2 : 1; i <= l; i++)
                {
                    if (ColumnLimit.HasValue)
                    {
                        if (ColumnLimit <= 0) { break; }
                        ColumnLimit--;
                    }

                    var docId = es.Cell(i, IdColumn).Value.ToString();

                    LoginData logdata = BinManApi.GetNextAccount();
                    if (BinManDocumentParser.TryParseObjects(logdata, docId, out var res))
                    {


                            for (int j = 0; j < res.Count; j++)
                            {
                                Log.Text("Load: " + j);
                                SQL.LoadFullBinmanObject(res[j], docId);
                                es.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC8E4B6");
                            }
                            GC.Collect();
                        
                    }
                    else { es.Row(i).Style.Fill.BackgroundColor = XLColor.Redwood; }

                }
                void MarkCellError(int i, int j)
                {
                    es.Cell(i, j).Style.Fill.BackgroundColor = XLColor.CoralRed;
                }
                using (var streamres = new MemoryStream())
                {
                    book.SaveAs(streamres);
                    var content = streamres.ToArray();
                    book.Dispose();
                    return File(content, file.ContentType, file.Name);
                }
            }

        }

        [HttpPost("addDocmentAccruals")]
        public IActionResult Get([FromQuery] int? ColumnLimit, [FromQuery] bool SkipFirstRow, IFormFile file)
        {

            const int idcol = 1;
            const int typecol = 3;



            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using var book = new XLWorkbook(stream);
                var es = book.Worksheets.First();
               // var c = es.Column(SourceColumn);
                var cc = es.Column(1).LastCellUsed();
                int l = 0;
                try
                {
                    l = cc.Address.RowNumber;
                }
                catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }

                es.Cell(1, 10).Value += "Дата";
                es.Cell(1, 11).Value += "Дата начало";
                es.Cell(1, 12).Value += "Дата конец";
                es.Cell(1, 13).Value += "Сумма";
                es.Cell(1, 14).Value += "Комментарий";

                for (int i = SkipFirstRow ? 2 : 1; i <= l; i++)
                {
                    if (ColumnLimit.HasValue)
                    {
                        if (ColumnLimit <= 0) { break; }
                        ColumnLimit--;
                    }
                    BinManAccrual bc = new BinManAccrual();
                    var cl = es.Cell(i, typecol).Value;
                    string val = cl.ToString();
                        
                    if(NamingMap.TryGetValue(val,out var type))
                    {
                        bc.typeRaw = ((int)type).ToString();
                    }
                    else {
                        es.Cell(i, 5).Value = "Не известный тип начисления";
                        es.Row(i).Style.Fill.BackgroundColor = XLColor.Redwood;
                        continue; 
                    }

                    val = es.Cell(i, idcol).Value.ToString();
                    if (long.TryParse(val, out var lres))
                    {
                        bc.doc_BinId = lres.ToString();
                    }
                    else
                    {
                        es.Cell(i, 8).Value = "Не известный формат документа"; es.Row(i).Style.Fill.BackgroundColor = XLColor.Redwood;
                        MarkCellError(i, 2);
                        continue;
                    }


                    switch (bc.typeRaw)
                    {
                        case "1":
                            break;
                        case "2":
                            break;
                        case "4":
                            break;
                        case "18":
                            break;
                        case "19":
                            int errorCell = 7;

                            int datecol = 4;
                            int summcol = 5;
                            int comcol = 6;
                           






                            val = es.Cell(i, datecol).Value.ToString();
                         //   if (!DateOnly.TryParse(val, out var date))
                         //   {
                                if (DateTime.TryParse(val, out var dt))
                                {
                                   // date = DateOnly.FromDateTime(dt);
                                bc.date = dt;
                                }
                                else
                                {
                                    errorCell = es.Row(i).LastCellUsed().Address.ColumnNumber;
                                    es.Cell(i, errorCell).Value = "Не известный формат даты"; es.Row(i).Style.Fill.BackgroundColor = XLColor.Redwood;
                                    MarkCellError(i, datecol);
                                    break;
                                }
                            //   }
                            //bc.date = date;

                            val = es.Cell(i, summcol).Value.ToString();

                            bc.summ = val;

                            val = es.Cell(i, comcol).Value.ToString();

                            bc.comment= val;


                            var samedocs=SQL.GetAllDocAnySummAccruals(bc.doc_BinId);

                            bool skip = true;
                            
                            foreach (var v in samedocs)
                            {
                                if (!skip)
                                {
                                   
                                    es.Cell(i, 10).Value += ", ";
                                    es.Cell(i, 11).Value += ", ";
                                    es.Cell(i, 12).Value += ", ";
                                    es.Cell(i, 13).Value += ", ";
                                    es.Cell(i, 14).Value += ", ";
                                }
                                skip = false;
                                es.Cell(i, 10).Value += v.date.HasValue ? v.date.Value.ToString("dd.MM.yyyy") : "";
                                es.Cell(i, 11).Value += v.dateFrom.HasValue ? v.dateFrom.Value.ToString("dd.MM.yyyy") : "";
                                es.Cell(i, 12).Value += v.dateTo.HasValue ? v.dateTo.Value.ToString("dd.MM.yyyy") : "";
                                es.Cell(i, 13).Value += v.summ;
                                es.Cell(i, 14).Value += v.comment;
                            }

                            if (samedocs.Count > 0)
                            {
                                var same = 0;
                                foreach (var v in samedocs)
                                {
                                    string numericDB = new String(v.comment.Where(Char.IsDigit).ToArray());
                                    string numericExcel = new String(bc.comment.Where(Char.IsDigit).ToArray());
                                    if (numericDB == numericExcel)
                                    {
                                        same = 1;
                                        break;
                                    }
                                    else
                                    {
                                        //var seam = ComputeSimilarity.CalculateSimilarity(numericDB, numericExcel);
                                        //if (seam > 0.85) { same = 2; break; }
        //                              var seam=  numericDB.GroupBy(c => c)
        //.Join(
        //    numericExcel.GroupBy(c => c),
        //    g => g.Key,
        //    g => g.Key,
        //    (lg, rg) => lg.Zip(rg, (l, r) => l).Count())
        //.Sum();
        //                                if (seam >= numericExcel.Length - 1) { same = false; break; }
                                    }
                                }

                                if (same == 0)
                                {
                                    es.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFA5D9E1");
                                }
                                else if (same == 1)
                                {
                                    es.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF557FE4");
                                }
                                else if (same == 2)
                                {
                                    es.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF8655E4");
                                }
                            }
                            else
                            {
                                if (
                                    true
                                    // BinManDocAccruals.AddAccrualToDoc(BinManApi.GetNextAccount(),bc)
                                    ) { es.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC8E4B6"); }
                                else
                                    es.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFE7E97C"); ;


                            }
                            break;
                        case "20":
                            break;
                        default:
                            errorCell = es.Row(i).LastCellUsed().Address.ColumnNumber;
                            es.Cell(i, errorCell).Value = "Не известный тип начисления"; es.Row(i).Style.Fill.BackgroundColor = XLColor.Redwood;
                            break;
                    }
                }
                void MarkCellError(int i,int j)
                {
                    es.Cell(i, j).Style.Fill.BackgroundColor = XLColor.CoralRed;
                }
                using (var streamres = new MemoryStream())
                {
                    book.SaveAs(streamres);
                    var content = streamres.ToArray();
                    book.Dispose();
                    return File(content, file.ContentType, file.Name);
                }
            }

        }

        private record struct MessageInfo(string subj,string shortBody,string from,string to);
        [HttpPost("getMailMessages")]
        public IActionResult GetAllMailMessages([FromQuery]string? Sended_0_Received_1,  [FromQuery] string login, [FromQuery] GetMailsType type, [FromQuery] int messageLimit, [FromQuery] string pass)
        {
            var res = MailService.GetAllSendedMailMessages(login, pass, type, messageLimit);
            var resp = new List<MessageInfo>();
           
            foreach(var v in res)
            {
                MessageInfo m = new MessageInfo();

                m.subj=v.Subject;
                var mess = v.Body.ToString();
                m.shortBody =mess.Substring(0,Math.Min(50,mess.Length));
                m.from = v.From.Mailboxes.First().ToString();
                m.to = v.To.Mailboxes.First().ToString();

                resp.Add(m);
                
            }
          return Ok(resp);
        }

        [HttpPost("dadata")]
        public IActionResult GetAddress( [FromBody] GeoPoint pos)
        {
            if (pos.mLatitude == 0 || pos.mLongitude == 0) pos = new GeoPoint(86.137, 55.335);
            BinmanAddresStorage bg = new BinmanAddresStorage();
            string Address = DadataApi.GetAddress(pos,bg,true);

            return Ok(new object[] { bg, Address });
        }

        [HttpGet("/toLower")]
        public ActionResult ToLower([FromQuery] string message)
        {
            return Ok(message.ToLower());
        }
#endif
    }

    public static class ComputeSimilarity
    {
        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarity(this string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
    }
}
