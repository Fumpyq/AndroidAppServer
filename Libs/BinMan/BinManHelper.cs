using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using BinManParser.Api;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManClientParser;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocumentParser;

namespace AndroidAppServer.Libs.BinMan
{
    public static class BinManHelper
    {
        public static bool ParseToDbDocumentObjects(string docId,string docGuid)
        {

            LoginData logdata = BinManApi.GetNextAccount();
            if (BinManDocumentParser.TryParseObjects(logdata, docId, out var res))
            {


                for (int j = 0; j < res.Count; j++)
                {
                    Log.Text("Load: " + j);
                    SQL.InsertBinManObjectsParse(res[j], docGuid);
                }
                GC.Collect();
                SQL.UpdateDogBinManObjectLoadStatus(docGuid);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void LoadObjectParse2Bd(string docGuid,List<DocObject> dobjs)
        {
            for (int j = 0; j < dobjs.Count; j++)
            {
                SQL.InsertBinManObjectsParse(dobjs[j], docGuid);
            }
        }       
        public static void LoadDocumentParse2Bd(BinDocDetails data,string clientGuid,string docGuid)
        {
            SQL.InsertBinManDocParse(data, clientGuid,docGuid);
        }       
        public static void LoadClientParse2Bd(BinClientInfo data,string clientGuid)
        {
            SQL.InsertBinManClientParse(data,clientGuid);
        }

        public static void LoadFullDocumentParse(BinDocumentParse parse,string DocGuid)
        {
           

            LoadObjectParse2Bd( DocGuid, parse.objects);
            string clientGuid = Guid.NewGuid().ToString();
            LoadDocumentParse2Bd(parse.docInfo, clientGuid, DocGuid);
            LoadClientParse2Bd(parse.client, clientGuid);
            SQL.CompleteBinParse(DocGuid);
        }
    }
}
