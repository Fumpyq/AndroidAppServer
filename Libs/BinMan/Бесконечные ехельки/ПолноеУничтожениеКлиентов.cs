using DocumentFormat.OpenXml.Wordprocessing;
using System.Data.SqlClient;
using System.Data;
using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class ПолноеУничтожениеКлиентов
    {
        public static void УничтожитьВсех(bool ВБазеТоже)
        {
            var data=SQL.GetKaListForDeletion();

            var ld = BinManApi.GetNextAccount();
            // Ошибочка вышли 239700 - Договор
            // Закоменчено по причине загрузки через несколько запусков, в будущем можно расскоментить и запускать сразу все
            foreach (var v in data.dogsIds)
            {
                
                УдалениеТарифаОбъектаПоСпискуДоговоров.УдалитьВсеОбъекты(ld, v.binId);
                // BinManDocuments.D
                // SQL.SimpleTextExecute("DELETE from Objects WHERE id = @id", Params: new Dictionary<string, object> { { "@id", v.dbId } });
            }
            foreach (var v in data.objsIds)
            {
                BinManObject.SetNewGeozoneList(ld, new List<int>(),v.binId);
                BinManObject.DeleteObject(ld, v.binId);
               // SQL.SimpleTextExecute("DELETE from Objects WHERE id = @id", Params: new Dictionary<string, object> { { "@id", v.dbId } });
            }
            
            foreach (var v in data.dogsIds)
            {
                BinManDocuments.SendDeleteDogRequest(ld, v.binId);
                //УдалениеТарифаОбъектаПоСпискуДоговоров.УдалитьВсеОбъекты(ld, v.binId);
                // BinManDocuments.D
                // SQL.SimpleTextExecute("DELETE from Objects WHERE id = @id", Params: new Dictionary<string, object> { { "@id", v.dbId } });
            }
            foreach(var v in data.kaIds)
            {
                BinManKa.DeleteKlient(ld, v.binId);
            }
            //foreach (var v in data.kaIds)
            //{
            //    BinManObject.SetNewGeozoneList(ld, new List<int>(), v.binId);
            //    BinManObject.DeleteObject(ld, v.binId);
            //    // SQL.SimpleTextExecute("DELETE from Objects WHERE id = @id", Params: new Dictionary<string, object> { { "@id", v.dbId } });
            //}
        }
    }
}
