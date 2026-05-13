using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace _Project.Code.Editor.SpreadsheetImporter
{
    /// <summary>
    /// Parses .xlsx files without external dependencies using System.IO.Compression + System.Xml.Linq.
    /// Returns data as: sheetName -> list of rows, each row is a column-name -> value dictionary.
    /// The first row of each sheet is treated as the header row.
    /// </summary>
    public static class XlsxReader
    {
        private static readonly XNamespace SpreadsheetNs =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        private static readonly XNamespace RelationshipsNs =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        private static readonly XNamespace PackageRelNs =
            "http://schemas.openxmlformats.org/package/2006/relationships";

        public static Dictionary<string, List<Dictionary<string, string>>> Read(byte[] data)
        {
            var result = new Dictionary<string, List<Dictionary<string, string>>>();

            using (var ms = new MemoryStream(data))
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                var sharedStrings = ReadSharedStrings(archive);
                var sheetRelIds = ReadWorkbookSheets(archive);
                var relPaths = ReadWorkbookRels(archive);

                foreach (var kvp in sheetRelIds)
                {
                    string sheetName = kvp.Key;
                    string relId = kvp.Value;

                    if (!relPaths.TryGetValue(relId, out var target))
                        continue;

                    // Normalise path: target is relative to xl/, e.g. "worksheets/sheet1.xml"
                    string entryPath = target.TrimStart('/');
                    if (!entryPath.StartsWith("xl/"))
                        entryPath = "xl/" + entryPath;

                    var entry = archive.GetEntry(entryPath);
                    if (entry == null)
                        continue;

                    using (var stream = entry.Open())
                    {
                        result[sheetName] = ReadWorksheet(stream, sharedStrings);
                    }
                }
            }

            return result;
        }

        // ── Shared strings ────────────────────────────────────────────────────

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var list = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
                return list;

            using (var stream = entry.Open())
            {
                var doc = XDocument.Load(stream);
                foreach (var si in doc.Descendants(SpreadsheetNs + "si"))
                {
                    var sb = new StringBuilder();
                    foreach (var t in si.Descendants(SpreadsheetNs + "t"))
                        sb.Append(t.Value);
                    list.Add(sb.ToString());
                }
            }

            return list;
        }

        // ── Workbook (sheet names) ─────────────────────────────────────────────

        private static Dictionary<string, string> ReadWorkbookSheets(ZipArchive archive)
        {
            var result = new Dictionary<string, string>(); // sheetName -> r:id
            var entry = archive.GetEntry("xl/workbook.xml");
            if (entry == null)
                return result;

            using (var stream = entry.Open())
            {
                var doc = XDocument.Load(stream);
                foreach (var sheet in doc.Descendants(SpreadsheetNs + "sheet"))
                {
                    var name = sheet.Attribute("name")?.Value;
                    var rid = sheet.Attribute(RelationshipsNs + "id")?.Value;
                    if (name != null && rid != null)
                        result[name] = rid;
                }
            }

            return result;
        }

        // ── Workbook relationships (r:id -> file path) ─────────────────────────

        private static Dictionary<string, string> ReadWorkbookRels(ZipArchive archive)
        {
            var result = new Dictionary<string, string>(); // r:id -> target
            var entry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (entry == null)
                return result;

            using (var stream = entry.Open())
            {
                var doc = XDocument.Load(stream);
                foreach (var rel in doc.Descendants(PackageRelNs + "Relationship"))
                {
                    var id = rel.Attribute("Id")?.Value;
                    var target = rel.Attribute("Target")?.Value;
                    if (id != null && target != null)
                        result[id] = target;
                }
            }

            return result;
        }

        // ── Worksheet rows ─────────────────────────────────────────────────────

        private static List<Dictionary<string, string>> ReadWorksheet(Stream stream, List<string> sharedStrings)
        {
            var result = new List<Dictionary<string, string>>();
            var doc = XDocument.Load(stream);

            var sheetData = doc.Descendants(SpreadsheetNs + "sheetData").FirstOrDefault();
            if (sheetData == null)
                return result;

            var rows = new List<XElement>(sheetData.Elements(SpreadsheetNs + "row"));
            if (rows.Count < 2)
                return result;

            // First row = headers
            var headers = new Dictionary<int, string>(); // colIndex -> name
            foreach (var cell in rows[0].Elements(SpreadsheetNs + "c"))
            {
                var cellRef = cell.Attribute("r")?.Value;
                if (cellRef == null)
                    continue;
                int col = ColumnIndex(cellRef);
                string value = CellValue(cell, sharedStrings);
                if (!string.IsNullOrWhiteSpace(value))
                    headers[col] = value.Trim();
            }

            // Data rows
            for (int i = 1; i < rows.Count; i++)
            {
                var row = new Dictionary<string, string>();
                foreach (var cell in rows[i].Elements(SpreadsheetNs + "c"))
                {
                    var cellRef = cell.Attribute("r")?.Value;
                    if (cellRef == null)
                        continue;
                    int col = ColumnIndex(cellRef);
                    if (!headers.TryGetValue(col, out var header))
                        continue;
                    row[header] = CellValue(cell, sharedStrings);
                }

                if (row.Count > 0)
                    result.Add(row);
            }

            return result;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string CellValue(XElement cell, List<string> sharedStrings)
        {
            var type = cell.Attribute("t")?.Value;
            var raw = cell.Element(SpreadsheetNs + "v")?.Value ?? string.Empty;

            if (type == "s") // shared string index
            {
                if (int.TryParse(raw, out int idx) && idx >= 0 && idx < sharedStrings.Count)
                    return sharedStrings[idx];
                return string.Empty;
            }

            if (type == "inlineStr")
                return cell.Descendants(SpreadsheetNs + "t").FirstOrDefault()?.Value ?? string.Empty;

            if (type == "b")
                return raw == "1" ? "TRUE" : "FALSE";

            return raw;
        }

        /// <summary>Converts a cell reference like "AB12" to a 0-based column index.</summary>
        private static int ColumnIndex(string cellRef)
        {
            int col = 0;
            foreach (char c in cellRef)
            {
                if (!char.IsLetter(c))
                    break;
                col = col * 26 + (char.ToUpperInvariant(c) - 'A' + 1);
            }
            return col - 1;
        }
    }
}
