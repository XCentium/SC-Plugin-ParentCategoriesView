namespace XCentium.Commerce.Plugin.AddParentCategoriesToSellableItem.Pipelines.Blocks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Pipelines;
    using KnownCatalogActionsPolicy = Policies.KnownCatalogActionsPolicy;
    using KnownCatalogViewsPolicy = Policies.KnownCatalogViewsPolicy;

    [PipelineDisplayName("XCentium.Catalog.block.PopulateParentCategoriesViewActions")]
    public class PopulateParentCategoriesViewActionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<KnownCatalogViewsPolicy>();
            if (string.IsNullOrEmpty(entityView?.Name) || !entityView.Name.Equals(policy.ParentCategories, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(entityView);

            var actionPolicy = entityView.GetPolicy<ActionsPolicy>();
            var entity = context.CommerceContext.GetObjects<CommerceEntity>().FirstOrDefault(e => e.Id.Equals(entityView.EntityId)) ?? context.CommerceContext.GetObjects<EntityViewArgument>().FirstOrDefault()?.Entity;
            if (!(entity is SellableItem))
                return Task.FromResult(entityView);

            var sellableItem = entity as SellableItem;
            var disassociateActionView = new EntityActionView
            {
                Name = context.GetPolicy<KnownCatalogActionsPolicy>().DisassociateItemFromCategory,
                DisplayName = "Disassociate Sellable Item from category",
                Description = "Disassociate item from Parent Category",
                IsEnabled = sellableItem.ParentCategoryList.Split('|').Any(),
                EntityView = string.Empty,
                RequiresConfirmation = true,
                Icon = "link_broken"
            };
            actionPolicy.Actions.Add(disassociateActionView);
            return Task.FromResult(entityView);
        }    
    }
}
