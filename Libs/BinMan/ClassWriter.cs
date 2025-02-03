using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs.BinMan;
using Newtonsoft.Json;
using System;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinManParser.Api
{
    public static class ClassWriter
    {
        public static void WritefromText(string text)
        {
            string[] res = text.Split("\n");

            StringBuilder sb = new StringBuilder();

            sb.Append("public Dictionary<string, string> GetEditFormData(string sessid) =>\r\n\r\n    new Dictionary<string, string>()\r\n   { { \"sessid\",sessid},");

            foreach (string s in res) {
                s.Replace("\n", "");
                if (s.Split("]").Length <= 1) continue;
                string value = s.Split("]")[^2];
                value = value.Split("[")[^1].Replace ("[","").Replace("]","");

                sb.AppendLine($"{{ \"{s.Split(":")[0].Replace(":", "")}\",{value} }},");
                    }
            sb.AppendLine("  };");
            Console.WriteLine(sb.ToString());
            
        }
        private record struct RawData(BinManObjectData.BinManObjectSubType[] data);
        public static void WriteObjectEnumText(string text)
        {
            RawData rd = JsonConvert.DeserializeObject<RawData>(text);
            BinManObjectData.BinManObjectSubType[] res=rd.data;
            StringBuilder sb = new StringBuilder();
            foreach(var v in res)
            {
                sb.AppendLine($"/// <summary> {v.TITLE} </summary>\n{Rus2Eng(v.TITLE)} = {v.ID},");
            }
            Log.Text(sb.ToString());
        }
        public static void GenerateTSQlFromDataTable(string tableName, DataTable dt)
        {
            var sb = new StringBuilder();
            var fieldsSb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($@"USE [TKO]
GO

/****** Object:  Table [dbo].[temp_geo]    Script Date: 04.08.2023 10:24:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[{tableName}](");
            int i = 0;
            foreach (DataColumn dr in dt.Columns)
            {

                fieldsSb.AppendLine($"{(i != 0 ? "," : " ")}[{dr.ColumnName}] [nvarchar](MAX) NULL");
                i++;
            }
            sb.AppendLine(fieldsSb.ToString());
            sb.AppendLine(") ON [PRIMARY]\r\nGO");
            Log.Text(sb.ToString());

            sb.Clear();

            sb.AppendLine();

            sb.AppendLine($@"
-- ================================================
--Автоматически создано для загрузки таблицы {tableName}
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TYPE dbo.AutoTable_{tableName}
AS TABLE
(
");
            sb.AppendLine(fieldsSb.ToString());
            sb.AppendLine($@");
GO
CREATE OR ALTER PROCEDURE TableLoad_{tableName}
@dt AS dbo.AutoTable_{tableName} READONLY
,@errcount BigInt
AS
BEGIN
SET NOCOUNT ON;

INSERT INTO dbo.{tableName}");

            sb.AppendLine(@$"SELECT * FROM @dt;
END
GO");
            Log.Text(sb.ToString());
            //  return sb.ToString();
        }
        private static string Rus2Eng(string str)
        {
            string res ="";
            foreach (var c in str)
            {
                switch (char.ToLower(c))
                {
                    case 'й': addChar("y"); break;
                    case 'ц': addChar("ch"); break;
                    case 'у': addChar("u"); break;
                    case 'к': addChar("k"); break;
                    case 'е': addChar("e"); break;
                    case 'н': addChar("n"); break;
                    case 'г': addChar("g"); break;
                    case 'ш': addChar("sh"); break;
                    case 'щ': addChar("sch"); break;
                    case 'з': addChar("z"); break;
                    case 'х': addChar("h"); break;
                    case 'ъ': break;
                    case 'ф': addChar("f"); break;
                    case 'ы': addChar("i"); break;
                    case 'в': addChar("v"); break;
                    case 'а': addChar("a"); break;
                    case 'п': addChar("p"); break;
                    case 'р': addChar("r"); break;
                    case 'о': addChar("o"); break;
                    case 'л': addChar("l"); break;
                    case 'д': addChar("d"); break;
                    case 'ж': addChar("j"); break;
                    case 'э': addChar("i"); break;
                    case 'я': addChar("i"); break;
                    case 'ч': addChar("ch"); break;
                    case 'с': addChar("s"); break;
                    case 'м': addChar("m"); break;
                    case 'и': addChar("i"); break;
                    case 'т': addChar("t"); break;
                    case 'ь':  break;
                    case ' ': addChar("_"); break;
                    case 'б': addChar("b"); break;
                    case 'ю': addChar("yu"); break;


                }

                void addChar(string cr)
                {
                    bool isUpperCase = char.IsUpper(c);
                    res += isUpperCase? cr.ToUpper():cr;
                }
            }
            return res;
        }
    }
}
