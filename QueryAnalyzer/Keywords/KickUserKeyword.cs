using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("kick", 295,
        Example = "kick user ()",
        LongHelp = "kicks the user",
        ShortHelp = "kicks the user",
        Syntax = "kick user (username)")]
    [ReservedWord("kick", 295)]
    [ReservedWord("user", 296)]

    public class KickUserKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            string userName = string.Empty;

            parser.Match(295, "\"kick\" expected");
            parser.Match(296, "\"user\" expected");
            parser.Match(32, "'(' expected");
            parser.Match(22, "username expected");
            userName = parser.Token.Value;
            parser.Match(18, "')' expected");

            return new KickUser(userName);
        }
    }
}