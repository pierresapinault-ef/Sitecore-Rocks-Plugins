using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
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

        //getting the parameters from the keyword class
        public RemoveVersion(string language, Opcode from)
        {
            Assert.ArgumentNotNull((object)language, "language");
            this.Language = language;
            this.From = from;
        }

        //Evaluate the query and execute the CleanUp method
        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");

            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to have admin rights to use this function");
            }

            //this will change the context language in the query analyzer as if we did "set language='Language';"
            Context.Language = LanguageManager.GetLanguage(Language);

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
            CleanUp(list, Language);

            //return when completed
            return (object)QueryExtensions.FormatItemsAffected(query, Enumerable.Count<Item>(items));
        }

        public void CleanUp(List<Item> list, string language)
        {
            string[] products = new string[] { "ils", "aya", "ilsh", "upa", "lsp", "lt", "highschool", "efc", "loc" };

            foreach (var item in list)
            {
                if (products.Any(item.Name.Contains))
                    continue;
                // get all versions of an Item
                Item[] versions = item.Versions.GetVersions(true);

                for (int i = 0; i < versions.Length; i++)
                {
                    if (versions[i].Language.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                    // check whether we have the correct language
                    {
                        using (new Sitecore.SecurityModel.SecurityDisabler())
                        // use the SecurityDisabler to access the Item
                        {
                            item.Database.Engines.DataEngine.RemoveVersion(versions[i]);
                            // remove the version(s)
                            string[] publishParams = { Context.User.Name.ToString(), item.Paths.FullPath.ToString(), language.ToString(), item.Version.ToString(), item.ID.ToString() };
                            Log.Info(string.Format("({0}) -> Remove Language Version(s) from: {1}, language: {2}, version: {3}, id: {4}", publishParams), Context.User);
                            //logging that in sitecore logs
                        }
                    }
                }
            }
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
    }
}