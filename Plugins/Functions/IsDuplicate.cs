using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("IsDuplicate", 
        Example = "IsDuplicate() from /sitecore/content/home/*", 
        LongHelp = "Returns a boolean value if the item has a duplicate under same parent, can be used with FindDuplicates() to limit results", 
        ShortHelp = "Looks for duplicate item names and returns a bool")]
    public class IsDuplicate : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            Item obj1 = args.ContextNode.GetQueryContextItem();
            if (obj1 == null)
            {
                throw new QueryException("No items to be scanned");
            }
            Item parent = obj1.Parent;

            foreach (Item obj2 in parent.Children)
            {
                if (!(obj2.ID == obj1.ID))
                {
                    if (string.Compare(obj2.Key, obj1.Key, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}