using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("EmptyRecycleBin", 285, Example = "EmptyRecycleBin",
        LongHelp = "It will completely empty the recycle bin",
        ShortHelp = "empties the recycle bin",
        Syntax = "\"EmptyRecycleBin\"")]

    public class RecycleKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(285, "\"EmptyRecycleBin\" expected");

            return (Opcode)new EmptyRecycleBin();
        }
    }
}