using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Rpc
{
    public class BaseNetMqMsg
    {
        public string SessionToken;
        public string RequestToken;
        public byte[] Data;
    }

}
