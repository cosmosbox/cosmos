using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Utils
{
    public class Md5Util
    {
        // 获取指定流的MD5
        public static string MD5_Stream(Stream stream)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Provider = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] hash_byte = md5Provider.ComputeHash(stream);

            string resule = System.BitConverter.ToString(hash_byte);

            resule = resule.Replace("-", "");

            return resule;
        }

        public static string MD5_File(string filePath)
        {
            try
            {
                using (FileStream get_file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return MD5_Stream(get_file);
                }
            }
            catch (Exception e)
            {
                return e.ToString();

            }
        }
        public static byte[] MD5_bytes(string str)
        {
            // MD5 文件名
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return md5.ComputeHash(System.Text.Encoding.Unicode.GetBytes(str));
        }

        // 字符串16位 MD5
        public static string String16(string str)
        {
            byte[] md5Bytes = MD5_bytes(str);
            str = System.BitConverter.ToString(md5Bytes, 4, 8);
            str = str.Replace("-", "");
            return str;
        }

    }
}
