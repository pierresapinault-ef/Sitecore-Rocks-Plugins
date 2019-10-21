using System;
using System.Collections.Generic;
using Sitecore.Data.Query;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("RemoveLanguageVersion", Example = "RemoveLanguageVersion ('en', true) from /sitecore/content/*" , 
        LongHelp = "Removes all language versions of a given item or path, with or without children items",
        ShortHelp = "Removes Versions in a given language")]

    public class RemoveLanguageVersion : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 2)
            {
                throw new QueryException("You need to specify the language and true or false if you want all descendants to be parsed instead or not");
            }
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to have admin rights to use this function");
            }
            
            Item itemToBeCleanedUp = args.ContextNode.GetQueryContextItem();
            string language = args.Arguments[0].Evaluate(args.Query, args.ContextNode).ToString();
            string deepArg = args.Arguments[1].Evaluate(args.Query, args.ContextNode).ToString().ToLower();
            string[] products = new string[] { "ils", "aya", "ilsh", "upa", "lsp", "lt", "highschool", "efc", "loc" };

            //this will set the context language to be the same as the language in the function
            Context.Language = LanguageManager.GetLanguage(language);
            var print = string.Empty;

            int i = itemToBeCleanedUp.Versions.Count;
            bool deep = false;
            if (deepArg == "true")
            {
                deep = true;
                i = itemToBeCleanedUp.Axes.GetDescendants().Length + 1;
                if (i == 1)
                {
                    print = "1 item affected";
                }
                else
                print = i + " items affected";
            }
            if (products.Any(itemToBeCleanedUp.Name.Contains))
            {
                if (deep)
                {
                    i = itemToBeCleanedUp.Axes.GetDescendants().Length;
                    print = "item " + itemToBeCleanedUp.Name + " skipped\r\n" + i + " descendants affected";
                }
                else
                print = "item '" + itemToBeCleanedUp.Name + "' skipped";
            }
            if (!deep)
            {
                print = i + " version(s) removed";
            }

            CleanUp(itemToBeCleanedUp, language, deep);

            return (object)print;
        }
        public void CleanUp(Item itemToBeCleanedUp, string language, bool deep)
        {
            // if deep get all descendants as well otherwise take itemToBeCleanedUp
            List<Item> itemList = (!deep ? new Item[] { itemToBeCleanedUp } : itemToBeCleanedUp.Axes.GetDescendants()).ToList();
            if (deep)
                itemList.Add(itemToBeCleanedUp);
            string[] products = new string[] { "ils", "aya", "ilsh", "upa", "lsp", "lt", "highschool", "efc", "loc" };

            foreach (var item in itemList)
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
                                Log.Audit(string.Format("({0}) -> Remove Language Version(s): {1}, language: {2}, version: {3}, id: {4}", publishParams), Context.User);
                        }
                    }
                }
            }
        }
    }
}