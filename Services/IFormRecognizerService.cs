using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Service interface for extracting tables from documents using Azure Form Recognizer.
    /// </summary>
    public interface IFormRecognizerService
    {
        /// <summary>
        /// Extracts tables from the given document stream.
        /// </summary>
        /// <param name="stream">The document stream (PDF, image, etc.).</param>
        /// <returns>A list of table results with row and column data.</returns>
        Task<List<TableResult>> ExtractTablesAsync(Stream stream);
    }
}
