using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Implementation of IFormRecognizerService using Azure Form Recognizer.
    /// </summary>
    public class FormRecognizerService : IFormRecognizerService
    {
        private readonly DocumentAnalysisClient _client;

        public FormRecognizerService(DocumentAnalysisClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public async Task<List<TableResult>> ExtractTablesAsync(Stream stream)
        {
            var tables = new List<TableResult>();
            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
            var result = operation.Value;

            foreach (var table in result.Tables)
            {
                var tableResult = new TableResult
                {
                    RowCount = table.RowCount,
                    ColumnCount = table.ColumnCount,
                    Cells = new List<TableCell>()
                };

                foreach (var cell in table.Cells)
                {
                    tableResult.Cells.Add(new TableCell
                    {
                        RowIndex = cell.RowIndex,
                        ColumnIndex = cell.ColumnIndex,
                        Content = cell.Content
                    });
                }

                tables.Add(tableResult);
            }

            return tables;
        }
    }
}
