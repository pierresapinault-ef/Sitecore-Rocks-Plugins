using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("AllLanguagesQuery", 278, 
        Example = "AllLanguagesQuery(\"update set @Title = 'Sitecore Rocks' from /sitecore/content/Home/*\"",
        LongHelp = "Generates your query for all languages in your sitecore instance, just need to copy/paste that in the Query Analyzer", 
        ShortHelp = "Generates your query for all languages",
        Syntax = "\"AllLanguagesQuery('query')\"")]

    public class AllLanguagesQueryKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(278, "\"AllLanguagesQuery\" expected");
            parser.Match(32, "'(' expected");
            string queryStr = parser.Token.Value;
            parser.Match(22, "query expected");
            parser.Match(18, "')' expected");
            return (Opcode)new AllLanguagesQuery(queryStr);
        }
    }
}