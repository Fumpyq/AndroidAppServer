using ADCHGKUser4.Controllers.Libs;
using BinManParser.Api;
using HtmlAgilityPack;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocumentParser;
using System.Diagnostics;
using DocumentFormat.OpenXml.Presentation;

namespace AndroidAppServer.Libs.BinMan.PageParsers
{
    public static class BinManClientParser
    {
        public record class BinClientInfo(string BinId,string name, string fullname, string inn, string ogrn, string kpp, string regDate, string UrAddress, string UrAddressIndex, string UrFiasCode
            , string factAddress, string factAddressIndex, string FacFiasCode, string Phone, string Email, string BIK, string bankName, string rasSchet, string corSchet, string OKPO, string OKVED
            , string rukov, string rukovPosition,string Fio,string Passport);
        public static string Url_Get(string clientId) => API.BaseUrl + $"cabinet/clients/detail/{clientId}/?param=REQUISITES";
        public static bool TryParseClientInfo(LoginData login, string clientID,int GlobalTimeOut, out BinClientInfo client)
        {


            client = null;

            try
            {
               
                    Stopwatch sw = Stopwatch.StartNew();
                    HttpRequestMessage hm = new(HttpMethod.Get, Url_Get(clientID));
                    var cookie = API.GetDeffaultCookie(login, "");
                    var req = API.SendRequest(hm,
                           new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("X-Requested-With","XMLHttpRequest")
                           },
                           cookie,secTimeOut: GlobalTimeOut
                           );

                    var t = req.Content.ReadAsStringAsync();

                    t.Wait();

                    var res = t.Result;

                    var htmlDoc = new HtmlDocument();

                    htmlDoc.LoadHtml(res);
                    sw.Stop();
                    Log.Text("Http Time: " + sw.ElapsedMilliseconds + " ms");
                    sw.Restart();

                    var Rows = htmlDoc.DocumentNode.Descendants("td")
                    .Where(d=>
                    string.IsNullOrEmpty(d.Attributes["class"]?.Value) 
                    || (d.Attributes["class"]!=null && d.Attributes["class"].Value is "name" or "na")
              
                    //|| (!d.Attributes["class"].Value.Contains("name"))
                    );

               string  CompanyName        = string.Empty;
               string  Fio                = string.Empty;
               string  Passport           = string.Empty;
               string  FullName           = string.Empty;
               string  Inn                = string.Empty;
               string  Ogrn               = string.Empty;
               string  Kpp                = string.Empty;
               string  RegDate            = string.Empty;
               string  UrAddress          = string.Empty;
               string  IndeUrAddress      = string.Empty;
               string  KodFiasurAddress   = string.Empty;
               string  FactAddress        = string.Empty;
               string  IndeFactAddress    = string.Empty;
               string  KodFiasFactAddress = string.Empty;
               string  Phone              = string.Empty;
               string  Email              = string.Empty;
               string  Bik                = string.Empty;
               string  BankName           = string.Empty;
               string  RasSchet           = string.Empty;
               string  KorSchet           = string.Empty;
               string  Ruk                = string.Empty;
               string  RukPosition        = string.Empty;
               string  OKPO               = string.Empty;
                string Okved              = string.Empty;

                   try{ Fio                =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "ФИО:")                            .index+1).InnerText); }catch(Exception ex){}
                   try{ Passport           =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Паспорт:")                        .index+1).InnerText); }catch(Exception ex){}
                   try{ CompanyName        =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Наименование компании:")          .index+1).InnerText); }catch(Exception ex){}
                   try{ FullName           =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Полное наименование:")            .index+1).InnerText); }catch(Exception ex){}
                   try{ Inn                =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "ИНН:")                            .index+1).InnerText); }catch(Exception ex){}
                   try{ Ogrn               =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "ОГРН:")                           .index+1).InnerText); }catch(Exception ex){}
                   try{ Kpp                =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "КПП:")                            .index+1).InnerText); }catch(Exception ex){}
                   try{ RegDate            =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Дата регистрации:")               .index+1).InnerText); }catch(Exception ex){}
                   try{ UrAddress          =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Юридический адрес:")              .index+1).InnerText); }catch(Exception ex){}
                   try{ IndeUrAddress      =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Индекс по юридическому адресу:")  .index+1).InnerText); }catch(Exception ex){}
                   try{ KodFiasurAddress   =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Код ФИАС по юридическому адресу:").index+1).InnerText); }catch(Exception ex){}
                   try{ FactAddress        =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Фактический адрес:")              .index+1).InnerText); }catch(Exception ex){}
                   try{ IndeFactAddress    =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Индекс по фактическому адресу:")  .index+1).InnerText); }catch(Exception ex){}
                   try{ KodFiasFactAddress =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Код ФИАС по фактическому адресу:").index+1).InnerText); }catch(Exception ex){}
                   try{ Phone              =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Телефон:")                        .index+1).InnerText); }catch(Exception ex){}
                   try{ Email              =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "E-mail:")                         .index+1).InnerText); }catch(Exception ex){}
                   try{ Bik                =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "БИК:")                            .index+1).InnerText); }catch(Exception ex){}
                   try{ BankName           =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Наименование банка:")             .index+1).InnerText); }catch(Exception ex){}
                   try{ RasSchet           =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Рассчетный счет:")                .index+1).InnerText); }catch(Exception ex){}
                   try{ KorSchet           =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Корреспонденсткий счет:")         .index+1).InnerText); }catch(Exception ex){}
                   try{ Ruk                =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Руководитель:")                   .index+1).InnerText); }catch(Exception ex){}
                   try{ RukPosition        =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "Должность руководителя:")         .index+1).InnerText); }catch(Exception ex){}
                   try{ OKPO               =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "ОКПО:")                           .index+1).InnerText); }catch(Exception ex){}
                   try{ Okved              =  BinManApi.TrimText( Rows.ElementAt(Rows.Select((e, index) => (e, index)).First(e=> BinManApi.TrimText(e.e.InnerText) is "ОКВЭД:")                          .index+1).InnerText); } catch (Exception ex) { }




                client = new BinClientInfo(
                        clientID,CompanyName,FullName,Inn,Ogrn,Kpp,RegDate,UrAddress,IndeUrAddress,KodFiasurAddress,FactAddress,IndeFactAddress,KodFiasFactAddress,Phone,Email
                        ,Bik,BankName,RasSchet,KorSchet,OKPO,Okved,Ruk,RukPosition,Fio,Passport
                        
                        );
                Log.Text("Client Parse Time: " + sw.ElapsedMilliseconds + " ms");
                sw.Stop();
                sw = null;
                return true;




                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);


                return false;
            }



        }

    }
}
