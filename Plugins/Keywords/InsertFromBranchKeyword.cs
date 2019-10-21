using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("InsertFromBranch", 293, Example = "InsertFromBranch ('branchID','branchID') from /sitecore/content/home/*",
        LongHelp = "Insert Items from branches", 
        ShortHelp = "Insert Items from branches",
        Syntax = "'InsertFromBranch' ('branchID','branchID',etc)' from [expression]")]
    [ReservedWord("InsertFromBranch", 293)]

    public class InsertFromBranchKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(293, "\"InsertFromBranch\" expected");
            parser.Match(32, "'(' expected");
            List<string> templateIdList = this.GetList(parser);
            parser.Match(18, "')' expected");
            Opcode from = null;
            if (parser.Token.Type == 203)
            {
                from = parser.GetFrom();
            }

            return (Opcode)new InsertFromBranch(from, templateIdList);
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