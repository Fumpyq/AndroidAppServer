using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Controllers;
using AndroidAppServer.FileTest;
using AndroidAppServer.Libs;
using AndroidAppServer.Libs.BinMan;
using AndroidAppServer.Libs.BinMan.PageParsers;
using AndroidAppServer.Libs.BinMan.Бесконечные_ехельки;
using BinManParser.Api;
using CHGKManager.Libs;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;
using Quartz.Impl.Triggers;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using static ADCHGKUser4.Controllers.Libs.SQL;
using static AndroidAppServer.Libs.BinMan.PageParsers.BinManDocumentParser;
using static AndroidAppServer.Libs.BinMan.Бесконечные_ехельки.СозданиеКорректировокНаСписокНачислений;

const string Cors = "MyAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{


    s.UseInlineDefinitionsForEnums();


});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(s => {});
    app.UseSwaggerUI();
}
//app.UseCors(Cors);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Lifetime.ApplicationStopped.Register(() =>
{
    DadataApi.OnAppClose();

});

//DadataApi.LoadSpecialThing11072024("C:\\Users\\a.m.maltsev\\Downloads\\DownloadsКоординаты.res.xlsx");

#region BinAPI
Console.Title = $"Android Server VER: '{Login.CurrentVersion}'";

string? bin = builder.Configuration.GetValue<string>("UseBinMan");
if (bin != null)
{
    if (BoolExtensions.SuperParse(bin))
    {

        Log.Warning("UseBinMan, Синхронизация с BinMan - ВЫКЛ");
    }
    else
    {
        BinManApi.IsApiEnabled = true;
        Log.Warning("UseBinMan не найден в конфигурации (appsettings), Синхронизация с BinMan - ВКЛ");
    }
}
else
{
    BinManApi.IsApiEnabled = true;
    Log.Warning("UseBinMan не найден в конфигурации (appsettings), Синхронизация с BinMan - ВКЛ");
}


#if DEBUG



BinManApi.IsApiEnabled = true;
#endif
BinManApi.IsApiEnabled = true;



//Task.УдалитьВсеНе0Начисления(() => { BinManApi.Init(); });
//await
//Task.УдалитьВсеНе0Начисления(() => { BinManApi.Init(); });
#if !DEBUG
await BinManApi.LogInAccounts();
Task.Run(()=>{ BinManApi.Init(); });
#endif
//await BinManApi.Init();
#endregion
ActiveDirectory ad = new ActiveDirectory(app.Configuration);
StaticSQl.Init();
DadataApi.Init();
Log.InitFileWriter();//12.12.2024
if (app.Environment.IsDevelopment() && false)
{

    {
        var ddaadata = SimpleExcel.CreateFormExcelRows<КорректировкаНачисленияНаСумму>("C:\\Users\\a.m.maltsev\\Downloads\\Корректировки бюджет 2024-12-11 (нобярь 2024) (1).xlsx");
        await BinManApi.LogInAccounts();
        var ld = BinManApi.GetNextAccount();


        СозданиеКорректировокНаСписокНачислений.ОткаректироватьНачисленияНаСумму(ld, "Техническая ошибка начисления, тарифа", ddaadata.ToArray());
    }
}

if (!app.Environment.IsDevelopment())
{
    Rosreestr.StartWorker();//!!!!!!!!!!!!!!!!!! --Это боевой код, можно в релиз
    BinManObjectsWorker.StartWorker();
}
if (app.Environment.IsDevelopment() && false)//Приостановка договоров
{
    //ОстановкиДоговоровНачислений.ПриостановкаДоговоровПоExcel("C:\\Users\\a.m.maltsev\\Downloads\\г. топки приостановленые обьекты на 2025 год_НОРМАЛЬНЫЙ.xlsx"
    //    , "291"
    //    , new DateTime(2025,01,01)
    //    , new DateTime(2026,01,01)
    //    );    
    ОстановкиДоговоровНачислений.ПриостановкаДоговоровПоExcel("C:\\Users\\a.m.maltsev\\Downloads\\приостановки на 2025 поселения (1) Норм.xlsx"
        , "291"
        , new DateTime(2025,01,01)
        , new DateTime(2026,01,01)
        );
}
   
    if (app.Environment.IsDevelopment() && false)
{
    //ImageFolderResize.ResizeAllInFolder("C:\\Users\\a.m.maltsev\\Desktop\\CrateMateObjMarkers");
   // DadataApi.AccurateObjectAddreses();//14.06.2024 -- Ищем кривые координаты Топканского р-на, села рассвет
    //await BinManApi.LogInAccounts();
    //var ldld = BinManApi.GetNextAccount();
    //var addr =  new Dadata.Model.Suggestion<Dadata.Model.Address>()
    //{
    //    data = new Dadata.Model.Address()
    //};
    //// BinManDocAccruals.TryGetAccrualSumm(ldld, "5906259",new DateTime(1800,01,02),new DateTime(1888,01,01),out var smama);
    //BinManDocuments.SQLInjectionSendCreateRequest(ldld, new BinManDocuments.BinManDogData()
    //{
    //    Number = "aaf",
    //    Client_BinManid = "5779536",
    //    Type_BinManCode = "13",
    //    Group_BinManCode = "1",
    //    dateFrom = DateTime.Now,
    //    dateTo = DateTime.Now,
    //    dateSign  = DateTime.Now,

    //}
    //    , out var bib) ;
    // 100000001
    //  await BinManApi.LogInAccounts();
    //  SQL.BinManLoad_LoadObjectFromBinManByCustomSql(); // Парсинг всех договоров на ОБЪЕКТЫ из бинман


    //ImageFolderResize.ResizeAllInFolder("C:\\Users\\a.m.maltsev\\Desktop\\CrateMateMarkersV7");
        if (false) // Удаление ошибочно созданных ссылок договор- оъбъект с неверным тарифом 03.05.2024 - Ошибка (07.05.2024 - Комментарий)
        {
            await BinManApi.LogInAccounts();
            var lddd = BinManApi.GetNextAccount();

            var recs4 = SQL.GetDogObjLinksListToSincBinMan();
            foreach (var v in recs4)
            {
                var dog_id = v.doc_BinManId;
                if (BinManDocumentParser.TryParseObjects(lddd, dog_id, out var resobj))
                {
                    if (resobj.Count > 1)
                    {
                        Log.Warning($"asdd {dog_id}");
                    }
                    foreach (var o in resobj)
                    {
                        var ToDelete = o.changes.Where(x => x.tarif_full_text == "2022-12 Проживающий 75,49 руб./чел");

                        foreach (var d in ToDelete)
                        {
                            BinManDocuments.SendDestroyLinkRequest(lddd, dog_id, d.link_bin_id);
                        }
                    }
                }
            }
        }
    // SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.08.2024 - загрузка 2024-08-05.xlsx");//05.08.2024
  

    // SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.03.2024 - загрузка 2024-02-27.xlsx");// Запускать тольк вместе с кусочком синхры с BinMan
    ///////////////////SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.05.2024 - загрузка 2024-05-02.xlsx");

    // var asdt =SQL.GetGeozoneCommentaries("781F7DED-4E65-468A-A41D-B1218BD174FB");
    // await BinManApi.LogInAccounts();
    // HandMadeLoadLoosedObjects();



    //await BinManApi.LogInAccounts();

    //var Cars = BinManCarParser.TryParseTarifs(BinManApi.GetNextAccount());
    //SQL.LoadBinManCarsInTemp(Cars);

    //var crs=   SQL.GetGeozoneVisits("1D93A646-D8C4-4788-B18E-D157069E6E12");
    // for(int i = 0; i < 8; i++)

    //BinManDocAccruals.AddAccrualToDoc(BinManApi.GetNextAccount(), new BinManAccrual()
    //{
    //    doc_BinId = "5906259",
    //    type = AccrualsType.accr_any_summ,
    //    dateFrom = new DateTime(2022, 12, 01),
    //    dateTo = new DateTime(2022, 12, 02),
    //    date = new DateTime(2022, 12, 02),
    //    summ = 0.98.ToString(),
    //    comment = "Test",
    //    db_guid = Guid.NewGuid().ToString()
    //},out string bin_id);

    /*

   var resese= BinManDocAccruals.CreateCorrectir(BinManApi.GetNextAccount(), new BinManAccrualCorrect()
    {
        doc_BinId = "5906259",
        type = AccrualsType.accr_by_doc,
        date = DateTime.Now,
        correctSumm = (-0.04).ToString()
        ,parentBinId = "57214243"
       // ,FinalSumm = "75.45"
        ,
       Comment = "TEST"
        ,db_guid = Guid.NewGuid().ToString()
   },out var bin_id2);
    */

}

//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Реестр МКД для перехода на прямые договоры с 01.03.2024 год (ред.).xlsx");
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Зарубино с 13.06.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Шишино с 18.06.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Центральный с 15.06.2024).xlsx");//19.06.2024
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Раздолье с 20.06.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Глубокое с 21.06.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Малый Корчуган с 19.06.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Верх-Падунский с 19.06.2024).xlsx");//22.06.2024 -- Дата загрузки файла
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Усть-Сосново с 24.06.2024).xlsx");//01.07.2024
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Усть-Сосново с 15.07.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Верх-Падунский с 15.07.2024).xlsx",
//    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Раздолье с 15.07.2024).xlsx");//24.07.2024
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (г Топки с 01.08.2024).xlsx");//02.08.2024

//ОшибкиПоНачислениямФл.УдалитьВсеНе0Начисления(lddd2, "C:\\Users\\a.m.maltsev\\Downloads\\Ошибки в базе ФЛ 2024-07-25 (исправления IT).xlsx"); //25.07.2024
//ДобавитьТарифВсемОбъектамДоговора.УдалитьВсеНе0Начисления(lddd2, "5164593" , "5164604");
//await BinManApi.LogInAccounts();
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Лс с 01.06.2024(ред.).xlsx");

//await BinManApi.LogInAccounts();
//ПростоОбновитьДоговорыВБинМанПоБД.run();
//SQL.DogReopeningFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\Заявка 16446 (Ред.).xlsx");

//GetBinmanGeozonesUpdateList();
//SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.03.2024 - загрузка 2024-02-27.xlsx");
//SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.07.2024 - загрузка 2024-07-05.xlsx");
//SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.07.2024 - загрузка 2024-06-14.xlsx");
//SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.11.2024 - загрузка 2024-11-11.xlsx");
//if (BinManApi.TryFormatPhoneNumberAsKa("79099099999", out var res123))
//SQL.GetBinManClientsTaskList();

