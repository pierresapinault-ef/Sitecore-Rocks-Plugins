using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using System.Web.UI;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class ShortID : Opcode
    {
        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            Item queryContextItem = contextNode.GetQueryContextItem();
            if (queryContextItem == null)
                return (object)string.Empty;
            return (object)queryContextItem.ID.ToShortID();
        }

        public override void Print(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull((object)output, "output");
            this.PrintIndent(output);
            this.PrintLine(output, this.GetType().Name);
        }
    }
}