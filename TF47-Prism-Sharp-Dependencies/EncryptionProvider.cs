using System;
using System.IO;
using System.Security.Cryptography;

namespace TF47_Prism_Sharp_Dependencies
{
    public class EncryptionProvider
    {
        private readonly Aes _aes;

        public EncryptionProvider()
        {
            _aes = Aes.Create();
        }

        public EncryptionProvider(string key)
        {
            _aes = Aes.Create();

            var keyAndIv = key.Split('_');
            _aes.Key = Convert.FromBase64String(keyAndIv[0]);
            _aes.IV = Convert.FromBase64String(keyAndIv[1]);
        }

        public EncryptionProvider CreateKey()
        {
            _aes.GenerateIV();
            _aes.GenerateKey();
            return this;
        }

        public string Decrypt(byte[] data)
        {
            var decryptor = _aes.CreateDecryptor();

            var decryptedString = string.Empty;

            using var memoryStream = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            decryptedString = streamReader.ReadToEnd();

            return decryptedString;
        }

        public byte[] Encrypt(string data)
        {
            byte[] encryptedData;
            var encryptor = _aes.CreateEncryptor();

            // Create the streams used for encryption.
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                //Write all data to the stream.
                streamWriter.Write(data);
            }
            return memoryStream.ToArray();
        }

        //key is encoded as base64
        public string GetKey()
        {
            return $"{Convert.ToBase64String(_aes.Key)}_{Convert.ToBase64String(_aes.IV)}";
        }
    }
}