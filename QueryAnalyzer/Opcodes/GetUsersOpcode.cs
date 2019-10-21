using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Web.Authentication;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class GetUsers : Opcode
    {

        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to have admin rights to use this function");
            }
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