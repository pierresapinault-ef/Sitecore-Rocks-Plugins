using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("InsertFromTemplate", 292, Example = "InsertFromTemplate ('','') from /sitecore/content/home/*", 
        LongHelp = "Insert Items from templates", 
        ShortHelp = "Insert Items from templates",
        Syntax = "'InsertFromTemplate' ('templateID','templateID',etc)' from [expression]")]
    [ReservedWord("InsertFromTemplate", 292)]

    public class InsertFromTemplateKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(292, "\"InsertFromTemplate\" expected");
            parser.Match(32, "'(' expected");
            List<string> templateIdList = this.GetList(parser);
            parser.Match(18, "')' expected");
            Opcode from = null;
            if (parser.Token.Type == 203)
            {
                from = parser.GetFrom();
            }

            return (Opcode)new InsertFromTemplate(from, templateIdList);
        }

        private List<string> GetList(Parser parser)
        {
            List<string> list = new List<string>();
            parser.Match();
            list.Add(parser.Token.Value);
            while (parser.Token.Type == 12)
            {
                parser.Match();
                list.Add(parser.Token.Value);
                parser.Match();
            }
            return list;
        }
    }
}