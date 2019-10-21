using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("PublishWithParams", 276, Example = "PublishWithParams ('fi|sv',true) #Live-web# from /sitecore/content//*[@@templatename='Sample Item']", 
        LongHelp = "Publishes items for selected languages with or without sub-items, smart publish method", 
        ShortHelp = "Publishes items", 
        Syntax = "\"PublishWithParams\" ('language1|language2|..','true' or 'false')\" [PublishingTargets] ['from' Expression]")]
    public class PublishWithParamsKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(276, "\"PublishWithParams\" expected");
            parser.Match(32, "'(' expected");
            string languageStr = parser.Token.Value;         
            parser.Match(22, "Language code expected");
            bool subitems = false;

            if (parser.Token.Type == 12)
                this.GetBool(parser, subitems);
                parser.Match(18, "')' expected");
            List<string> targets = new List<string>();
            if (parser.Token.Type == 21)
                this.GetTargets(parser, targets);
            Opcode from = (Opcode)null;
            if (parser.Token.Type == 203)
                from = parser.GetFrom();

            return (Opcode)new PublishWithParams(from, targets, languageStr, subitems);
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
        private void GetBool(Parser parser, bool subitems)
        {
            parser.Match(12, "comma expected");
                if (parser.Token.Type == 107)
                {
                    parser.Match(107, "False expected");
                    subitems = false;
                }
                if (parser.Token.Type == 115)
                {
                    parser.Match(115, "True expected");
                    subitems = true;
                }
        }
    }
}