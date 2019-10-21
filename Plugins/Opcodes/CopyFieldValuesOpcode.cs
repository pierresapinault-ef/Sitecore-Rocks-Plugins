using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class CopyFieldValuesOpcode : Opcode
    {
        public List<string> SourceList { get; set; }

        public List<string> TargetList { get; set; }

        public string SourceLanguage { get; set; }

        public string TargetLanguages { get; set; }

        public string SourceItem { get; set; }

        public string TargetItem { get; set; }

        public CopyFieldValuesOpcode(List<string> sourceList, List<string> targetList, string sourceLanguage, string targetLanguages, string sourceItem, string targetItem)
        {
            Assert.ArgumentNotNull((object)sourceList, "columns");
            Assert.ArgumentNotNull((object)targetList, "values");
            this.SourceList = sourceList;
            this.TargetList = targetList;
            this.SourceLanguage = sourceLanguage;
            this.TargetLanguages = targetLanguages;
            this.SourceItem = sourceItem;
            this.TargetItem = targetItem;
        }

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            Sitecore.Data.Database database = contextNode.GetQueryContextItem().Database;
            char[] chArray = new char[1]
            {
                '|'
            };
            //creating a list with all the target languages
            List<Language> languagesList = new List<Language>();
            foreach (string language in TargetLanguages.Split(chArray))
            {
                if (!string.IsNullOrEmpty(language))
                {
                    Language lang = LanguageManager.GetLanguage(language);
                    languagesList.Add(lang);
                }
            }
            Item sourceItem = null;
            Item targetItem = null;
            List<Item> targetItemList = new List<Item>();

            //retrieving source and target items
            Language sourceLng = LanguageManager.GetLanguage(SourceLanguage);
            sourceItem = database.GetItem(Sitecore.Data.ID.Parse(SourceItem), sourceLng);
            if (sourceItem == null)
                throw new QueryException("source item ID missing");

            foreach (Language lng in languagesList)
            {
                targetItem = database.GetItem(Sitecore.Data.ID.Parse(TargetItem), lng);
                targetItemList.Add(targetItem);
                if (targetItem == null)
                    throw new QueryException("target item ID missing");
            }
            if (targetItemList.Count == 0)
                throw new QueryException("target item ID missing");

            this.UpdateFieldValues(sourceItem, targetItemList);

            return (object)QueryExtensions.FormatItemsAffected(query, 1);
        }

        private void UpdateFieldValues(Item sourceItem, List<Item> targetItemList)
        {
            using (new Sitecore.Data.BulkUpdateContext())
            foreach (Item item in targetItemList)
            {
                item.Editing.BeginEdit();
                for (int index = 0; index < this.TargetList.Count; ++index)
                {             
                    string sourceField = this.SourceList[index];
                    string targetField = this.TargetList[index];
                    string fieldValue = sourceItem.Fields[sourceField].Value;
                    Pipeline<SetFieldValuePipeline>.Run().WithParameters(item, targetField, fieldValue);
                }
                item.Editing.EndEdit();
            }
        }
    }
}