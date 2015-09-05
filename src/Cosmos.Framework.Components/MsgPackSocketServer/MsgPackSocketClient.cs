using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Framework.Components.MsgPackSocketServer
{
    public class MsgPackSocketClient
    {
        private Socket _socket;
        public MsgPackSocketClient()
        {

            _socket.BeginConnect(IPAddress.Parse("127.0.0.1"), 12345, OnConnected, null);
        }

        private void OnConnected(IAsyncResult ar)
        {
            
        }
    }
}
