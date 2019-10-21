using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("republish", 277,
        Example = "republish from /sitecore/content//*[@@templatename='Sample Item']\nrepublish Internet",
        LongHelp = "Publishes items with republish option.\nYou need to have admin rights to use this function",
        ShortHelp = "Republishes items",
        Syntax = "'republish' [PublishingTargets] ['from' Expression]\nPublishingTargets = PublishingTarget | (PublishingTargets ',' PublishingTarget)\nPublishingTargets = Identifier ")]

    public class RepublishKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(277, "\"republish\" expected");
            List<string> targets = new List<string>();
            if (parser.Token.Type == 21)
                this.GetTargets(parser, targets);
            Opcode from = (Opcode)null;
            if (parser.Token.Type == 203)
                from = parser.GetFrom();
            return (Opcode)new Republish(from, targets);
        }

        private void GetTargets(Parser parser, List<string> targets)
        {
            targets.Add(parser.Token.Value);
            parser.Match(21, "Literal expected.");
            while (parser.Token.Type == 12)
            {
                parser.Match();
                targets.Add(parser.Token.Value);
                parser.Match(21, "Literal expected.");
            }
        }
    }
}