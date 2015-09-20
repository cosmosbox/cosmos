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
        /// <summary>
        /// 通过网络传输的object数组，会被转换成MessagePackObject,
        /// 这里进行数组转换
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static object[] ConvertMsgPackObjectArray(object[] arr)
        {
            var arguments = new object[arr.Length];
            for (var i = 0; i < arguments.Length; i++) // MsgPack.MessagePackObject arg in requestProto.Arguments)
            {
                if (arr[i] is MsgPack.MessagePackObject)
                {
                    MsgPack.MessagePackObject arg = (MsgPack.MessagePackObject) arr[i];
                    arguments[i] = arg.ToObject();
                }
                else
                {
                    arguments[i] = arr[i];
                }

            }
            return arguments;
        }

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

        public static byte[] GetBytes(Type type, object obj)
        {
            var serializer = MessagePackSerializer.Get(type);
            return serializer.PackSingleObject(obj);
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
        public static object GetMsg(Type type, byte[] data)
        {
            var serializer = MessagePackSerializer.Get(type);
            return serializer.UnpackSingleObject(data);
        }

    }

}
