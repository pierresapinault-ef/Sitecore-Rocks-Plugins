using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Composition;
using Sitecore.Rocks.Server.QueryAnalyzers;
using Sitecore.Rocks.Server.QueryAnalyzers.Opcodes;
using Sitecore.Rocks.Server.QueryAnalyzers.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Keywords
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Keywords.SelectKeyword))]

    [ReservedWord("from", 203)]
    [ReservedWord("by", 232)]
    [ReservedWord("order", 231)]
    [Keyword("select", 201, Example = "select * from //*[@@templatename='Sample Item'];\nselect @Text + \" text\" as TextField from /sitecore/content//* order by @Text desc", LongHelp = "Performs a query and displays the results in a grid.\n\nThe Order By clause operates on items - not on selected columns.", ShortHelp = "Performs a query", Syntax = "'select' ['distinct'] ('*' | Fields) ['from' Expression] [order by FieldNames]\n    Fields = Field | (Fields ',' Field)\n    Field = Expression ['as' Identifier]\n    FieldNames = FieldName ['asc' | 'desc'] | (FieldNames ',' FieldName)")]
    [ReservedWord("search", 208)]
    [ReservedWord("as", 206)]
    [ReservedWord("desc", 234)]
    [ReservedWord("asc", 233)]
    [ReservedWord("distinct", 235)]
    [ReservedWord("AllFields", 297)]

    public class NewSelectKeyword : IQueryAnalyzerKeyword
    {
        public Opcode Parse(Parser parser)
        {
            Assert.ArgumentNotNull((object)parser, "parser");
            parser.Match(201, "\"select\" expected");
            bool isDistinct = false;
            if (parser.Token.Type == 235)
            {
                parser.Match();
                isDistinct = true;
            }
            bool isAllFields = false;
            if (parser.Token.Type == 297)
            {
                parser.Match();
                isAllFields = true;

            }
            List<ColumnExpression> selectColumns = this.GetSelectColumns(parser);
            Opcode from = (Opcode)null;
            if (parser.Token.Type == 203)
                from = parser.GetFrom();
            IEnumerable<OrderByColumn> orderBy = (IEnumerable<OrderByColumn>)null;
            if (parser.Token.Type == 231)
                orderBy = this.GetOrderBy(parser, selectColumns);
            return (Opcode)new NewSelect(selectColumns, from, orderBy, isDistinct, isAllFields);
        }

        private OrderByColumn GetOrderByColumn(Parser parser)
        {
            string columnName = parser.Token.Value;
            parser.Match(21, "Column name expected (do not include @)");
            int direction = 1;
            if (parser.Token.Type == 233)
            {
                parser.Match();
                direction = 1;
            }
            else if (parser.Token.Type == 234)
            {
                parser.Match();
                direction = -1;
            }
            return new OrderByColumn(columnName, direction);
        }

        private IEnumerable<OrderByColumn> GetOrderBy(Parser parser, List<ColumnExpression> selectFields)
        {
            parser.Match(231, "\"order\" expected");
            parser.Match(232, "\"by\" expected");
            List<OrderByColumn> list = new List<OrderByColumn>();
            OrderByColumn orderByColumn1 = this.GetOrderByColumn(parser);
            list.Add(orderByColumn1);
            while (parser.Token.Type == 12)
            {
                parser.Match();
                OrderByColumn orderByColumn2 = this.GetOrderByColumn(parser);
                list.Add(orderByColumn2);
            }
            for (int index1 = 0; index1 < list.Count; ++index1)
            {
                string column0Name = list[index1].ColumnName;
                if (!Enumerable.Any<ColumnExpression>((IEnumerable<ColumnExpression>)selectFields, (Func<ColumnExpression, bool>)(f => f.ColumnName == column0Name)))
                    parser.Raise(string.Format("Order By column \"{0}\" is not part of the selection", (object)column0Name));
                for (int index2 = index1 + 1; index2 < list.Count; ++index2)
                {
                    OrderByColumn orderByColumn2 = list[index2];
                    if (orderByColumn2.ColumnName == column0Name)
                        parser.Raise(string.Format("Order By column \"{0}\" is already specified", (object)orderByColumn2.ColumnName));
                }
            }
            return (IEnumerable<OrderByColumn>)list;
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
                if (string.Compare(fieldElement.Name, "@path", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "Path";
                    columnExpression.FieldName = (string)null;
                    columnExpression.Expression = (Opcode)new ItemPath();
                }
                else if (string.Compare(fieldElement.Name, "@version", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "Version";
                    columnExpression.FieldName = (string)null;
                    columnExpression.Expression = (Opcode)new ItemVersion();
                }
                else if (string.Compare(fieldElement.Name, "@language", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "Language";
                    columnExpression.FieldName = (string)null;
                    columnExpression.Expression = (Opcode)new ItemLanguage();
                }
                else if (string.Compare(fieldElement.Name, "@database", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "Database";
                    columnExpression.FieldName = (string)null;
                    columnExpression.Expression = (Opcode)new ItemDatabase();
                }
                else if (string.Compare(fieldElement.Name, "@shortid", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    columnExpression.ColumnName = "ShortId";
                    columnExpression.FieldName = (string)null;
                    columnExpression.Expression = (Opcode)new ItemShortId();
                }
                else if (!fieldElement.Name.StartsWith("@"))
                {
                    columnExpression.ColumnName = fieldElement.Name;
                    columnExpression.FieldName = fieldElement.Name;
                    columnExpression.Expression = (Opcode)null;
                }
            }
            if (parser.Token.Type == 206)
            {
                parser.Match();
                columnExpression.ColumnName = parser.Token.Value;
                parser.Match(21, "Identifier expected");
            }
            return columnExpression;
        }

        private List<ColumnExpression> GetSelectColumns(Parser parser)
        {
            List<ColumnExpression> columnExpressions = new List<ColumnExpression>();
            if (parser.Token.Type == 30)
            {
                this.GetStarColumns(parser, columnExpressions);
                return columnExpressions;
            }
            ColumnExpression selectColumn1 = this.GetSelectColumn(parser);
            string columnName = selectColumn1.ColumnName;
            if (Enumerable.Any<ColumnExpression>((IEnumerable<ColumnExpression>)columnExpressions, (Func<ColumnExpression, bool>)(c => c.ColumnName == columnName)))
                throw new ParseException(string.Format("Column \"{0}\" is already defined. Use the \"as\" operator to give a column a new name.", (object)columnName));
            columnExpressions.Add(selectColumn1);
            while (parser.Token.Type == 12)
            {
                parser.Match();
                ColumnExpression selectColumn2 = this.GetSelectColumn(parser);
                string name = selectColumn2.ColumnName;
                if (Enumerable.Any<ColumnExpression>((IEnumerable<ColumnExpression>)columnExpressions, (Func<ColumnExpression, bool>)(c => c.ColumnName == name)))
                    throw new ParseException(string.Format("Column \"{0}\" is already defined. Use the \"as\" operator to give a column a new name.", (object)name));
                columnExpressions.Add(selectColumn2);
            }
            return columnExpressions;
        }

        private void GetStarColumns(Parser parser, List<ColumnExpression> columnExpressions)
        {
            parser.Match();
            columnExpressions.Add(new ColumnExpression()
            {
                ColumnName = "Name",
                Expression = (Opcode)new FieldElement("@name")
            });
            columnExpressions.Add(new ColumnExpression()
            {
                ColumnName = "ID",
                Expression = (Opcode)new FieldElement("@id")
            });
            columnExpressions.Add(new ColumnExpression()
            {
                ColumnName = "Template Name",
                Expression = (Opcode)new FieldElement("@templatename")
            });
            columnExpressions.Add(new ColumnExpression()
            {
                ColumnName = "Path",
                Expression = (Opcode)new ItemPath()
            });
        }
        private void GetAllFields(Parser parser, List<ColumnExpression> columnExpressions)
        {
            parser.Match();
        }
    }
}