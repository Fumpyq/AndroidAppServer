using ADCHGKUser4.Controllers.Libs;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class СозданиеНачисленийПоСпискуДоговоровНаДату
    {
        public static void Run(LoginData ld, DateTime datefrom,DateTime dateTo, params string[] idDogovor)
        {
            foreach (var d in idDogovor)
            {
                if (BinManDocAccruals.TryGetAccrualSumm(ld, d, datefrom, dateTo, out var Summa))
                {
                    BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
                    {
                        doc_BinId = d,
                        comment = "",
                        summ = Summa.ToString(),
                        date = dateTo,
                        typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
                        dateFrom = datefrom,
                        dateTo = dateTo

                    }, out var BinIdd);
                }
            }
        }

        public static void СоздатьНачисленияПомесячноСДо(LoginData ld, DateTime datefrom, DateTime dateTo, params string[] idDogovor)
        {
            foreach (var d in idDogovor)
            {
                var DateFrom = datefrom;
                while (DateFrom <= dateTo)
                {
                    var MonthStart = new DateTime(DateFrom.Year, DateFrom.Month, 01);
                    var  SummMonthStart = 
                       // DateFrom > MonthStart ? DateFrom :
                        MonthStart;
                    var MonthEnd = new DateTime(DateFrom.Year, DateFrom.Month, DateTime.DaysInMonth(DateFrom.Year, DateFrom.Month));
                    var SummMonthEnd = dateTo < MonthEnd ? dateTo: MonthEnd;
                    if (BinManDocAccruals.TryGetAccrualSumm(ld, d, SummMonthStart, SummMonthEnd, out var Summa))
                    {
                        BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
                        {
                            doc_BinId = d,
                            comment = "",
                            summ = Summa.ToString(),
                            date = MonthEnd,
                            typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
                            dateFrom = MonthStart,
                            dateTo = MonthEnd

                        }, out var BinIdd);
                    }
                    DateFrom = DateFrom.AddMonths(1);
                     
                }
            }
        }

    }
}
