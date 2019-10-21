using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensibility.Composition;
using System.Linq;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    [Unexport(typeof(Sitecore.Rocks.Server.QueryAnalyzers.Opcodes.Delete))]
    public class Newdelete : ItemsOpcode
    {
        public Newdelete(Opcode from)
            : base(from)
        {
        }
        protected override void Execute(Item item)
        {
            string[] products = new string[] { "ils", "aya", "ilsh", "upa", "lsp", "lt", "highschool", "efc", "loc","academy"};
            if (products.Any(item.Name.Equals))
            {
                throw new QueryException(string.Format("You cannot delete this item : {0}, {1}",item.Name, item.Paths.Path));
            }
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to be an admin to delete with the Query Analyzer");
            }
            if (item.ID == ItemIDs.RootID)
            {
                throw new QueryException("Deleting the root item is too scary. Please wipe the database another way.");
            }
            item.Recycle();
            string[] publishParams = { Context.User.Name, item.Paths.FullPath, item.ID.ToString() };
            Log.Info(string.Format("({0}) -> delete item: {1}, id: {2}", publishParams), Context.User);
        }
    }
}