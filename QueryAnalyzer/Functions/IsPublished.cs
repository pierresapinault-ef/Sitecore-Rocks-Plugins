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
        ShortHelp = "Checks if item is published and returns a bool")]

    public class IsPublished : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 0)
            {
                throw new QueryException("this function doesn't take any argument");
            }
            Item itemToCheck = args.ContextNode.GetQueryContextItem();
            Database currentDatabase = Factory.GetDatabase(itemToCheck.Database.Name);
            Assert.IsNotNull(currentDatabase, itemToCheck.Database.Name);
            if (itemToCheck != null)
                foreach (Database targetDatabase in GetTargets(itemToCheck))
                {
                    Item targetItem = targetDatabase.GetItem(itemToCheck.ID);
                    if (targetItem == null)
                        return false;
                }
            return true;
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