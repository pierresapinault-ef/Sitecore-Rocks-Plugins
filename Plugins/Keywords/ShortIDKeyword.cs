using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.QueryAnalyzers;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    class ShortIDKeyword : Sitecore.Rocks.Server.QueryAnalyzers.Keywords.SelectKeyword
    {
        private void GetStarColumns(Parser parser, List<ColumnExpression> columnExpressions)
        {
            parser.Match();
            columnExpressions.Add(new ColumnExpression()
            {
                ColumnName = "Short ID",
                Expression = (Opcode)new FieldElement("@shortid")
            });
        }
        private ColumnExpression GetSelectColumn(Parser parser)
        {
            ColumnExpression columnExpression = new ColumnExpression()
            {
                Expression = parser.GetExpression()
            };
            FieldElement fieldElement = columnExpression.Expression as FieldElement;
            if (fieldElement != null)
            {
                if (string.Compare(fieldElement.Name, "@shortid", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "Short ID";
                    columnExpression.FieldName = (string)null;
                    columnExpression.Expression = (Opcode)new ShortID();
                }
                if (string.Compare(fieldElement.Name, "@id", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "ID";
                    columnExpression.FieldName = (string)null;
                }
            }
            return columnExpression;
        }
    }
}