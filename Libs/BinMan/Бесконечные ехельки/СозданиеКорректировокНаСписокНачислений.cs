using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class СозданиеКорректировокНаСписокНачислений
    {
        public class AccrCorrectirFix
        {
            public string Binid_accr;
            public string Binid_dog;
        }
        public class КорректировкаНачисленияНаСумму
        {
            public string Binid_dog;
            public string Binid_accr;
            public string summ;
            public string summFinal;
        }
        public static void ЗанулитьНачисленияКорректировкой(LoginData ld ,params AccrCorrectirFix[] ToFix)
        {

            Dictionary<string, List<string>>  FixMap = new Dictionary<string, List<string>>();

            foreach(var v in ToFix)
            {
                if(FixMap.TryGetValue(v.Binid_dog,out var l)){
                    if (!l.Contains(v.Binid_accr))
                    l.Add(v.Binid_accr);
                }
                else
                {
                    FixMap.Add(v.Binid_dog,new List<string>() { v.Binid_accr});
                }

            }

            foreach (var v in FixMap)
            {
                BinManDocAccrualsParser.TrySearchUntil((x) =>
                {
                    var ff = v;
                    if (v.Value.Contains(x.ID))
                    {
                        v.Value.Remove(x.ID);
                        if (x.ID == "60479169" || x.HeadRow?.ID == "60479169")
                        {

                        }
                        if (x.SUMM > 0)
                        {

                        }
                        if ((x.HeadRow != null && x.HeadRow.SUMM >0)||(x.HeadRow == null && x.SUMM>0))
                        BinManDocAccruals.CreateCorrectir(ld, new BinManAccrual()
                        {
                            summ = (-(x.HeadRow!=null? x.HeadRow.SUMM:x.SUMM)).ToString("f2"),
                            parentBinId = x.ID,
                            typeRaw = "1",
                            date = DateTime.Now,
                            doc_BinId = ff.Key

                        }, out _);
                        if(v.Value.Count<=0) return BinManDocAccrualsParser.SearchCommand.Break;
                    }

                    return BinManDocAccrualsParser.SearchCommand.DoNothing;
                }, (x) => BinManDocAccrualsParser.SearchCommand.DoNothing, ld, v.Key, out _);
            }
        }
        public static void ОткаректироватьНачисленияНаСумму(LoginData ld,string ОбщийКомментарий, params КорректировкаНачисленияНаСумму[] ToFix)
        {
            foreach(var fix in ToFix.Skip(1))
            {
                BinManDocAccruals.CreateCorrectir(ld, new BinManAccrual()
                {
                    summ = fix.summ,
                    parentBinId = fix.Binid_accr,
                    typeRaw = "1",
                    date = new DateTime(2024, 11, 30),
                    doc_BinId = fix.Binid_dog,
                    finalSumm = fix.summFinal,
                    dateFrom = new DateTime(2024,11,01),
                    dateTo = new DateTime(2024, 11, 30),
                    comment = ОбщийКомментарий


                }, out _);
            }
        }
    }
}
