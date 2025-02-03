using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using static ADCHGKUser4.Controllers.Libs.SQL;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public static class ПростоОбновитьДоговорыВБинМанПоБД
    {
        public static void run()
        {
            var recs3 = SQL.GetDogListToSincBinMan();
            foreach (var v in recs3)
            {
                LoginData ld = BinManApi.GetNextAccount();
                if (!(string.IsNullOrEmpty(v.bin_id) || v.bin_id == "0" || v.bin_id.Contains("-")))
                {
                    if (BinManDocuments.SendEditRequest(ld, v))
                    {
                        SQL.MarkDogUpdated(v.Db_Guid, BinManOperationStatusString.OK, v.bin_id, v.Number);
                    }
                    else
                    {
                        SQL.DogIgnoreList.TryAdd(v.Db_Guid, "BinMan Fail");
                        SQL.MarkDogUpdated(v.Db_Guid, BinManOperationStatusString.Failed, string.Empty, string.Empty);

                    }

                }
            }
        }


    }
}
