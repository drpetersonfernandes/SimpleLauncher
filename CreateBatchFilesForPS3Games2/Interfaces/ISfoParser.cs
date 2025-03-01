using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CreateBatchFilesForPS3Games2.Interfaces
{
    public interface ISfoParser
    {
        Task<Dictionary<string, string>?> ParseSfoFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}