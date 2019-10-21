using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Composition;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.QueryAnalyzers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.UI;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Opcodes.Select))]

    public class NewSelect : Opcode
    {
        public List<ColumnExpression> ColumnExpressions { get; set; }

        public Opcode From { get; set; }

        public bool IsDistinct { get; set; }

        public bool IsAllFields { get; set; }          

        public IEnumerable<OrderByColumn> OrderBy { get; set; }

        public NewSelect(List<ColumnExpression> columnExpressions, Opcode from, IEnumerable<OrderByColumn> orderBy, bool isDistinct, bool isAllFields)
        {
            Assert.ArgumentNotNull((object)columnExpressions, "columnExpressions");
            this.ColumnExpressions = columnExpressions;
            this.From = from;
            this.OrderBy = orderBy ?? Enumerable.Empty<OrderByColumn>();
            this.IsDistinct = isDistinct;
            this.IsAllFields = isAllFields;
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string strA, string strB);

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            SelectDataTable selectDataTable = new SelectDataTable();
            this.BuildColumns(selectDataTable);
            object obj = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                obj = QueryExtensions.Evaluate(query, from, contextNode);
                if (obj == null)
                    return (object)selectDataTable;
            }
            QueryContext[] queryContextArray = obj as QueryContext[];
            if (queryContextArray != null)
            {
                foreach (QueryContext queryContext in queryContextArray)
                {
                    if (!this.AddBatchScriptItem(query, selectDataTable, queryContext.GetQueryContextItem(), Enumerable.Count<QueryContext>((IEnumerable<QueryContext>)queryContextArray)))
                        break;
                }
                if (this.IsDistinct)
                    this.MakeDistinct(selectDataTable);
                if (Enumerable.Any<OrderByColumn>(this.OrderBy))
                    this.Sort(selectDataTable);
                return (object)selectDataTable;
            }
            QueryContext queryContext1 = obj as QueryContext;
            if (queryContext1 != null)
                this.AddBatchScriptItem(query, selectDataTable, queryContext1.GetQueryContextItem(), 1);
            return (object)selectDataTable;
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

        private bool AddBatchScriptItem(Query query, SelectDataTable dataTable, Item item, int count)
        {
            bool flag = true;
            SelectItem selectItem = new SelectItem()
            {
                Item = item
            };
            int num = 0;
            foreach (ColumnExpression columnExpression in this.ColumnExpressions)
            {
                SelectField selectField = new SelectField();
                if (columnExpression.FieldName != null)
                {
                    selectField.Value = item[columnExpression.FieldName];
                    selectField.Field = item.Fields[columnExpression.FieldName];
                }
                else if (columnExpression.Expression != null)
                {
                    object obj = QueryExtensions.EvaluateSubQuery(query, columnExpression.Expression, item);
                    if (obj != null)
                    {
                        selectField.Value = obj.ToString();
                    }
                    else
                    {
                        Function function = columnExpression.Expression as Function;
                        if (function != null && string.Compare(function.Name, "count", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            selectField.Value = count.ToString();
                            flag = false;
                        }
                        else
                            selectField.Value = "<null>";
                    }
                }
                string str = columnExpression.ColumnName;
                if (string.IsNullOrEmpty(str))
                {
                    str = "Column " + (object)num;
                    ++num;
                }
                selectField.ColumnName = str;
                selectItem.Fields.Add(selectField);
            }
            dataTable.Items.Add(selectItem);
            return flag;
        }

        private void BuildColumns(SelectDataTable result)
        {
            int num = 0;
            foreach (ColumnExpression columnExpression in this.ColumnExpressions)
            {
                string str = columnExpression.ColumnName;
                if (string.IsNullOrEmpty(str))
                {
                    str = "Column " + (object)num;
                    ++num;
                }
                SelectColumn selectColumn = new SelectColumn()
                {
                    Header = str,
                    IsReadOnly = string.IsNullOrEmpty(columnExpression.FieldName)
                };
                result.Columns.Add(selectColumn);
            }
        }

        private int Compare(SelectItem x, SelectItem y)
        {
            foreach (OrderByColumn orderByColumn in this.OrderBy)
            {
                string orderByValue1 = this.GetOrderByValue(x, orderByColumn.ColumnName);
                string orderByValue2 = this.GetOrderByValue(y, orderByColumn.ColumnName);
                if (orderByValue1 == null && orderByValue2 == null)
                    return 0;
                if (orderByValue1 == null)
                    return -1 * orderByColumn.Direction;
                if (orderByValue2 == null)
                    return orderByColumn.Direction;
                int num = Select.StrCmpLogicalW(orderByValue1, orderByValue2);
                if (num != 0)
                    return num * orderByColumn.Direction;
            }
            return 0;
        }

        private int GetHashValue(SelectItem item)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (SelectField selectField in item.Fields)
            {
                stringBuilder.Append("|");
                stringBuilder.Append(selectField.ColumnName);
                stringBuilder.Append(":");
                stringBuilder.Append(selectField.Value);
            }
            return stringBuilder.ToString().GetHashCode();
        }

        private string GetOrderByValue(SelectItem item, string columnName)
        {
            SelectField selectField = Enumerable.FirstOrDefault<SelectField>((IEnumerable<SelectField>)item.Fields, (Func<SelectField, bool>)(f => f.ColumnName == columnName));
            if (selectField == null)
                return (string)null;
            return selectField.Value;
        }

        private void MakeDistinct(SelectDataTable dataTable)
        {
            List<Pair<int, SelectItem>> list = new List<Pair<int, SelectItem>>();
            foreach (SelectItem part2 in dataTable.Items)
            {
                Pair<int, SelectItem> pair = new Pair<int, SelectItem>(this.GetHashValue(part2), part2);
                list.Add(pair);
            }
            list.Sort((Comparison<Pair<int, SelectItem>>)((p0, p1) =>
            {
                if (p0.Part1 == p1.Part1)
                    return 0;
                return p0.Part1 <= p1.Part1 ? -1 : 1;
            }));
            for (int index = list.Count - 2; index >= 0; --index)
            {
                if (list[index].Part1 == list[index + 1].Part1)
                    list.RemoveAt(index + 1);
            }
            dataTable.Items.Clear();
            dataTable.Items.AddRange(Enumerable.Select<Pair<int, SelectItem>, SelectItem>((IEnumerable<Pair<int, SelectItem>>)list, (Func<Pair<int, SelectItem>, SelectItem>)(p => p.Part2)));
        }

        private void Sort(SelectDataTable dataTable)
        {
            dataTable.Items.Sort(new Comparison<SelectItem>(this.Compare));
        }
    }
}
