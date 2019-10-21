using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Data.Query;
using Sitecore.Exceptions;
using Sitecore.Links;
using Sitecore.Data;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Collections;
using Sitecore.Layouts;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("GetReferrers", Example = "GetReferrers() from /sitecore/content/*",
        LongHelp = "Gets the items and the fields referencing the queried item",
        ShortHelp = "Gets the referrers of the queried item")]

    public class GetReferrers : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 0)
            {
                throw new QueryException("this function doesn't take any argument");
            }
            Item itemToCheck = args.ContextNode.GetQueryContextItem();
            Database currentDatabase = Factory.GetDatabase(itemToCheck.Database.Name);
            Assert.IsNotNull(currentDatabase, itemToCheck.Database.Name);
            string print = String.Empty;
            using (new SecurityDisabler())
            {
                ItemLink[] referrers = Globals.LinkDatabase.GetReferrers(itemToCheck);
                if (referrers.Length > 0)
                    foreach (ItemLink itemLink in referrers)
                    {
                        ID sourceItemID = itemLink.SourceItemID;
                        Item sourceItem = currentDatabase.GetItem(sourceItemID);
                        sourceItem.Fields.ReadAll();
                        FieldCollection fieldCollection = sourceItem.Fields;
                        var sourceFieldId = itemLink.SourceFieldID;
                        var sourcePath = currentDatabase.GetItem(itemLink.SourceItemID).Paths.FullPath;
                        var sourceFieldName = fieldCollection.Where(f => f.ID == sourceFieldId).FirstOrDefault().Name;
                        //IEnumerable<Field> fieldCollection = currentDatabase.GetItem(itemLink.SourceItemID).Fields.Where(f => f.ID == sourceFieldId);
                        print += "Referrer Item: " + sourcePath + "\nReferrer Field: " + sourceFieldName + "\n";
                    }
            }
            return (object)print;
        }
    }
}