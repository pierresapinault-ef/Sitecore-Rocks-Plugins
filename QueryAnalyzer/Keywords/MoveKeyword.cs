using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("move", 282,
        Example = "move item from /sitecore/content//*[@@templatename='Sample Item'] to /sitecore/content/new-location",
        LongHelp = "Move items to the desired location",
        ShortHelp = "Move items",
        Syntax = "'move' ")]
    [ReservedWord("item", 284)]
    [ReservedWord("to", 283)]

    public class MoveKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(282, "\"Move\" expected");
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
            string star = "*";
            if (to.Contains(star)) 
            {
                parser.Raise("cannot move the item to multiple location, use \"copy\" instead");
            }
            return (Opcode)new Move(from, to, type);
        }
    }
}