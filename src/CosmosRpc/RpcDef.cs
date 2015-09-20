using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack;
using MsgPack.Serialization;

namespace Cosmos.Rpc
{
    /// <summary>
    /// a method mark with this Attribute will be as a rpc function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceFuncAttribute : Attribute
    {
        public string Tag { get; set; }
    }

    /// <summary>
    /// ReturnResult
    /// </summary>
    public struct ResponseMsg
    {
        public byte[] Data;  // MessagePackObject
        public bool IsError;
        public string ErrorMessage;
    }
    /// <summary>
    /// Mark the type, the object
    /// </summary>
    public struct RequestMsg
    {
        public string RequestTypeName;
        public byte[] RequestObjectData;

        public Type RequestType
        {
            get { return Type.GetType(RequestTypeName); }
        }
        [System.Obsolete]
        public string FuncName;
        [System.Obsolete]
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
            if (response.Data == null)
                Value = default(T);
            else
            {
                Value = MsgPackTool.GetMsg<T>(response.Data);
            }

        }
    }

}
