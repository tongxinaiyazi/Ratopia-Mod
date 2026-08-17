using System;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class CsvTableTests
    {
        [Fact]
        public void ParsesQuotedCommasAndEscapedQuotes()
        {
            var table = CsvTable.Parse("Name,Description\r\nA,\"hello, \"\"rat\"\"\"\r\n");

            Assert.Single(table.Rows);
            Assert.Equal("A", table.Rows[0]["Name"]);
            Assert.Equal("hello, \"rat\"", table.Rows[0]["Description"]);
        }

        [Fact]
        public void RejectsRowsWhoseColumnCountDiffersFromHeader()
        {
            var error = Assert.Throws<FormatException>(() => CsvTable.Parse("A,B\n1\n"));

            Assert.Contains("第 2 行", error.Message);
        }
    }
}