//SQL.ReadBaseData(out var districtsList, out var containersMap, out var geoTypeMap);
if (app.Environment.IsDevelopment() // Отладочные запросы в BinMan 
                                   // && true
&& false
)
{
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();

    BinManContainers binContainer = new BinManContainers();



    binContainer.NAME = "ТЕСТОВЫЙ";
    binContainer.VOLUME = "0.11";
    binContainer.TYPE = Geo_container_type.evro;

    var code = binContainer.SendCreateRequest(ld, out var containerBin_Id);
}
if (app.Environment.IsDevelopment() // Запросы дадаты по файлу Ехель
                                    //&& true
&& false
)
{
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса 1.xlsx",1,2);
    // MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\Адреса в работу2.xlsx", 2, 3);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса перевозчиков.xlsx", 1, 2);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса диспетчерской.xlsx", 1, 2);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\Адреса диспетчерской в работу2.xlsx", 2, 3);
    //MassDadata.МассоваяПростановкаАдресаГеозонам("C:\\Users\\a.m.maltsev\\Downloads\\На исправление адресов геозон 2024-09-04.xlsx");
    MassDadata.МассоваяПростановкаАдресаГеозонам("C:\\Users\\a.m.maltsev\\Downloads\\На исправление адресов геозон 2024-09-05.xlsx");
}
if (app.Environment.IsDevelopment() // Запросы дадаты по файлу Ехель
                                  //  && true
&& false
) {
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса 1.xlsx",1,2);
    // MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\Адреса в работу2.xlsx", 2, 3);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса перевозчиков.xlsx", 1, 2);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса диспетчерской.xlsx", 1, 2);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\Адреса диспетчерской в работу2.xlsx", 2, 3);
    //MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\адреса 30.08.xlsx", 1, 2,false);
    MassDadata.ПоЭкселю1йСтолбецСРезультатомВо2йСтолбец("C:\\Users\\a.m.maltsev\\Downloads\\Адреса в формат DaData.xlsx", 1, 2,true);
}
if (app.Environment.IsDevelopment() // Массовые начисления
                                    // && true
&& false
)
{
    await BinManApi.LogInAccounts();
    LoginData ld = BinManApi.GetNextAccount();
    СозданиеНачисленийПоСпискуДоговоровНаДату.СоздатьНачисленияПомесячноСДо(ld, new DateTime(2023, 06, 20), new DateTime(2024, 07, 31)
      // , "5964721"
, "5964621"
, "5964738"
, "5964750"
, "5964601"
, "5964609"
, "5964639"
, "5964599"
, "5964598"
, "5964710"
, "5964730"
, "5964604"
, "5964746"
, "5964758"
, "5964690"
, "5964607"
, "5964647"
, "5964720"
, "5964766"
, "5964650"
, "5964582"
, "5964587"
, "5964722"
, "5964706"
, "5964613"
, "5964614"
, "5964666"
, "5964585"
, "5964763"
, "5964687"
, "5964744"
, "5964596"
, "5964590"
, "5964632"
, "5964608"
, "5964741"
, "5964620"
, "5964686"
, "5964694"
, "5964683"
, "5964593"
, "5964696"
, "5964691"
, "5964732"
, "5964652"
, "5964629"
, "5964692"
, "5964699"
, "5964584"
, "5964656"
, "5964739"
, "5964684"
, "5964624"
, "5964631"
, "5964734"
, "5964723"
, "5964677"
, "5964678"
, "5964747"
, "5964762"
, "5964648"
, "5964605"
, "5964709"
, "5964633"
, "5964660"
, "5964753"
, "5964719"
, "5964733"
, "5964654"
, "5964714"
, "5964675"
, "5964627"
, "5964759"
, "5964657"
, "5964644"
, "5964662"
, "5964701"
, "5964700"
, "5964715"
, "5964724"
, "5964767"
, "5964623"
, "5964717"
, "5964742"
, "5964619"
, "5964622"
, "5964610"
, "5964681"
, "5964752"
, "5964583"
, "5964718"
, "5964708"
, "5964702"
, "5964665"
, "5964653"
, "5964725"
, "5964642"
, "5964659"
, "5964603"
, "5964740"
, "5964630"
, "5964658"
, "5964663"
, "5964617"
, "5964754"
, "5964635"
, "5964606"
, "5964670"
, "5964689"
, "5964589"
, "5964682"
, "5964661"
, "5964698"
, "5964672"
, "5964680"
, "5964697"
, "5964760"
, "5964612"
, "5964713"
, "5964770"
, "5964749"
, "5964626"
, "5964727"
, "5964737"
, "5964761"
, "5964664"
, "5964634"
, "5964751"
, "5964685"
, "5964600"
, "5964594"
, "5964588"
, "5964704"
, "5964679"
, "5964637"
, "5964748"
, "5964628"
, "5964729"
, "5964768"
, "5964769"
, "5964640"
, "5964735"
, "5964716"
, "5964745"
, "5964676"
, "5964581"
, "5964611"
, "5964641"
, "5964625"
, "5964618"
, "5964755"
, "5964597"
, "5964726"
, "5964615"
, "5964674"
, "5964712"
, "5964707"
, "5964638"
, "5964771"
, "5964636"
, "5964695"
, "5964645"
, "5964765"
, "5964693"
, "5964646"
, "5964736"
, "5964616"
, "5964592"
, "5964667"
, "5964743"
, "5964756"
, "5964651"
, "5964703"
, "5964731"
, "5964602"
, "5964586"
, "5964764"
, "5964757"
, "5964671"
, "5964668"
, "5964669"
, "5964595"
, "5964655"

       );
    СозданиеНачисленийПоСпискуДоговоровНаДату.СоздатьНачисленияПомесячноСДо(ld, new DateTime(2022, 11, 01), new DateTime(2024, 05, 31)
       // , "5964673"
, "5964649"
, "5964705"
, "5964591"
, "5964643"
, "5964711"
, "5964688"
, "5964728"

        );
}

