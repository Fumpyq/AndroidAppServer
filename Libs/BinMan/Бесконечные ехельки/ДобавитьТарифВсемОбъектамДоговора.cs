using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public static class ДобавитьТарифВсемОбъектамДоговора
    {
        public static void Run(LoginData ld,DateTime date,int TarifCode= 243, params string[] dogBinIds)
        {
            foreach (var d in dogBinIds)
            if (BinManDocumentParser.TryParseObjects(ld, d, out var resobj))
            {
                foreach (var oo in resobj)
                {
                    var tar = oo.tarif_volume;
                        if (oo.taxSumm == "0,00") continue; // Условно Приостановленный
                        if (BinManDocuments.SendAttachObjectRequest(ld, new BinManDocuments.AttachObjectInfo()
                    {

                        doc_BinManId = d,
                        activeFrom = date,
                        obj_BinManId = oo.binid,
                        tarif_BinManCode = TarifCode.ToString(), // TARIF ! 243 2024-07 административные здания 5,03 руб/м2
                        tarif_value = tar//ACTUAL VALUE

                    }))
                    {

                    }
                }
            }
        }
    }
}
