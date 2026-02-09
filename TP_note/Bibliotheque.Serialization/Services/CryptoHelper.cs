using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace TP_note.Bibliotheque.Serialization.Services
{
    public static class CryptoHelper
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;

    /// <summary>
    /// Chiffre les données en utilisant AES-256 et sauvegarde le résultat dans un fichier avec salt et IV.
    /// </summary>
    /// <param name="filePath">Chemin du fichier de destination</param>
    /// <param name="writePlainData">Action delegate pour écrire les données non chiffrées dans le flux</param>
    /// <param name="password">Mot de passe optionnel (utilise le SID Windows par défaut)</param>
    public static void EncryptToFile(string filePath, Action<Stream> writePlainData, string password = null)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (writePlainData == null) throw new ArgumentNullException(nameof(writePlainData));

            var keyMaterial = GetKeyMaterial(password);
            var salt = GenerateRandomBytes(SaltSize);
            var iv = GenerateRandomBytes(16);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(salt, 0, salt.Length);
                fs.Write(iv, 0, iv.Length);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = DeriveKey(keyMaterial, salt);
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var cryptoStream = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        writePlainData(cryptoStream);
                        cryptoStream.FlushFinalBlock();
                    }
                }
            }
        }

    /// <summary>
    /// Déchiffre un fichier chiffré avec AES-256 et traite les données déchiffrées.
    /// </summary>
    /// <param name="filePath">Chemin du fichier à déchiffrer</param>
    /// <param name="readPlainData">Action delegate pour lire les données déchiffrées depuis le flux</param>
    /// <param name="password">Mot de passe optionnel (utilise le SID Windows par défaut)</param>
    public static void DecryptFromFile(string filePath, Action<Stream> readPlainData, string password = null)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (readPlainData == null) throw new ArgumentNullException(nameof(readPlainData));

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var salt = new byte[SaltSize];
                if (fs.Read(salt, 0, salt.Length) != salt.Length)
                    throw new InvalidOperationException("Fichier corrompu (salt).");

                var iv = new byte[16];
                if (fs.Read(iv, 0, iv.Length) != iv.Length)
                    throw new InvalidOperationException("Fichier corrompu (IV).");

                var keyMaterial = GetKeyMaterial(password);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = DeriveKey(keyMaterial, salt);
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var cryptoStream = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        readPlainData(cryptoStream);
                    }
                }
            }
        }

        // Retourne le matériau de clé : le mot de passe fourni ou le SID de l'utilisateur Windows.
        private static byte[] GetKeyMaterial(string password)
        {
            if (!string.IsNullOrEmpty(password))
                return Encoding.UTF8.GetBytes(password);

            var sid = WindowsIdentity.GetCurrent().User?.Value ?? "DefaultSid";
            return Encoding.UTF8.GetBytes(sid);
        }

        // Dérive une clé AES-256 à partir du matériau de clé et du salt en utilisant PBKDF2 avec 10000 itérations.
        private static byte[] DeriveKey(byte[] keyMaterial, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(keyMaterial, salt, 10000))
            {
                return deriveBytes.GetBytes(KeySize);
            }
        }

        // Génère un tableau d'octets aléatoires de la longueur spécifiée en utilisant un générateur cryptographiquement sécurisé.
        private static byte[] GenerateRandomBytes(int length)
        {
            var buffer = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return buffer;
        }
    }
}
