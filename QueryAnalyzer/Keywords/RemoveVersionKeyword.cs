using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{

    [Keyword("RemoveVersion", 275,
    ShortHelp = "Removes Versions in a given language",
    LongHelp = "Removes all language versions of a given item or path, with or without sub-items",
    Syntax = "'RemoveVersion ('language','true' or 'false') ['from' Expression]",
    Example = "RemoveVersion ('it',true) from /sitecore/content/Home/")]

    public class RemoveVersionKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");

            parser.Match(275, "\"RemoveVersion\" expected");
            parser.Match(32, "'(' expected");
            string language = parser.Token.Value;
            parser.Match(22, "Language code expected");
            bool subItems = false;
            parser.Match(12, "comma expected");
            if (parser.Token.Type == 107)
            {
                parser.Match(107, "False expected");
                subItems = false;
            }
            if (parser.Token.Type == 115)
            {
                parser.Match(115, "True expected");
                subItems = true;
            }
            if (string.IsNullOrEmpty(language))
            {
                parser.Raise("You need to specify the language(s) for which versions should be removed");
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
            return (Opcode) new RemoveVersion(language, from, subItems);
        }
    }
}