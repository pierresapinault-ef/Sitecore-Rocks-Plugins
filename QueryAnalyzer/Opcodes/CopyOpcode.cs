using System;
using System.Linq;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Copy : Opcode
    {
        public Copy(Opcode from, string to, string type)
        {
            Assert.ArgumentNotNull(to, "to");
            Assert.ArgumentNotNull(type, "type");
            Assert.ArgumentNotNull(@from, "from");
            From = from;
            To = to;
            Type = type;
        }

        protected Opcode From { get; set; }
        protected string To { get; set; }
        protected string Type { get; set; }

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, "query");
            Assert.ArgumentNotNull(contextNode, "contextNode");
            return query.FormatItemsAffected(Execute(query, contextNode));
        }

        public override void Print(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");
            PrintIndent(output);
            PrintLine(output, GetType().Name);
            if (From == null)
                return;
            ++output.Indent;
            From.Print(output);
            --output.Indent;
        }

        //public string Execute(string databaseName, string itemId, string newParentId)
        private int Execute(Query query, QueryContext contextNode)
        {
            var database = contextNode.GetQueryContextItem().Database;

            //get the source path
            object result = contextNode;
            var from = From;
            if (from != null)
            {
                result = query.Evaluate(from, contextNode);
            }
            var items = query.GetItems(result);
            var list = query.GetItems(result).ToList();
            Item destination = null;

            //get the destination item
            if (Type == "path")
            {
                destination = database.GetItem(To);
            }
            if (Type == "id")
            {
                var itemId = ID.Parse(To);
                destination = database.GetItem(itemId);
            }


            foreach (var item in list)
            {
                if (item == null)
                    throw new Exception("Item not found");
                if (destination == null)
                    throw new Exception("Parent item not found");
                CopyItem(item.ID.ToString(),destination.ID,item.Name,database.Name);
            }
            return list.Count();
        }

        public void CopyItem(string id, ID newParent, string name, string databaseName)
        {
            Assert.ArgumentNotNull(id, "id");
            Assert.ArgumentNotNull(newParent, "newParent");
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(databaseName, "databaseName");
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
                throw new Exception("Database not found");
            if (database.GetItem(id) == null)
                throw new Exception("Item not found");
            name = "copy-of-" + name;
            var obj = database.GetItem(id).CopyTo(database.Items[newParent], name);
        }
    }
}