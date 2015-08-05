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
        public object Value;  // MessagePackObject
        public bool IsError;
        public string ErrorMessage;
    }
    public struct RequestMsg
    {
        public string FuncName;
        public object[] Arguments;  // will be MsgPackObject
    }

    public class RpcCallResult<T>
    {
        public bool IsError { get { return Msg.IsError; } }

        public string ErrorMessage
        {
            get
            {
                if (IsError)
                    return Msg.ErrorMessage;
                return null;
            }
        }

        public T Value;
        public ResponseMsg Msg;
        public RpcCallResult(ResponseMsg response)
        {
            Msg = response;
            if (response.IsError)
            {

            }
            if (response.Value == null)
                Value = default(T);
            else
            {
                var msgObj = (MsgPack.MessagePackObject)response.Value;
                Value = (T)msgObj.ToObject();
            }

        }
    }

}
