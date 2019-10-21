namespace Sitecore.Rocks.Server.Plugins
{
    using Sitecore.VisualStudio.Commands;
    using Sitecore.VisualStudio.ContentTrees;
    using Sitecore.VisualStudio.ContentTrees.Items;

    /// <summary>Defines the content tree command class.</summary>
    [Command(Submenu = "Clipboard")]
    public class ShortIDCmd : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortIDCmd"/> class.
        /// </summary>
        public ShortIDCmd()
        {
            this.Text = "Copy ShortID";
            this.Group = "Clipboard";
            this.SortingValue = 2020;
        }

        #endregion

        #region Public Methods

        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }
            //if (context.SelectedItems.Count() == 1 &&
            //    !context.SelectedItems.OfType<DatabaseTreeViewItem>().Any() &&
            //    !context.SelectedItems.OfType<SiteTreeViewItem>().Any())
            return true;
        }

        /// <summary>Execute the command.</summary>
        /// <param name="parameter">The parameter.</param>
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

        #endregion
    }
}