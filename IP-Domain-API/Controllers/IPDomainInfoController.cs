using IP_Domain_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web.Http;

namespace IP_Domain_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPDomainInfoController : ControllerBase
    {
        private const string IP_STACK_API_KEY = "f3cadd5cee330e0e895e53dc37faedc9";

        private const string RDAP_DOT_COM = "https://rdap.verisign.com/com/v1/domain/";
        private const string RDAP_DOT_IP = "https://rdap.arin.net/registry/ip/";
        private const string RDAP_DOT_ORG = "https://rdap.publicinterestregistry.net/rdap/org/domain/";

        static readonly HttpClient client = new HttpClient();

        private string[] acceptedServices = { "RDAP", "GeoLocation", "Ping", "ReverseDns" };

        private readonly ILogger<IPDomainInfoController> _logger;

        public IPDomainInfoController(ILogger<IPDomainInfoController> logger)
        {
            _logger = logger;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{nameOrAddress}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<IPDomainInfo>>> GetAsync(string nameOrAddress, [FromUri] string serviceList = "RDAP,GeoLocation,Ping,ReverseDns")
        {
            string[] serviceListArray = serviceList.Split(',');
            Task<IPDomainInfo>[] tasks = new Task<IPDomainInfo>[serviceListArray.Length];

            int index = 0;
            foreach (string serviceName in serviceListArray)
            {
                if (!acceptedServices.Contains(serviceName))
                {
                    return BadRequest("Service name " + serviceName + " is not a valid option");
                }
                switch (serviceName)
                {
                    case "RDAP":
                        tasks[index] = RDAP(nameOrAddress);
                        break;
                    case "GeoLocation":
                        tasks[index] = GeoLocation(nameOrAddress);
                        break;
                    case "Ping":
                        tasks[index] = Ping(nameOrAddress);
                        break;
                    case "ReverseDns":
                        tasks[index] = ReverseDns(nameOrAddress);
                        break;
                }
                index++;
            }

            IPDomainInfo[] results = await Task.WhenAll(tasks);

            return results;
        }

        private async Task<IPDomainInfo> ReverseDns(string nameOrAddress)
        {
            IPDomainInfo reverseDnsResult = new IPDomainInfo();
            reverseDnsResult.ServiceName = "ReverseDns";
            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(nameOrAddress);
                var hostInfo = new { hostName = hostEntry.HostName.ToString() };

                reverseDnsResult.Result = JsonConvert.SerializeObject(hostInfo);
                return reverseDnsResult;
            }
            catch (Exception e)
            {
                reverseDnsResult.Result = JsonConvert.SerializeObject(e);
                return reverseDnsResult;
            }

        }
        private async Task<IPDomainInfo> Ping(string nameOrAddress)
        {
            IPDomainInfo pingResult = new IPDomainInfo();
            pingResult.ServiceName = "Ping";
            try
            {

                Ping ping = new Ping();
                PingReply pingReply = await ping.SendPingAsync(nameOrAddress);
                var pingInfo = new { status = pingReply.Status.ToString(), roundTripTime = pingReply.RoundtripTime + "ms" };
                pingResult.Result = JsonConvert.SerializeObject(pingInfo);
                return pingResult;
            }
            catch (Exception e)
            {
                pingResult.Result = JsonConvert.SerializeObject(e);
                return pingResult;
            }
        }

        private async Task<IPDomainInfo> GeoLocation(string nameOrAddress)
        {
            return await callExternalAPI("http://api.ipstack.com/" + nameOrAddress + "?access_key=" + IP_STACK_API_KEY, "GeoLocation");
        }

        private async Task<IPDomainInfo> RDAP(string nameOrAddress)
        {
            IPAddress tempForParsing;
            if (IPAddress.TryParse(nameOrAddress, out tempForParsing))
            {
                return await callExternalAPI(RDAP_DOT_IP + nameOrAddress, "RDAP");
            }
            else if (nameOrAddress.Contains(".com"))
            {
                return await callExternalAPI(RDAP_DOT_COM + nameOrAddress, "RDAP");
            }
            else
            {
                return await callExternalAPI(RDAP_DOT_ORG + nameOrAddress, "RDAP");
            }
        }

        private static async Task<IPDomainInfo> callExternalAPI(string url, string serviceName)
        {
            IPDomainInfo info = new IPDomainInfo();
            info.ServiceName = serviceName;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                info.Result = await response.Content.ReadAsStringAsync();
                return info;
            }
            catch (HttpRequestException e)
            {
                info.Result = JsonConvert.SerializeObject(e);
                return info;
            }
        }
    }
}
