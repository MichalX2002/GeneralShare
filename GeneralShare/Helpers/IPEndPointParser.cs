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
        /// Converts a IPv4 or IPv6 address to an <see cref="IPEndPoint"/> instance.
        /// </summary>
        /// <param name="value">value to parse</param>
        /// <returns>the IPEndPoint</returns>
        public static bool TryParse(string value, out IPEndPoint result)
        {
            string[] ep = value.Split(':');

            int port = 0;
            IPAddress ip;
            if (ep.Length > 2)
            {
                int.TryParse(ep[ep.Length - 1], out port);
                if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                {
                    result = null;
                    return false;
                }

                string address = string.Join(":", ep, 0, ep.Length - 1);
                if (!IPAddress.TryParse(address, out ip) && !ResolveDNS(address, out ip))
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
            if (TryParse(value, out IPEndPoint result))
                return result;
            throw new FormatException($"{nameof(value)} is not in the correct format.");
        }
    }
}