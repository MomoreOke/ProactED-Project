using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Stub implementation of IFormRecognizerService that returns empty results.
    /// Used when Azure Form Recognizer is not configured.
    /// </summary>
    public class StubFormRecognizerService : IFormRecognizerService
    {
        /// <inheritdoc />
        public Task<List<TableResult>> ExtractTablesAsync(Stream stream)
        {
            // Return empty list when service is not configured
            return Task.FromResult(new List<TableResult>());
        }
    }
}
