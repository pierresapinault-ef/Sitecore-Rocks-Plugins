using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System.Linq;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class InsertFromTemplate : Opcode
    {
        protected Opcode From { get; set; }
        public List<string> TemplateIdList { get; set; }
        public List<string> ItemNameList { get; set; }

        public InsertFromTemplate(Opcode from, List<string> templatesList, List<string> itemNameList)
        {
            Assert.ArgumentNotNull((object)templatesList, "templatesList");
            this.From = from;
            this.TemplateIdList = templatesList;
            this.ItemNameList = itemNameList;
        }

        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            if (ItemNameList.Count != 0 && TemplateIdList.Count != ItemNameList.Count)
            {
                throw new QueryException("The number of templates and item names has to be the same");
            }
            return (object)QueryExtensions.FormatItemsAffected(query, this.Execute(query, contextNode));
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
            using (new Sitecore.Data.BulkUpdateContext())
            foreach (Item item in list)
            {
                //foreach (string str in this.TemplateIdList)
                //{
                    if (ItemNameList.Count != 0)
                    {
                        for (int index = 0; index < this.TemplateIdList.Count; ++index)
                        {
                            TemplateItem TemplateItm = database.GetItem(Sitecore.Data.ID.Parse(this.TemplateIdList[index]));
                            string itemName = this.ItemNameList[index];
                            item.Add(itemName, TemplateItm);
                        }
                    }
                    else
                    {
                        foreach (string str in this.TemplateIdList)
                        {
                            TemplateItem TemplateItm = database.GetItem(Sitecore.Data.ID.Parse(str));
                            item.Add(TemplateItm.Name, TemplateItm);
                        }
                    }
                //}
            }
            return Enumerable.Count<Item>((IEnumerable<Item>)list);
        }
    }
}