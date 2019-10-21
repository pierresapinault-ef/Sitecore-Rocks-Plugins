using System.Linq;
using Sitecore.VisualStudio.Commands;
using Sitecore.VisualStudio.ContentTrees;
using Sitecore.VisualStudio.ContentTrees.Items;

namespace Sitecore.VisualStudio.UI.TemplateDesigner.Commands
{

    [Command(Submenu = "Clipboard")]
    public class CopyShortIDCommand : CommandBase
    {
        public CopyShortIDCommand()
        {
            Text = "Copy ShortID";
            Group = "clipboard";
            SortingValue = 2020;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            return context != null &&
                   context.SelectedItems.Count() == 1 &&
                   !context.SelectedItems.OfType<DatabaseTreeViewItem>().Any() &&
                   !context.SelectedItems.OfType<SiteTreeViewItem>().Any();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            var itemTreeViewItem = context
                .ContentTree
                .SelectedItems
                .OfType<ItemTreeViewItem>()
                .First();
            AppHost.Server.GetItemHeader(itemTreeViewItem.ItemUri,
                value => AppHost.Clipboard.SetText(value.ItemId.ToShortId()));
        }
    }
}