using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("ToUpper", Example = "update set @field = ToUpper(@field) from /sitecore/content/home", 
        LongHelp = "Convert a string to uppercase, can be same field or different field from same item.", 
        ShortHelp = "Convert a string to uppercase")]

    public class ToUpper : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            if (args.Arguments.Length != 1)
                throw new QueryException("You need to specify a field or a value in ()");
            object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            return obj == null ? (object)string.Empty : (object)obj.ToString().ToUpper();
        }
    }
}