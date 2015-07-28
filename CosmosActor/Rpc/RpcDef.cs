using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;
namespace Cosmos.Rpc
{
    public class RpcShare
    {
        public static MessagePackSerializer<RpcRequestProto> RequestSerializer = MessagePackSerializer.Get<RpcRequestProto>();
        public static MessagePackSerializer<RpcResponseProto> _responseSerializer = MessagePackSerializer.Get<RpcResponseProto>();
    }

    public struct RpcResponseProto
    {
        public int RequestId;
        public object Result;
    }
    public struct RpcRequestProto
    {
        public int RequestId;
        public string FuncName;
        public object[] Arguments;
    }

}
