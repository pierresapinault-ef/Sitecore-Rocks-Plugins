using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("GetUsers", 294,
        Example = "GetUsers",
        LongHelp = "Get the current users",
        ShortHelp = "Get the current users",
        Syntax = "GetUsers")]
    [ReservedWord("GetUsers", 294)]

    public class GetActiveUsersKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(294, "\"GetUsers\" expected");
            return new GetUsers();
        }
    }
}