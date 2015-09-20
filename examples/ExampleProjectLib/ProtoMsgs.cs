/// <summary>
/// All Msg protocol 
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ExampleProjectLib
{
    class LoginResProto
    {
        public string GameServerHost;
        public int GameServerPort;
        public int SubcribePort;
        public int Id;
    }

    public class LoginRequest
    {
        public int Id;
    }

    public class LoginResponse
    {
        public string GameServerHost;
        public int GameServerPort;
        public int SubcribePort;
        public int Id;
    }
}
