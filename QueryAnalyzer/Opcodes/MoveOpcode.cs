using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Jobs;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Move : Opcode
    {
        protected Opcode From { get; set; }
        protected string To { get; set; }
        protected string Type { get; set; }

        public Move(Opcode from, string to, string type)
        {
            Assert.ArgumentNotNull((object)to, "to");
            Assert.ArgumentNotNull((object)type, "type");
            Assert.ArgumentNotNull((object)from, "from");
            this.From = from;
            this.To = to;
            this.Type = type;
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

        //public string Execute(string databaseName, string itemId, string newParentId)
        private int Execute(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Sitecore.Data.Database database = contextNode.GetQueryContextItem().Database;

            //get the source path
            object result = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                result = QueryExtensions.Evaluate(query, from, contextNode);
            }
            IEnumerable<Item> items = QueryExtensions.GetItems(query, result);
            List<Item> list = Enumerable.ToList<Item>(QueryExtensions.GetItems(query, result));
            Item destination = null;

            //get the destination item
            if (Type == "path")
            {
                destination = database.GetItem(To);
            }
            if (Type == "id")
            {
                ID itemID = Sitecore.Data.ID.Parse(To);
                destination = database.GetItem(itemID);
            }
            

            foreach (Item item in list)
            {
                if (item == null)
                    throw new Exception("Item not found");
                if (destination == null)
                    throw new Exception("Parent item not found");
                item.Editing.BeginEdit();
                item.MoveTo(destination);
                item.Editing.EndEdit();
                BackgroundJob.Run("Move Item", "Updating Items", (Action)(() => this.Update(item)));
            }
            return Enumerable.Count<Item>((IEnumerable<Item>)list);
        }

        private void Update(Item item)
        {
            using (new SecurityDisabler())
            {
                ItemLink[] referrers = Globals.LinkDatabase.GetReferrers(item);
                if (referrers.Length > 0)
                {
                    foreach (ItemLink itemLink in referrers)
                    {
                        Sitecore.Data.Database database = Factory.GetDatabase(itemLink.SourceDatabaseName);
                        if (database != null)
                        {
                            Item obj1 = database.GetItem(itemLink.SourceItemID);
                            if (obj1 != null)
                            {
                                foreach (Item obj2 in obj1.Versions.GetVersions())
                                {
                                    Field field1 = obj2.Fields[itemLink.SourceFieldID];
                                    if (field1 != null)
                                    {
                                        CustomField field2 = FieldTypeManager.GetField(field1);
                                        if (field2 != null)
                                        {
                                            obj2.Editing.BeginEdit();
                                            field2.UpdateLink(itemLink);
                                            obj2.Editing.EndEdit();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (Item obj in item.Children)
                this.Update(obj);
        }
    }
}