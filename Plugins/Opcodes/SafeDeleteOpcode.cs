using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensibility.Composition;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Opcodes.Delete))]
    public class newdelete : ItemsOpcode
    {
        public newdelete(Opcode from)
            : base(from)
        {
        }
        protected override void Execute(Item item)
        {
            string[] products = new string[] { "ils", "aya", "ilsh", "upa", "lsp", "lt", "highschool", "efc", "loc"};
            if (products.Any(item.Name.Contains))
            {
                throw new QueryException(string.Format("You cannot delete this item : {0}",item.Name));
            }
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to be an admin to delete with the Query Analyzer");
            }
            if (item.ID == ItemIDs.RootID)
            {
                throw new QueryException("Deleting the root item is too scary. Please wipe the database another way.");
            }
            using (new Sitecore.Data.BulkUpdateContext())
            item.Recycle();
            string[] publishParams = { Context.User.Name.ToString(), item.Paths.FullPath.ToString(), item.ID.ToString() };
            Log.Info(string.Format("({0}) -> delete item: {1}, id: {2}", publishParams), Context.User);
        }
    }
}