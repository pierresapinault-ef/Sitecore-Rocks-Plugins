using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Web.Authentication;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("GetUsers", Example = "select GetUsers()",
        LongHelp = "",
        ShortHelp = "")]

    public class GetUsers : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            if (args.Arguments.Length != 0)
                throw new QueryException("this function doesn't take any arguments");
            List<DomainAccessGuard.Session> sessions = Sitecore.Web.Authentication.DomainAccessGuard.Sessions;
            var print = string.Empty;
            foreach (var session in sessions)
            {
                print += session.UserName + "\n";
            }
            return (object)print.TrimEnd();
        }
    }
}