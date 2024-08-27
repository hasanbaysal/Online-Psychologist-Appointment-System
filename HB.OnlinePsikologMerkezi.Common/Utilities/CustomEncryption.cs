using System.Security.Cryptography;

namespace HB.OnlinePsikologMerkezi.Common.Utilities
{



    public static class CustomEncryption
    {

        private static byte[] _key = null!;
        private static byte[] _iv = null!;


        private static List<byte> listKey = new();
        private static List<byte> listIv = new();

        //configuration üzerinden
        //key ve iv değerlerini string array olarak alıp buraya gönder
        public static void LoadKeyAndIv(string[] arrKey, string[] arrIv)
        {

            arrKey!.ToList().ForEach(x =>
            {
                listKey.Add(byte.Parse(x.Split("x")[1], System.Globalization.NumberStyles.HexNumber));
            });

            arrIv!.ToList().ForEach(x =>
            {
                listIv.Add(byte.Parse(x.Split("x")[1], System.Globalization.NumberStyles.HexNumber));
            });

            _key = listKey.ToArray();
            _iv = listIv.ToArray();
        }






        public static string Encrypt(string text)
        {

            if (string.IsNullOrEmpty(text))
            {
                return text;
            }



            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(text);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }




    }
}
