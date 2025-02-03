using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using ClosedXML.Excel;
using static ADCHGKUser4.Controllers.Libs.SQL;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class ОшибкиПоНачислениямФл
    {
        private class ExcelRow
        {
            public DateTime DogStartDate;
            public string ndoc;
            public string binId;
            public string id_nach;
            public int Row;
        }
        public static void Run(LoginData ld, string ExcellFilePath)
        {
            var docs = new List<ExcelRow>();
            var DeleteNachs = new HashSet<string>();

            using var book = new XLWorkbook(ExcellFilePath);
            var SaveAt = Path.GetDirectoryName(ExcellFilePath) + Path.GetFileNameWithoutExtension(ExcellFilePath) + ".res" + Path.GetExtension(ExcellFilePath);

            var es = book.Worksheets.First();

            var c = es.Column(1);
            var cc = c.LastCellUsed();
            int l = 0;
            try
            {
                l = cc.Address.RowNumber;
            }
            catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
            var SkipFirstRow = true;
            //  var Ndoc_Col = 6;
            var check_Col = 3;
            var NDog_Col = 6;
            var Date_Col = 8;
            var DogBinId_Col = 26;
            var IdNach_Col = 17;

            for (int i = SkipFirstRow ? 2 : 1; i <= l; i++) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! 2= > TEST Потом поменять
            {
                var check  = es.Cell(i, check_Col).Value.ToString().Trim();
                if (check != "INDIVIDUAL") continue;

                var IdDog       = es.Cell(i, NDog_Col).Value.ToString().Trim();
                var Date        = es.Cell(i, Date_Col).Value.ToString().Trim();
                var DogBinId    = es.Cell(i, DogBinId_Col).Value.ToString().Trim();
                var IdNach      = es.Cell(i, IdNach_Col).Value.ToString().Trim();


                var DateStart_dt = DateTime.Parse(Date);

                var row = new ExcelRow();
                row.ndoc = IdDog;
                row.DogStartDate = DateStart_dt;
                row.binId = DogBinId;
                row.id_nach = IdNach;
                row.Row = i;
                docs.Add(row);
                DeleteNachs.Add(row.id_nach);
            }

            foreach (var d in docs)
            {
                //if(false)
                BinManDocAccrualsParser.TrySearchUntil((x) =>
                {
                    //var ff = v;
                    if (DeleteNachs.Contains(x.ID))
                    {
                        //Необходимо удалить начисления за июль по договорам из файла (из Бинман и из Клинит)
                        //А в клините их и небыло ))
                        var ress = BinManDocAccruals.DeleteAccrual(ld, x.ID, d.binId);
                        return BinManDocAccrualsParser.SearchCommand.Break;
                    }

                    return BinManDocAccrualsParser.SearchCommand.DoNothing;
                }, (x) => BinManDocAccrualsParser.SearchCommand.DoNothing, ld, d.binId, out _);
                if (d.DogStartDate < new DateTime(2024, 08, 01))
                {
                    if (BinManDocumentParser.TryParseObjects(ld, d.binId, out var resobj))
                    {
                        foreach (var oo in resobj)
                        {
                            var tar = oo.tarif_volume;
                                //= SQL.GetObjectTariff(oo.binid, d.binId);
                            //В Бинман в карточке договора (кроме договоров у которых дата начала 01.08.2024)
                            //в объектах с 01.07.2024 проставить соответствующий тариф, остальные данные взять из предыдущей
                            //А какие остальные  ?, оно автоматом работает
                            if (BinManDocuments.SendAttachObjectRequest(ld, new BinManDocuments.AttachObjectInfo()
                            {

                                doc_BinManId = d.binId,
                                activeFrom = new DateTime(2024, 07, 01),
                                obj_BinManId = oo.binid,
                                tarif_BinManCode = "237", // TARIF !
                                tarif_value = tar//ACTUAL VALUE
                                
                            }))
                            {

                            }
                        }
                    }
                    else
                    {
                        WriteMessage("TryParseObjects fails");
                    }
                        //Важно поменять дату на нужный периуд !
                    if (BinManDocAccruals.TryGetAccrualSumm(ld, d.binId, new DateTime(2024, 07, 01), new DateTime(2024, 07, 31), out var Summa))
                    {//После исправления создать начисления за июль(кроме договоров у которых дата начала 01.08.2024).
                        BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
                        {
                            doc_BinId = d.binId,
                            comment = "",
                            summ = Summa.ToString(),
                            date = new DateTime(2024, 07, 31),
                            typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
                            dateFrom = new DateTime(2024, 07, 01),
                            dateTo = new DateTime(2024, 07, 31)

                        }, out var BinIdd);
                    }
                    else
                    {
                        WriteMessage("TryGetAccrualSumm fails");
                        continue;
                    }
                }
                WriteMessage("Ok");
                void WriteMessage(string Err)
                {
                    es.Cell(d.Row, check_Col).Value = Err;
                    book.Save();
                }
                
            }

            //SQL.MarkDogUpdated(v.Db_Guid, BinManOperationStatusString.OK, v.bin_id, v.Number);
        }
    
    }
}
