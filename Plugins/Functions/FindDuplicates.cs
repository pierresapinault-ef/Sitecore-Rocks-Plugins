using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("FindDuplicates", 
        Example = "FindDuplicates() from /sitecore/content/home/*", 
        LongHelp = "Looks for duplicate item names that are under the same parent item",  
        ShortHelp = "Looks for duplicate item names")]
    public class FindDuplicates : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            List<string> duplicates = new List<string>();
            HashSet<string> duplicatesResult = new HashSet<string>();
            QueryContext contextNode = args.ContextNode;
            Item obj1 = contextNode.GetQueryContextItem();
            if (obj1 == null)
            {
                throw new QueryException("No items to be scanned");
            }
            Item parent = obj1.Parent;
            if (parent.Children.Count == 1)
            {
                throw new QueryException("Only one child item under this node");
            }

            string print = string.Empty;

            foreach (Item obj2 in parent.Children)
            {
                if (!(obj2.ID == obj1.ID))
                {
                    if (string.Compare(obj2.Key, obj1.Key, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        print = "Item Name: " + obj1.Key;
                        print = print + "\r\nCurrent Item ID: " + obj1.ID;
                        print = print + "\r\nUnder the path: " + obj1.Paths.ParentPath;
                        print = print + "\r\nDuplicate ID(s):";
                        duplicatesResult.Add(obj2.ID.ToString());
                    }
                }
            }
            foreach (string result in duplicatesResult)
            {              
                    print = print + "\r\n" + result;
            }
            if (string.IsNullOrEmpty(print))
            {
                return (object)string.Empty;
            }
            return (object)print;
        }
    }
}