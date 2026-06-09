using System.Security.Cryptography;
using System.Text;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class CrossPlatformCredentialProtector : ICredentialProtector
{
    private static readonly byte[] Salt = "SimpleLauncher.Salt"u8.ToArray();

    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var key = DeriveKey();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
        return Convert.ToBase64String(result);
    }

    public string Unprotect(string protectedData)
    {
        if (string.IsNullOrEmpty(protectedData))
            return string.Empty;

        try
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            var key = DeriveKey();
            using var aes = Aes.Create();
            aes.Key = key;
            var iv = new byte[16];
            Buffer.BlockCopy(protectedBytes, 0, iv, 0, 16);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = new byte[protectedBytes.Length - 16];
            Buffer.BlockCopy(protectedBytes, 16, encryptedBytes, 0, encryptedBytes.Length);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static byte[] DeriveKey()
    {
        var machineId = $"{Environment.MachineName}:{Environment.UserName}";
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(machineId), Salt, 10000, HashAlgorithmName.SHA256, 32);
    }
}
