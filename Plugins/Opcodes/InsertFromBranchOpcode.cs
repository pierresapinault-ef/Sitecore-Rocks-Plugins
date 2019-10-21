using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System.Linq;
using System.Web.UI;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class InsertFromBranch : Opcode
    {
        protected Opcode From { get; set; }
        public List<string> TemplateIdList { get; set; }

        public InsertFromBranch(Opcode from, List<string> templatesList)
        {
            Assert.ArgumentNotNull((object)templatesList, "templatesList");
            this.From = from;
            this.TemplateIdList = templatesList;
        }

        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            return (object)QueryExtensions.FormatItemsAffected(query, this.Execute(query, contextNode));
        }

        public override void Print(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull((object)output, "output");
            this.PrintIndent(output);
            this.PrintLine(output, this.GetType().Name);
            if (this.From == null)
                return;
            ++output.Indent;
            this.From.Print(output);
            --output.Indent;
        }

        private int Execute(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            object result = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                result = QueryExtensions.Evaluate(query, from, contextNode);
                if (result == null)
                    return 0;
            }
            Sitecore.Data.Database database = contextNode.GetQueryContextItem().Database;
            List<Item> list = Enumerable.ToList<Item>(QueryExtensions.GetItems(query, result));
            foreach (Item item in list)
            {
                foreach (string str in this.TemplateIdList)
                {
                    BranchItem BranchItm = database.GetItem(Sitecore.Data.ID.Parse(str));
                    item.Add(BranchItm.Name, BranchItm);
                }
            }
            return Enumerable.Count<Item>((IEnumerable<Item>)list);
        }
    }
}