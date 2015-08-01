using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;

namespace Cosmos.Rpc
{
    public class MsgPackTool
    {
        public static void WriteStream<T>(MemoryStream stream, T responseMsg)
        {
            var serializer = MessagePackSerializer.Get<T>();
            serializer.Pack(stream, responseMsg);
        }
        public static T ReadStream<T>(MemoryStream stream)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.Unpack(stream);
        }

        public static byte[] GetBytes<T>(T msg)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.PackSingleObject(msg);
        }
        public static T GetMsg<T>(byte[] data)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.UnpackSingleObject(data);
        }

    }

}
