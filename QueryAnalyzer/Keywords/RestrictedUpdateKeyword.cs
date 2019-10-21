using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Composition;
using Sitecore.Rocks.Server.QueryAnalyzers;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Keywords.UpdateKeyword))] 
    [ReservedWord("reset", 236)]
    [Keyword("update", 205, Example = "TEST update set @Text = \"Hello\" from /sitecore/content//*[@@templatename = \"Sample Item\"]", 
        LongHelp = "Updates fields in items.", 
        ShortHelp = "Updates item fields", 
        Syntax = "'update' ['set' | 'reset'] Fields ['from' Expression]\n    Fields = Fields | (Fields ',' Field)\n    Field = Attribute '=' Expression\n")]
    public class RestrictedUpdateKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(205, "\"update\" expected");
            if (parser.Token.Type == 236)
            {
                parser.Match(236, "\"reset\" expected");
                List<string> resetColumns = this.GetResetColumns(parser);
                Opcode from = (Opcode)null;
                if (parser.Token.Type == 203)
                    from = parser.GetFrom();
                return (Opcode)new Reset(resetColumns, from);
            }
            else
            {
                parser.Match(207, "\"set\" expected");
                List<ColumnExpression> updateColumns = this.GetUpdateColumns(parser);
                Opcode from = (Opcode)null;
                if (parser.Token.Type == 203)
                    from = parser.GetFrom();
                return (Opcode)new RestrictedUpdate(updateColumns, from);
            }
        }

        private string GetColumnName(Parser parser)
        {
            FieldElement fieldElement = parser.GetAttribute() as FieldElement;
            if (fieldElement != null)
                return fieldElement.Name;
            parser.Raise("Field name expected");
            return string.Empty;
        }

        private List<string> GetResetColumns(Parser parser)
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

        private ColumnExpression GetUpdateColumn(Parser parser)
        {
            string columnName = this.GetColumnName(parser);
            parser.Match(34, "'=' expected");
            Opcode expression = parser.GetExpression();
            ColumnExpression columnExpression = new ColumnExpression()
            {
                ColumnName = columnName,
                Expression = expression
            };
            FieldElement fieldElement = columnExpression.Expression as FieldElement;
            if (fieldElement != null)
            {
                columnExpression.ColumnName = fieldElement.Name;
                columnExpression.FieldName = fieldElement.Name;
                columnExpression.Expression = (Opcode)null;
            }
            return columnExpression;
        }

        private List<ColumnExpression> GetUpdateColumns(Parser parser)
        {
            List<ColumnExpression> list = new List<ColumnExpression>();
            ColumnExpression updateColumn1 = this.GetUpdateColumn(parser);
            list.Add(updateColumn1);
            while (parser.Token.Type == 12)
            {
                parser.Match();
                ColumnExpression updateColumn2 = this.GetUpdateColumn(parser);
                list.Add(updateColumn2);
            }
            return list;
        }
    }
}