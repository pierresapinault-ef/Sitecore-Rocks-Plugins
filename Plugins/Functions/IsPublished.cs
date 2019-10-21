using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Data.Query;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("IsPublished", Example = "IsPublished() from /sitecore/content/*",
        LongHelp = "Checks if the item exist in target database and if it is up-to-date",
        ShortHelp = "Checks if item is published")]

    public class IsPublished : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 0)
            {
                throw new QueryException("this function doesn't take any argument");
            }
            var result = string.Empty;
            Item itemToBeChecked = args.ContextNode.GetQueryContextItem();
            string itemToBeCheckedDate = itemToBeChecked["__Updated"];//20150312T053159:635617351195944971
            Database currentDatabase = Factory.GetDatabase(itemToBeChecked.Database.Name);
            Assert.IsNotNull(currentDatabase, itemToBeChecked.Database.Name);
            if (itemToBeChecked != null)
                foreach (Database targetDatabase in GetTargets(itemToBeChecked))
                {
                    Item targetItem = targetDatabase.GetItem(itemToBeChecked.ID);
                    if (targetItem == null)
                    {
                        return result = string.Format("item is not published in target database \"{0}\"", targetDatabase.Name);
                    }
                    string targetItemDate = targetItem["__Updated"];
                    if (!string.IsNullOrEmpty(itemToBeCheckedDate) && !string.IsNullOrEmpty(targetItemDate))
                    {
                        itemToBeCheckedDate = itemToBeCheckedDate.Remove(15);
                        targetItemDate = targetItemDate.Remove(15);
                        System.DateTime date1 = System.DateTime.ParseExact(itemToBeCheckedDate, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                        System.DateTime date2 = System.DateTime.ParseExact(targetItemDate, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                        if (itemToBeChecked.Version.Number == 0 && targetItem.Version.Number > 0)
                        {
                            result = string.Format("item has a version in \"{0}\" but no version in \"{1}\"", targetDatabase.Name, currentDatabase);
                        }
                        if (itemToBeChecked.Version.Number > 0 && targetItem.Version.Number == 0)
                        {
                            result = string.Format("item has a version in \"{1}\" but no version in \"{0}\"", targetDatabase.Name, currentDatabase);
                        }
                        if (date1 == date2)
                        {
                            result = "item exists in target database and is up-to-date";
                        }
                        if (date1 > date2)
                        {
                            result = string.Format("item exists in \"{0}\" but is outdated", targetDatabase.Name);
                        }
                        if (date1 < date2)
                        {
                            result = string.Format("item exists in \"{0}\" and is more recent than in \"{1}\"", targetDatabase.Name, currentDatabase);
                        }
                    }
                    else
                    result = "item exists in target database (could not check last update date)";
                }
            return (object)result;
        }
        private List<Database> GetTargets(Item item)
        {
            using (new SecurityDisabler())
            {
                Item publishingTargets = item.Database.Items[@"/sitecore/system/publishing targets"];
                if (publishingTargets != null)
                {
                    List<Database> list = new List<Database>();
                    foreach (Item target in publishingTargets.Children)
                    {
                        string targetDatabase = target["Target database"];
                        if (targetDatabase.Length > 0)
                        {
                            Database database = Factory.GetDatabase(targetDatabase, false);
                            if (database != null)
                            {
                                list.Add(database);
                            }
                            else
                            {
                                Log.Warn("Unknown database in PublishAction: " + targetDatabase, this);
                            }
                        }
                    }
                    return list;
                }
            }
            return new List<Database>();
        }
    }
}