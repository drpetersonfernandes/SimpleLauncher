using System;
using System.Threading;
using System.Threading.Tasks;
using CreateBatchFilesForPS3Games2.Models;

namespace CreateBatchFilesForPS3Games2.Interfaces
{
    public interface IBatchFileService
    {
        Task<int> CreateBatchFilesAsync(
            BatchCreationOptions options,
            IProgress<BatchCreationProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}