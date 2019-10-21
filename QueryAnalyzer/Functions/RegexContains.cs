using Sitecore.Diagnostics;
using System.Text.RegularExpressions;
using Sitecore.Data.Query;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("RegexContains", Example = "RegexContains(@field,regexPattern) from /sitecore/content/home", 
        LongHelp = "returns true or false when searching fields with a regex pattern \r\n this is C# regex and your expression will fit in here: @\"((?i)regexPattern)\", case insensitive", 
        ShortHelp = "contains method using regex, case insensitive")]

    public class RegexContains : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 2)
            {
                throw new QueryException("You need to specify a field or a value in () as well as your Regex pattern");
            }

            object valueObj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            string value = valueObj.ToString();
            object regexObj = args.Arguments[1].Evaluate(args.Query, args.ContextNode);
            string regexPattern = new Regex(@regexObj.ToString(),RegexOptions.IgnoreCase).ToString();
            if (value.Length == 0)
                {
                    return false;
                }
            if (Regex.Matches(value, regexPattern,RegexOptions.IgnoreCase).Count == 0)
                {
                    return false;
                }
            if (Regex.Matches(value, regexPattern, RegexOptions.IgnoreCase).Count > 0)
                {
                    return true;
                }
            return false;
        }
    }
}