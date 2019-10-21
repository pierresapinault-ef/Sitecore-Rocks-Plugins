using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using HtmlAgilityPack;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("AgilityFixHTML", Example = "update set @field = FixHTML(@field) from /sitecore/content/home", LongHelp = "this method tries to fix the bad html", ShortHelp = "fix bad html in a text field")]
    public class AgilityFixHTML : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 1)
            {
                throw new QueryException("You need to specify a field or a value in ()");
            }
            object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            string Obj = obj.ToString();
            HtmlAgilityPack.HtmlNode.ElementsFlags["p"] = HtmlElementFlag.Closed;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(Obj);
            doc.OptionFixNestedTags = true;
            doc.ToString();
            doc.Save(Obj);
            return (object)Obj;
        }
    }
}