if (app.Environment.IsDevelopment() // Удаление множественных госпошлин
 //&& true
&& false
)
{
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();
    УдалениеВсехНачисленийПоСпискуДоговоров.УдалитьОшибочныеГоспошлины(ld,
  "888676"
, "890621"
, "891572"
, "894476"
, "896380"
, "897046"
, "897085"
, "897481"
, "899455"
, "899992"
, "900613"
, "902627"
, "906749"
, "909851"
, "911942"
, "912413"
, "913334"
, "913355"
, "915533"
, "915614"
, "918044"
, "918620"
, "919103"
, "925208"
, "930254"
, "1000186"
, "1011181"
        );
}
    if (app.Environment.IsDevelopment()
    //&& true
    && false
    )
{
    //await BinManApi.LogInAccounts();
    //var ld = BinManApi.GetNextAccount();
    //var Bc = new BinManContainers();
    //Bc.VOLUME = "0.01";
    //Bc.NAME = "Test";
    //Bc.SendCreateRequest(ld,out var BinId);
}
    if (app.Environment.IsDevelopment()
    //&& true
    && false
    )
{
    //  ОстановкиДоговоровНачислений.УдалитьВсеНе0Начисления();//10.07.2024 //C:\Users\a.m.maltsev\Downloads\Верх-Падунский_ Раздолье_ Усть-Сосоново.xlsx
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();

    СозданиеКорректировокНаСписокНачислений.ЗанулитьНачисленияКорректировкой(ld,
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649113", Binid_accr ="59721373" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648509", Binid_accr ="59721172" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648479", Binid_accr ="59721162" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648675", Binid_accr ="59721227" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648954", Binid_accr ="59721320" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648852", Binid_accr ="60479200" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648963", Binid_accr ="60479237" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648729", Binid_accr ="60479159" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648675", Binid_accr ="60479141" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648614", Binid_accr ="59721207" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648843", Binid_accr ="60479197" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648948", Binid_accr ="60479232" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648906", Binid_accr ="59721304" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648614", Binid_accr ="60479121" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648951", Binid_accr ="60479233" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648906", Binid_accr ="60479218" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648855", Binid_accr ="60479201" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648771", Binid_accr ="59721259" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649068", Binid_accr ="60479272" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648717", Binid_accr ="60479155" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648711", Binid_accr ="59721239" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649116", Binid_accr ="59721374" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648750", Binid_accr ="60479166" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648774", Binid_accr ="59721260" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648888", Binid_accr ="59721298" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648626", Binid_accr ="59721211" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648431", Binid_accr ="60479060" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648620", Binid_accr ="59721209" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648792", Binid_accr ="60479180" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648819", Binid_accr ="59721275" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648690", Binid_accr ="59721232" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648870", Binid_accr ="60479206" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648732", Binid_accr ="60479160" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648825", Binid_accr ="60479191" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648512", Binid_accr ="59721173" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648966", Binid_accr ="60479238" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648870", Binid_accr ="59721292" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648828", Binid_accr ="60479192" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648638", Binid_accr ="59721215" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649026", Binid_accr ="59721344" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648542", Binid_accr ="59721183" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648587", Binid_accr ="59721198" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648792", Binid_accr ="59721266" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649044", Binid_accr ="59721350" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648542", Binid_accr ="60479097" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648569", Binid_accr ="60479106" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648512", Binid_accr ="60479087" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648984", Binid_accr ="60479244" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648452", Binid_accr ="59721153" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648626", Binid_accr ="60479125" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649053", Binid_accr ="59721353" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648900", Binid_accr ="59721302" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648660", Binid_accr ="60479136" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649053", Binid_accr ="60479267" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648500", Binid_accr ="59721169" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648497", Binid_accr ="59721168" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649104", Binid_accr ="59721370" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649056", Binid_accr ="60479268" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648476", Binid_accr ="60479075" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648476", Binid_accr ="59721161" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648557", Binid_accr ="59721188" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649107", Binid_accr ="59721371" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648467", Binid_accr ="60479072" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649098", Binid_accr ="59721368" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648482", Binid_accr ="60479077" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648921", Binid_accr ="59721309" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648515", Binid_accr ="60479088" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648527", Binid_accr ="60479092" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649101", Binid_accr ="60479283" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648497", Binid_accr ="60479082" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648924", Binid_accr ="60479224" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648566", Binid_accr ="60479105" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648500", Binid_accr ="60479083" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648981", Binid_accr ="60479243" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648921", Binid_accr ="60479223" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648798", Binid_accr ="59721268" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648987", Binid_accr ="59721331" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649023", Binid_accr ="60479257" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648623", Binid_accr ="60479124" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649083", Binid_accr ="59721363" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648789", Binid_accr ="59721265" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648822", Binid_accr ="60479190" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649002", Binid_accr ="59721336" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648972", Binid_accr ="60479240" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649092", Binid_accr ="60479280" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648795", Binid_accr ="60479181" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648738", Binid_accr ="60479162" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648696", Binid_accr ="59721234" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649110", Binid_accr ="60479286" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648993", Binid_accr ="60479247" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648735", Binid_accr ="59721247" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648903", Binid_accr ="59721303" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648735", Binid_accr ="60479161" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649020", Binid_accr ="59721342" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648434", Binid_accr ="60479061" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649047", Binid_accr ="60479265" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649092", Binid_accr ="59721366" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648795", Binid_accr ="59721267" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649074", Binid_accr ="59721360" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648903", Binid_accr ="60479217" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648578", Binid_accr ="59721195" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648650", Binid_accr ="60479133" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648741", Binid_accr ="59721249" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648990", Binid_accr ="60479246" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648395", Binid_accr ="59721134" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648786", Binid_accr ="60479178" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648990", Binid_accr ="59721332" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648647", Binid_accr ="59721218" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648443", Binid_accr ="59721150" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648879", Binid_accr ="59721295" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648413", Binid_accr ="59721140" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648939", Binid_accr ="60479229" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648632", Binid_accr ="60479127" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648657", Binid_accr ="60479135" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648885", Binid_accr ="59721297" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648759", Binid_accr ="60479169" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648699", Binid_accr ="60479149" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648861", Binid_accr ="59721289" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648759", Binid_accr ="59721255" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648632", Binid_accr ="59721213" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648867", Binid_accr ="60479205" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648678", Binid_accr ="59721228" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648879", Binid_accr ="60479209" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648425", Binid_accr ="60479058" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648650", Binid_accr ="59721219" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648590", Binid_accr ="60479113" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648560", Binid_accr ="60479103" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648473", Binid_accr ="59721160" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648464", Binid_accr ="59721157" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648596", Binid_accr ="59721201" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648458", Binid_accr ="59721155" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648837", Binid_accr ="60479195" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648530", Binid_accr ="59721179" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648596", Binid_accr ="60479115" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648551", Binid_accr ="59721186" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648458", Binid_accr ="60479069" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648840", Binid_accr ="60479196" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648834", Binid_accr ="60479194" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648765", Binid_accr ="59721257" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649059", Binid_accr ="59721355" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648533", Binid_accr ="59721180" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648554", Binid_accr ="59721187" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648518", Binid_accr ="60479089" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648473", Binid_accr ="60479074" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648461", Binid_accr ="60479070" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648831", Binid_accr ="60479193" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648837", Binid_accr ="59721281" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648918", Binid_accr ="59721308" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648834", Binid_accr ="59721280" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648518", Binid_accr ="59721175" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648419", Binid_accr ="59721142" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648446", Binid_accr ="60479065" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648407", Binid_accr ="60479052" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648942", Binid_accr ="60479230" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648416", Binid_accr ="59721141" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648882", Binid_accr ="59721296" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648644", Binid_accr ="59721217" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648999", Binid_accr ="59721335" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648873", Binid_accr ="60479207" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648942", Binid_accr ="59721316" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648780", Binid_accr ="59721262" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648410", Binid_accr ="60479053" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648440", Binid_accr ="60479063" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648975", Binid_accr ="59721327" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648864", Binid_accr ="60479204" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648720", Binid_accr ="60479156" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648933", Binid_accr ="59721313" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648653", Binid_accr ="60479134" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648693", Binid_accr ="60479147" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649035", Binid_accr ="59721347" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648407", Binid_accr ="59721138" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648882", Binid_accr ="60479210" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648446", Binid_accr ="59721151" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648816", Binid_accr ="59721274" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648410", Binid_accr ="59721139" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648801", Binid_accr ="59721269" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649029", Binid_accr ="60479259" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648669", Binid_accr ="59721225" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648723", Binid_accr ="60479157" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649119", Binid_accr ="59721375" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648768", Binid_accr ="60479172" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648617", Binid_accr ="59721208" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648602", Binid_accr ="59721203" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648846", Binid_accr ="60479198" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648422", Binid_accr ="59721143" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648912", Binid_accr ="60479220" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648846", Binid_accr ="59721284" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648801", Binid_accr ="60479183" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649029", Binid_accr ="59721345" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648602", Binid_accr ="60479117" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649119", Binid_accr ="147649"   },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648804", Binid_accr ="60479184" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648669", Binid_accr ="60479139" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648969", Binid_accr ="59721325" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648617", Binid_accr ="60479122" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648723", Binid_accr ="59721243" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648807", Binid_accr ="60479185" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648804", Binid_accr ="59721270" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648912", Binid_accr ="59721306" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648783", Binid_accr ="59721263" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648954", Binid_accr ="60479234" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649071", Binid_accr ="60479273" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648449", Binid_accr ="60479066" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648948", Binid_accr ="59721318" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648891", Binid_accr ="60479213" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648849", Binid_accr ="60479199" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648509", Binid_accr ="60479086" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648539", Binid_accr ="59721182" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648774", Binid_accr ="60479174" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648717", Binid_accr ="59721241" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648855", Binid_accr ="59721287" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648852", Binid_accr ="59721286" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648666", Binid_accr ="60479138" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648479", Binid_accr ="60479076" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648888", Binid_accr ="60479212" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648711", Binid_accr ="60479153" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648663", Binid_accr ="59721223" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648663", Binid_accr ="60479137" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649113", Binid_accr ="60479287" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648849", Binid_accr ="59721285" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648957", Binid_accr ="60479235" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648608", Binid_accr ="60479119" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648714", Binid_accr ="59721240" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648963", Binid_accr ="59721323" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648608", Binid_accr ="59721205" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648876", Binid_accr ="60479208" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648681", Binid_accr ="60479143" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648428", Binid_accr ="60479059" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649020", Binid_accr ="60479256" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649074", Binid_accr ="60479274" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648404", Binid_accr ="60479051" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648629", Binid_accr ="60479126" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648545", Binid_accr ="60479098" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648987", Binid_accr ="60479245" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648428", Binid_accr ="59721145" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648927", Binid_accr ="60479225" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648696", Binid_accr ="60479148" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648545", Binid_accr ="59721184" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648623", Binid_accr ="59721210" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648629", Binid_accr ="59721212" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649032", Binid_accr ="59721346" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649110", Binid_accr ="59721372" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648936", Binid_accr ="59721314" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648738", Binid_accr ="59721248" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648927", Binid_accr ="59721311" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648404", Binid_accr ="59721137" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648822", Binid_accr ="59721276" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648936", Binid_accr ="60479228" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648993", Binid_accr ="59721333" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649080", Binid_accr ="59721362" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648900", Binid_accr ="60479216" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648494", Binid_accr ="60479081" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648506", Binid_accr ="60479085" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649101", Binid_accr ="59721369" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649041", Binid_accr ="60479263" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648467", Binid_accr ="59721158" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648981", Binid_accr ="59721329" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649041", Binid_accr ="59721349" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648482", Binid_accr ="59721163" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648660", Binid_accr ="59721222" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648536", Binid_accr ="60479095" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648455", Binid_accr ="60479068" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648491", Binid_accr ="59721166" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648503", Binid_accr ="59721170" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648485", Binid_accr ="60479078" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649098", Binid_accr ="60479282" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648455", Binid_accr ="59721154" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649017", Binid_accr ="59721341" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649014", Binid_accr ="59721340" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649038", Binid_accr ="60479262" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649038", Binid_accr ="59721348" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648527", Binid_accr ="59721178" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649107", Binid_accr ="60479285" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648924", Binid_accr ="59721310" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648494", Binid_accr ="59721167" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648684", Binid_accr ="60479144" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648572", Binid_accr ="60479107" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649005", Binid_accr ="60479251" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648732", Binid_accr ="59721246" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648930", Binid_accr ="59721312" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648452", Binid_accr ="60479067" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649008", Binid_accr ="60479252" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649044", Binid_accr ="60479264" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648690", Binid_accr ="60479146" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648401", Binid_accr ="59721136" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648401", Binid_accr ="60479050" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649008", Binid_accr ="59721338" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648930", Binid_accr ="60479226" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649086", Binid_accr ="59721364" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648587", Binid_accr ="60479112" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649077", Binid_accr ="59721361" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649050", Binid_accr ="60479266" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648684", Binid_accr ="59721230" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648638", Binid_accr ="60479129" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648966", Binid_accr ="59721324" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649005", Binid_accr ="59721337" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648777", Binid_accr ="60479175" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648819", Binid_accr ="60479189" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648777", Binid_accr ="59721261" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648828", Binid_accr ="59721278" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648939", Binid_accr ="59721315" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648861", Binid_accr ="60479203" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648885", Binid_accr ="60479211" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648705", Binid_accr ="59721237" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648395", Binid_accr ="60479048" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648810", Binid_accr ="59721272" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648641", Binid_accr ="60479130" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648945", Binid_accr ="59721317" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648741", Binid_accr ="60479163" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648810", Binid_accr ="60479186" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648786", Binid_accr ="59721264" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648858", Binid_accr ="59721288" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648657", Binid_accr ="59721221" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648647", Binid_accr ="60479132" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648726", Binid_accr ="59721244" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648443", Binid_accr ="60479064" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648699", Binid_accr ="59721235" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648578", Binid_accr ="60479109" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648945", Binid_accr ="60479231" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648425", Binid_accr ="59721144" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648687", Binid_accr ="59721231" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649065", Binid_accr ="60479271" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648867", Binid_accr ="59721291" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649065", Binid_accr ="59721357" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648548", Binid_accr ="60479099" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648590", Binid_accr ="59721199" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648521", Binid_accr ="59721176" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648831", Binid_accr ="59721279" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648488", Binid_accr ="59721165" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648563", Binid_accr ="59721190" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648581", Binid_accr ="60479110" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648765", Binid_accr ="60479171" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648551", Binid_accr ="60479100" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648813", Binid_accr ="59721273" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648521", Binid_accr ="60479090" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648554", Binid_accr ="60479101" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648762", Binid_accr ="60479170" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648563", Binid_accr ="60479104" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648464", Binid_accr ="60479071" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648813", Binid_accr ="60479187" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648840", Binid_accr ="59721282" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648593", Binid_accr ="59721200" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648470", Binid_accr ="60479073" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648530", Binid_accr ="60479093" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648461", Binid_accr ="59721156" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648584", Binid_accr ="60479111" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648524", Binid_accr ="59721177" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648918", Binid_accr ="60479222" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649059", Binid_accr ="60479269" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648560", Binid_accr ="59721189" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648702", Binid_accr ="60479150" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648909", Binid_accr ="60479219" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648644", Binid_accr ="60479131" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648437", Binid_accr ="60479062" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649095", Binid_accr ="60479281" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648416", Binid_accr ="60479055" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648933", Binid_accr ="60479227" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648702", Binid_accr ="59721236" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649035", Binid_accr ="60479261" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648744", Binid_accr ="59721250" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648909", Binid_accr ="59721305" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648780", Binid_accr ="60479176" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648744", Binid_accr ="60479164" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648999", Binid_accr ="60479249" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648693", Binid_accr ="59721233" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648975", Binid_accr ="60479241" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648635", Binid_accr ="60479128" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649062", Binid_accr ="59721356" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648720", Binid_accr ="59721242" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648816", Binid_accr ="60479188" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648635", Binid_accr ="59721214" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648440", Binid_accr ="59721149" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648864", Binid_accr ="59721290" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648653", Binid_accr ="59721220" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648873", Binid_accr ="59721293" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648960", Binid_accr ="60479236" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648972", Binid_accr ="59721326" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649023", Binid_accr ="59721343" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649002", Binid_accr ="60479250" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648398", Binid_accr ="60479049" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648876", Binid_accr ="59721294" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648960", Binid_accr ="59721322" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648681", Binid_accr ="59721229" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648798", Binid_accr ="60479182" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648398", Binid_accr ="59721135" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649083", Binid_accr ="60479277" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649047", Binid_accr ="59721351" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648434", Binid_accr ="59721147" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648789", Binid_accr ="60479179" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648575", Binid_accr ="59721194" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649080", Binid_accr ="60479276" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649032", Binid_accr ="60479260" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648575", Binid_accr ="60479108" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648762", Binid_accr ="59721256" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649011", Binid_accr ="60479253" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648581", Binid_accr ="59721196" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648533", Binid_accr ="60479094" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648584", Binid_accr ="59721197" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648524", Binid_accr ="60479091" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648599", Binid_accr ="60479116" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648470", Binid_accr ="59721159" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648599", Binid_accr ="59721202" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648593", Binid_accr ="60479114" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649011", Binid_accr ="59721339" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648488", Binid_accr ="60479079" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649104", Binid_accr ="60479284" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648566", Binid_accr ="59721191" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649014", Binid_accr ="60479254" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648756", Binid_accr ="59721254" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648515", Binid_accr ="59721174" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649056", Binid_accr ="59721354" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648978", Binid_accr ="60479242" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648491", Binid_accr ="60479080" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648503", Binid_accr ="60479084" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648978", Binid_accr ="59721328" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648557", Binid_accr ="60479102" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648756", Binid_accr ="60479168" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649017", Binid_accr ="60479255" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648485", Binid_accr ="59721164" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648536", Binid_accr ="59721181" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648506", Binid_accr ="59721171" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648620", Binid_accr ="60479123" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648753", Binid_accr ="60479167" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648825", Binid_accr ="59721277" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648572", Binid_accr ="59721193" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648753", Binid_accr ="59721253" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649050", Binid_accr ="59721352" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648431", Binid_accr ="59721146" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648984", Binid_accr ="59721330" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648996", Binid_accr ="59721334" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649086", Binid_accr ="60479278" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648611", Binid_accr ="59721206" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649026", Binid_accr ="60479258" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648569", Binid_accr ="59721192" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649077", Binid_accr ="60479275" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648996", Binid_accr ="60479248" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648611", Binid_accr ="60479120" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "638414", Binid_accr ="59719571" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649095", Binid_accr ="59721367" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648419", Binid_accr ="60479056" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649062", Binid_accr ="60479270" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649089", Binid_accr ="60479279" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649089", Binid_accr ="59721365" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648437", Binid_accr ="59721148" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649119", Binid_accr ="60479289" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649119", Binid_accr ="835224"   },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648915", Binid_accr ="60479221" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648807", Binid_accr ="59721271" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648747", Binid_accr ="59721251" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648969", Binid_accr ="60479239" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648605", Binid_accr ="59721204" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648915", Binid_accr ="59721307" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648768", Binid_accr ="59721258" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648605", Binid_accr ="60479118" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648783", Binid_accr ="60479177" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648747", Binid_accr ="60479165" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648422", Binid_accr ="60479057" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "638537", Binid_accr ="59719610" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648858", Binid_accr ="60479202" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648726", Binid_accr ="60479158" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648548", Binid_accr ="59721185" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648641", Binid_accr ="59721216" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648678", Binid_accr ="60479142" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648413", Binid_accr ="60479054" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648687", Binid_accr ="60479145" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648705", Binid_accr ="60479151" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "638684", Binid_accr ="59719654" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "638282", Binid_accr ="59719532" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648843", Binid_accr ="59721283" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648708", Binid_accr ="60479152" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648449", Binid_accr ="59721152" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648714", Binid_accr ="60479154" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649071", Binid_accr ="59721359" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648708", Binid_accr ="59721238" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648951", Binid_accr ="59721319" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648672", Binid_accr ="60479140" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648672", Binid_accr ="59721226" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649068", Binid_accr ="59721358" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648891", Binid_accr ="59721299" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648894", Binid_accr ="59721300" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648729", Binid_accr ="59721245" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "649116", Binid_accr ="60479288" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648957", Binid_accr ="59721321" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648666", Binid_accr ="59721224" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648897", Binid_accr ="60479215" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648750", Binid_accr ="59721252" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648897", Binid_accr ="59721301" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648539", Binid_accr ="60479096" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648771", Binid_accr ="60479173" },
            new СозданиеКорректировокНаСписокНачислений.AccrCorrectirFix() { Binid_dog = "648894", Binid_accr ="60479214" }
           
        
        );
}
if (app.Environment.IsDevelopment() // Удаление 0 геозон
//&& true
&& false
)
{
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();
    BinManGeozoneParser.ForeachOnMainSearch(ld, (gp) =>
    {

        if (gp.name.Substring(0, 2) == "0 ")
        {
            if (!SQL.CheckGeoExists(gp.binId))
                return BinManGeozoneParser.SearchCommand.DoAction;
        }
        return BinManGeozoneParser.SearchCommand.DoNothing;

    }, (gp, conts) =>
    {
        foreach (var v in conts)
        {
            BinManContainers.SendDeleteRequest(ld, v.guid);
        }
        
        BinManGeozone.SendSetArchiveRequest(ld, gp.binId, true);
        BinManGeozone.SendDeleteRequest(ld, gp.binId);
        return BinManGeozoneParser.SearchCommand.DoNothing;
    });
}
if (app.Environment.IsDevelopment() // Создание начислений
// && true
 && false
)
{
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();
    var arr = new string[]
    {
         "5964582"
, "5964584"
, "5964585"
, "5964589"
, "5964590"
, "5964597"
, "5964598"
, "5964600"
, "5964601"
, "5964604"
, "5964605"
, "5964606"
, "5964608"
, "5964609"
, "5964613"
, "5964615"
, "5964618"
, "5964619"
, "5964622"
, "5964623"
, "5964625"
, "5964627"
, "5964630"
, "5964631"
, "5964632"
, "5964634"
, "5964637"
, "5964640"
, "5964642"
, "5964648"
, "5964650"
, "5964651"
, "5964652"
, "5964654"
, "5964655"
, "5964656"
, "5964657"
, "5964658"
, "5964659"
, "5964661"
, "5964662"
, "5964666"
, "5964668"
, "5964669"
, "5964671"
, "5964672"
, "5964675"
, "5964676"
, "5964677"
, "5964678"
, "5964679"
, "5964681"
, "5964683"
, "5964684"
, "5964686"
, "5964687"
, "5964692"
, "5964693"
, "5964694"
, "5964695"
, "5964697"
, "5964699"
, "5964703"
, "5964704"
, "5964707"
, "5964709"
, "5964713"
, "5964714"
, "5964720"
, "5964721"
, "5964724"
, "5964726"
, "5964730"
, "5964732"
, "5964733"
, "5964736"
, "5964738"
, "5964739"
, "5964744"
, "5964745"
, "5964746"
, "5964747"
, "5964750"
, "5964755"
, "5964757"
, "5964761"
, "5964762"
, "5964763"
, "5964769"
, "5964771"
    };
  
    //УдалениеВсехНачисленийПоСпискуДоговоров.УдалитьВсеНе0Начисления(ld,arr);
    УдалениеТарифаОбъектаПоСпискуДоговоров.УдалитьВсеОбъекты(ld, arr);

}

