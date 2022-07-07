using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// A very primitive HTML table printer
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Table body
        /// </summary>
        private readonly List<TableCell[]> rows;
        /// <summary>
        /// Header row
        /// </summary>
        /// <remarks>Yes this only supports one header row</remarks>
        private TableCell[] header;

        /// <summary>
        /// Table columns
        /// </summary>
        /// <remarks>This is not really validated anymore except for the header row</remarks>
        public int Columns { get; private set; }

        /// <summary>
        /// Gets a copy of the table header row
        /// </summary>
        public TableCell[] Headers { get => (TableCell[])header.Clone(); }

        /// <summary>
        /// Creates a new table with the given columns
        /// </summary>
        /// <param name="Columns">Table column count</param>
        public Table(int Columns)
        {
            if (Columns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Columns));
            }
            rows = new List<TableCell[]>();
            this.Columns = Columns;
        }

        /// <summary>
        /// Sets new column count
        /// </summary>
        /// <param name="columns">Column count</param>
        /// <remarks>This will clear the table if the count differs from the current count</remarks>
        public void SetColumns(int columns)
        {
            if (columns == Columns)
            {
                return;
            }
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }
            Columns = columns;
            rows.Clear();
            header = null;
        }

        /// <summary>
        /// Sets the header row
        /// </summary>
        /// <param name="Fields">Header row fields</param>
        /// <remarks>
        /// The total field count must match <see cref="Columns"/>,
        /// but this function properly handles the <see cref="TableCell.Colspan"/> value
        /// </remarks>
        public void SetHeader(IEnumerable<TableCell> Fields)
        {
            if (Fields is null)
            {
                throw new ArgumentNullException(nameof(Fields));
            }
            var Entries = Fields.ToArray();
            var colspan = Entries.Sum(m => m.Colspan);
            if (Entries.Any(m => m.Rowspan != 1))
            {
                throw new ArgumentException($"Headers canot have a non-default {nameof(TableCell.Rowspan)} property");
            }
            if (colspan != Columns)
            {
                throw new ArgumentException($"Header doesn't has {Columns} entries but {colspan}");
            }
            header = Entries;
        }

        /// <summary>
        /// Sets the header row
        /// </summary>
        /// <param name="Fields">Header row fields</param>
        public void SetHeader(IEnumerable<string> Fields)
        {
            SetHeader(Fields.Select(m => new TableCell(m)));
        }

        /// <summary>
        /// Adds a row to the table body
        /// </summary>
        /// <param name="Values">Row cells</param>
        public void AddRow(params TableCell[] Values)
        {
            if (Values is null)
            {
                throw new ArgumentNullException(nameof(Values));
            }
            rows.Add(Values);
        }

        /// <summary>
        /// Renders the table as HTML
        /// </summary>
        /// <returns>HTML table string</returns>
        public string Render()
        {
            using (var SW = new StringWriter())
            {
                Render(SW);
                return SW.ToString();
            }
        }

        /// <summary>
        /// Renders the table onto the given output
        /// </summary>
        /// <param name="output">Output</param>
        public void Render(TextWriter output)
        {
            output.Write("<table border=1><thead><tr>");
            foreach (var Entry in header)
            {
                output.Write(Entry.Render(true));
            }
            output.Write("</tr></thead>\n<tbody>");
            foreach (var row in rows)
            {
                output.Write("<tr>");
                foreach (var Entry in row)
                {
                    output.Write(Entry.Render(false));
                }
                output.Write("</tr>\n");
            }
            output.Write("</tbody></table>");
        }
    }

    /// <summary>
    /// Represents a table cell
    /// </summary>
    public class TableCell
    {
        /// <summary>
        /// Column span (horizontal)
        /// </summary>
        public int Colspan { get; set; }
        /// <summary>
        /// Row span (vertical)
        /// </summary>
        public int Rowspan { get; set; }
        /// <summary>
        /// Cell content
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new cell
        /// </summary>
        /// <param name="Value">Text content</param>
        /// <param name="Colspan">Column span</param>
        /// <param name="Rowspan">Row span</param>
        public TableCell(object Value = null, int Colspan = 1, int Rowspan = 1)
        {
            if (Value == null)
            {
                this.Value = null;
            }
            else
            {
                this.Value = Value.ToString();
            }
            if (Colspan < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Colspan));
            }
            if (Rowspan < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Rowspan));
            }
            this.Colspan = Colspan;
            this.Rowspan = Rowspan;
        }

        /// <summary>
        /// Renders the cell as string
        /// </summary>
        /// <param name="AsHeader">Use TH instead of TD</param>
        /// <returns>HTML</returns>
        /// <remarks>This does minimal HTML escaping</remarks>
        public string Render(bool AsHeader = false)
        {
            var open = AsHeader ? "<th" : "<td";

            if (Colspan > 1)
            {
                open += $" colspan=\"{Colspan}\"";
            }
            if (Rowspan > 1)
            {
                open += $" rowspan=\"{Rowspan}\"";
            }

            return
                open + ">" +
                (string.IsNullOrEmpty(Value) ? "" : Escape(Value)) +
                (AsHeader ? "</th>" : "</td>");
        }

        /// <summary>
        /// Escapes text to be HTML safe
        /// </summary>
        /// <param name="Input">Text</param>
        /// <returns>Escaped text</returns>
        /// <remarks>
        /// The result should be safe to put anywhere in a HTML document.
        /// Be aware that this replaces linebreaks with BR tags.
        /// Do not use this function in your projects for HTML attribute values.
        /// In that case, remove the line break replacement.
        /// </remarks>
        private static string Escape(string Input)
        {
            return Input
                //required HTML escape sequences
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")
                //Replace line breaks with <br />
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Replace("\n", "<br />\n");
        }
    }
}
