using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Rocks.Server.Plugins.Plugins.Keywords
{
    [Keyword("ExcelToRocks", 279, Example = "ExcelToRocks \"C:\\users\\CurrentUser\\Desktop\\ExcelFile.xsl\"",
        LongHelp = "Will convert a CopyTool generated Excel file to Sitecore Rocks Queries\r\nIf directory is denied, check that the folder containing the file is allowed for IIS_IUSR",
        ShortHelp = "Convert Excel to Queries",
        Syntax = "ExcelToRocks [Source]")]

    public class ExcelToRocksKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(279, "\"ExcelToRocks\" expected");
            parser.Match(22, "file path expected");
            string file = parser.Token.Value;

            return (Opcode)new ExcelToRocks(file);
        }
    }
}