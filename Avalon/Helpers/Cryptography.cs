using Avalon.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Avalon.Helpers
{
    // The Cryptography code used here is addapted from this video and code.
    // https://www.youtube.com/watch?v=rLEJLuA3hd0
    // Code sample in zip file - https://tinyurl.com/y3jzbq2m

    public class Cryptography : ICryptography
    {
        private readonly string _key;
        private readonly string _iv;

        public Cryptography(IConfiguration config)
        {
            _key = config.GetValue<string>("Cryptography_Key");
            _iv = config.GetValue<string>("Cryptography_IV");
        }

        public string Encrypt(string PlainText)
        {
            Aes cipher = this.CreateCipher();

            //Create the encryptor, convert to bytes, and encrypt
            ICryptoTransform cryptTransform = cipher.CreateEncryptor();
            byte[] plaintext = Encoding.UTF8.GetBytes(PlainText);
            byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

            //Convert to base64 for display
            return Convert.ToBase64String(cipherText);
        }

        public string Decrypt(string CipherText)
        {
            Aes cipher = this.CreateCipher();

            //Create the decryptor, convert from base64 to bytes, decrypt
            ICryptoTransform cryptTransform = cipher.CreateDecryptor();
            byte[] cipherText = Convert.FromBase64String(CipherText);
            byte[] plainText = cryptTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(plainText);
        }
        private Aes CreateCipher()
        {
            Aes cipher = Aes.Create();  //Defaults - Keysize 256, Mode CBC, Padding PKC27
            cipher.Padding = PaddingMode.ISO10126;

            cipher.IV = Convert.FromBase64String(_iv);
            cipher.Key = this.HexToByteArray(_key);

            return cipher;
        }

        private byte[] HexToByteArray(string hexString)
        {
            if (0 != (hexString.Length % 2))
            {
                throw new ApplicationException("Hex string must be multiple of 2 in length");
            }

            int byteCount = hexString.Length / 2;
            byte[] byteValues = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                byteValues[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return byteValues;
        }

        // Below code is taken from this Microsoft documentation.
        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netcore-3.1

        //public void trythisout()
        //{
        //    string original = "Here is some data to encrypt!";

        //    // Create a new instance of the Aes
        //    // class.  This generates a new key and initialization
        //    // vector (IV).
        //    using (Aes myAes = Aes.Create())
        //    {
        //        // Encrypt the string to an array of bytes.
        //        byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

        //        // Decrypt the bytes to a string.
        //        string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

        //        //Display the original data and the decrypted data.
        //        Console.WriteLine("Original:   {0}", original);
        //        Console.WriteLine("Round Trip: {0}", roundtrip);
        //    }
        //}

        //private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        //{
        //    // Check arguments.
        //    if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException("plainText");
        //    if (Key == null || Key.Length <= 0) throw new ArgumentNullException("Key");
        //    if (IV == null || IV.Length <= 0) throw new ArgumentNullException("IV");

        //    byte[] encrypted;

        //    // Create an Aes object
        //    // with the specified key and IV.
        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.Key = Key;
        //        aesAlg.IV = IV;

        //        // Create an encryptor to perform the stream transform.
        //        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        // Create the streams used for encryption.
        //        using (MemoryStream msEncrypt = new MemoryStream())
        //        {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        //                {
        //                    //Write all data to the stream.
        //                    swEncrypt.Write(plainText);
        //                }
        //                encrypted = msEncrypt.ToArray();
        //            }
        //        }
        //    }

        //    // Return the encrypted bytes from the memory stream.
        //    return encrypted;
        //}

        //private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        //{
        //    // Check arguments.
        //    if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException("cipherText");
        //    if (Key == null || Key.Length <= 0) throw new ArgumentNullException("Key");
        //    if (IV == null || IV.Length <= 0) throw new ArgumentNullException("IV");

        //    // Declare the string used to hold
        //    // the decrypted text.
        //    string plaintext = null;

        //    // Create an Aes object
        //    // with the specified key and IV.
        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.Key = Key;
        //        aesAlg.IV = IV;

        //        // Create a decryptor to perform the stream transform.
        //        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        //        // Create the streams used for decryption.
        //        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        //        {
        //            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        //                {

        //                    // Read the decrypted bytes from the decrypting stream
        //                    // and place them in a string.
        //                    plaintext = srDecrypt.ReadToEnd();
        //                }
        //            }
        //        }
        //    }

        //    return plaintext;
        //}
    }
}
