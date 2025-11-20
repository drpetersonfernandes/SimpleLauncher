using System;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

public interface ILogErrors
{
    Task LogErrorAsync(Exception ex, string contextMessage = null);
}