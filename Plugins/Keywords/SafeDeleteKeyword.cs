using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Composition;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Keywords.DeleteKeyword))] 
    [Keyword("delete", 204,
    ShortHelp = "Deletes items",
    LongHelp = "Deletes items from the database.",
    Syntax = "'delete' ['from' Expression]",
    Example = "delete from /sitecore/content//*[@@templatename='Sample Item']")]

    public class NewDeleteKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull(parser, "parser");

            parser.Match(204, "\"delete\" expected");

            Opcode from = null;
            if (parser.Token.Type == 203)
            {
                from = parser.GetFrom();
            }

            return new newdelete(from);
        }
    }
} 