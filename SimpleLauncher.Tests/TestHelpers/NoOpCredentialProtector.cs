using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpCredentialProtector : ICredentialProtector
{
    public string Protect(string plaintext)
    {
        return plaintext;
    }

    public string Unprotect(string protectedData)
    {
        return protectedData;
    }
}
