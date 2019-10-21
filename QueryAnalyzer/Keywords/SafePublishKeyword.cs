using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Composition;
using Sitecore.Rocks.Server.QueryAnalyzers;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Keywords.PublishKeyword))]
    [Keyword("publish", 212, Example = "publish from /sitecore/content//*[@@templatename='Sample Item']\npublish Internet", 
        LongHelp = "Publishes items in the context language of the query.", 
        ShortHelp = "Publishes items", 
        Syntax = "'publish' [PublishingTargets] ['from' Expression]\nPublishingTargets = PublishingTarget | (PublishingTargets ',' PublishingTarget)\nPublishingTargets = Identifier ")]

    public class SafePublishKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(212, "\"publish\" expected");
            List<string> targets = new List<string>();
            if (parser.Token.Type == 21)
                this.GetTargets(parser, targets);
            Opcode from = (Opcode)null;
            if (parser.Token.Type == 203)
                from = parser.GetFrom();
            return (Opcode)new newPublish(from, targets);
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