using Newtonsoft.Json;

namespace AndroidAppServer.Libs
{
    
    public class Activity
    {
        public string activityCode { get; set; }
        public string activityName { get; set; }
        public string activityKind { get; set; }
    }

    public class Authority
    {
        public string authorityCode { get; set; }
        public string authorityName { get; set; }
        public List<Permission> permissions { get; set; }
    }

    public class Contact
    {
        public string phone { get; set; }
        public string site { get; set; }
        public string mail { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public Info info { get; set; }
        public List<Authority> authorities { get; set; }
        public List<Activity> activities { get; set; }
        public List<Head> heads { get; set; }
        public List<Succession> successions { get; set; }
        public List<FacialAccount> facialAccounts { get; set; }
        public List<FoAccount> foAccounts { get; set; }
        public List<ParticipantPermission> participantPermissions { get; set; }
        public List<NonParticipantPermission> nonParticipantPermissions { get; set; }
        public List<ProcurementPermission> procurementPermissions { get; set; }
        public List<Contact> contacts { get; set; }
        public List<object> acceptAuths { get; set; }
        public List<Transfauth> transfauth { get; set; }
        public List<object> attachment { get; set; }
        public List<object> contracts { get; set; }
        public List<object> ubptransfauthbp { get; set; }
        public List<object> ubptransfauthbu { get; set; }
        public List<object> ubpfin { get; set; }
        public List<object> ksaccounts { get; set; }
    }

    public class FacialAccount
    {
        public string kindName { get; set; }
        public string kindCode { get; set; }
        public string num { get; set; }
        public string createDate { get; set; }
        public string closeDate { get; set; }
        public string status { get; set; }
        public string openUfkCode { get; set; }
        public string openUfkName { get; set; }
        public string openTofkName { get; set; }
        public string srvUfkCode { get; set; }
        public string srvUfkName { get; set; }
        public string accountorgcode { get; set; }
        public string accountorgfullname { get; set; }
        public string ppocode { get; set; }
        public string pponame { get; set; }
        public string refopenUfkCode { get; set; }
        public string refsrvUfkCode { get; set; }
    }

    public class FoAccount
    {
        public string foName { get; set; }
        public string foCode { get; set; }
        public string accountTypeName { get; set; }
        public string num { get; set; }
    }

    public class Head
    {
        public string fio { get; set; }
        public string post { get; set; }
        public string docName { get; set; }
        public string docNum { get; set; }
        public string docDate { get; set; }
        public string headMain { get; set; }
    }

