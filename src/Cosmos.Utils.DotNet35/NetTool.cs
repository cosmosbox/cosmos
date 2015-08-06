using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Cosmos.Tool
{
    class NetTool
    {
        /// <summary>
        /// Check a port is available or not
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool CheckNetworkPort(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            
            return isAvailable;
        }

        /// <summary>
        /// Check a port is available or not
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool TestPort(int port)
        {
            TcpListener tcpListener = null;

            try
            {
                var ipAddress = Dns.GetHostEntry("127.0.0.1").AddressList[0];

                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();

                return true;
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (tcpListener != null)
                    tcpListener.Stop();
            }

            return false;
        }
    }
}
