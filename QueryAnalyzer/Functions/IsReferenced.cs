using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Data.Query;
using Sitecore.Exceptions;
using Sitecore.Links;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("IsReferenced", Example = "IsReferenced() from /sitecore/content/*",
        LongHelp = "Checks if the item is being referenced by other items",
        ShortHelp = "Checks if item is being referenced and returns a bool")]

    public class IsReferenced : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 0)
            {
                throw new QueryException("this function doesn't take any argument");
            }
            Item itemToCheck = args.ContextNode.GetQueryContextItem();
            using (new SecurityDisabler())
            {
                ItemLink[] referrers = Globals.LinkDatabase.GetReferrers(itemToCheck);
                if (referrers.Length > 0)
                    return true;
            }
            return false;
        }
    }
}