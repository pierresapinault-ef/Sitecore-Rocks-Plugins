using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Data.Archiving;
using System.Collections;
using Sitecore.Rocks.Server.Jobs;
using System;
using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class EmptyRecycleBin : Opcode
    {

        public override object Evaluate(Sitecore.Data.Query.Query query, QueryContext contextNode)
        {
            Assert.ArgumentNotNull((object)query, "query");
            if (!Context.User.IsAdministrator)
            {
                throw new QueryException("You need to have admin rights to use this function");
            }
            string archiveName = "recyclebin";
            var database = Sitecore.Data.Database.GetDatabase("master");
            Archive archive = Sitecore.Data.Archiving.ArchiveManager.GetArchive(archiveName, database);
            var print = archive.GetEntryCount() + " items being removed";

            BackgroundJob.Run("Remove items from RecycleBin", "Delete Items", (Action)(() => this.RemoveEntries(archive, database)));
            return (object)print;
        }
        public void RemoveEntries(Archive archive, Sitecore.Data.Database db)
        {
            IEnumerable<ArchiveEntry> archiveList = archive.GetEntries(1, int.MaxValue);
            foreach (ArchiveEntry entry in archiveList)
            {
                Sitecore.Data.ID archiveID = Sitecore.Data.ID.Parse(entry.ArchivalId);
                archive.RemoveEntries(archiveID);
            }
        }
    }
}