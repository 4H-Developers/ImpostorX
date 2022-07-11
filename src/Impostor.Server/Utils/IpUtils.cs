using System.Linq;
using System.Net;
using System.Net.Sockets;
using Impostor.Api;

namespace Impostor.Server.Utils
{
    internal static class IpUtils
    {
        public static string ResolveIp(string ip)
        {
            // Check if valid ip was entered.
            if (!IPAddress.TryParse(ip, out var ipAddress))
            {
                // Attempt to resolve DNS.
                try
                {
                    var hostAddresses = Dns.GetHostAddresses(ip);
                    if (hostAddresses.Length == 0)
                    {
                        throw new ImpostorConfigException($"输入的IP地址无效 '{ip}'.");
                    }

                    // Use first IPv4 result.
                    ipAddress = hostAddresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
                catch (SocketException)
                {
                    throw new ImpostorConfigException($"无法解析主机名 '{ip}'.");
                }
            }

            // Only IPv4.
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                throw new ImpostorConfigException($"此 '{ipAddress}', ip无效私服只支持ipv4");
            }

            return ipAddress.ToString();
        }
    }
}
