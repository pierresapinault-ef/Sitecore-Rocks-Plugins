using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Republish : Opcode
    {
        protected Opcode From { get; set; }

        protected List<string> Targets { get; set; }

        public Republish(Opcode from, List<string> targets)
        {
            Assert.ArgumentNotNull((object)targets, "targets");
            this.From = from;
            this.Targets = targets;
        }

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to have admin rights to use the Republish function");
            }
            return (object)QueryExtensions.FormatItemsAffected(query, this.Execute(query, contextNode));
        }

        public override void Print(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull((object)output, "output");
            this.PrintIndent(output);
            this.PrintLine(output, this.GetType().Name);
            if (this.From == null)
                return;
            ++output.Indent;
            this.From.Print(output);
            --output.Indent;
        }

        private int Execute(Query query, QueryContext contextNode)
        {
            Sitecore.Data.Database database = contextNode.GetQueryContextItem().Database;
            ItemList publishingTargets = PublishManager.GetPublishingTargets(database);
            foreach (string str in this.Targets)
            {
                string target = str;
                if (!Enumerable.Any<Item>((IEnumerable<Item>)publishingTargets, (Func<Item, bool>)(t => string.Compare(t.Name, target, StringComparison.InvariantCultureIgnoreCase) == 0)))
                    throw new QueryException(string.Format("Publishing target \"{0}\" not found", (object)target));
            }
            Sitecore.Data.Database[] targets = Enumerable.ToArray<Sitecore.Data.Database>(Enumerable.Select<Item, Sitecore.Data.Database>(Enumerable.Where<Item>((IEnumerable<Item>)publishingTargets, (Func<Item, bool>)(i =>
            {
                if (Enumerable.Any<string>((IEnumerable<string>)this.Targets))
                    return Enumerable.Any<string>((IEnumerable<string>)this.Targets, (Func<string, bool>)(t => string.Compare(t, i.Name, StringComparison.InvariantCultureIgnoreCase) == 0));
                return true;
            })), (Func<Item, Sitecore.Data.Database>)(target => Factory.GetDatabase(target["Target database"]))));
            
            String contextLanguage = Context.Language.ToString();
            Language[] languages = new Language[1]{Context.Language};

            object result = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                result = QueryExtensions.Evaluate(query, from, contextNode);
                if (result == null)
                    return 0;
            }
            List<Item> list = Enumerable.ToList<Item>(QueryExtensions.GetItems(query, result));
            foreach (Item obj in list)
            {
                PublishManager.PublishItem(obj, targets, languages, false, false);
                string[] publishParams = { Context.User.Name.ToString(), obj.Paths.FullPath.ToString(), contextLanguage, obj.Version.ToString(), obj.ID.ToString() };
                Log.Audit(string.Format("({0}) -> Republish item: {1}, language: {2}, version: {3}, id: {4}", publishParams), Context.User);
            }
            return Enumerable.Count<Item>((IEnumerable<Item>)list);
        }
    }
}