using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Keyword("CopyFieldValues", 287, Example = "CopyFieldValues \nlanguages ('en','fi|sv|fr')\nitem ('SourceItemID','TargetItemID')\nsource fields(@Title, @Text)\ntarget fields (@Title, @Text)", 
        LongHelp = "Copy field values from a source item to a target item\nfrom a source language to one or multiple target language(s)\nfrom source fields to target fields", 
        ShortHelp = "Copy field values",
        Syntax = "'CopyFieldValues' 'languages' '('sourcelanguage,targetlanguages')' '\nlanguages = ('sourcelanguage','targetlanguage1|targetlanguage2|...') \nitem = ('sourceID' | 'targetID')\nsource fields ('Field | Fields')\ntarget fields ('Field | Fields')")]
    [ReservedWord("item", 284)]
    [ReservedWord("fields", 288)]
    [ReservedWord("source", 289)]
    [ReservedWord("languages", 290)]
    [ReservedWord("target", 291)]

    public class CopyFieldValuesKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(287, "\"CopyFieldValues\" expected");
            //get the languages
            parser.Match(290, "\"languages\" expected");
            parser.Match(32, "'(' expected");
            string sourceLanguage = null;
            if (parser.Token.Type == 22)
            {
                parser.Match(22, "source language expected");
                sourceLanguage = parser.Token.Value;
            }
            parser.Match(12, "\"comma\" expected");
            string targetLanguages = null;
            if (parser.Token.Type == 22)
            {
                parser.Match(22, "target language(s) expected");
                targetLanguages = parser.Token.Value;
            }
            parser.Match(18, "')' expected");
            //get source and target item IDs
            parser.Match(284,"'item' expected");
            parser.Match(32, "'(' expected");
            string sourceItem = null;
            if (parser.Token.Type == 22)
            {
                parser.Match(22, "source item ID expected");
                sourceItem = parser.Token.Value;
            }
            parser.Match(12, "\"comma\" expected");
            string targetItem = null;
            if (parser.Token.Type == 22)
            {
                parser.Match(22, "target item ID expected");
                targetItem = parser.Token.Value;
            }
            parser.Match(18, "')' expected");
            //get source fields
            parser.Match(289, "'source' expected");
            parser.Match(288, "'fields' expected");
            parser.Match(32, "'(' expected");
            List<string> sourceList = this.GetInsertColumns(parser);
            parser.Match(18, "')' expected");
            //get target fields
            parser.Match(291, "'target' expected");
            parser.Match(288, "'fields' expected");
            parser.Match(32, "'(' expected");
            List<string> targetList = this.GetInsertColumns(parser);
            parser.Match(18, "')' expected");
            if (sourceList.Count != targetList.Count)
                parser.Raise("Number of columns and number of values do not match");
            return (Opcode)new CopyFieldValuesOpcode(sourceList, targetList, sourceLanguage, targetLanguages, sourceItem,targetItem);
        }

        private List<string> GetInsertColumns(Parser parser)
        {
            List<string> list = new List<string>();
            list.Add(this.GetColumnName(parser));
            while (parser.Token.Type == 12)
            {
                parser.Match();
                list.Add(this.GetColumnName(parser));
            }
            return list;
        }

        private string GetColumnName(Parser parser)
        {
            FieldElement fieldElement = parser.GetAttribute() as FieldElement;
            if (fieldElement != null)
                return fieldElement.Name;
            parser.Raise("Field name expected");
            return string.Empty;
        }
    }
}
