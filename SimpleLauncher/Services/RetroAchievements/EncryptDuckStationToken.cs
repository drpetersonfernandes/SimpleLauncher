using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.RetroAchievements;

public class EncryptDuckStationToken
{
    public static string EncryptDuckStationTokenMethod(string token, string username, bool isPortable)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username)) return string.Empty;

        try
        {
            var key = GetDuckStationEncryptionKey(username, isPortable);

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros; // DuckStation uses zero padding (aligns to block size with 0s)

            // Key is first 16 bytes, IV is last 16 bytes of the 32-byte derived key
            var aesKey = new byte[16];
            var aesIv = new byte[16];
            Array.Copy(key, 0, aesKey, 0, 16);
            Array.Copy(key, 16, aesIv, 0, 16);

            aes.Key = aesKey;
            aes.IV = aesIv;

            var tokenBytes = Encoding.UTF8.GetBytes(token);

            using var encryptor = aes.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(tokenBytes, 0, tokenBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to encrypt DuckStation token.");
            return string.Empty;
        }
    }

    private static byte[] GetDuckStationEncryptionKey(string username, bool isPortable)
    {
        var inputBytes = new List<byte>();

        // Only use machine key if not portable and on Windows
        if (!isPortable && System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            var machineGuid = GetWindowsMachineGuid();
            if (!string.IsNullOrEmpty(machineGuid))
            {
                inputBytes.AddRange(Encoding.UTF8.GetBytes(machineGuid));
            }
        }

        inputBytes.AddRange(Encoding.UTF8.GetBytes(username));

        var key = SHA256.HashData(inputBytes.ToArray());

        // Extra rounds (100)
        for (var i = 0; i < 100; i++)
        {
            key = SHA256.HashData(key);
        }

        return key;
    }

    private static string GetWindowsMachineGuid()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid") as string;
        }
        catch
        {
            return null;
        }
    }
}