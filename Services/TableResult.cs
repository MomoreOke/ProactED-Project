using System.Collections.Generic;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Represents a table extracted from a document.
    /// </summary>
    public class TableResult
    {
        /// <summary>
        /// Number of rows in the table.
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Number of columns in the table.
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Cells contained in the table.
        /// </summary>
        public List<TableCell> Cells { get; set; } = new List<TableCell>();
    }

    /// <summary>
    /// Represents an individual cell in a table extracted from a document.
    /// </summary>
    public class TableCell
    {
        /// <summary>
        /// Zero-based row index of the cell.
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Zero-based column index of the cell.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// The text content of the cell.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
