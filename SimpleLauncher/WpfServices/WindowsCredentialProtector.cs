using System.Security.Cryptography;
using System.Text;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

/// <summary>
/// Windows-specific implementation of <see cref="ICredentialProtector"/> using DPAPI (Data Protection API).
/// </summary>
public class WindowsCredentialProtector : ICredentialProtector
{
    private static readonly byte[] Entropy = "SimpleLauncher.Salt"u8.ToArray();

    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var protectedBytes = ProtectedData.Protect(plaintextBytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public string Unprotect(string protectedData)
    {
        if (string.IsNullOrEmpty(protectedData))
            return string.Empty;

        try
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            var plaintextBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintextBytes);
        }
        catch (CryptographicException)
        {
            // Data may be corrupted or from a different user/machine
            return string.Empty;
        }
    }
}
