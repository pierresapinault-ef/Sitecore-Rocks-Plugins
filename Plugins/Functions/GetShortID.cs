using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("ShortID", Example = "ShortID() from /sitecore/content/home", 
        LongHelp = "Returns the short ID of the item", 
        ShortHelp = "Returns the short ID of the item")]

    public class ShortID : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 0)
            {
                throw new QueryException("This function takes no arguments");
            }
            Item item = args.ContextNode.GetQueryContextItem();
            return (object)item.ID.ToShortID();
        }
    }
}