using ADCHGKUser4.Controllers.Libs;
using AndroidAppServer.Libs;
using Dadata.Model;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using System;
using static AndroidAppServer.Libs.DadataApi;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AndroidAppServer.Controllers
{
  

    [Route("api/[controller]")]
    [ApiController]
    public class DadataController : ControllerBase
    {
        public const string DomStr = "дом";
        public const string DomStrWithSpace = $"{DomStr} ";
        public static string DOnDomReplace(string dd)
        {
            
            return string.IsNullOrEmpty(dd)? string.Empty: dd.Replace(", д ", $", {DomStr} ").Replace(", д. ", $", {DomStr} ");
        }
        public static void TryFormatAddresDToDom(Suggestion<Address> addr)
        {
            try
            {
                addr.value = DOnDomReplace(addr.value);
                addr.unrestricted_value = DOnDomReplace(addr.unrestricted_value);
                addr.data.house_with_type = DOnDomReplace(addr.data.house_with_type);
                addr.data.house = DOnDomReplace(addr.data.house);
                addr.data.house_type = DOnDomReplace(addr.data.house_type);
            }catch(Exception ex)
            {

            }

        }
        // GET: api/<DadataController>
        [HttpPost("address")]
        public IActionResult GetAddress([FromQuery] string token, [FromBody] GeoPoint pos)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                string Address = DadataApi.GetAddress(pos);
                try
                {
                    Address = DOnDomReplace(Address);
                }
                catch
                (Exception ex) { }
                return Ok(Address);
            }
            return Unauthorized();
        }

        public class DetailedAddress
        {
            public DetailedAddress(string guid, Suggestion<Address> info)
            {
                this.guid = guid;
                this.info = info;
            }
            public bool? isHandmade { get; set; }
            public string? guid { get; set; }
            public Suggestion<Address> info { get; set; }


        }
        [HttpPost("district")]
        public IActionResult DetectDistrict([FromQuery] string token, [FromQuery] string address)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                var res = SQL.GetDistrictGuidByAddress(address);
                if (string.IsNullOrEmpty(res)){
                    return NoContent();
                }


                return Ok(res);
            }
            return Unauthorized();
        }




        [HttpPost("address/all")]
        public IActionResult GetAllAddress([FromQuery] string token, [FromBody] GeoPoint pos)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                var Address = DadataApi.GetAllAddress(pos);
                DetailedAddress[] res =new DetailedAddress[Address.Count];
                int i = 0;
                foreach (var a  in Address)
                {

                   
                    var guid = Guid.NewGuid().ToString();

                    DetailedAddress adr = new DetailedAddress(guid, a);
                   // a.
                    res[i]= adr;
                    i++;
                }

                if (Address.Count <= 0) return NoContent();
                return Ok(res);
            }
            return Unauthorized();
        }


        [HttpPost("address/direct")]
        public IActionResult GetOwnedAddress([FromQuery] string token, [FromQuery] string addressOwner)
        {
            if (Login.ValidateToken(token, out _, out string UserGuid))
            {
                var t = SQL.GetOwnedAddressInfo(addressOwner, out string guid);

                if (t == null) return NoContent();

                DetailedAddress res = new DetailedAddress (guid, t);


                return Ok(res);
            }
            return Unauthorized();
        }





































































































    }
}
