using System.Security.Cryptography;
using System.Text;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.WpfServices;

/// <summary>
/// Windows-specific implementation of <see cref="ICredentialProtector"/> using DPAPI (Data Protection API).
/// </summary>
public class WindowsCredentialProtector : ICredentialProtector
{
    private static readonly byte[] Entropy = "SimpleLauncher.Salt"u8.ToArray();

    /// <summary>Encrypts the specified plaintext using DPAPI.</summary>
    /// <param name="plaintext">The plaintext string to protect.</param>
    /// <returns>A Base64-encoded string of the encrypted data.</returns>
    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return "";

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var protectedBytes = ProtectedData.Protect(plaintextBytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    /// <summary>Decrypts the specified DPAPI-protected data.</summary>
    /// <param name="protectedData">The Base64-encoded encrypted string to unprotect.</param>
    /// <returns>The decrypted plaintext, or <c>null</c> if decryption fails.</returns>
    public string? Unprotect(string protectedData)
    {
        if (string.IsNullOrEmpty(protectedData))
            return "";

        try
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            var plaintextBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintextBytes);
        }
        catch (CryptographicException)
        {
            // Data may be corrupted or from a different user/machine
            return null;
        }
    }
}
