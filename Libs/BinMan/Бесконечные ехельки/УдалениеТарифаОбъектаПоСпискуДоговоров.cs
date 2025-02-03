using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class УдалениеТарифаОбъектаПоСпискуДоговоров
    {
        public static void УдалитьВсеОбъекты(LoginData ld, params string[] idDogovor_ToFix)
        {
            foreach (var d in idDogovor_ToFix) {
               if( BinManDocumentParser.TryParseObjects(ld,d,out var Objs)){
                    foreach(var v in Objs)
                    {
                        foreach(var link in v.changes)
                        BinManDocuments.SendDestroyLinkRequest(ld, d, link.link_bin_id);
                    }

                }
            }
            
        }
    }
}
