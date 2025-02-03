using ADCHGKUser4.Controllers.Libs;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using System;

namespace AndroidAppServer.Libs
{
    public record struct FormData(string TAO, string phoneEmail, string ULName, string naselPunkt);
    public class GoogleFormEmulator
    {

      

         public static Dictionary<string, string> GetCreateFormData(FormData data) =>
            
            

        new Dictionary<string, string>()
        { 
        { "entry.900241506", "CleanIT" },
        { "entry.376245371", data.TAO},//TAO
        { "entry.263872491", data.phoneEmail},//Phone / email
        { "entry.1781140519", data.ULName},//UL Name
        { "entry.2028655212", data.naselPunkt},//address
        { "entry.809158722", "0" },
        { "entry.231138980","feedback@"},
        { "entry.128775664", "ЮЛ"},
        { "entry.340770647", "Консультация для ЮЛ"},
        { "entry.1497848301", "Заявка на заключение договора (кнопка с сайта)"},
        { "entry.1025085495", "Нужна ОС"},
        { "entry.231138980_sentinel", ""},
        { "entry.128775664_sentinel", ""},
        { "entry.340770647_sentinel", ""},
        { "entry.1497848301_sentinel", ""},
        { "entry.1025085495_sentinel", ""},
        { "fvv", "1"},
        { "partialResponse", "[null,null,\"-579424948744562603\"]"},
        { "pageHistory", "0"},
        { "fbzx", "-579424948744562603"},
  };

        public static bool SendForm(FormData data)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {

                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, formURL);
                    var formdata = GetCreateFormData(data);
                    req.Content = new FormUrlEncodedContent(formdata);

                    var res = client.Send(req);

                    var txt = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (txt.Contains("Ты самый лучший оператор! Спасибо за работу!"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                    return false;
                }

            }
        }

    }
}
