using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        public const String IP_STACK_API_KEY = "f3cadd5cee330e0e895e53dc37faedc9";
        public const String RDAP_DOT_COM = "https://rdap.verisign.com/com/v1/domain/";
        public const String RDAP_DOT_IP = "https://rdap.arin.net/registry/ip/";
        public const String RDAP_DOT_ORG = "https://rdap.publicinterestregistry.net/rdap/org/domain/";

        static readonly HttpClient client = new HttpClient();

        private readonly ILogger<IPDomainInfoController> _logger;

        public IPDomainInfoController(ILogger<IPDomainInfoController> logger)
        {
            _logger = logger;
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{nameOrAddress}")]
        public async Task<IEnumerable<string>> GetAsync(String nameOrAddress, [FromUri] String serviceList = "test,test")
        {
            foreach (string serviceName in serviceList.Split(','))
            {

            }
            Task<string>[] tasks = new Task<string>[4];

            tasks[0] = RDAPAsync(nameOrAddress);
            tasks[1] = GeoLocation(nameOrAddress);
            tasks[2] = Ping(nameOrAddress);
            tasks[3] = ReverseDns(nameOrAddress);

            String[] results = await Task.WhenAll(tasks);
            Array.ForEach(results, Console.WriteLine);

            return results;
        }

        private async Task<string> ReverseDns(String nameOrAddress)
        {
            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(nameOrAddress);
                return hostEntry.HostName;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
        private async Task<string> Ping(String nameOrAddress)
        {
            Ping ping = new Ping();
            try
            {
                object tempUserToken = new object();
                PingReply pingReply = await ping.SendPingAsync(nameOrAddress);
                return pingReply.Status.ToString() + " " + pingReply.RoundtripTime.ToString() + "ms";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private async Task<string> GeoLocation(String nameOrAddress)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://api.ipstack.com/" + nameOrAddress + "?access_key=" + IP_STACK_API_KEY);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                return e.Message;
            }
        }

        private async Task<string> RDAPAsync(String nameOrAddress)
        {
            IPAddress tempForParsing;
            if (IPAddress.TryParse(nameOrAddress, out tempForParsing))
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(RDAP_DOT_IP + nameOrAddress);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    return e.Message;
                }
            }
            else
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(RDAP_DOT_COM + nameOrAddress);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    return e.Message;
                }
            }
        }
    }
}
