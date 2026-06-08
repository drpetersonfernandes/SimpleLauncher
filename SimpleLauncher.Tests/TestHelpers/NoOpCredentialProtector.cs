using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpCredentialProtector : ICredentialProtector
{
    public string Protect(string plaintext) => plaintext;

    public string Unprotect(string protectedData) => protectedData;
}
