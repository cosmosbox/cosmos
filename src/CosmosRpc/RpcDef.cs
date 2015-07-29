using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack;
using MsgPack.Serialization;
namespace Cosmos.Rpc
{
    public class RpcShare
    {
        public static MessagePackSerializer<RequestMsg> RequestSerializer = MessagePackSerializer.Get<RequestMsg>();
        public static MessagePackSerializer<ResponseMsg> ResponseSerializer = MessagePackSerializer.Get<ResponseMsg>();
    }

    public struct ResponseMsg
    {
        public int RequestId;
        public object Result;
    }
    public struct RequestMsg
    {
        public int RequestId;
        public string FuncName;
        public object[] Arguments;  // will be MsgPackObject
    }

}
