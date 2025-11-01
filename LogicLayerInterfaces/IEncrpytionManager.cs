namespace LogicLayerInterfaces
{
    /// <summary>
    /// Interface for encryption and decryption operations
    /// </summary>
    public interface IEncryptionManager
    {
        /// <summary>
        /// Encrypts a plaintext string
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted string</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts an encrypted string
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string</param>
        /// <returns>Decrypted plaintext</returns>
        string Decrypt(string encryptedText);

        /// <summary>
        /// Checks if a string appears to be encrypted (Base64 format)
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value appears encrypted</returns>
        bool IsEncrypted(string value);
    }
}