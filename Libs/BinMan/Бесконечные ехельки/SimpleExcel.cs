using ClosedXML.Excel;
using System.Reflection;

namespace AndroidAppServer.Libs.BinMan.Бесконечные_ехельки
{
   
    
    
    
    
    
    
    
    
    
    
    
    public static class SimpleExcel
    {
        public static List<T> CreateFormExcelRows<T>(string ExcellFilePath,bool isFirstRowHeader = true) where T : class, new()
        {
          
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
            var SkipFirstRow = isFirstRowHeader;

            List<T> result = new List<T>();
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int cols = es.LastColumnUsed().ColumnNumber();

            for (int i = SkipFirstRow ? 2 : 1; i <= l; i++) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! 2= > TEST Потом поменять
            {
                T instance = new T();
                
                for (int col = 1; col <= cols; col++)
                {
                    try
                    {
                        var cellVal = es.Cell(i, col).Value.ToString().Trim();
                        fields[col-1].SetValue(instance, cellVal);

                    }
                    catch (Exception ex)
                    {

                        //Log.Error(ex);
                    }
                }
                result.Add(instance);
                
            }
            return result;
        }
    }
}
