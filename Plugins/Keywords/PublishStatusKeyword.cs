using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("PublishStatus", 281, Example = "PublishStatus from /sitecore/content//*[@@templatename='Sample Item']",
        LongHelp = "Checks if the item exist in target database and if it is up-to-date",
        ShortHelp = "Returns the publishing status of that item",
        Syntax = "\"PublishStatus\" ['from' Expression]")]

    public class PublishStatusKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(281, "\"PublishStatus\" expected");
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
            return (Opcode)new PublishStatus(from);
        }
    }
}