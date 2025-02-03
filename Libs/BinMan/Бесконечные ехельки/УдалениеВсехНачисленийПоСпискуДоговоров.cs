using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocAccrualsParser;
using static AndroidAppServer.Libs.BinMan.Бесконечные_ехельки.СозданиеКорректировокНаСписокНачислений;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class УдалениеВсехНачисленийПоСпискуДоговоров
    {
        public static void УдалитьВсеНе0Начисления(LoginData ld, params string[] idDogovor_ToFix)
        {

           
            foreach (var v in idDogovor_ToFix)
            {
                List<string> AccrToDelete = new List<string>();
                BinManDocAccrualsParser.TrySearchUntil((x) =>
                {
                    if(x.payment != 0)
                    {
                        return BinManDocAccrualsParser.SearchCommand.DoNothing;
                    }
                    if (x.SUMM != 0)
                    {
                        AccrToDelete.Add(x.ID);
                        
                    }
                    return BinManDocAccrualsParser.SearchCommand.DoNothing;
                }, (x) => BinManDocAccrualsParser.SearchCommand.DoNothing, ld, v, out _);
                foreach (var a in AccrToDelete)
                    BinManDocAccruals.DeleteAccrual(ld, a, v);
            }
           
        }

        public static void УдалитьОшибочныеГоспошлины(LoginData ld, params string[] idDogovor_ToFix)
        {


            foreach (var v in idDogovor_ToFix)
            {
                bool SaveFirstNach = true;
                DocAccrualListInfo LastHead=null;
                List<string> AccrToDelete = new List<string>();
                BinManDocAccrualsParser.TrySearchUntil((x) =>
                {
                    if (x.payment != 0)
                    {
                        return BinManDocAccrualsParser.SearchCommand.DoNothing;
                    }
                   

                    if (x.SUMM == 215)
                    {
                        if (x.HeadRow != null)
                        {

                            if (x.HeadRow.DATE_ACCURAL_TIMESTAMP != new DateTime(2024, 08, 13) && x.HeadRow.randomComment != "Произвольная сумма")
                            {
                                return BinManDocAccrualsParser.SearchCommand.DoNothing;
                            }

                            LastHead = x.HeadRow;
                        }
                        else
                        {
                            if (x.DATE_ACCURAL_TIMESTAMP != new DateTime(2024, 08, 13) && x.randomComment != "Произвольная сумма")
                            {
                                return BinManDocAccrualsParser.SearchCommand.DoNothing;
                            }
                            LastHead = null;
                        }
                        if (LastHead == null && SaveFirstNach)
                        {
                            SaveFirstNach = false;
                            //Сохраняем такие начисления, в данном случае, просто не удаляя их
                        }
                        else
                        {
                            AccrToDelete.Add(x.ID);
                        }
                    }
                    return BinManDocAccrualsParser.SearchCommand.DoNothing;
                }, (x) => BinManDocAccrualsParser.SearchCommand.DoNothing, ld, v, out _);
                foreach(var a in  AccrToDelete)
                BinManDocAccruals.DeleteAccrual(ld, a, v);
            }
        }
    }
}
