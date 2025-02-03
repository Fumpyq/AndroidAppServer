using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan.PageParsers;
using BinManParser.Api;
using ClosedXML.Excel;
using static ADCHGKUser4.Controllers.Libs.SQL;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocumentParser;

namespace AndroidAppServer.Libs.BinMan
{

    public static class ОстановкиДоговоровНачислений
    {    /// <summary>
         /// Работает на SQL, в котором нужно предварительно поменять запрос или'и Пометить договоры
         /// </summary>
        public static void ХзЧетУстаревшее()
        {
            if(BinManApi.Accounts.Length<=0)
                BinManApi.LogInAccounts().GetAwaiter().GetResult();
            var recs3 = SQL.GetDogListToSincBinMan();
            foreach (var v in recs3)
            {
                if (string.IsNullOrEmpty(v.bin_id) || v.bin_id == "0" || v.bin_id.Contains("-"))
                {

                }
                else
                {
                    LoginData ld = BinManApi.GetNextAccount();

                    if (BinManDocuments.SendEditRequest(ld, v))
                    {
                        BinManDocAccrualsParser.TrySearchUntil((x) =>
                        {
                            var ff = v;
                            if (x.randomComment == "Вывоз и утилизация ТКО за период с 01.06.24 по 30.06.24" && x.SUMM >0)
                            {
                                BinManDocAccruals.CreateCorrectir(ld, new BinManAccrual()
                                {
                                    summ = (-x.SUMM).ToString("f2"),
                                    parentBinId = x.ID,
                                    typeRaw = "19",
                                    date = DateTime.Now,
                                    doc_BinId = ff.bin_id

                                }, out _);
                            }

                            return  BinManDocAccrualsParser.SearchCommand.DoNothing;
                        }, (x) => BinManDocAccrualsParser.SearchCommand.DoNothing, ld, v.bin_id, out _);

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
    
        public static void ПриостановкаДоговоровПоExcel(string excel,string BinMan_КодНовогоТарифа_ПродолжитьС, DateTime ОстановитьС,DateTime ПродолжитьС)
        {
            BinManApi.LogInAccounts().GetAwaiter().GetResult();
            //var BookPath = "C:\\Users\\a.m.maltsev\\Downloads\\Список договоров на приостановку 2024-01-23.xlsx";
            var BookPath = excel;
            var FileName = Path.GetFileNameWithoutExtension(BookPath);
            var Savepath = BookPath.Replace(FileName, FileName + " " + DateTime.Now.ToString("HH-MM"));
            using var book = new XLWorkbook(BookPath);
            var es = book.Worksheets.First();
            var c = es.Column(1);
            var cc = c.LastCellUsed();
            int l = 0;
            var SkipFirstRow = false;
            try
            {
                l = cc.Address.RowNumber;
            }

            catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
            // l = Math.Min(l, 3);
            var DateStopFrom = ОстановитьС; //new DateTime(2025, 01, 01);
            var DateContinueFrom = ПродолжитьС;//new DateTime(2026, 1, 1);
            for (int i = (SkipFirstRow ? 2 : 1); i <= l; i++)
            {
                string id1 = es.Cell(i, 1).Value.ToString().Trim().NDoc2BinId();

                // string id2 = es.Cell(i, 2).Value.ToString().Trim();

                var did = id1;
                //   var oId = "637303";
                var lcd = BinManApi.GetNextAccount();
                #region RERE
                //if (BinManDocumentParser.TryFindObject(lcd, did, oId, out var resobj))
                //{
                //    var NotNullTarif = 0f;
                //    var tt = 0f;
                //    //if (float.TryParse(resobj.tarif_volume, out var tt)) {
                //    //     NotNullTarif = tt;
                //    //}
                //    if (NotNullTarif <= 0)
                //    {
                //        var trv = resobj.changes.FirstOrDefault(x =>
                //        {
                //            if (float.TryParse(x.tarif_volume, out tt))
                //            { return !string.IsNullOrEmpty(x.tarif_volume) && tt > 0; }
                //            return false;
                //        }
                //        , new BinManDocumentParser.DocObjectChange() { tarif_volume = "-1" }).tarif_volume;
                //        if (float.TryParse(trv, out tt))
                //        {
                //            NotNullTarif = tt;

                //        }
                //    }

                //    if (NotNullTarif <= 0)
                //    {
                //        BinManDocuments.SendStopObjectRequest(lcd, new BinManDocuments.StopDogObject()
                //        {
                //            dog_BinId = did,
                //            object_BinId = oId,
                //            DateFrom = new DateTime(2024, 1, 1)
                //        });

                //        BinManDocuments.SendAttachObjectRequest(lcd, new BinManDocuments.AttachObjectInfo()
                //        {
                //            doc_BinManId = did,
                //            activeFrom = new DateTime(2025, 1, 1),
                //            obj_BinManId = oId,
                //            tarif_BinManCode = "255",
                //            tarif_value = NotNullTarif.ToString()
                //        });
                //    }
                //}
                #endregion

                if (BinManDocumentParser.TryParseObjects(lcd, did, out var resobj))
                {

                    //if (float.TryParse(resobj.tarif_volume, out var tt)) {
                    //     NotNullTarif = tt;
                    //}

                    foreach (var v in resobj)
                    {
                        #region ОтсанавливаемВсеЗаписиМеждуНачаломИКонцомДиопазона
                        var oId = v.binid;
                        var InBetween = v.changes.Where(x =>
                            x.DT_PeriodFrom != DateTime.MinValue && x.DT_PeriodFrom > DateStopFrom && x.DT_PeriodTo < DateContinueFrom && x.DT_PeriodFrom < DateContinueFrom
                            );

                        var StopList = InBetween.Where(x => x.Status == BinManDocumentParser.DocObjectChangeStatus.Active);

                        var AddTarifList = InBetween;
                        var EndTarif = v.changes.FirstOrDefault(x => x.DT_PeriodFrom != DateTime.MinValue && x.DT_PeriodFrom >= DateContinueFrom, DocObjectChange.Empty);
                        foreach (var Stop in StopList)
                        {
                            if (BinManDocuments.SendStopObjectRequest(lcd, new BinManDocuments.StopDogObject()
                            {
                                dog_BinId = did,
                                object_BinId = oId,
                                DateFrom = Stop.DT_PeriodFrom
                            })) { es.Cell(i, 5).Value += $"O:{oId} D:{Stop.DT_PeriodFrom};"; }
                            else
                            {
                                es.Cell(i, 5).Value += $"O:{oId} D:Не удалось ?!, но планировалось {Stop.DT_PeriodFrom};";
                            }
                        }
                        #endregion
                        //if (true) continue;

                        var NotNullTarif = 0f;
                        var tt = 0f;
                        //var did = "637304";

                        if (NotNullTarif <= 0)//Пытаемся найти не 0й объем (обычно в человеках) в истории по объекту ?
                        {
                            var trv = v.changes.FirstOrDefault(x =>
                            {
                                if (float.TryParse(x.tarif_volume, out tt))
                                { return !string.IsNullOrEmpty(x.tarif_volume) && tt > 0; }
                                return false;
                            }
                            , new BinManDocumentParser.DocObjectChange() { tarif_volume = "-1" }).tarif_volume;
                            if (float.TryParse(trv, out tt))
                            {
                                NotNullTarif = tt;

                            }
                        }

                        if (NotNullTarif > 0)//Нашли - тогда можно приостанавливать, и продолжать договор с введенной даты {ПродолжитьС}
                        {
                            if (BinManDocuments.SendStopObjectRequest(lcd, new BinManDocuments.StopDogObject()
                            {
                                dog_BinId = did,
                                object_BinId = oId,
                                DateFrom = DateStopFrom
                            }))
                            {

                                if (BinManDocuments.SendAttachObjectRequest(lcd, new BinManDocuments.AttachObjectInfo()
                                {
                                    doc_BinManId = did,
                                    activeFrom = DateContinueFrom,
                                    obj_BinManId = oId,
                                    tarif_BinManCode = BinMan_КодНовогоТарифа_ПродолжитьС,
                                    tarif_value = NotNullTarif.ToString()
                                }))
                                {

                                }
                                else
                                {
                                    LogError();
                                }
                            }
                            else
                            {
                                LogError();

                            }
                            void LogError()
                            {
                                es.Cell(i, 2).Value += $"Не получилось o: {oId};";
                            }
                        }
                    }
                }

            }
            book.SaveAs(Savepath);
        }
    
    
    }
}
