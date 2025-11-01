using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using DataObjects;

namespace LogicLayer
{
    /// <summary>
    /// Manager for encrypting and decrypting sensitive configuration data
    /// </summary>
    public class EncryptionManager : IEncryptionManager
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly ILogger<EncryptionManager>? _logger;

        public EncryptionManager(IConfiguration configuration, ILogger<EncryptionManager>? logger = null)
        {
            _logger = logger;

            // Get encryption key from configuration or generate a default one
            var keyString = configuration["ENCRYPTION_KEY"];
            var ivString = configuration["ENCRYPTION_IV"];

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
            {
                _logger?.LogWarning("Encryption key or IV not found in configuration. Using default values. This is NOT secure for production!");

                // Default key and IV - DO NOT use in production
                _key = SHA256.HashData(Encoding.UTF8.GetBytes("BactaBot-Default-Key-Change-This"));
                _iv = MD5.HashData(Encoding.UTF8.GetBytes("BactaBot-Default-IV"));
            }
            else
            {
                _key = Convert.FromBase64String(keyString);
                _iv = Convert.FromBase64String(ivString);

                if (_key.Length != 32)
                {
                    throw new ArgumentException("Encryption key must be 32 bytes (256 bits) when base64 decoded");
                }

                if (_iv.Length != 16)
                {
                    throw new ArgumentException("Encryption IV must be 16 bytes (128 bits) when base64 decoded");
                }
            }
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);

                swEncrypt.Write(plainText);
                swEncrypt.Close();

                var encrypted = msEncrypt.ToArray();
                var result = Convert.ToBase64String(encrypted);

                _logger?.LogDebug("Successfully encrypted data of length {Length}", plainText.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to encrypt data");
                throw new EncryptionException("Failed to encrypt data", ex);
            }
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));
            }

            try
            {
                var cipherBytes = Convert.FromBase64String(encryptedText);

                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                var result = srDecrypt.ReadToEnd();

                _logger?.LogDebug("Successfully decrypted data");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to decrypt data");
                throw new EncryptionException("Failed to decrypt data", ex);
            }
        }

        public bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                // Check if it's valid Base64 and has reasonable length for encrypted data
                var bytes = Convert.FromBase64String(value);
                return bytes.Length >= 16; // Minimum length for AES encrypted data
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a new random encryption key (32 bytes for AES-256)
        /// </summary>
        /// <returns>Base64 encoded encryption key</returns>
        public static string GenerateEncryptionKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[32]; // 256 bits
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }

        /// <summary>
        /// Generates a new random initialization vector (16 bytes for AES)
        /// </summary>
        /// <returns>Base64 encoded IV</returns>
        public static string GenerateIV()
        {
            using var rng = RandomNumberGenerator.Create();
            var ivBytes = new byte[16]; // 128 bits
            rng.GetBytes(ivBytes);
            return Convert.ToBase64String(ivBytes);
        }
    }
}