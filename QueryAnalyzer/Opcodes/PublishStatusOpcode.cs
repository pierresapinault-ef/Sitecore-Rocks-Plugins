using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class PublishStatus : Opcode
    {
        protected Opcode From { get; set; }

        public PublishStatus(Opcode from)
        {
            this.From = from;
        }
        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");

            var print = string.Empty;

            //get the context node
            object result = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                result = QueryExtensions.Evaluate(query, from, contextNode);
                if (result == null)
                    return 0;
            }
            List<Item> list = Enumerable.ToList<Item>(QueryExtensions.GetItems(query, result));

            //check the item from context node against the target DB
            foreach (Item item in list)
            {
                string itemToBeCheckedDate = item["__Updated"];//20150312T053159:635617351195944971
                Sitecore.Data.Database currentDatabase = Factory.GetDatabase(item.Database.Name);
                Assert.IsNotNull(currentDatabase, item.Database.Name);
                if (item != null)
                    foreach (Sitecore.Data.Database targetDatabase in GetTargets(item))
                    {
                        Item targetItem = targetDatabase.GetItem(item.ID);
                        if (targetItem == null)
                        {
                            return print = string.Format("item is not published in target database \"{0}\"", targetDatabase.Name);
                        }
                        string targetItemDate = targetItem["__Updated"];
                        if (!string.IsNullOrEmpty(itemToBeCheckedDate) && !string.IsNullOrEmpty(targetItemDate))
                        {
                            itemToBeCheckedDate = itemToBeCheckedDate.Remove(15);
                            targetItemDate = targetItemDate.Remove(15);
                            System.DateTime date1 = System.DateTime.ParseExact(itemToBeCheckedDate, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                            System.DateTime date2 = System.DateTime.ParseExact(targetItemDate, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                            if (date1 == date2)
                            {
                                print = "item exists in target database and is up-to-date";
                            }
                            if (date1 > date2)
                            {
                                print = string.Format("item exists in \"{0}\" but is outdated", targetDatabase.Name);
                            }
                            if (date1 < date2)
                            {
                                print = string.Format("item exists in \"{0}\" and is more recent than in \"{1}\"", targetDatabase.Name, currentDatabase);
                            }
                        }
                        else
                            print = "item exists in target database (could not check last update date)";
                    }
            }
            return (object)print;
        }
        private List<Sitecore.Data.Database> GetTargets(Item item)
        {
            using (new SecurityDisabler())
            {
                Item publishingTargets = item.Database.Items[@"/sitecore/system/publishing targets"];
                if (publishingTargets != null)
                {
                    List<Sitecore.Data.Database> list = new List<Sitecore.Data.Database>();
                    foreach (Item target in publishingTargets.Children)
                    {
                        string targetDatabase = target["Target database"];
                        if (targetDatabase.Length > 0)
                        {
                            Sitecore.Data.Database database = Factory.GetDatabase(targetDatabase, false);
                            if (database != null)
                            {
                                list.Add(database);
                            }
                            else
                            {
                                throw new QueryException("Unknown database: " + targetDatabase);
                            }
                        }
                    }
                    return list;
                }
            }
            return new List<Sitecore.Data.Database>();
        }
    }
}