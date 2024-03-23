using System;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers
{
    [PluginController("Eshoppgsoftweb")]
    public class EshoppgsoftwebApiController : _BaseApiController
    {
        // ~/Naplnspajzu/ContactFormApiKey
        public ApiKeyPair ContactFormApiKey()
        {
            return new ApiKeyValidator().GetNewKeyPair();
        }
    }

    public class ApiKeyValidator
    {
        string key1 { get; set; } = "ajHsy478$!jds7^hskasdiu&42b";
        string key2 { get; set; } = "dHGteu4^*@jdskjdUJ738jas)ah";

        int ticksOffset = 12345;

        public ApiKeyPair GetNewKeyPair()
        {
            DateTime now = DateTime.Now;
            DateTime later = now.AddMilliseconds(ticksOffset);

            ApiKeyPair ret = new ApiKeyPair()
            {
                MainKey = Encrypt(now.Ticks.ToString(), key1),
                SubKey = Encrypt(later.Ticks.ToString(), key2),
            };

            return ret;
        }

        public bool IsValid(string apiKey1, string apiKey2)
        {
            if (string.IsNullOrEmpty(apiKey1) || string.IsNullOrEmpty(apiKey2))
            {
                return false;
            }
            try
            {
                return IsValid(new ApiKeyPair() { MainKey = apiKey1, SubKey = apiKey2 });
            }
            catch
            {
                return false;
            }
        }
        public bool IsValid(ApiKeyPair keyPair)
        {
            string now = Decrypt(keyPair.MainKey, key1);
            string later = Decrypt(keyPair.SubKey, key2);

            long ticksNow, ticksLater;
            if (!long.TryParse(now, out ticksNow) || !long.TryParse(later, out ticksLater))
            {
                return false;
            }

            DateTime dtNow = new DateTime(ticksNow).AddMilliseconds(ticksOffset);
            DateTime dtLater = new DateTime(ticksLater);
            if (dtNow != dtLater)
            {
                return false;
            }

            if (dtNow < DateTime.Now.AddHours(-5))
            {
                return false;
            }

            return true;
        }

        public string Encrypt(string text, string key)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public string Decrypt(string cipher, string key)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }
    }

    public class ApiKeyPair
    {
        public string MainKey { get; set; }
        public string SubKey { get; set; }
    }
}
