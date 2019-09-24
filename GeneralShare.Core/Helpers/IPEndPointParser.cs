using System;
using System.Net;
using System.Net.Sockets;

namespace GeneralShare
{
    public static class IPEndPointParser
    {
        private static bool ResolveDNS(string address, out IPAddress result)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                result = IPAddress.Loopback;
                return true;
            }

            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(address).AddressList;
                for (int i = 0; i < addresses.Length; i++)
                {
                    AddressFamily family = addresses[i].AddressFamily;
                    if (family == AddressFamily.InterNetworkV6 ||
                        family == AddressFamily.InterNetwork)
                    {
                        result = addresses[i];
                        return true;
                    }
                }
            }
            catch
            {
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Resolves a IPv4, IPv6 or URL address to an <see cref="IPEndPoint"/> instance.
        /// </summary>
        /// <param name="address">Address to resolve.</param>
        /// <returns>the IPEndPoint</returns>
        public static bool TryResolve(string address, out IPEndPoint result)
        {
            string[] ep = address.Split(':');

            int port = 0;
            IPAddress ip;
            if (ep.Length >= 2)
            {
                int.TryParse(ep[ep.Length - 1], out port);
                if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                {
                    result = null;
                    return false;
                }

                string ipStr = string.Join(":", ep, 0, ep.Length - 1);
                if (!IPAddress.TryParse(ipStr, out ip) && !ResolveDNS(ipStr, out ip))
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip) && !ResolveDNS(ep[0], out ip))
                {
                    result = null;
                    return false;
                }
            }

            result = new IPEndPoint(ip, port);
            return true;
        }
        
        public static IPEndPoint Parse(string value)
        {
            if (TryResolve(value, out IPEndPoint result))
                return result;
            throw new FormatException($"{nameof(value)} is not in the correct format.");
        }
    }
}