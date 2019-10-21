using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System.Text.RegularExpressions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("ToLower", Example = "update set @field = ToLower(@field,true) from /sitecore/content/home", 
        LongHelp = "Convert a string to lowercase, can be same field or different field from same item.\nIf you add true as a parameter it will keep the first characters in uppercase", 
        ShortHelp = "Convert a string to lowercase")]

    public class ToLower : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            if (args.Arguments.Length < 1 | args.Arguments.Length > 2)
            {
                throw new QueryException("too many or not enough arguments for ToLower()");
            }
            object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);

            if (args.Arguments.Length == 1)
            {
                return obj == null ? (object)string.Empty : (object)obj.ToString().ToLower();
            }
            string smart = args.Arguments[1].Evaluate(args.Query, args.ContextNode).ToString().ToLower();
            if (args.Arguments.Length == 2 && smart != "true")
            {
                return obj == null ? (object)string.Empty : (object)obj.ToString().ToLower();
            }
            if (args.Arguments.Length == 2 && smart == "true")
            {
                string Obj = obj.ToString().ToLower();
                //for all characters after a dot
                Regex firstCharacter = new Regex(@"(\.[\s\r\n]*[a-z])");
                MatchCollection firstCharacterCollection = firstCharacter.Matches(Obj);
                foreach (Match match in firstCharacterCollection)
                {
                    string matchStr = match.ToString();
                    string matchStr2 = matchStr.ToUpper();
                    Obj = Obj.Replace(matchStr, matchStr2);
                }
                // for the very first character of the string
                char[] a = Obj.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                Obj = new string(a);
                return (object)Obj;
            }
            return obj;
        }
    }
}