using System.Net.Http;

namespace AndroidAppServer.Controllers
{

    public static class OdinCApi
    {
        public const string ApiBase = "Http://192.168.0.0:7777/";

        /// <summary>
        /// `Акт сверки`
        /// </summary>
        public static void Get_ActSverki()
        {
            //HttpClient client = new HttpClient();   

            //HttpRequestMessage hr = new HttpRequestMessage(HttpMethod.Get, ApiBase+"/test");

            //var res = client.Send(hr);

            //var data = res.Content.ReadAsStream();

            //var fileStream = Fik.Create("C:\\Users\\a.m.maltsev\\source\\repos\\AndroidAppServer\\FileTest");
            //data.Seek(0, SeekOrigin.Begin);
            //data.CopyTo(fileStream);
            //fileStream.Close();

        }


    }
}
