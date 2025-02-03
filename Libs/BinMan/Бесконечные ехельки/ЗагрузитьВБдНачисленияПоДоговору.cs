using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using BinManParser.Api;
using System.Data;
using UUIDNext;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class ЗагрузитьВБдНачисленияПоДоговору
    {
        public static void ЗагрузитьНачисленияПоДоговоруВБД(string idDog)
        {
            var Dt = new DataTable();

            Dt.Columns.Add("id", typeof(Guid));
            Dt.Columns.Add("dog_binId");
            Dt.Columns.Add("accr_binId");
            Dt.Columns.Add("Начисление");
            Dt.Columns.Add("Платеж");
            Dt.Columns.Add("date", typeof(DateTime));
            Dt.Columns.Add("comment");

            var strr = SQL.CreateTableScript("Binman_Accruals", Dt);
            var ld = BinManApi.GetNextAccount();


            foreach(var x in  BinManDocAccrualsParser.ForEachAccrual(ld, idDog))
            {
                if(x.childs.Count<=0)
                Dt.Rows.Add(
                    Uuid.NewDatabaseFriendly(Database.SqlServer)
                    , idDog
                    , x.ID
                    , x.IsNach ?x.SUMM:0
                    , !x.IsNach ? x.SUMM : 0
                    , x.DATE_ACCURAL_TIMESTAMP
                    , x.randomComment);
                else
                foreach(var hist in x.childs)
                {
                    Dt.Rows.Add(
                    Uuid.NewDatabaseFriendly(Database.SqlServer)
                    , idDog
                    , x.ID
                    , x.IsNach ? x.SUMM : 0
                    , !x.IsNach ? x.SUMM : 0
                    , x.DATE_ACCURAL_TIMESTAMP
                    , x.randomComment);
                }
                if (Dt.Rows.Count >= 30)
                {
                    SQL.TableBulkInsert(ref Dt, "Binman_Accruals", null);
                    Dt.Rows.Clear();
                }
            }
          
            SQL.TableBulkInsert(ref Dt, "Binman_Accruals",null);
        }
    }
}
