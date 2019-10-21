using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("Copy", 286,
        Example = "Copy item to ('path',/sitecore/content/home) from /sitecore/content//*[@@templatename='Sample Item']",
        LongHelp = "Copy items to the desired location",
        ShortHelp = "Copy items",
        Syntax = "'copy' 'item' 'to'(['id' or 'path'],'value') from [expression] ")]
    [ReservedWord("item", 284)]
    [ReservedWord("to", 283)]

    public class CopyKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(286, "\"Copy\" expected");
            parser.Match(284, "\"item\" expected");
            string type = null;
            string to = string.Empty;
            if (parser.Token.Type == 283)
            {
                parser.Match(283, "\"to\" expected");
                parser.Match(32, "'(' expected");
                if (parser.Token.Type == 21)
                {
                    parser.Match(21, "only \"id\" or \"path\" are accepted");
                    if (parser.Token.Value == "id")
                        type = "id";
                    else if (parser.Token.Value == "path")
                        type = "path";
                    else
                        parser.Raise("only \"id\" or \"path\" are accepted");
                }
                if (string.IsNullOrEmpty(type))
                    parser.Raise("only \"id\" or \"path\" are accepted");
                parser.Match(12, "comma expected");
                parser.Match(22, "\"id\" or \"path\" expected");
                to = parser.Token.Value;
                parser.Match(18, "')' expected");
            }

            Opcode from = (Opcode)null;
            if (parser.Token.Type == 203)
                from = parser.GetFrom();

            return (Opcode)new Copy(from, to, type);
        }
    }
}