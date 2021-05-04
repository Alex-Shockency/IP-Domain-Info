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
        private const string WHO_IS_API_KEY = "at_yeiqw8Xfe0soIx5WSXYx0ZkMQP10S";

        private const string RDAP_DOT_COM = "https://rdap.verisign.com/com/v1/domain/";
        private const string RDAP_DOT_NET = "https://rdap.verisign.com/net/v1/domain/";
        private const string RDAP_DOT_IP = "https://rdap.arin.net/registry/ip/";
        private const string RDAP_DOT_ORG = "https://rdap.publicinterestregistry.net/rdap/org/domain/";

        static readonly HttpClient client = new HttpClient();

        private string[] acceptedServices = { "RDAP", "GeoLocation", "Ping", "ReverseDns", "IsDomainAvailable" };

        private readonly ILogger<IPDomainInfoController> _logger;

        public IPDomainInfoController(ILogger<IPDomainInfoController> logger)
        {
            _logger = logger;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{nameOrAddress}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<IPDomainInfo>>> GetAsync(string nameOrAddress, [FromUri] string serviceList = "RDAP,GeoLocation,Ping,ReverseDns,IsDomainAvailable")
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
                    case "IsDomainAvailable":
                        tasks[index] = IsDomainAvailable(nameOrAddress);
                        break;
                }
                index++;
            }

            IPDomainInfo[] results = await Task.WhenAll(tasks);

            return results;
        }

        private async Task<IPDomainInfo> ReverseDns(string nameOrAddress)
        {
            IPAddress tempForParsing;
            if (IPAddress.TryParse(nameOrAddress, out tempForParsing))
            {
                return await callExternalAPI("https://reverse-ip.whoisxmlapi.com/api/v1?apiKey=" + WHO_IS_API_KEY + "&ip=" + nameOrAddress, "ReverseDns");
            }
            else
            {
                IPDomainInfo reverseDnsInfo = new IPDomainInfo();
                reverseDnsInfo.ServiceName = "ReverseDns";
                try
                {
                    IPHostEntry hostEntry = await Dns.GetHostEntryAsync(nameOrAddress);
                    var hostInfo = new { hostName = hostEntry.HostName.ToString() };

                    reverseDnsInfo.Result = JsonConvert.SerializeObject(hostInfo);
                    return reverseDnsInfo;
                }
                catch (Exception e)
                {
                    reverseDnsInfo.Result = JsonConvert.SerializeObject(e);
                    return reverseDnsInfo;
                }
            }


        }
        private async Task<IPDomainInfo> Ping(string nameOrAddress)
        {
            IPDomainInfo pingInfo = new IPDomainInfo();
            pingInfo.ServiceName = "Ping";
            try
            {
                Ping ping = new Ping();
                PingReply pingReply;

                IPAddress tempForParsing;
                if (IPAddress.TryParse(nameOrAddress, out tempForParsing))
                {
                    pingReply = await ping.SendPingAsync(tempForParsing);
                }
                else
                {
                    pingReply = await ping.SendPingAsync(nameOrAddress);
                }

                var pingReplyInfo = new { status = pingReply.Status.ToString(), roundTripTime = pingReply.RoundtripTime + "ms" };
                pingInfo.Result = JsonConvert.SerializeObject(pingReplyInfo);
                return pingInfo;
            }
            catch (Exception e)
            {
                pingInfo.Result = JsonConvert.SerializeObject(e);
                return pingInfo;
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
            else if (nameOrAddress.Contains(".net"))
            {
                return await callExternalAPI(RDAP_DOT_NET + nameOrAddress, "RDAP");
            }
            else
            {
                return await callExternalAPI(RDAP_DOT_ORG + nameOrAddress, "RDAP");
            }
        }

        private async Task<IPDomainInfo> IsDomainAvailable(string nameOrAddress)
        {
            IPAddress tempForParsing;
            IPDomainInfo info = new IPDomainInfo();
            if (IPAddress.TryParse(nameOrAddress, out tempForParsing))
            {
                info.ServiceName = "IsDomainAvailable";
                info.Result = "{\"Error\":\"Domain availability only works when passing in a domain\"}";
                return info;
            }
            else if (nameOrAddress.Contains(".com"))
            {
                info = await callExternalAPI(RDAP_DOT_COM + nameOrAddress, "IsDomainAvailable");
                if (info.Result.Contains("404"))
                {
                    info.Result = "true";
                }
                else
                {
                    info.Result = "false";
                }
                return info;
            }
            else if (nameOrAddress.Contains(".net"))
            {
                info = await callExternalAPI(RDAP_DOT_NET + nameOrAddress, "IsDomainAvailable");
                if (info.Result.Contains("404"))
                {
                    info.Result = "true";
                }
                else
                {
                    info.Result = "false";
                }
                return info;
            }
            else
            {
                info = await callExternalAPI(RDAP_DOT_ORG + nameOrAddress, "IsDomainAvailable");
                if (info.Result.Contains("404"))
                {
                    info.Result = "true";
                }
                else
                {
                    info.Result = "false";
                }
                return info;
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
