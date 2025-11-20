using System;
using System.Threading.Tasks;

namespace SimpleLauncher.Interfaces;

public interface ILogErrors
{
    Task LogErrorAsync(Exception ex, string contextMessage = null);
}