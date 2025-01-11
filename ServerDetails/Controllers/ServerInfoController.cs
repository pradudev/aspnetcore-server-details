using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ServerDetails.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ServerInfoController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public ServerInfoController(ILogger<ServerInfoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<object> Get()
        {
            dynamic reqObj = new ExpandoObject();

            reqObj.TitleMsg = _configuration["TitleMsg"];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            reqObj.serverName = Environment.MachineName;
            reqObj.serverHostName = ipHostInfo.HostName;
            /*reqObj.networks = new
            {
                //serverIps = string.Join(", ", (object[])ipHostInfo.AddressList),
                serverIps = GetServerIPs(),
                dnsIps = GetDnsIps(),
                defaultGatewayIp = GetDefaultGateway(),
                dhcpServerIp = GetDhcpServer()
            };*/

            reqObj.networkInfo = GetNetworkInfo();
            reqObj.outboundIp = GetOutboundIp();
            reqObj.userDomainName = Environment.UserDomainName;
            reqObj.domainController = Environment.GetEnvironmentVariable("LOGONSERVER");
            reqObj.userName = Environment.UserName;
            reqObj.userDnsDomain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
            reqObj.userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            reqObj.systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
            reqObj.serverOS = new
            {
                platform = Environment.OSVersion.Platform,
                version = Environment.OSVersion.Version,
                servicePack = Environment.OSVersion.ServicePack,
                nameAndVer = RuntimeInformation.OSDescription,
                isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
            };

            reqObj.webServer = new
            {
                clientHostIp = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                clientHostPort = Request.HttpContext.Connection.RemotePort,
                serverHostIp = Request.HttpContext.Connection.LocalIpAddress.ToString(),
                serverHostPort = Request.HttpContext.Connection.LocalPort,
                requestHostName = Request.Host.Host,
                requestHostPort = Request.Host.Port,
                xForwardedFor = Request.Headers["X-Forwarded-For"],
                xForwardedProto = Request.Headers["X-Forwarded-Proto"],
                xForwardedPort = Request.Headers["X-Forwarded-Port"]
            };

            reqObj.process = new 
            {
                id= Process.GetCurrentProcess().Id,
                name = Process.GetCurrentProcess().ProcessName
            };

            reqObj.requestHeaders = GetRequestHeaders();

            reqObj.environmentVariables = GetAllEnvironmentVariables();

            return reqObj;
        }

        private SortedDictionary<string, string> GetRequestHeaders()
        {
            var sortedDic = new SortedDictionary<string, string>();

            foreach (var h in Request.Headers)
            {
                sortedDic.Add(h.Key, h.Value);
            }

            return sortedDic;
        }

        private SortedDictionary<string, string> GetAllEnvironmentVariables()
        {
            var sortedDic = new SortedDictionary<string, string>();

            var envVars = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry de in envVars)
            {
                sortedDic.Add(de.Key.ToString(), de.Value.ToString());
            }

            return sortedDic;
        }

        private string GetServerIPs()
        {
            var ipList = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.UnicastAddresses)
                .Select(g => g);

            var ipstrList = new List<string>();

            foreach (var ipInfo in ipList)
            {
                ipstrList.Add($"IP: {ipInfo.Address.ToString()} | Mask: {ipInfo.IPv4Mask.ToString()}");
            }

            return string.Join(", ", (object[])ipstrList.ToArray());
        }

        private string GetDnsIps()
        {
            var dnsList = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.DnsAddresses)
                .Select(n => n.ToString())
                .ToArray();

            return string.Join(", ", (object[])dnsList);
        }

        private string GetOutboundIp()
        {
            string outboundIp = string.Empty;

            using (HttpClient httpClient = new HttpClient())
            {
                outboundIp = httpClient.GetStringAsync("https://api.ipify.org").Result;
            }

            return outboundIp;

        }

        private string GetDefaultGateway()
        {
            var gatewayIp = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .FirstOrDefault();

            if (gatewayIp != null)
                return gatewayIp.ToString();

            return null;
        }

        private string GetDhcpServer()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.DhcpServerAddresses)
                .Select(n => n.ToString())
                .FirstOrDefault();
        }

        private List<object> GetNetworkInfo()
        {
            var listObj = new List<object>();

            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in allNetworkInterfaces)
            {
                var niObj = new {
                    interfaceName = ni.Name,
                    interfaceDesc = ni.Description,
                    interfaceType = ni.NetworkInterfaceType.ToString(),
                    interfaceStatus = ni.OperationalStatus.ToString(),
                    ipAddress = ni.GetIPProperties()?.UnicastAddresses.FirstOrDefault()?.Address.ToString(),
                    subnetMask = ni.GetIPProperties()?.UnicastAddresses.FirstOrDefault()?.IPv4Mask.ToString(),
                    defaultGateway = ni.GetIPProperties()?.GatewayAddresses.FirstOrDefault()?.Address.ToString(),
                    dhcpServer = ni.GetIPProperties()?.DhcpServerAddresses.FirstOrDefault()?.ToString(),
                    dnsServers = ni.GetIPProperties()?.DnsAddresses.Select(x => x.ToString()).ToList()
                };

                listObj.Add(niObj);
            }

            return listObj;
        }
    }
}