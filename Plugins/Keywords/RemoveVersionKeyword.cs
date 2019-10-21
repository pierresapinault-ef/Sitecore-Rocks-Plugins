using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{

    [Keyword("RemoveVersion", 275,
    ShortHelp = "Removes Versions in a given language",
    LongHelp = "Removes all language versions of a given item or path",
    Syntax = "'RemoveVersion ('language') ['from' Expression]",
    Example = "RemoveVersion ('sv') from /sitecore/content//*[@@templatename='Sample Item']")]

    public class RemoveVersionKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");

            parser.Match(275, "\"RemoveVersion\" expected");
            parser.Match(32, "'(' expected");
            string language = parser.Token.Value;
            parser.Match(22, "Language code expected");
            if (string.IsNullOrEmpty(language))
            {
                parser.Raise("You need to specify the language for which versions should be removed");
            }
            parser.Match(18, "')' expected");
            Opcode from = (Opcode)null;
            if (parser.Token.Type == 203)
            {
                from = parser.GetFrom();
            }
            else
            {
                parser.Raise("'from' expected");
            }
            if (from == null)
            {
                parser.Raise("You need to specify a path");
            }
            return (Opcode) new RemoveVersion(language, from);
        }
    }
}