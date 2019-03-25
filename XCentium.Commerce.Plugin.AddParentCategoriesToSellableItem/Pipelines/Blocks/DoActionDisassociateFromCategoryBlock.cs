namespace XCentium.Commerce.Plugin.AddParentCategoriesToSellableItem.Pipelines.Blocks
{
    using System;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using KnownCatalogActionsPolicy = Policies.KnownCatalogActionsPolicy;

    [PipelineDisplayName("XCentium.Catalog.block.DoActionDisassociateFromCategory")]
    public class DoActionDisassociateFromCategoryBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly DeleteRelationshipCommand _deleteRelationshipCommand;

        public DoActionDisassociateFromCategoryBlock(DeleteRelationshipCommand deleteRelationshipCommand)
        {
            this._deleteRelationshipCommand = deleteRelationshipCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");
            if (string.IsNullOrEmpty(entityView.Action) || !entityView.Action.Equals(context.GetPolicy<KnownCatalogActionsPolicy>().DisassociateItemFromCategory, StringComparison.OrdinalIgnoreCase))
                return entityView;

            if (!(entityView.EntityId.StartsWith(CommerceEntity.IdPrefix<SellableItem>()) &&
                  entityView.ItemId.StartsWith(CommerceEntity.IdPrefix<Category>())))
                return entityView;

            await this._deleteRelationshipCommand.Process(context.CommerceContext, entityView.ItemId, entityView.EntityId, "CategoryToSellableItem");

            return entityView;
        }
    }
}
