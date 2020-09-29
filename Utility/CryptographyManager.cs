using HashLib;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Winter.Utility
{

    /// <summary>
    /// </summary>
    public static class CryptographyManager
    {
        private const string HexAlphabet = "0123456789abcdef";

        public static byte[] SH224(string str)
        {
            IHash hash = HashFactory.Crypto.CreateSHA224();
            HashResult r = hash.ComputeString(str, System.Text.Encoding.UTF8);


            return r.GetBytes();
        }
        public static string ToHexString(this byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);

            foreach (var b in bytes)
            {
                result.Append(HexAlphabet[b >> 4]);
                result.Append(HexAlphabet[b & 0xF]);
            }

            return result.ToString();
        }
    

    private static readonly byte[] _slat =
        {
            83,
            110,
            100,
            97,
            32,
            67,
            82,
            77,
            32,
            88,
            117,
            97,
            110,
            121,
            101
        };
        public static string Sha1Encrypt(string encryptingString)
        {
            var buffer = Encoding.UTF8.GetBytes(encryptingString);
            var data = SHA1.Create().ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
        public static string Md5Encrypt(string encryptingString)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(encryptingString));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static string AESEncrypt(string toEncrypt, string password)
        {
            var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, _slat, 1024);
            return AESEncrypt(toEncrypt, rfc2898DeriveBytes.GetBytes(32), rfc2898DeriveBytes.GetBytes(16));
        }

        public static string AESEncrypt(string toEncrypt, byte[] keyArray, byte[] ivArray)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(toEncrypt);
            var cryptoTransform = Aes.Create().CreateEncryptor(keyArray, ivArray);

            byte[] array = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(array, 0, array.Length);
        }

        public static Rfc2898DeriveBytes RFCDB(string password)
        {
            return new Rfc2898DeriveBytes(password, _slat, 1024);
        }

        public static string AESDecrypt(string toDecrypt, string password)
        {
            var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, _slat, 1024);
            return AESDecrypt(toDecrypt, rfc2898DeriveBytes.GetBytes(32), rfc2898DeriveBytes.GetBytes(16));
        }

        public static string AESDecrypt(string toDecrypt, byte[] keyArray, byte[] ivArray)
        {
            byte[] array = Convert.FromBase64String(toDecrypt);
            var cryptoTransform = Aes.Create().CreateEncryptor(keyArray, ivArray);

            byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
            string @string = Encoding.UTF8.GetString(bytes);
            return @string.Replace("\0", "");
        }

         
        public static string Sha1(string plainText)
        {
            using (var sha1 = SHA1.Create())
            {
                var myByteArray = Encoding.UTF8.GetBytes(plainText);
                var hash = sha1.ComputeHash(myByteArray);

                StringBuilder EnText = new StringBuilder();
                foreach (byte iByte in hash)
                {
                    EnText.AppendFormat("{0:x2}", iByte);
                }
                return EnText.ToString();
            }
             
        }

        /// <summary>
        ///  基于Sha256的自定义加密字符串方法：返回64位十六进制字符串 （256位）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Sha256Encrypt(string str)
        {
            var hashAlgorithm = new SHA256CryptoServiceProvider();//SHA256.Create();
            return HashAlgorithm(str, hashAlgorithm);
        }

        /// <summary>
        /// 基于Sha512的自定义加密字符串方法：返回128位十六进制字符串 （512位）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Sha512Encrypt(string str)
        {
            var hashAlgorithm = new SHA512CryptoServiceProvider();//SHA512.Create();
            return HashAlgorithm(str, hashAlgorithm);
        }
        private static string HashAlgorithm(string str, HashAlgorithm hashAlgorithm)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = hashAlgorithm.ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in data)
            {
                //格式每一个十六进制字符串
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }

        #region 微信小程序数据解密 

        /// <summary>
        /// 微信小程序数据解密
        /// </summary>
        /// <param name="encryptedDataStr"></param>
        /// <param name="sessionKey"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string MPDecrypt(string encryptedDataStr, string sessionKey, string iv)
        {
            RijndaelManaged rijalg = new RijndaelManaged();
            //-----------------    
            //设置 cipher 格式 AES-128-CBC    

            rijalg.KeySize = 128;

            rijalg.Padding = PaddingMode.PKCS7;
            rijalg.Mode = CipherMode.CBC;

            rijalg.Key = Convert.FromBase64String(sessionKey);
            rijalg.IV = Convert.FromBase64String(iv);


            byte[] encryptedData = Convert.FromBase64String(encryptedDataStr);
            //解密    
            ICryptoTransform decryptor = rijalg.CreateDecryptor(rijalg.Key, rijalg.IV);

            string result;

            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        result = srDecrypt.ReadToEnd();
                    }
                }
            }

            return result;

        }

        #endregion

    }
}