    public class Info
    {
        public string regNum { get; set; } //Учетный номер организации
        public string code { get; set; }//Код организации (обособленного подразделения) по Сводному реестру
        public string divisionParentName { get; set; }//Наименование организации, создавшей подразделение
        public string divisionParentCode { get; set; }//Код организации, создавшей подразделение
        public string ogrn { get; set; }
        public string fullName { get; set; }
        public string shortName { get; set; }
        public string inn { get; set; }
        public string kpp { get; set; }
        public string regDate { get; set; }//Дата постановки на учет в ФНС
        public string okopfName { get; set; }//Наименование по ОКОПФ
        public string okopfCode { get; set; }//Код по ОКОПФ
        public string okfsName { get; set; }//Наименование по ОКФС
        public string okfsCode { get; set; }//Код по ОКФС
        public string postIndex { get; set; }//Индекс
        public string cityType { get; set; }
        public string cityName { get; set; }
        public string streetType { get; set; }
        public string streetName { get; set; }
        public string house { get; set; }
        public string oktmoName { get; set; }
        public string oktmoCode { get; set; }
        public string orfkName { get; set; }
        public string orfkCode { get; set; }
        public string oksmName { get; set; }
        public string oksmCode { get; set; }
        public string location { get; set; }
        public string kbkName { get; set; }
        public string kbkCode { get; set; }
        public string okoguName { get; set; }
        public string okoguCode { get; set; }
        public string okpoCode { get; set; }
        public string orgTypeName { get; set; }
        public string orgTypeCode { get; set; }
        public string establishmentKindName { get; set; }
        public string establishmentKindCode { get; set; }
        public string legalPersonKindName { get; set; }
        public string legalPersonKindCode { get; set; }
        public string ougvName { get; set; }
        public string ougvCode { get; set; }
        public string uoName { get; set; }
        public string uoCode { get; set; }
        public string creatorKindName { get; set; }
        public string creatorKindCode { get; set; }
        public string creatorPlaceName { get; set; }
        public string creatorPlaceCode { get; set; }
        public string founderKindName { get; set; }
        public string founderKindCode { get; set; }
        public string founderPlaceName { get; set; }
        public string founderPlaceCode { get; set; }
        public string budgetLvlName { get; set; }
        public string budgetLvlCode { get; set; }
        public string budgetName { get; set; }
        public string budgetCode { get; set; }
        public string statusCode { get; set; }
        public string statusName { get; set; }
        public string regionType { get; set; }
        public string regionName { get; set; }
        public string isOGV { get; set; }
        public string isObosob { get; set; }
        public string orgStatus { get; set; }
        public string recordNum { get; set; }
        public string parentCode { get; set; }
        public string parentName { get; set; }
        public string okatoCode { get; set; }
        public string okatoName { get; set; }
        public string guid { get; set; }
        public string status { get; set; }
        public string controlNum { get; set; }
        public string bidNum { get; set; }
        public string firstRegDate { get; set; }
        public string firstRegGuid { get; set; }
        public string lastRegGuid { get; set; }
        public string lastRegDate { get; set; }
        public string lastRegNum { get; set; }
        public string updateReason { get; set; }
        public string updateNum { get; set; }
        public string inclusionDate { get; set; }
        public string exclusionDate { get; set; }
        public string pubpCode { get; set; }
        public string rubpCode { get; set; }
        public string nubpCode { get; set; }
        public string cpzCode { get; set; }
        public string pgmyCode { get; set; }
        public string firmName { get; set; }
        public string kofkCode { get; set; }
        public string nameDocs { get; set; }
        public string accMgmt { get; set; }
        public string naibznachuch { get; set; }
        public string regionCode { get; set; }
        public string areaCode { get; set; }
        public string areaType { get; set; }
        public string areaName { get; set; }
        public string cityCode { get; set; }
        public string localCode { get; set; }
        public string localName { get; set; }
        public string localType { get; set; }
        public string streetCode { get; set; }
        public string building { get; set; }
        public string apartment { get; set; }
        public string reformationDocument { get; set; }
        public string reformationDocumentNum { get; set; }
        public string reformationDocumentDate { get; set; }
        public string reformationName { get; set; }
        public string reformationCode { get; set; }
        public string reformationStartDate { get; set; }
        public string reformationEndDate { get; set; }
        public string dateUpdate { get; set; }
        public string isExcluded { get; set; }
        public string isReorg { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string loadDate { get; set; }
        public string regionKladrCode { get; set; }
        public string egrulnotincluded { get; set; }
        public string parentrecordnum { get; set; }
        public string planningstructuretype { get; set; }
        public string planningstructurename { get; set; }
        public string contourTypeCode { get; set; }
        public string specEventCode { get; set; }
        public string speceventcodedop1 { get; set; }
        public string speceventcodedop2 { get; set; }
        public string speceventcodedop3 { get; set; }
        public string dsp { get; set; }
        public string ppotypecode { get; set; }
        public string ppotypename { get; set; }
        public string reforfkCode { get; set; }
        public string isUch { get; set; }
    }

    public class Limits
    {
    }

    public class NonParticipantPermission
    {
        public string name { get; set; }
        public string registryNum { get; set; }
        public string code { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string authBudgName { get; set; }
        public string authBudgCode { get; set; }
        public string authPPOName { get; set; }
        public string authPPOCode { get; set; }
        public string authKBKGlavaName { get; set; }
        public string authKBKGlavaCode { get; set; }
    }

    public class ParticipantPermission
    {
        public string name { get; set; }
        public string code { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }

    public class Permission
    {
        public string permissionCode { get; set; }
        public string permissionName { get; set; }
    }

    public class ProcurementPermission
    {
        public string name { get; set; }
        public string code { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }

    public class ElBudgetData
    {

        public int offset { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }
        public int recordCount { get; set; }
        public List<Datum> data { get; set; }
        public string order { get; set; }
        public string orderDirection { get; set; }
        public Limits limits { get; set; }
        public string version { get; set; }
        public int pageCount { get; set; }

        public static int GetRecordCount()
        {
            using (HttpClient hc = new HttpClient())
            {
                var res = hc.GetAsync("https://www.budget.gov.ru/epbs/registry/ubpandnubp/data?blocks=null&pageSize=0");
                var txt = res.GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();

                ElBudgetData myDeserializedClass = JsonConvert.DeserializeObject<ElBudgetData>(txt);
                return myDeserializedClass.recordCount;
            }
        }
    }

    public class Succession
    {
        public string parentName { get; set; }
        public string parentCode { get; set; }
        public string ogrn { get; set; }
        public string docname { get; set; }
        public string numberdoc { get; set; }
        public string documentdate { get; set; }
        public string datasource { get; set; }
    }

    public class Transfauth
    {
        public string authfovillagescode { get; set; }
        public string authfovillagesname { get; set; }
        public string authfomunicipalcode { get; set; }
        public string authfomunicipalname { get; set; }
        public string authstartdate { get; set; }
        public string authenddate { get; set; }
        public string kbkglavacode { get; set; }
        public string budgetcode { get; set; }
        public string authregnum { get; set; }
        public string authfovillagesppocode { get; set; }
        public string authfovillagespponame { get; set; }
    }
}
