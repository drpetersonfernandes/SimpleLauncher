using System;
using System.Threading;
using System.Threading.Tasks;
using CreateBatchFilesForPS3Games.Models;

namespace CreateBatchFilesForPS3Games.Interfaces
{
    public interface IBatchFileService
    {
        Task<int> CreateBatchFilesAsync(
            BatchCreationOptions options,
            IProgress<BatchCreationProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}