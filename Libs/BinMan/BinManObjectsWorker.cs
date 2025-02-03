using ADCHGKUser4.Controllers.Libs;

namespace AndroidAppServer.Libs.BinMan
{
    public static class BinManObjectsWorker
    {
        public static void StartWorker()
        {
            Task.Run(() => {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(20000);
                        SQL.BinManLoad_LoadObjectFromBinManByCustomSql();
                    }
                    catch (Exception ex) {
                        Log.Error(ex);
                    }
                  
                }
                    });
        }
    }
}