if (app.Environment.IsDevelopment() // Создание начислений
//&& true
 && false
)
{ 
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();
    СозданиеНачисленийПоСпискуДоговоровНаДату.Run(ld, new DateTime(2024, 07, 01), new DateTime(2024, 07, 31)
,
    "638282"
, "638414"
, "648395"
, "648398"
, "648401"
, "648404"
, "648407"
, "648410"
, "648413"
, "648416"
, "648419"
, "648422"
, "648425"
, "648428"
, "648431"
, "648434"
, "648437"
, "648440"
, "648443"
, "648446"
, "648449"
, "648452"
, "648455"
, "648458"
, "648461"
, "648464"
, "648467"
, "648470"
, "648473"
, "648476"
, "648479"
, "648482"
, "648485"
, "648488"
, "648491"
, "648494"
, "648497"
, "648500"
, "648503"
, "648506"
, "648509"
, "648512"
, "648515"
, "648518"
, "648521"
, "648524"
, "648527"
, "648530"
, "648533"
, "648536"
, "648539"
, "648542"
, "648545"
, "648548"
, "648551"
, "648554"
, "648557"
, "648560"
, "648563"
, "648566"
, "648569"
, "648572"
, "648575"
, "648578"
, "648581"
, "648584"
, "648587"
, "648590"
, "648593"
, "648596"
, "648599"
, "648602"
, "648605"
, "648608"
, "648611"
, "648614"
, "648617"
, "648620"
, "648623"
, "648626"
, "648629"
, "648632"
, "648635"
, "648638"
, "648641"
, "648644"
, "648647"
, "648650"
, "648653"
, "648657"
, "648660"
, "648663"
, "648666"
, "648669"
, "648672"
, "648675"
, "648678"
, "648681"
, "648684"
, "648687"
, "648690"
, "648693"
, "648696"
, "648699"
, "648702"
, "648705"
, "648708"
, "648711"
, "648714"
, "648717"
, "648720"
, "648723"
, "648726"
, "648729"
, "648732"
, "648735"
, "648738"
, "648741"
, "648744"
, "648747"
, "648750"
, "648753"
, "648756"
, "648759"
, "648762"
, "648765"
, "648768"
, "648771"
, "648774"
, "648777"
, "648780"
, "648783"
, "648786"
, "648789"
, "648792"
, "648795"
, "648798"
, "648801"
, "648804"
, "648807"
, "648810"
, "648813"
, "648816"
, "648819"
, "648822"
, "648825"
, "648828"
, "648831"
, "648834"
, "648837"
, "648840"
, "648843"
, "648846"
, "648849"
, "648855"
, "648858"
, "648861"
, "648864"
, "648867"
, "648870"
, "648873"
, "648876"
, "648879"
, "648882"
, "648885"
, "648888"
, "648891"
, "648894"
, "648897"
, "648900"
, "648903"
, "648906"
, "648909"
, "648912"
, "648915"
, "648918"
, "648921"
, "648927"
, "648930"
, "648933"
, "648936"
, "648939"
, "648942"
, "648945"
, "648948"
, "648951"
, "648954"
, "648957"
, "648960"
, "648963"
, "648966"
, "648969"
, "648972"
, "648975"
, "648978"
, "648981"
, "648984"
, "648987"
, "648990"
, "648993"
, "648996"
, "648999"
, "649002"
, "649005"
, "649008"
, "649011"
, "649014"
, "649017"
, "649020"
, "649023"
, "649026"
, "649029"
, "649032"
, "649035"
, "649038"
, "649041"
, "649044"
, "649047"
, "649050"
, "649053"
, "649056"
, "649059"
, "649062"
, "649065"
, "649068"
, "649071"
, "649074"
, "649077"
, "649080"
, "649083"
, "649086"
, "649089"
, "649092"
, "649095"
, "649098"
, "649101"
, "649104"
, "649107"
, "649110"
, "649113"
, "649116"
, "649119"
, "654858"
);
}
if (app.Environment.IsDevelopment()
//&& true
 && false
)
{
    await BinManApi.LogInAccounts();
    var ld = BinManApi.GetNextAccount();
    УдалениеВсехНачисленийПоСпискуДоговоров.УдалитьВсеНе0Начисления(ld,
  "638282"
, "638414"
, "648395"
, "648398"
, "648401"
, "648404"
, "648407"
, "648410"
, "648413"
, "648416"
, "648419"
, "648422"
, "648425"
, "648428"
, "648431"
, "648434"
, "648437"
, "648440"
, "648443"
, "648446"
, "648449"
, "648452"
, "648455"
, "648458"
, "648461"
, "648464"
, "648467"
, "648470"
, "648473"
, "648476"
, "648479"
, "648482"
, "648485"
, "648488"
, "648491"
, "648494"
, "648497"
, "648500"
, "648503"
, "648506"
, "648509"
, "648512"
, "648515"
, "648518"
, "648521"
, "648524"
, "648527"
, "648530"
, "648533"
, "648536"
, "648539"
, "648542"
, "648545"
, "648548"
, "648551"
, "648554"
, "648557"
, "648560"
, "648563"
, "648566"
, "648569"
, "648572"
, "648575"
, "648578"
, "648581"
, "648584"
, "648587"
, "648590"
, "648593"
, "648596"
, "648599"
, "648602"
, "648605"
, "648608"
, "648611"
, "648614"
, "648617"
, "648620"
, "648623"
, "648626"
, "648629"
, "648632"
, "648635"
, "648638"
, "648641"
, "648644"
, "648647"
, "648650"
, "648653"
, "648657"
, "648660"
, "648663"
, "648666"
, "648669"
, "648672"
, "648675"
, "648678"
, "648681"
, "648684"
, "648687"
, "648690"
, "648693"
, "648696"
, "648699"
, "648702"
, "648705"
, "648708"
, "648711"
, "648714"
, "648717"
, "648720"
, "648723"
, "648726"
, "648729"
, "648732"
, "648735"
, "648738"
, "648741"
, "648744"
, "648747"
, "648750"
, "648753"
//"648756"
//, "648759"
//, "648762"
//, "648765"
//, "648768"
//, "648771"
//, "648774"
//, "648777"
//, "648780"
//, "648783"
//, "648786"
//, "648789"
//, "648792"
//, "648795"
//, "648798"
//, "648801"
//, "648804"
//, "648807"
//, "648810"
//, "648813"
//, "648816"
//, "648819"
//, "648822"
//, "648825"
//, "648828"
//, "648831"
//, "648834"
//, "648837"
//, "648840"
//, "648843"
//, "648846"
//, "648849"
//, "648855"
//, "648858"
//, "648861"
//, "648864"
//, "648867"
//, "648870"
//, "648873"
//, "648876"
//, "648879"
//, "648882"
//, "648885"
//, "648888"
//, "648891"
//, "648894"
//, "648897"
//, "648900"
//, "648903"
//, "648906"
//, "648909"
//, "648912"
//, "648915"
//, "648918"
//, "648921"
//, "648927"
//, "648930"
//, "648933"
//, "648936"
//, "648939"
//, "648942"
//, "648945"
//, "648948"
//, "648951"
//, "648954"
//, "648957"
//, "648960"
//, "648963"
//, "648966"
//, "648969"
//, "648972"
//, "648975"
//, "648978"
//, "648981"
//, "648984"
//, "648987"
//, "648990"
//, "648993"
//, "648996"
//, "648999"
//, "649002"
//, "649005"
//, "649008"
//, "649011"
//, "649014"
//, "649017"
//, "649020"
//, "649023"
//, "649026"
//, "649029"
//, "649032"
//, "649035"
//, "649038"
//, "649041"
//, "649044"
//, "649047"
//, "649050"
//, "649053"
//, "649056"
//, "649059"
//, "649062"
//, "649065"
//, "649068"
//, "649071"
//, "649074"
//, "649077"
//, "649080"
//, "649083"
//, "649086"
//, "649089"
//, "649092"
//, "649095"
//, "649098"
//, "649101"
//, "649104"
//, "649107"
//, "649110"
//, "649113"
//, "649116"
//, "649119"
//, "654858"


        );
}

    if (app.Environment.IsDevelopment()
    && false
    ) //Выгрузка договоров для остановки
{
    await BinManApi.LogInAccounts();
    //var Books = new List<string>()
    //{
    //    "C:\\Users\\a.m.maltsev\\Downloads\\Заявка 16446 (1).xlsx",
    //    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Зарубино с 13.06.2024).xlsx",
    //    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Шишино с 18.06.2024).xlsx",
    //    "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Центральный с 15.06.2024).xlsx"
    //};//19.06.2024
    var Books = new List<string>()
    {
         "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Раздолье с 20.06.2024).xlsx",
         "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Глубокое с 21.06.2024).xlsx",
         "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Малый Корчуган с 19.06.2024).xlsx",
         "C:\\Users\\a.m.maltsev\\Downloads\\Переоткрытие ЛС (Верх-Падунский с 19.06.2024).xlsx"
    };//22.06.2024
    foreach (var BookPath in Books)
    {


        //var BookPath = "C:\\Users\\a.m.maltsev\\Downloads\\Список договоров на приостановку 2024-01-23.xlsx";
        //var BookPath = "C:\\Users\\a.m.maltsev\\Downloads\\Массовое закрытие ЛС (ред.).xlsx";
        using var book = new XLWorkbook(BookPath);
        var es = book.Worksheets.First();
        var c = es.Column(1);
        var cc = c.LastCellUsed();
        int l = 0;
        var SkipFirstRow = true;
        try
        {
            l = cc.Address.RowNumber;
        }

        catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
        // l = Math.Min(l, 3);
        var DateStopFrom = new DateTime(2024, 04, 04);
        var DateContinueFrom = new DateTime(2028, 1, 1);
        for (int i = (SkipFirstRow ? 2 : 1); i <= l; i++)
        {
            string id1 = es.Cell(i, 1).Value.ToString().Trim();
            var DogId = id1.NDoc2BinId();

            var did = (string.IsNullOrEmpty(DogId) ? id1 : DogId);
            var lcd = BinManApi.GetNextAccount();


            if (BinManDocumentParser.TryParseObjects(lcd, did, out var resobj))
            {


                var StopList = resobj;
                foreach (var Stop in StopList)
                {
                    if (BinManDocuments.SendStopObjectRequest(lcd, new BinManDocuments.StopDogObject()
                    {
                        dog_BinId = did,
                        object_BinId = Stop.binid,
                        DateFrom = new DateTime(2019,07,01),
                        comment  = "Не обслуживался"
                    }))
                    { }
                }

                //  if (true) continue;
            }
        }
    }
    
}

