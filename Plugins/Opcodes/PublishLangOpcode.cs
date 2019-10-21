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
    public class PublishWithParams : Opcode
    {
        protected Opcode From { get; set; }

        protected List<string> Targets { get; set; }

        protected string Languages { get; set; }

        protected bool Subitems { get; set; }

        public PublishWithParams(Opcode from, List<string> targets, string languageStr, bool subitems)
        {
            Assert.ArgumentNotNull((object)targets, "targets");
            this.From = from;
            this.Targets = targets;
            this.Languages = languageStr;
            this.Subitems = subitems;
        }

        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
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

        private int Execute(Sitecore.Data.Query.Query query, QueryContext contextNode)
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

            char[] chArray = new char[1]
            {
                '|'
            };
            List<Language> languagesList = new List<Language>();
            foreach (string language in Languages.Split(chArray))
            {
                if (!string.IsNullOrEmpty(language))
                {
                    Language lang = LanguageManager.GetLanguage(language);
                    languagesList.Add(lang);
                }
            }
            Language[] languages = languagesList.ToArray();

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
                PublishManager.PublishItem(obj, targets, languages, Subitems, true);
                string[] publishParams = { Context.User.Name.ToString(), obj.Paths.FullPath.ToString(), Languages, obj.Version.ToString(), obj.ID.ToString() };
                Log.Audit(string.Format("({0}) -> Publish item: {1}, language: {2}, version: {3}, id: {4}", publishParams), Context.User);
            }
            return Enumerable.Count<Item>((IEnumerable<Item>)list);
        }
    }
}