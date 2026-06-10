namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides platform-agnostic credential protection for sensitive data like passwords and tokens.
/// </summary>
public interface ICredentialProtector
{
    /// <summary>
    /// Protects (encrypts) the specified plaintext data.
    /// </summary>
    /// <param name="plaintext">The plaintext data to protect.</param>
    /// <returns>The protected (encrypted) data as a Base64-encoded string.</returns>
    string Protect(string plaintext);

    /// <summary>
    /// Unprotects (decrypts) the specified protected data.
    /// </summary>
    /// <param name="protectedData">The protected (encrypted) data as a Base64-encoded string.</param>
    /// <returns>The unprotected (decrypted) plaintext data.</returns>
    string Unprotect(string protectedData);
}