if (false)
{
    int antiSpam1 = 3;
    int antiSpam2 = 3;
    try
    {
        var recs = SQL.GetBinManClientsTaskList();
        if (recs.Count > 0) { Log.System("Updating Kas"); antiSpam2 = 3; }
        else { if (antiSpam2 > 0) { Log.System("no Kas to update"); antiSpam2--; } return; }

        foreach (var v in recs)
        {
            var ld = BinManApi.GetNextAccount();
            if (string.IsNullOrEmpty(v.ID) || v.ID.Length < 2)
            {
                if (BinManKa.SendCreateRequest(ld, v, out var BinId))
                {
                    SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.OK);
                }
                else
                {
                    SQL.KaIgnoreList.TryAdd(v.KA_DbGuid, "BinMan failed");
                    SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.Failed);
                }

            }
            else
            {
                if (BinManKa.SendEditRequest(ld, v))
                {
                    SQL.BinManMarkClientSucces(v.KA_DbGuid, string.Empty, SQL.BinManOperationStatusString.OK);
                }
                else
                {
                    SQL.KaIgnoreList.TryAdd(v.KA_DbGuid, "BinMan failed");
                    SQL.BinManMarkClientSucces(v.KA_DbGuid, string.Empty, SQL.BinManOperationStatusString.Failed);
                }
            }
        }
        Thread.Sleep(7000);
    }
    catch (Exception ex)
    {
        Log.Error("KA BinManUpdate Cycle", ex);
    }
}

