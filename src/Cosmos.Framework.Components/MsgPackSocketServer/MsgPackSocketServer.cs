using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Framework.Components.MsgPackSocketServer
{
    public class MsgPackSocketServer
    {
        private Socket _socket;
        public MsgPackSocketServer()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 88));
            _socket.Listen(int.MaxValue);
        }

        private void BeginAccept()
        {
            _socket.BeginAccept(OnAccept, null);
        }

        private void OnAccept(IAsyncResult ar)
        {
            
            BeginAccept();
        }
    }
}
