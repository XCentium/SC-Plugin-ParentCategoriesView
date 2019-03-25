namespace XCentium.Commerce.Plugin.AddParentCategoriesToSellableItem.Policies
{
    public class KnownCatalogActionsPolicy : Sitecore.Commerce.Plugin.Catalog.KnownCatalogActionsPolicy
    {
        public string DisassociateItemFromCategory = nameof(DisassociateItemFromCategory);
    }
}
