using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Web.Authentication;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class KickUser : Opcode
    {
        public string UserName { get; set; }

        public KickUser(string userName)
        {
            this.UserName = userName;
        }

        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to have admin rights to use this function");
            }
            List<DomainAccessGuard.Session> sessions = Sitecore.Web.Authentication.DomainAccessGuard.Sessions;
            foreach (var user in sessions.Where(u => u.UserName == this.UserName))
            {
                if (user == null)
                    throw new QueryException("user not found");
                DomainAccessGuard.Kick(user.SessionID);
                return (object)"user has been kicked";
            }
            return (object) string.Empty;
        }
    }
}