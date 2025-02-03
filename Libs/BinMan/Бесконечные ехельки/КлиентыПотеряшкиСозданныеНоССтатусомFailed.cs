using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class КлиентыПотеряшкиСозданныеНоССтатусомFailed
    {
        /// <summary>
        /// Warning Вставляет idКлиентов в бд, По запросу из Sql
        /// </summary>
        public static void Load()
        {
            var Ld = BinManApi.GetNextAccount();
            var list = SQL.GetClientsFailedListToFixIfTheyCreated();

            foreach ( var client in list )
            {
             
                if (BinManKa.FindClientId(Ld,client,new DateTime(2024,08,05),out var BinId))

                SQL.BinManMarkClientSucces(client.KA_DbGuid,BinId,SQL.BinManOperationStatusString.OK);

            }

        }
    }
}