if (false)
{


    //DataTable dtContacts = new DataTable();
    //dtContacts.Columns.Add(new DataColumn("id_parent"));
    //dtContacts.Columns.Add(new DataColumn("phone"));
    //dtContacts.Columns.Add(new DataColumn("site"));
    //dtContacts.Columns.Add(new DataColumn("mail"));

//ClassWriter.GenerateTSQlFromDataTable("Temp_EPBS_RF", dt);
// ClassWriter.GenerateTSQlFromDataTable("Temp_EPBS_RF_Contacts", dtContacts);

    using (HttpClient hc = new HttpClient())
    {
       var count= ElBudgetData.GetRecordCount();
        long offest = 0;
        while (count > 0)
        {
            //dt.Clear();
            // 1000;
            var res = hc.GetAsync($"https://www.budget.gov.ru/epbs/registry/ubpandnubp/data?pageSize=1000&offset={offest}");
            var txt = res.GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();

            ElBudgetData r= JsonConvert.DeserializeObject<ElBudgetData>(txt);
            DataTable dtContacts = new DataTable();
            DataTable dt = new DataTable();
            dtContacts.Columns.Add(new DataColumn("id_parent"));
            dtContacts.Columns.Add(new DataColumn("phone"));
            dtContacts.Columns.Add(new DataColumn("site"));
            dtContacts.Columns.Add(new DataColumn("mail"));

            dt.Columns.Add(new DataColumn("id"));
            dt.Columns.Add(new DataColumn("inn"));
            dt.Columns.Add(new DataColumn("kpp"));
            dt.Columns.Add(new DataColumn("SpecEventCode"));
            dt.Columns.Add(new DataColumn("regionCode"));
            dt.Columns.Add(new DataColumn("areaCode"));
            dt.Columns.Add(new DataColumn("areaType"));
            dt.Columns.Add(new DataColumn("areaName"));
            dt.Columns.Add(new DataColumn("cityCode"));
            dt.Columns.Add(new DataColumn("cityType"));
            dt.Columns.Add(new DataColumn("cityName"));
            dt.Columns.Add(new DataColumn("localCode"));
            dt.Columns.Add(new DataColumn("localType"));
            dt.Columns.Add(new DataColumn("localName"));
            dt.Columns.Add(new DataColumn("streetCode"));
            dt.Columns.Add(new DataColumn("streetType"));
            dt.Columns.Add(new DataColumn("streetName"));
            dt.Columns.Add(new DataColumn("house"));
            dt.Columns.Add(new DataColumn("building"));
            dt.Columns.Add(new DataColumn("apartment"));
            dt.Columns.Add(new DataColumn("kbkCode"));
            dt.Columns.Add(new DataColumn("kbkName"));
            dt.Columns.Add(new DataColumn("orgTypeCode"));
            dt.Columns.Add(new DataColumn("orgTypeName"));
            dt.Columns.Add(new DataColumn("establishmentKindCode"));
            dt.Columns.Add(new DataColumn("establishmentKindName"));
            dt.Columns.Add(new DataColumn("founderPlaceCode"));
            dt.Columns.Add(new DataColumn("budgetLvlName"));
            dt.Columns.Add(new DataColumn("budgetLvlCode"));
            dt.Columns.Add(new DataColumn("budgetName"));
            dt.Columns.Add(new DataColumn("budgetCode"));
            dt.Columns.Add(new DataColumn("statusCode"));
            dt.Columns.Add(new DataColumn("statusName"));
            dt.Columns.Add(new DataColumn("orgStatus"));
            dt.Columns.Add(new DataColumn("parentName"));
            dt.Columns.Add(new DataColumn("firmName"));
            foreach (var d in r.data)
            {
                var guid = Guid.NewGuid();
                // d.contacts
                var t1= Task.Run(() =>
                {
                   

                    foreach (var cc in d.contacts)
                    {
                        dtContacts.Rows.Add(new object[]
                        {
                        guid.ToString(),
                        cc.phone,
                        cc.site,
                        cc.mail
                        });
                    }
                    
                });
                var tt= Task.Run(() =>
                {
                   
                   
                    dt.Rows.Add(new object[] {
                guid.ToString(),
                d.info.inn,
                d.info.kpp,
                d.info.specEventCode,
                d.info.regionCode ,
                d.info.areaCode,
                d.info.areaType,
                d.info.areaName,
                d.info.cityCode,
                d.info.cityType,
                d.info.cityName,
                d.info.localCode,
                d.info.localType,
                d.info.localName,
                d.info.streetCode,
                d.info.streetType,
                d.info.streetName,
                d.info.house,
                d.info.building,
                d.info.apartment,
                d.info.kbkCode,
                d.info.kbkName,
                d.info.orgTypeCode,
                d.info.orgTypeName,
                d.info.establishmentKindCode,
                d.info.establishmentKindName,
                d.info.founderPlaceCode,
                d.info.budgetLvlName,
                d.info.budgetLvlCode,
                d.info.budgetName,
                d.info.budgetCode,
                d.info.statusCode,
                d.info.statusName,
                d.info.orgStatus,
                d.info.parentName,
                d.info.firmName,

            });
                   
                });
                Task.WaitAll(t1, tt);
            }
            var ttt = Task.Run(() => { InsertTableByProcedure("TableLoad_Temp_EPBS_RF_Contacts", dtContacts); });
            var tttt = Task.Run(() => {InsertTableByProcedure("TableLoad_Temp_EPBS_RF", dt);});


            Task.WhenAll(ttt, tttt);
            offest += r.data.Count;
            count -= r.data.Count;
        }
      

    }
}


if (app.Environment.IsDevelopment() // 2- я часть загрузки в binman
    && true
    )  //JUST TEST PURPOSE  // BUT DO NOT DELETE !!! IMPORTANT CODE :)
{
    await BinManApi.LogInAccounts();
    //ПолноеУничтожениеКлиентов.УничтожитьВсех(false); ;
  //  return;
    // SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.02.2024 - загрузка 2024-01-15 (часть 2).xlsx");
    //SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 20.06.2023 - загрузка 2024-08-23.xlsx");
    //SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.11.2022 по 31.05.2024 - загрузка 2024-08-23.xlsx");
   // SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.10.2024 - загрузка 2024-09-25.xlsx");//Поменять как аня напишет... 25.09.2024
   // SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.12.2024 - загрузка 2024-11-11.xlsx");//20.11.2024
    //SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.12.2024 - загрузка 2024-11-27.xlsx",new DateTime(2024,11,27,16,41,10));//20.11.2024
    //SQL.LoadDataFromBinmanFormatExcel("C:\\Users\\a.m.maltsev\\Downloads\\База ФЛ с 01.01.2025 - загрузка 2024-12-27.xlsx", new DateTime(2024,12,27,14,14,12));//27.12.2024
   // SQL.LoadDataFromBinmanFormatExcel("D:\\Downloads\\База ФЛ с 01.02.2025 - загрузка 2025-01-29.xlsx", new DateTime(2025,01,29,13,20,22));//29.01.2025

    //КлиентыПотеряшкиСозданныеНоССтатусомFailed.Load();//06.08.2024;
    #region KA_BINMAN_TEST 
    ////WARNING УЖЕ ОБНОВЛЯЕТСЯ АВТОМАТИЧЕСКИ В Bin Update   ! 
    //var recs = SQL.GetBinManClientsTaskList();

    //foreach (var v in recs)
    //{
    //    var ld = BinManApi.GetNextAccount();
    //    if (string.IsNullOrEmpty(v.ID) || v.ID.Length < 2)
    //    {
    //        if (BinManKa.SendCreateRequest(ld, v, out var BinId))
    //        {
    //            SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.OK);
    //        }
    //        else
    //        {
    //            SQL.KaIgnoreList.TryAdd(v.KA_DbGuid, "BinMan failed");
    //            SQL.BinManMarkClientSucces(v.KA_DbGuid, BinId, SQL.BinManOperationStatusString.Failed);
    //        }

    //    }
    //    else
    //    {
    //        if (BinManKa.SendEditRequest(ld, v))
    //        {
    //            SQL.BinManMarkClientSucces(v.KA_DbGuid, string.Empty, SQL.BinManOperationStatusString.OK);
    //        }
    //        else
    //        {
    //            SQL.KaIgnoreList.TryAdd(v.KA_DbGuid, "BinMan failed");
    //            SQL.BinManMarkClientSucces(v.KA_DbGuid, string.Empty, SQL.BinManOperationStatusString.Failed);
    //        }
    //    }
    //}
    #endregion
    // Thread.Sleep(1000 * 60 * 60 * 1);//1 час;
    int retrys = 4555;
    while (retrys>0)
    {
        if (true) // ` if (false)` Добавлено по причине Удаление ошибочно созданных ссылок договор- оъбъект с неверным тарифом 03.05.2024 - Ошибка (07.05.2024 - Комментарий)
        {
            #region OBJECT_BINMAN_TEST
            var ObjectUpdateList = SQL.GetObjectListToSyncBinMan();
            if (ObjectUpdateList.Count > 0)
            {
                //else { return; }
                //  var tasks = new List<Task>();
                foreach (var v in ObjectUpdateList/*.Where(x=>x.BinId == "5914214")*/)
                {
                    //  tasks.Add(Task.УдалитьВсеНе0Начисления(() =>
                    // {
                    try
                    {
                        // if (!DadataApi.TryFillAddres(v)) continue;
                        LoginData ld = v.ld;
                        if (ld == null)
                        {
                            ld = BinManApi.GetNextAccount();
                        }
                        if (string.IsNullOrEmpty(v.BinId) || v.BinId == "0" || v.BinId.Contains("-"))
                        {
                            try
                            {
                                if (BinManObject.SendCreateRequest(ld, v, out long binId))
                                {
                                    SQL.UpdateObjectBinId(v.DataBase_Guid, binId);
                                    Log.Message($"[SYNC O I] Created object {v.DataBase_Guid} ({v.NAME}) BIN ID: {binId}");
                                }
                                else
                                {
                                    Log.Error($"{v.DataBase_Guid}({v.NAME}) failed to create in binman");
                                    SQL.IgnoredObjectsId.TryAdd(v.DataBase_Guid, $"{v.DataBase_Guid}({v.NAME}) failed to create in binman");
                                }
                            }catch (Exception ex)
                            {
                                SQL.IgnoredObjectsId.TryAdd(v.DataBase_Guid, $"{v.DataBase_Guid}({v.NAME}) EXC to create in binman");
                            }
                        }
                        else
                        {
                            if (BinManObject.SendEditRequest(ld, v))
                            {
                                SQL.SetObjectStatus(v.DataBase_Guid, BinManSyncStatus.ok);
                                Log.Message($"[SYNC O U] Updated object {v.DataBase_Guid} ({v.NAME}) BIN ID: {v.BinId}");
                            }
                            else
                            {
                                SQL.IgnoredObjectsId.TryAdd(v.DataBase_Guid, $"{v.DataBase_Guid}({v.NAME}) failed to update in binman");
                            }
                        }
                    }
                    catch (Exception ex) { Log.Error("Error while updating Objects Inner"); Log.Error(ex); }
                }
            }
            #endregion //5914260 // 5914212

            #region DOG_BINMAN_TEST
            var recs3 = SQL.GetDogListToSincBinMan();
            foreach (var v in recs3)
            {
              
                if (string.IsNullOrEmpty(v.bin_id) || v.bin_id == "0" || v.bin_id.Contains("-"))
                {
                    LoginData ld = BinManApi.GetNextAccount();
                    if (BinManDocuments.SendCreateRequest(ld, v, out var bin_id))
                    {
                        SQL.MarkDogUpdated(v.Db_Guid, BinManOperationStatusString.OK, bin_id, v.Number);
                    }
                    else
                    {
                        SQL.DogIgnoreList.TryAdd(v.Db_Guid, "BinMan Fail");
                        SQL.MarkDogUpdated(v.Db_Guid, BinManOperationStatusString.Failed, string.Empty, string.Empty);

                    }

                }
            }
            #endregion
        }
        #region DOG_OBJ_BINMAN_TEST
        var recs4 = SQL.GetDogObjLinksListToSincBinMan();
        foreach (var v in recs4)
        {
            LoginData ld = BinManApi.GetNextAccount();

            if (BinManDocuments.SendAttachObjectRequest(ld, v))
            {
                SQL.MarkDogObjUpdated(v.Db_Guid, BinManOperationStatusString.OK);
            }
            else
            {
                SQL.IdDogObjLinksIgnoreList.TryAdd(v.Db_Guid, "BinMan Fail");
                SQL.MarkDogObjUpdated(v.Db_Guid, BinManOperationStatusString.Failed);
            }
        }
        #endregion
        Thread.Sleep(1000 * 60 * 60 * 1);
        retrys--;
    }

}

