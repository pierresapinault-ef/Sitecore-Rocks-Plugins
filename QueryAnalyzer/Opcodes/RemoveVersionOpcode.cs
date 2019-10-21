using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class RemoveVersion : Opcode
    {
        public string Language { get; set; }
        protected Opcode From { get; set; }
        protected bool SubItems { get; set; }

        //getting the parameters from the keyword class
        public RemoveVersion(string language, Opcode from, bool subItems)
        {
            Assert.ArgumentNotNull((object)language, "language");
            this.Language = language;
            this.From = from;
            this.SubItems = subItems;
        }

        //Evaluate the query and execute the CleanUp method
        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");

            if (!Context.User.IsAdministrator)
                throw new QueryException("You need to have admin rights to use this function");

            Context.Language = LanguageManager.GetLanguage(this.Language);

            if (Context.Language == null)
                throw new QueryException("Language not found");

            object result = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                result = QueryExtensions.Evaluate(query, from, contextNode);
            }
            IEnumerable<Item> items = QueryExtensions.GetItems(query, result);

            //the items we want to clean
            List<Item> list = Enumerable.ToList<Item>(QueryExtensions.GetItems(query, result));

            //let's do the work
            CleanUp(list, Language, SubItems);

            //return when completed
            if (SubItems)
            {
                //TO DO = find a way to count all children
                return (object)QueryExtensions.FormatItemsAffected(query, Enumerable.Count<Item>(items));
            }
            return (object)QueryExtensions.FormatItemsAffected(query, Enumerable.Count<Item>(items));
        }

        public void CleanUp(List<Item> list, string language, bool deep)
        {
            string[] products = new string[] { "ils", "aya", "ilsh", "upa", "lsp", "lt", "highschool", "efc", "loc", "academy" };

            foreach (Item parentItem in list)
            {
                List<Item> itemsList = new List<Item>();
                itemsList.Add(parentItem);

                if (deep)
                {
                    List<Item> subitemsList = parentItem.Axes.GetDescendants().ToList();
                    subitemsList.Add(parentItem);
                    itemsList = subitemsList;
                }
                using (new Sitecore.Data.BulkUpdateContext())
                    foreach (Item item in itemsList)
                    {
                        if (products.Any(item.Name.Equals))
                            continue;
                        DoWorkBis(item, language);
                    }
            }
        }
        public void DoWorkBis(Item item, string language)
        {
            using (new Sitecore.SecurityModel.SecurityDisabler())

            item.Versions.RemoveAll(false);

            //logging that in sitecore logs
            string[] publishParams = { Context.User.Name, item.Paths.FullPath, language, item.ID.ToString() };
            Log.Info(string.Format("({0}) -> Remove Language Version(s) from: {1}, language: {2}, id: {3}", publishParams), Context.User);
        }

        //obsolete method, no need to check the language because we already set Context.Language
        public void DoWork(Item item, string language)
        {
            // get all versions of an Item
            Item[] versions = item.Versions.GetVersions(true);

            for (int i = 0; i < versions.Length; i++)
            {
                // check whether we have the correct language
                if (versions[i].Language.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    using (new Sitecore.SecurityModel.SecurityDisabler())
                    {
                        // remove the version(s)
                        item.Database.Engines.DataEngine.RemoveVersion(versions[i]);

                        //logging that in sitecore logs
                        string[] publishParams = { Context.User.Name.ToString(), item.Paths.FullPath.ToString(), language.ToString(), item.Version.ToString(), item.ID.ToString() };
                        Log.Info(string.Format("({0}) -> Remove Language Version(s) from: {1}, language: {2}, version: {3}, id: {4}", publishParams), Context.User);
                    }
                }
            }
        }
    }
}