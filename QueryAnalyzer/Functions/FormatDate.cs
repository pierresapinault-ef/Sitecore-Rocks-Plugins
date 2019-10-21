using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System.Net;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("FormatDate", Example = "select FormatDate(@#__Updated#) from /sitecore/content/Home",
        LongHelp = "Formats the raw value of a date field",
        ShortHelp = "Formats the raw value of a date field")]
    public class FormatDate : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 1)
            {
                throw new QueryException("You need to specify a field or a value in ()");
            }

            object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            string ObjStr = obj.ToString();
            if (string.IsNullOrEmpty(ObjStr))
                return (object) string.Empty;
            if (ObjStr.Length > 15)
                ObjStr = ObjStr.Remove(15);
            System.DateTime date1 = System.DateTime.ParseExact(ObjStr, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            return (object)date1.ToString();
        }
    }
}