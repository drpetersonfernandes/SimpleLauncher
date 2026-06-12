using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// No-op implementation of <see cref="ICredentialProtector"/> for unit tests.
/// Protect and Unprotect return the input unchanged (identity transform).
/// </summary>
public class NoOpCredentialProtector : ICredentialProtector
{
    /// <summary>
    /// Returns the plaintext string unchanged.
    /// </summary>
    /// <param name="plaintext">The plaintext string to protect.</param>
    /// <returns>The same <paramref name="plaintext"/> value without encryption.</returns>
    public string Protect(string plaintext)
    {
        return plaintext;
    }

    /// <summary>
    /// Returns the protected data string unchanged.
    /// </summary>
    /// <param name="protectedData">The protected data to unprotect.</param>
    /// <returns>The same <paramref name="protectedData"/> value without decryption.</returns>
    public string Unprotect(string protectedData)
    {
        return protectedData;
    }
}
