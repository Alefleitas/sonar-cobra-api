using System;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace nordelta.cobra.webapi.Utils
{
    public static class AesManager
    {
        public static string GetConnectionString(string connectionString, string JwtKey)
        {
            try
            {
                var passEncript = connectionString.Split(';')[3];
                passEncript = passEncript[(passEncript.IndexOf("=") + 1)..];
                var decrypted = passEncript.Split('.')[0];
                var iv = passEncript.Split('.')[1];
                var pass = Decrypt(decrypted, JwtKey, iv);

                return connectionString.Replace(passEncript, pass);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error al momento de desencriptar la contraseña.");
            }
            return String.Empty;
        }

        public static string GetPassword(string password, string secretKey)
        {
            try
            {
                var decrypted = password.Split('.')[0];
                var iv = password.Split('.')[1];
                var pass = Decrypt(decrypted, secretKey, iv);

                return pass;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error al momento de desencriptar la contraseña.");
            }
            return String.Empty;
        }

        public static Aes GetAES(String secretKey)
        {
            int keysize = 256;

            var keyBytes = new byte[keysize / 8];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));

            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = keysize;
            aes.BlockSize = 128;//AES es siempre 128 el blocksize
            aes.Key = keyBytes;
            aes.GenerateIV();

            return aes;
        }

        private static byte[] Encrypt(byte[] plainBytes, Aes aes)
        {
            byte[] result;
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var resultStream = new MemoryStream())
            {
                using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                using (var plainStream = new MemoryStream(plainBytes))
                {
                    plainStream.CopyTo(aesStream);
                }

                return result = resultStream.ToArray();
            }
        }

        private static byte[] Decrypt(byte[] encryptedData, Aes aes)
        {
            return aes.CreateDecryptor()
                .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }

        // Encrypts plaintext using AES 128bit key and a Chain Block Cipher and returns a base64 encoded string
        public static dynamic Encrypt(String plainText, String key)
        {
            var aes = GetAES(key);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            var ret = new
            {
                encrypted = Convert.ToBase64String(Encrypt(plainBytes, aes)),
                iv = Convert.ToBase64String(aes.IV)
            };
            return ret;
        }

        public static String Decrypt(String encryptedText, String key, String iv)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var aes = GetAES(key);
            aes.IV = Convert.FromBase64String(iv);
            return Encoding.UTF8.GetString(Decrypt(encryptedBytes, aes));
        }

        public static dynamic EncryptForUrl(String plainText, String key)
        {
            var aes = GetAES(key);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            var ret = new
            {
                encrypted = Convert.ToBase64String(Encrypt(plainBytes, aes)).Replace('+', '-').Replace('/', '_'),
                iv = Convert.ToBase64String(aes.IV).Replace('+', '-').Replace('/', '_')
            };
            return ret;
        }

        public static dynamic DecryptFromUrl(String encryptedText, String key, String iv)
        {
            try
            {
                encryptedText = encryptedText.Replace('-', '+').Replace('_', '/');
                iv = iv.Replace('-', '+').Replace('_', '/');
                return Decrypt(encryptedText, key, iv);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error al momento de desencriptar la contraseña.");
            }
            return String.Empty;
        }
    }
}
