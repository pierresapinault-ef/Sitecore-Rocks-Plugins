using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("GetMediaUrl", Example = "GetMediaUrl() from /sitecore/#media library#//*", 
        LongHelp = "Returns the URL for the selected images", 
        ShortHelp = "Returns the URL for the selected images")]
    public class GetMediaUrl : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 0)
            {
                throw new QueryException("This function takes no arguments");
            }
            Item item = args.ContextNode.GetQueryContextItem();
            if (!item.Paths.Path.Contains("media library"))
                throw new QueryException("You need to point to the media library");
            string[] templateNames = { "Jpeg", "Image" };
            if (!templateNames.Any(item.TemplateName.ToString().Contains))
                return "invalid template";
            string mediaUrl = "http://media.ef.com/~/media/" + item.ID.ToShortID() + ".ashx";

            return (object)mediaUrl;
        }
    }
}