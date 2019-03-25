namespace XCentium.Commerce.Plugin.AddParentCategoriesToSellableItem.Policies
{
    public class KnownCatalogViewsPolicy : Sitecore.Commerce.Plugin.Catalog.KnownCatalogViewsPolicy
    {
        public string ParentCategories { get; set; } = nameof(ParentCategories);

        public string FairMarketValue { get; set; } = nameof(FairMarketValue);
    }
}
