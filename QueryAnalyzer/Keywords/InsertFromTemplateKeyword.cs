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
        Syntax = "'InsertFromTemplate' ('templateID','templateID',etc)' [optional] ItemName ('itemname1','itemname2',etc) from [expression]")]
    [ReservedWord("InsertFromTemplate", 292)]
    [ReservedWord("ItemName", 297)]


    public class InsertFromTemplateKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(292, "\"InsertFromTemplate\" expected");
            parser.Match(32, "'(' expected");
            List<string> templateIdList = this.GetList(parser);
            parser.Match(18, "')' expected");
            List<string> itemNameList = new List<string>();
            if (parser.Token.Type == 297)
            {
                this.GetItemNameList(parser, itemNameList);
            }
            Opcode from = null;
            if (parser.Token.Type == 203)
            {
                from = parser.GetFrom();
            }

            return (Opcode)new InsertFromTemplate(from, templateIdList, itemNameList);
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
        private List<string> GetItemNameList(Parser parser,List<string> list)
        {
            parser.Match();
            parser.Match(32, "'(' expected");
            list.Add(parser.Token.Value);
            parser.Match();
            while (parser.Token.Type == 12)
            {
                parser.Match();
                list.Add(parser.Token.Value);
                parser.Match();
            }
            parser.Match(18, "')' expected");
            return list;
        }
    }
}