using ADCHGKUser4.Controllers.Libs;
using ClosedXML.Excel;
using Dadata;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
    public class MassDadata
    {

        List<LoginInstance> Accs = new List<LoginInstance>();
        public int CurrentToken;

        public class LoginInstance
        {
            public string token;
            SuggestClientSync api;

            public LoginInstance(string token)
            {
                this.token = token;
                api = new SuggestClientSync(token);
            }
        }

      
        public static void SendMassRequest()
        {
              // SQL.UpdateAddress
        }
        /// <summary>
        /// WARN ! Проставляется прямо в базу, и запрашивает дадату
        /// </summary>
        /// <param name="ExcellFilePath"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="targetColumn"></param>
        /// <param name="SkipFirstRow"></param>
        public static void МассоваяПростановкаАдресаГеозонам(string ExcellFilePath, bool SkipFirstRow = true)
        {
            var SaveAt = Path.GetDirectoryName(ExcellFilePath) + Path.GetFileNameWithoutExtension(ExcellFilePath) + ".res" + Path.GetExtension(ExcellFilePath);



            if (File.Exists(SaveAt)) { ExcellFilePath = SaveAt; }

            using var book = new XLWorkbook(ExcellFilePath);

            var es = book.Worksheets.First();

            var c = es.Column(1);
            var cc = c.LastCellUsed();
            int l = 0;
            try
            {
                l = cc.Address.RowNumber;
            }
            catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }

            int saveStep = 25;
            // l = Math.Min(2000,l);
            for (int i = SkipFirstRow ? 2 : 1; i <= l; i++) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! 2= > TEST Потом поменять
            {

              
                var DuplicateCheck = es.Cell(i, 4).Value.ToString().Trim();

                if (!string.IsNullOrEmpty(DuplicateCheck))
                {
                    continue;
                }
                var BinId = es.Cell(i, 1).Value.ToString().Trim();
                var Address = es.Cell(i, 3).Value.ToString().Trim();
                var GeozoneDbGuid = SQL.GetGeozoneGuidByBinId(BinId);
   

                if (DadataApi.TryFindAddressByAddress(Address,out var Addr))
                {

                    if(SQL.UpdateGeozoneAddressWithPartAddress(GeozoneDbGuid, Addr))
                    es.Cell(i, 4).Value = "OK";
                    else
                        es.Cell(i, 4).Value = "fail";
                }
                else
                {
                    es.Cell(i, 4).Value = "Не найден";
                    es.Cell(i, 4).Style.Fill.BackgroundColor = XLColor.Redwood;
                }

                saveStep--;
                if (saveStep <= 0)
                {
                    book.SaveAs(SaveAt);
                    saveStep = 25;
                }
            }
            book.SaveAs(SaveAt);
        }

        public static void ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец(string ExcellFilePath,int sourceColumn,int targetColumn,bool SkipFirstRow = true)
        {
            var SaveAt = Path.GetDirectoryName(ExcellFilePath) + Path.GetFileNameWithoutExtension(ExcellFilePath) + ".res" + Path.GetExtension(ExcellFilePath);

            

            if(File.Exists(SaveAt)) { ExcellFilePath = SaveAt; }

            using var book = new XLWorkbook(ExcellFilePath);
        
            var es = book.Worksheets.First();

            var c = es.Column(1);
            var cc = c.LastCellUsed();
            int l = 0;
            try
            {
                l = cc.Address.RowNumber;
            }
            catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
            
            int saveStep = 25;
           // l = Math.Min(2000,l);
            for (int i = SkipFirstRow ? 2 : 1; i <= l; i++) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! 2= > TEST Потом поменять
            {

                var Address = es.Cell(i, sourceColumn).Value.ToString().Trim();
                var DuplicateCheck = es.Cell(i, targetColumn).Value.ToString().Trim();
                var dd2 = false;
                if (Address.Contains(" здом ") && string.IsNullOrEmpty(DuplicateCheck))
                {
                    Address = Address.Replace(" здом ", " зд ");
                    dd2 = true;
                }
                var dd3 = false;
                if (!DuplicateCheck.Contains(", дом") && !DuplicateCheck.Contains("Не найден") && DuplicateCheck.Contains("дом "))
                {
                   
                    dd3 = true;
                }
                var Dd = DuplicateCheck.Contains(" здом ");
                if ((!string.IsNullOrEmpty(DuplicateCheck) && !(dd2)) && (DuplicateCheck != "Подозрительно короткий"))
                {
                    continue;
                }

                if (Address.Length < 3)
                {
                    es.Cell(i, targetColumn).Value = "Подозрительно короткий";
                    es.Cell(i, targetColumn).Style.Fill.BackgroundColor = XLColor.DarkCyan;
                }
                else
                {
                    Thread.Sleep(24);
                    if (DadataApi.TryFindAddressByAddress(Address, out var res))
                    {
                        es.Cell(i, targetColumn).Value = res.value.Replace("Кемеровская область - Кузбасс, ", "").Replace(", д ", ", дом ");
                        es.Cell(i, targetColumn).Style.Fill.BackgroundColor = XLColor.NoColor;
                    }
                    else
                    {
                        es.Cell(i, targetColumn).Value = "Не найден";
                        es.Cell(i, targetColumn).Style.Fill.BackgroundColor = XLColor.Redwood;
                    }
                   
                }
                saveStep--;
                if (saveStep <= 0)
                {
                    book.SaveAs(SaveAt);
                    saveStep = 25;
                }
            }
            book.SaveAs(SaveAt);
        }
    }
}
