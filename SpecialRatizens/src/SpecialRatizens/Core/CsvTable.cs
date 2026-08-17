using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpecialRatizens.Core
{
    internal sealed class CsvTable
    {
        private CsvTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyDictionary<string, string>> rows)
        {
            Headers = headers;
            Rows = rows;
        }

        public IReadOnlyList<string> Headers { get; }

        public IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; }

        public static CsvTable Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var records = ParseRecords(text);
            if (records.Count == 0)
            {
                throw new FormatException("CSV 没有标题行。");
            }

            var headers = records[0];
            if (headers.Count == 0)
            {
                throw new FormatException("CSV 标题行为空。");
            }

            var seenHeaders = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < headers.Count; i++)
            {
                headers[i] = headers[i].Trim().TrimStart('\ufeff');
                if (headers[i].Length == 0 || !seenHeaders.Add(headers[i]))
                {
                    throw new FormatException($"CSV 标题无效或重复：{headers[i]}");
                }
            }

            var rows = new List<IReadOnlyDictionary<string, string>>();
            for (var recordIndex = 1; recordIndex < records.Count; recordIndex++)
            {
                var values = records[recordIndex];
                if (values.Count == 1 && values[0].Length == 0)
                {
                    continue;
                }

                if (values.Count != headers.Count)
                {
                    throw new FormatException(
                        $"CSV 第 {recordIndex + 1} 行有 {values.Count} 列，标题要求 {headers.Count} 列。");
                }

                var row = new Dictionary<string, string>(StringComparer.Ordinal);
                for (var column = 0; column < headers.Count; column++)
                {
                    row.Add(headers[column], values[column]);
                }

                rows.Add(new ReadOnlyDictionary<string, string>(row));
            }

            return new CsvTable(
                new ReadOnlyCollection<string>(headers),
                new ReadOnlyCollection<IReadOnlyDictionary<string, string>>(rows));
        }

        private static List<List<string>> ParseRecords(string text)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var field = new System.Text.StringBuilder();
            var quoted = false;

            for (var index = 0; index < text.Length; index++)
            {
                var current = text[index];
                if (quoted)
                {
                    if (current == '"')
                    {
                        if (index + 1 < text.Length && text[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                        }
                        else
                        {
                            quoted = false;
                        }
                    }
                    else
                    {
                        field.Append(current);
                    }

                    continue;
                }

                switch (current)
                {
                    case '"':
                        if (field.Length != 0)
                        {
                            throw new FormatException("CSV 引号必须位于字段开头。");
                        }
                        quoted = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                    case '\n':
                        if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                        {
                            index++;
                        }
                        record.Add(field.ToString());
                        field.Clear();
                        records.Add(record);
                        record = new List<string>();
                        break;
                    default:
                        field.Append(current);
                        break;
                }
            }

            if (quoted)
            {
                throw new FormatException("CSV 存在未闭合的引号。");
            }

            if (field.Length != 0 || record.Count != 0)
            {
                record.Add(field.ToString());
                records.Add(record);
            }

            return records;
        }
    }
}
