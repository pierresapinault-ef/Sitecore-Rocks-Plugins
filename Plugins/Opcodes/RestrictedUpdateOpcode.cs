using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;
using Sitecore.Security.Accounts;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class RestrictedUpdate : Opcode
    {
        public List<ColumnExpression> ColumnExpressions { get; set; }

        public Opcode From { get; set; }

        public RestrictedUpdate(List<ColumnExpression> columnExpressions, Opcode from)
        {
            Assert.ArgumentNotNull((object)columnExpressions, "columnExpressions");
            this.ColumnExpressions = columnExpressions;
            this.From = from;
        }

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            Assert.ArgumentNotNull((object)contextNode, "contextNode");
            Role developer = Security.Accounts.Role.FromName("sitecore\\Developer");
            if (Context.User.IsAdministrator == false)
            {
                throw new QueryException("You don't have privilege to use this function within the Query Analyzer");
            }

            object result = (object)contextNode;
            Opcode from = this.From;
            if (from != null)
            {
                result = QueryExtensions.Evaluate(query, from, contextNode);
                if (result == null)
                    return (object)QueryExtensions.FormatItemsAffected(query, 0);
            }
            IEnumerable<Item> items = QueryExtensions.GetItems(query, result);
            foreach (Item obj in items)
                this.UpdateItem(query, obj);
            return (object)QueryExtensions.FormatItemsAffected(query, Enumerable.Count<Item>(items));
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

        private void ChangeTemplateById(Item item, string value)
        {
            Item obj = item.Database.GetItem(value);
            Assert.IsNotNull((object)obj, "Template \"" + value + "\" not found");
            item.ChangeTemplate((TemplateItem)obj);
        }

        private void ChangeTemplateByName(Item item, string value)
        {
            Template template = TemplateManager.GetTemplate(value, item.Database);
            Assert.IsNotNull((object)template, "Template \"" + value + "\" not found");
            Item obj = item.Database.GetItem(template.ID);
            Assert.IsNotNull((object)obj, "Template \"" + value + "\" not found");
            item.ChangeTemplate((TemplateItem)obj);
        }

        private void UpdateItem(Query query, Item item)
        {
            bool flag = false;
            foreach (ColumnExpression columnExpression in this.ColumnExpressions)
            {
                string str = (string)null;
                string columnName = columnExpression.ColumnName;
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (columnExpression.FieldName != null)
                        str = item[columnExpression.FieldName];
                    else if (columnExpression.Expression != null)
                    {
                        object obj = QueryExtensions.EvaluateSubQuery(query, columnExpression.Expression, item);
                        str = obj != null ? obj.ToString() : string.Empty;
                    }
                    if (str != null)
                    {
                        if (!flag)
                        {
                            item.Editing.BeginEdit();
                            flag = true;
                        }
                        switch (columnName.ToLowerInvariant())
                        {
                            case "@name":
                                item.Name = str;
                                continue;
                            case "@templatename":
                                this.ChangeTemplateByName(item, str);
                                continue;
                            case "@templateid":
                                this.ChangeTemplateById(item, str);
                                continue;
                            default:
                                Pipeline<SetFieldValuePipeline>.Run().WithParameters(item, columnName, str);
                                continue;
                        }
                    }
                }
            }
            if (!flag)
                return;
            item.Editing.EndEdit();
        }
    }
}