if (false) //Выгрузка договоров для остановки
{
    await BinManApi.LogInAccounts();
    //var BookPath = "C:\\Users\\a.m.maltsev\\Downloads\\Список договоров на приостановку 2024-01-23.xlsx";
    var BookPath = "C:\\Users\\a.m.maltsev\\Downloads\\Массовое закрытие ЛС (ред.).xlsx";
    var FileName = Path.GetFileNameWithoutExtension(BookPath);
    var Savepath = BookPath.Replace(FileName, FileName + " "+DateTime.Now.ToString("HH-MM"));
    using var book = new XLWorkbook(BookPath);
    var es = book.Worksheets.First();
    var c = es.Column(1);
    var cc = c.LastCellUsed();
    int l = 0;
    var SkipFirstRow = true;
    try
    {
        l = cc.Address.RowNumber;
    }
  
    catch (Exception ex) { l = es.LastRowUsed().RowNumber(); }
   // l = Math.Min(l, 3);
    var DateStopFrom = new DateTime(2025, 01, 01);
    var DateContinueFrom = new DateTime(2026, 1, 1);
    for (int i = (SkipFirstRow ? 2 : 1) ; i <= l; i++)
    {
        string id1 = es.Cell(i, 1).Value.ToString().Trim();
        
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

        if (BinManDocumentParser.TryParseObjects(lcd, did,  out var resobj))
        {

            //if (float.TryParse(resobj.tarif_volume, out var tt)) {
            //     NotNullTarif = tt;
            //}

            foreach (var v in resobj)
            {
                var oId = v.binid;
                var InBetween = v.changes.Where(x =>
                    x.DT_PeriodFrom != DateTime.MinValue && x.DT_PeriodFrom > DateStopFrom && x.DT_PeriodTo < DateContinueFrom && x.DT_PeriodFrom < DateContinueFrom
                    ) ;

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

                if (true) continue;

                var NotNullTarif = 0f;
                var tt = 0f;
                //var did = "637304";
               
                if (NotNullTarif <= 0)
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

                if (NotNullTarif > 0)
                {
                    if (BinManDocuments.SendStopObjectRequest(lcd, new BinManDocuments.StopDogObject()
                    {
                        dog_BinId = did,
                        object_BinId = oId,
                        DateFrom = DateStopFrom
                    }))
                    {

                        if( BinManDocuments.SendAttachObjectRequest(lcd, new BinManDocuments.AttachObjectInfo()
                        {
                            doc_BinManId = did,
                            activeFrom = DateContinueFrom,
                            obj_BinManId = oId,
                            tarif_BinManCode = "255",
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
                    void   LogError(){
                        es.Cell(i, 2).Value += $"Не получилось o: {oId};";
                    }
                }
            }
        }
        
    }
    book.SaveAs(Savepath);
}

if (false) //Выгрузка договоров для остановки
{
    await BinManApi.LogInAccounts();
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

if (false)
{
    await BinManApi.LogInAccounts();

    var DateStopFrom = new DateTime(2024, 1, 1);
    var DateContinueFrom = new DateTime(2025, 1, 1);

    
        string id1 = "430710";

        // string id2 = es.Cell(i, 2).Value.ToString().Trim();

        var did = id1;
        //   var oId = "637303";
        var lcd = BinManApi.GetNextAccount();


        if (BinManDocumentParser.TryParseObjects(lcd, did, out var resobj))
        {

            //if (float.TryParse(resobj.tarif_volume, out var tt)) {
            //     NotNullTarif = tt;
            //}

            foreach (var v in resobj)
            {
                var oId = v.binid;
            var InBetween= v.changes.Where(x =>
                x.DT_PeriodFrom != DateTime.MinValue && x.DT_PeriodFrom >= DateStopFrom && x.DT_PeriodTo < DateContinueFrom 
                );

            var StopList = InBetween.Where(x =>  x.Status == BinManDocumentParser.DocObjectChangeStatus.Active );

            var AddTarifList = InBetween;
            var EndTarif = v.changes.FirstOrDefault(x => x.DT_PeriodFrom != DateTime.MinValue && x.DT_PeriodFrom >= DateContinueFrom, DocObjectChange.Empty);

            if (false)
            { // Вставка тарифа по логике cleanIt
                if (SQL.TryGetTarifCodeByName(EndTarif.tarif_full_text, out var binCode))
                {
                    foreach (var tr in AddTarifList)
                    {
                        if (BinManDocuments.SendAttachObjectRequest(lcd, new BinManDocuments.AttachObjectInfo()
                        {
                            doc_BinManId = did,
                            activeFrom = tr.DT_PeriodFrom,
                            obj_BinManId = oId,
                            tarif_BinManCode = "255", // TARIF !
                            tarif_value = 12.ToString()//ACTUAL VALUE
                        })) ;
                    }
                    if (!EndTarif.IsNullOrEmpty)
                    {

                        if (BinManDocuments.SendAttachObjectRequest(lcd, new BinManDocuments.AttachObjectInfo()
                        {
                            doc_BinManId = did,
                            activeFrom = DateContinueFrom,
                            obj_BinManId = oId,
                            tarif_BinManCode = binCode, // TARIF !
                            tarif_value = 12.ToString(),//ACTUAL VALUE

                        })) ;

                    }
                }
            }

                foreach (var Stop in StopList)
                {
                    if (BinManDocuments.SendStopObjectRequest(lcd, new BinManDocuments.StopDogObject()
                    {
                        dog_BinId = did,
                        object_BinId = oId,
                        DateFrom = Stop.DT_PeriodFrom
                    })) { };
                }

                if (true) continue;

               
            }
        }
       // book.SaveAs(Savepath);
    
}




//BinManDocuments.SendAttachFile(BinManApi.GetNextAccount(), new BinManDocuments.AttachFileRequestData()
//{
//    doc_binId = "5912233",
//    file = File.ReadAllBytes("C:\\Users\\a.m.maltsev\\Downloads\\20MB.zip"),
//    fileName = "File20mb.txt"
//}, out var rese);
//SQL.ProceedAccruals();
//var copy = rese;
//if ( DadataApi.TryFindAddressByAddress("Мирная 9", out var res)) {
//    BinManKa.SendCreateRequest(BinManApi.GetNextAccount(), new ClientData()
//    {
//        F_NAME = "Пупкин",
//        F_SURNAME = "Вася",
//        TYPE = ClientType.INDIVIDUAL,
//        address = res
//    }
//        , out string BinId);// ka Minimal request
//}
//else
//{
//    Log.Error("DADATA not found");
//}
//BinManKa.SendCreateRequest(BinManApi.GetNextAccount(), new ClientData()
//{
//    UR_NAME = "ИП Вася пупкин",
//    INN="1029517281",
//    TYPE = ClientType.U
//}
//    , out string BinId);//Ur minimal request

//BinManDocuments.SendEditRequest(BinManApi.GetNextAccount(), new BinManDocuments.DocInfo()
//{
//    bin_id = "5906259",
//    Type_BinManCode = "163",
//    Number = "100586893",
//    Organization_BinManId = "112790",
//    Client_BinManid = "5779536",
//    Group_BinManCode = "14",
//    dateFrom = new DateTime(2023, 11, 30),
//    dateTo = new DateTime(2023, 12, 01),
//    dateSign = new DateTime(2023, 11, 01)
//});

//BinManDocAccruals.AddAccrualToDoc(BinManApi.GetNextAccount(), new BinManAccrual()
//{
//    doc_BinId = "5906259",
//    type = AccrualsType.accr_any_summ,
//    dateFrom = new DateTime(2023, 12, 01),
//    dateTo = new DateTime(2023, 12, 02),
//    summ = 12.ToString(),
//    comment = "Test"
//});
//BinManDocAccruals.CreateCorrectir(BinManApi.GetNextAccount(), new BinManAccrualCorrect()
//{
//    doc_BinId = "172111",
//    type = AccrualsType.accr_by_doc,
//    date = DateTime.Now,
//    correctSumm = (-0.01).ToString()
//    ,parentBinId = "54962999"
//    ,
//    FinalSumm="75.45"
//    ,Comment="TEST"
//});
//BinManDocAccruals.DeleteAccrual(BinManApi.GetNextAccount(), "54962992", "172111");
//BinManDocuments.SendAttachObjectRequest(BinManApi.GetNextAccount(), new BinManDocuments.AttachObjectInfo()
//{
//    obj_BinManId = "5780306",
//    tarif_BinManCode ="182" ,
//    activeFrom = DateTime.Now.AddDays(-1),
//    tarif_value = "25",
//   doc_BinManId= "5906259"
//}) ;
//BinManDocuments.SendCreateRequest(BinManApi.GetNextAccount(),new BinManDocuments.DocInfo()
//{
//    Type_BinManCode="163",
//    Number = "100586893",
//    Organization_BinManId ="112790",
//    Client_BinManid = "5779536",
//    Group_BinManCode= "14",
//    dateFrom = new DateTime(2023,11,30),
//    dateTo = new DateTime(2023,12,01),
//    dateSign = new DateTime(2023,11,01),

//},out var binId);



//  var tasks = new List<Task>();
/*
var ObjectUpdateList = SQL.GetObjectListToSyncBinMan();
foreach (var v in ObjectUpdateList)
{
    //  tasks.Add(Task.УдалитьВсеНе0Начисления(() =>
    // {
    try
    {
        if (!DadataApi.TryFillAddres(v)) continue;

        LoginData ld = v.ld;
        if (ld == null)
        {
            ld = BinManApi.GetNextAccount();
        }
        if (string.IsNullOrEmpty(v.BinId) || v.BinId == "0" || v.BinId.Contains("-"))
        {

            if (v.SendCreateRequest(ld, out long binId))
            {
                SQL.UpdateObjectBinId(v.DataBase_Guid, binId);
                Log.Message($"[SYNC O I] Created object {v.DataBase_Guid} ({v.NAME}) BIN ID: {binId}");
            }
            else
            {
                Log.Error($"{v.DataBase_Guid}({v.NAME}) failed to create in binman");
                SQL.IgnoredObjectsId.Add(v.DataBase_Guid);
            }
        }
        else
        {
            if (v.SendEditRequest(ld))
            {
                SQL.SetObjectStatus(v.DataBase_Guid, BinManSyncStatus.ok);
                Log.Message($"[SYNC O U] Updated object {v.DataBase_Guid} ({v.NAME}) BIN ID: {v.BinId}");
            }
            else
            {
                if (!SQL.IgnoredObjectsId.Contains(v.DataBase_Guid))
                    SQL.IgnoredObjectsId.Add(v.DataBase_Guid);
            }
        }
    }
    catch (Exception ex) { Log.Error("Error while updating Objects Inner"); Log.Error(ex); }
    //  }));


    //  Thread.Sleep(500);
}

*/

//SQL.MarkGeozoneBinmanArchived("C:\\Users\\a.m.maltsev\\Downloads\\log.txt");
//SQL.SimpleContainersCountFix();
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\Прямые договора общий 2023-09.xlsx");
//SQL.LoadGeozoneOwnersFromExcel("C:\\Users\\a.m.maltsev\\Downloads\\84x6pwg4szkzzsv80hvpx9gsqpt2bv2z (ред.).xlsx");
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\Лист Microsoft Excel.xlsx", 1, 2);
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\Для загрузки владельцев (Промышленная).xlsx", 1, 2);
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\Для загрузки владельцев (Белово).xlsx", 1, 2);
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\На загрузку владельцев УК (Анжерка).xlsx", 1, 2);
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\На загрузку владельцев УК (Кемерово).xlsx", 1, 2); // Это прямые договоры !
//SQL.LoadGeozoneOwnersFromExcelDirectDogs("C:\\Users\\a.m.maltsev\\Downloads\\Владельцы УК на загрузку 2024-03-21 (1).xlsx", 1, 2);

//SQL.HandMadeLoadLoosedObjects();

//BinManApi.StartDocumentParserWorker();
//BinManDocWorker vvs = new BinManDocWorker();
//vvs.Execute(null);
//BinManObject.TryParseObjectType(ld, "5895696",out var rerasd); //ex
//Task tttt = Task.УдалитьВсеНе0Начисления(async () =>
//{




//    for (int i = 0; i < 0; i++)
//    {
//        Thread.Sleep(100);
//        string DocGuid = Guid.NewGuid().ToString();
//        Task t = Task.УдалитьВсеНе0Начисления(() =>
//        {
//            SQL.DocParseRequestJ("5889578", "8369760C-0EEE-436D-8A07-3689D068A785", DocGuid);

//            if (BinManDocumentParser.TryParseDocumentInfo(BinManApi.GetNextAccount(), "5889578", out var data))
//            {

//                BinManHelper.LoadFullDocumentParse(data, DocGuid);

//            }
//        });

//    }
//    while (true)
//    {
//        var v = Console.ReadLine();
//        if (v == "r")
//        {
//            await BinManApi.LogInAccounts();
//        }
//        else
//        {
//            string DocGuid = Guid.NewGuid().ToString();
//            Task t = Task.УдалитьВсеНе0Начисления(() =>
//            {
//                SQL.DocParseRequestJ("5889578", "8369760C-0EEE-436D-8A07-3689D068A785", DocGuid);

//                if (BinManDocumentParser.TryParseDocumentInfo(BinManApi.GetNextAccount(), "5889578", out var data))
//                {

//                    BinManHelper.LoadFullDocumentParse(data, DocGuid);

//                }
//            });
//        }
//    }
//});



//Log.Json(data);



//DadataApi.AccurateDbAddreses();

//LoginData ld = BinManApi.GetNextAccount();
//if (BinManObject.GetAttachedGeozones(ld, "5292126", out var links))
//{
//    if (BinManGeozone.AttachToObject(ld, links, "5872750", "5292126") != BinManGeozone.AttachResult.Failed) { }
//}

//Rosreestr.tryFindByAddres_FIAS("Кемерово Пушкино 5", out var ress);
//Rosreestr.ParseKadastrsByAddreses();
//Rosreestr.ParseKadTypesByKadastr();

//BinManObject.GetAttachedGeozones(BinManApi.GetNextAccount(), "5780306", out var res);

//for (int i = 0; i < 1000; i++)
//{
//    try
//    {
//        ContainersOwerflowFix.TryParseMainPage(BinManApi.GetNextAccount(), i);
//    }
//    catch (Exception ex) { Log.Error(ex);  }
//}

//var tt = SQL.GetAllIllegalTrashPiles();
//foreach (var v in tt)
//{
//    try
//    {
//        if (Rosreestr.tryFindByCoords(v.position, out var res))
//            SQL.UpdateIllegalTrashKadastr(v.guid, res.id);
//    }
//    catch (Exception e) { }
//}
//if (Rosreestr.tryFindByCoords(new GeoPoint(55.274405, 86.309822), out var res))
//{
//    Log.Json(res);
//    Log.Text(res.id);
//}



app.Run();

















#region FixAccruals 26-28.05.2024
/*
await BinManApi.LogInAccounts();
LoginData ld = BinManApi.GetNextAccount();
var DogId = "939628";
//if (BinManDocAccruals.TryGetAccrualSumm(ld, DogId, new DateTime(2024, 05, 01), new DateTime(2024, 05, 31), out var Summa))
//{
//    BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
//    {
//        doc_BinId = DogId,
//        comment = "test",
//        summ = Summa.ToString(),
//        date = new DateTime(2024, 05, 31),
//        typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
//        dateFrom = new DateTime(2024, 05, 01),
//        dateTo = new DateTime(2024, 05, 31)

//    }, out var BinIdd);
//}


var List1 = SQL.GetAccrualsListToCreateAutomaticly(new DateTime(2024, 04, 01), new DateTime(2024, 05, 01));
foreach (var v in List1)
{
    if (v == "939628") continue;
    if (BinManDocAccruals.TryGetAccrualSumm(ld, v, new DateTime(2024, 04, 01), new DateTime(2024, 04, 30), out var Summa))
    {
        BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
        {
            doc_BinId = DogId,
            comment = "test",
            summ = Summa.ToString(),
            date = new DateTime(2024, 04, 30),
            typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
            dateFrom = new DateTime(2024, 04, 01),
            dateTo = new DateTime(2024, 04, 30)

        }, out var BinIdd);
    }
    else
    {
        Log.Error($"ASDASDASD!!!! : {v}");
        //throw new Exception();
    }
}

var List2 = SQL.GetAccrualsListToCreateAutomaticly(new DateTime(2024, 05, 01), new DateTime(2024, 06, 01));
foreach (var v in List2)
{
    if (v == "939628") continue;
    if (BinManDocAccruals.TryGetAccrualSumm(ld, v, new DateTime(2024, 05, 01), new DateTime(2024, 05, 31), out var Summa))
    {
        BinManDocAccruals.AddAccrualToDoc(ld, new BinManAccrual()
        {
            doc_BinId = v,
            comment = "",
            summ = Summa.ToString(),
            date = new DateTime(2024, 05, 31),
            typeRaw = ((int)AccrualsType.accr_by_doc).ToString(),
            dateFrom = new DateTime(2024, 05, 01),
            dateTo = new DateTime(2024, 05, 31)

        }, out var BinIdd);
    }
    else
    {
        Log.Error($"ASDASDASD!!!! : {v}");
        //throw new Exception();
    }
}
*/
#endregion