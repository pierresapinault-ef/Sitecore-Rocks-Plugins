using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("Trim", Example = "update set @field = Trim(@field) from /sitecore/content/home", 
        LongHelp = "Remove whitespace from both sides of a string.", 
        ShortHelp = "Remove whitespace from both sides of a string")]

    public class Trim : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            if (args.Arguments.Length != 1)
                throw new QueryException("You need to specify a field or a value in ()");
            object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            return obj == null ? (object)string.Empty : (object)obj.ToString().Trim();
        }
    }
}