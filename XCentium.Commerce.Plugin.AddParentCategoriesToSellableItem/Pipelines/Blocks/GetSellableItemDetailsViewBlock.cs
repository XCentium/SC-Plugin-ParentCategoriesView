namespace XCentium.Commerce.Plugin.AddParentCategoriesToSellableItem.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Pricing;
    using Sitecore.Commerce.Plugin.Search;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using KnownCatalogViewsPolicy = Policies.KnownCatalogViewsPolicy;

    [PipelineDisplayName("XCentium.Catalog.block.GetSellableItemDetailsView")]
    public class GetSellableItemDetailsViewBlock : GetListViewBlock
    {
        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");
            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var request = context.CommerceContext.GetObject<EntityViewArgument>();
            if (string.IsNullOrEmpty(request?.ViewName) || !request.ViewName.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase) && !request.ViewName.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase) && (!request.ViewName.Equals(viewsPolicy.Variant, StringComparison.OrdinalIgnoreCase) && !request.ViewName.Equals(viewsPolicy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase)))
                return entityView;
            if (request.ForAction.Equals("AssociateSellableItemToCatalog", StringComparison.OrdinalIgnoreCase) || request.ForAction.Equals("AssociateSellableItemToCategory", StringComparison.OrdinalIgnoreCase))
            {
                this.AssociateToCatalogOrCategory(context, entityView, request, request.ForAction);
                return entityView;
            }
            var num = request.ForAction.Equals("AddSellableItem", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            var isEditAction = request.ForAction.Equals("EditSellableItemDetails", StringComparison.OrdinalIgnoreCase);
            if (num != 0 && request.ViewName.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase))
            {
                await this.AddEntityProperties(context, entityView, entityView, null, true, false, request.ViewName);
                return await Task.FromResult(entityView);
            }
            if (isEditAction && request.ViewName.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase))
            {
                await this.AddEntityProperties(context, entityView, entityView, (SellableItem)request.Entity, false, true, request.ViewName);
                return await Task.FromResult(entityView);
            }
            if (!(request.Entity is SellableItem) || !string.IsNullOrEmpty(request.ForAction))
                return entityView;
            var detailsView = entityView;
            if (request.ViewName.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase) || request.ViewName.Equals(viewsPolicy.Variant, StringComparison.OrdinalIgnoreCase) || request.ViewName.Equals(viewsPolicy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase))
            {
                var entityView1 = new EntityView
                {
                    EntityId = request.Entity.Id ?? string.Empty,
                    EntityVersion = request.Entity.EntityVersion,
                    Name = viewsPolicy.Details,
                    UiHint = "Flat"
                };
                detailsView = entityView1;
                entityView.ChildViews.Add(detailsView);
            }
            await this.AddEntityProperties(context, entityView, detailsView, (SellableItem)request.Entity, false, false, request.ViewName);
            return await Task.FromResult(entityView);
        }

        public void AssociateToCatalogOrCategory(CommercePipelineExecutionContext context, EntityView entityView, EntityViewArgument request, string action)
        {
            var policy1 = context.GetPolicy<KnownCatalogViewsPolicy>();
            var policy2 = context.CommerceContext.Environment.GetComponent<PolicySetsComponent>().GetPolicy<SearchScopePolicy>();
            var policyList = new List<Policy>
            {
                new Policy()
                {
                    PolicyId = "EntityType", Models = new List<Model>() {new Model() {Name = "SellableItem"}}
                },
                policy2
            };
            var properties = entityView.Properties;
            var viewProperty = new ViewProperty
            {
                DisplayName = policy1.SellableItem,
                Name = policy1.SellableItem,
                IsRequired = true,
                Value = string.Empty,
                UiType = "Autocomplete",
                OriginalType = string.Empty.GetType().FullName,
                Policies = policyList
            };
            properties.Add(viewProperty);
        }

        public async Task AddEntityProperties(CommercePipelineExecutionContext context, EntityView entityView, EntityView detailsView, SellableItem entity, bool isAddAction, bool isEditAction, string viewName)
        {
            var policy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var itemId = entityView.ItemId;
            if (!string.IsNullOrEmpty(itemId))
                detailsView.ItemId = itemId;
            var variation = entity.GetVariation(itemId);
            var flag1 = variation != null;
            var properties1 = detailsView.Properties;
            var viewProperty1 = new ViewProperty
            {
                Name = flag1 ? "VariantId" : "ProductId",
                RawValue = flag1 ? variation.Id : entity?.ProductId ?? string.Empty,
                IsReadOnly = !isAddAction,
                IsRequired = isAddAction | isEditAction,
                IsHidden = false
            };
            properties1.Add(viewProperty1);
            var properties2 = detailsView.Properties;
            var viewProperty2 = new ViewProperty
            {
                Name = "Name",
                RawValue = variation != null ? variation.Name : entity?.Name ?? string.Empty,
                IsReadOnly = !isAddAction,
                IsRequired = isAddAction,
                IsHidden = !isAddAction
            };
            properties2.Add(viewProperty2);
            var properties3 = detailsView.Properties;
            var viewProperty3 = new ViewProperty
            {
                Name = "DisplayName",
                RawValue = variation != null ? variation.DisplayName : entity?.DisplayName ?? string.Empty,
                IsReadOnly = !isAddAction && !isEditAction,
                IsRequired = isAddAction | isEditAction,
                IsHidden = false
            };
            properties3.Add(viewProperty3);
            if (!isAddAction && !isEditAction)
            {
                var definitions = new List<string>();
                if (entity != null && entity.HasComponent<CatalogsComponent>())
                    entity.GetComponent<CatalogsComponent>().Catalogs.Where(c => !string.IsNullOrEmpty(c.ItemDefinition)).ForEach(c => definitions.Add(
                        $"{c.Name} - {c.ItemDefinition}"));
                var properties4 = detailsView.Properties;
                var viewProperty4 = new ViewProperty
                {
                    Name = "ItemDefinitions",
                    RawValue = definitions,
                    IsReadOnly = true,
                    IsRequired = false,
                    UiType = "List",
                    OriginalType = "List"
                };
                properties4.Add(viewProperty4);
            }
            var properties5 = detailsView.Properties;
            var viewProperty5 = new ViewProperty
            {
                Name = "Description",
                RawValue = variation != null ? variation.Description : entity?.Description ?? string.Empty,
                IsReadOnly = !isAddAction && !isEditAction,
                IsRequired = false,
                IsHidden = false
            };
            properties5.Add(viewProperty5);
            var properties6 = detailsView.Properties;
            var viewProperty6 = new ViewProperty
            {
                Name = "Brand",
                RawValue = entity?.Brand ?? string.Empty,
                IsReadOnly = ((isAddAction ? 0 : (!isEditAction ? 1 : 0)) | (flag1 ? 1 : 0)) != 0,
                IsRequired = false,
                IsHidden = false
            };
            properties6.Add(viewProperty6);
            var properties7 = detailsView.Properties;
            var viewProperty7 = new ViewProperty
            {
                Name = "Manufacturer",
                RawValue = entity?.Manufacturer ?? string.Empty,
                IsReadOnly = ((isAddAction ? 0 : (!isEditAction ? 1 : 0)) | (flag1 ? 1 : 0)) != 0,
                IsRequired = false,
                IsHidden = false
            };
            properties7.Add(viewProperty7);
            var properties8 = detailsView.Properties;
            var viewProperty8 = new ViewProperty
            {
                Name = "TypeOfGood",
                RawValue = entity?.TypeOfGood ?? string.Empty,
                IsReadOnly = ((isAddAction ? 0 : (!isEditAction ? 1 : 0)) | (flag1 ? 1 : 0)) != 0,
                IsRequired = false,
                IsHidden = false
            };
            properties8.Add(viewProperty8);
            var source = ((variation?.Tags ?? entity?.Tags) ?? new List<Tag>()).Select(x => x.Name);
            var properties9 = detailsView.Properties;
            var viewProperty9 = new ViewProperty
            {
                Name = "Tags",
                RawValue = source.ToArray(),
                IsReadOnly = !isAddAction && !isEditAction,
                IsRequired = false,
                IsHidden = false,
                UiType = isEditAction | isAddAction ? "Tags" : "List",
                OriginalType = "List"
            };
            properties9.Add(viewProperty9);
            var flag2 = entityView.Name.Equals(policy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase);
            if (flag2)
            {
                var properties4 = detailsView.Properties;
                var viewProperty4 = new ViewProperty
                {
                    Name = "VariationProperties",
                    RawValue = string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties4.Add(viewProperty4);
                var properties10 = detailsView.Properties;
                var viewProperty10 = new ViewProperty
                {
                    Name = "SitecoreId",
                    RawValue = entity?.SitecoreId ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = true
                };
                properties10.Add(viewProperty10);
                var properties11 = detailsView.Properties;
                var viewProperty11 = new ViewProperty
                {
                    Name = "ParentCatalogList",
                    RawValue = entity?.ParentCatalogList ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = true
                };
                properties11.Add(viewProperty11);
                var properties12 = detailsView.Properties;
                var viewProperty12 = new ViewProperty
                {
                    Name = "ParentCategoryList",
                    RawValue = entity?.ParentCategoryList ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = true
                };
                properties12.Add(viewProperty12);
            }
            if (isAddAction || isEditAction)
                return;
            var dictionary = new Dictionary<string, object>();
            var entityView1 = new EntityView
            {
                DisplayName = "Identifiers",
                Name = "Identifiers",
                UiHint = "Flat",
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = itemId
            };
            var entityView2 = entityView1;
            if (entity != null && entity.HasComponent<IdentifiersComponent>(itemId) | flag2)
            {
                var component = entity.GetComponent<IdentifiersComponent>(itemId);
                dictionary.Add("ISBN", component.ISBN);
                dictionary.Add("LEICode", component.LEICode);
                dictionary.Add("SKU", component.SKU);
                dictionary.Add("TaxID", component.TaxID);
                dictionary.Add("gtin8", component.gtin8);
                dictionary.Add("gtin12", component.gtin12);
                dictionary.Add("gtin13", component.gtin13);
                dictionary.Add("mbm", component.mbm);
                dictionary.Add("ISSN", component.ISSN);
                foreach (var keyValuePair in dictionary)
                {
                    var properties4 = entityView2.Properties;
                    var viewProperty4 = new ViewProperty
                    {
                        Name = keyValuePair.Key,
                        RawValue = keyValuePair.Value ?? string.Empty,
                        IsReadOnly = true,
                        IsRequired = false,
                        IsHidden = false
                    };
                    properties4.Add(viewProperty4);
                }
            }
            var entityView3 = new EntityView
            {
                DisplayName = "Display Properties",
                Name = "DisplayProperties",
                UiHint = "Flat",
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = itemId
            };
            var entityView4 = entityView3;
            dictionary.Clear();
            if (entity != null && entity.HasComponent<DisplayPropertiesComponent>(itemId) | flag2)
            {
                var component = entity.GetComponent<DisplayPropertiesComponent>(itemId);
                var properties4 = entityView4.Properties;
                var viewProperty4 = new ViewProperty
                {
                    Name = "Color",
                    RawValue = component.Color ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties4.Add(viewProperty4);
                var properties10 = entityView4.Properties;
                var viewProperty10 = new ViewProperty
                {
                    Name = "Size",
                    RawValue = component.Size ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties10.Add(viewProperty10);
                var properties11 = entityView4.Properties;
                var viewProperty11 = new ViewProperty
                {
                    Name = "DisambiguatingDescription",
                    RawValue = component.DisambiguatingDescription ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties11.Add(viewProperty11);
                var properties12 = entityView4.Properties;
                var viewProperty12 = new ViewProperty
                {
                    Name = "DisplayOnSite",
                    RawValue = component.DisplayOnSite,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties12.Add(viewProperty12);
                var properties13 = entityView4.Properties;
                var viewProperty13 = new ViewProperty
                {
                    Name = "DisplayInProductList",
                    RawValue = component.DisplayInProductList,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties13.Add(viewProperty13);
                var properties14 = entityView4.Properties;
                var viewProperty14 = new ViewProperty
                {
                    Name = "Style",
                    RawValue = component.Style,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties14.Add(viewProperty14);
            }
            var entityView5 = new EntityView
            {
                DisplayName = "Images",
                Name = "Images",
                UiHint = "Table",
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = itemId
            };
            var entityView6 = entityView5;
            var entityView7 = new EntityView
            {
                DisplayName = "Item Specifications",
                Name = "ItemSpecifications",
                UiHint = "Flat",
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = itemId
            };
            var entityView8 = entityView7;
            dictionary.Clear();
            if (entity != null && entity.HasComponent<ItemSpecificationsComponent>(itemId) | flag2)
            {
                var component = entity.GetComponent<ItemSpecificationsComponent>(itemId);
                if (component.AreaServed == null)
                    component.AreaServed = new GeoLocation();
                var city = component.AreaServed.City;
                var region = component.AreaServed.Region;
                var postalCode = component.AreaServed.PostalCode;
                var str1 = string.IsNullOrWhiteSpace(region) ? "" : ", " + region;
                var str2 = string.IsNullOrWhiteSpace(postalCode) ? "" : ", " + postalCode;
                var str3 = $"{city}{str1}{str2}".TrimStart(',');
                dictionary.Add("AreaServed", str3);
                if (flag2)
                {
                    dictionary.Add("Weight", component.Weight);
                    dictionary.Add("WeightUnitOfMeasure", component.WeightUnitOfMeasure);
                    dictionary.Add("Length", component.Length);
                    dictionary.Add("Width", component.Width);
                    dictionary.Add("Height", component.Height);
                    dictionary.Add("DimensionsUnitOfMeasure", component.DimensionsUnitOfMeasure);
                    dictionary.Add("SizeOnDisk", component.Weight);
                    dictionary.Add("SizeOnDiskUnitOfMeasure", component.WeightUnitOfMeasure);
                }
                else
                {
                    dictionary.Add("Weight", $"{component.Weight} {component.WeightUnitOfMeasure}");
                    var str4 = string.Format("{0}{3}x{1}{3}x{2}{3}", component.Width, component.Height, component.Length, component.DimensionsUnitOfMeasure);
                    dictionary.Add("Dimensions", str4);
                    dictionary.Add("DigitalProperties", $"{component.SizeOnDisk} {component.SizeOnDiskUnitOfMeasure}");
                }
                foreach (var keyValuePair in dictionary)
                {
                    var properties4 = entityView8.Properties;
                    var viewProperty4 = new ViewProperty
                    {
                        Name = keyValuePair.Key,
                        RawValue = keyValuePair.Value,
                        IsReadOnly = true,
                        IsRequired = false,
                        IsHidden = false
                    };
                    properties4.Add(viewProperty4);
                }
            }
            this.AddSellableItemPricing(entityView, entity, variation, context);
            AddSellableItemVariations(entityView, entity, context);
            await AddSellableItemParentCategories(entityView, entity, context);
            entityView.ChildViews.Add(entityView2);
            entityView.ChildViews.Add(entityView4);
            entityView.ChildViews.Add(entityView6);
            entityView.ChildViews.Add(entityView8);
            if (!entityView.Name.Equals("Variant", StringComparison.OrdinalIgnoreCase))
                return;
            var properties15 = entityView.Properties;
            var viewProperty15 = new ViewProperty
            {
                DisplayName = "DisplayName",
                Name = "DisplayName",
                RawValue = variation?.DisplayName
            };
            properties15.Add(viewProperty15);
        }

        private void AddSellableItemPricing(EntityView entityView, SellableItem entity, ItemVariationComponent variation, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var entityView1 = new EntityView
            {
                Name = policy.SellableItemPricing,
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = variation != null ? variation.Id : string.Empty,
                UiHint = "Flat"
            };
            var entityView2 = entityView1;
            var entityView3 = new EntityView
            {
                Name = policy.SellableItemListPricing,
                EntityId = entityView.EntityId,
                EntityVersion = entityView.EntityVersion,
                ItemId = variation != null ? variation.Id : string.Empty,
                UiHint = "Table"
            };
            var entityView4 = entityView3;
            if (entity != null)
            {
                var str = variation != null ? variation.GetPolicy<PriceCardPolicy>().PriceCardName : entity.GetPolicy<PriceCardPolicy>().PriceCardName;
                var properties1 = entityView2.Properties;
                var viewProperty1 = new ViewProperty
                {
                    Name = "PriceCardName",
                    RawValue = str ?? string.Empty,
                    IsReadOnly = true,
                    IsRequired = false,
                    IsHidden = false
                };
                properties1.Add(viewProperty1);
                foreach (var price in (variation != null ? variation.GetPolicy<ListPricingPolicy>() : entity.GetPolicy<ListPricingPolicy>()).Prices)
                {
                    var entityView5 = new EntityView
                    {
                        Name = context.GetPolicy<KnownCatalogViewsPolicy>().Summary,
                        EntityId = entityView.EntityId,
                        ItemId = (variation != null ? variation.Id : string.Empty) + "|" + price.CurrencyCode,
                        UiHint = "Flat"
                    };
                    var entityView6 = entityView5;
                    var properties2 = entityView6.Properties;
                    var viewProperty2 = new ViewProperty
                    {
                        Name = "Currency",
                        RawValue = price.CurrencyCode
                    };
                    properties2.Add(viewProperty2);
                    var properties3 = entityView6.Properties;
                    var viewProperty3 = new ViewProperty
                    {
                        Name = "ListPrice",
                        RawValue = price.Amount
                    };
                    properties3.Add(viewProperty3);
                    entityView4.ChildViews.Add(entityView6);
                }
            }
            entityView2.ChildViews.Add(entityView4);
            entityView.ChildViews.Add(entityView2);
        }

        private static void AddSellableItemVariations(EntityView entityView, CommerceEntity entity, CommercePipelineExecutionContext context)
        {
            var policy = context.GetPolicy<KnownCatalogViewsPolicy>();
            if (entity == null || !entityView.Name.Equals(policy.Master, StringComparison.OrdinalIgnoreCase) && !entityView.Name.Equals(policy.Details, StringComparison.OrdinalIgnoreCase) && !entityView.Name.Equals(policy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase))
                return;
            var entityView1 = new EntityView
            {
                Name = policy.SellableItemVariants,
                EntityId = entity.Id,
                EntityVersion = entity.EntityVersion,
                UiHint = "Table"
            };
            var entityView2 = entityView1;
            var list = entity.GetComponent<ItemVariationsComponent>().ChildComponents.OfType<ItemVariationComponent>().ToList();
            if (list.Count > 0)
            {
                foreach (var variationComponent in list)
                {
                    var entityView3 = new EntityView
                    {
                        Name = policy.Variant,
                        EntityId = entity.Id,
                        EntityVersion = entity.EntityVersion,
                        ItemId = variationComponent.Id
                    };
                    var entityView4 = entityView3;
                    var properties1 = entityView4.Properties;
                    var viewProperty1 = new ViewProperty
                    {
                        Name = "Id",
                        RawValue = variationComponent.Id,
                        IsReadOnly = true,
                        UiType = "ItemLink"
                    };
                    properties1.Add(viewProperty1);
                    var properties2 = entityView4.Properties;
                    var viewProperty2 = new ViewProperty
                    {
                        Name = "DisplayName",
                        RawValue = variationComponent.DisplayName,
                        IsReadOnly = true
                    };
                    properties2.Add(viewProperty2);
                    var properties3 = entityView4.Properties;
                    var viewProperty3 = new ViewProperty
                    {
                        Name = "Disabled",
                        RawValue = variationComponent.Disabled,
                        IsReadOnly = true
                    };
                    properties3.Add(viewProperty3);
                    entityView2.ChildViews.Add(entityView4);
                }
            }
            entityView.ChildViews.Add(entityView2);
        }

        private async Task AddSellableItemParentCategories(EntityView entityView, SellableItem entity, CommercePipelineExecutionContext context)
        {
            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            if (entity == null || !entityView.Name.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase) && !entityView.Name.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase) && !entityView.Name.Equals(viewsPolicy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase))
                return;
            var parentCategoriesEntityView = new EntityView
            {
                Name = viewsPolicy.ParentCategories,
                EntityId = entity.Id,
                EntityVersion = entity.EntityVersion,
                UiHint = "Table"
            };

            await this.SetListMetadata(parentCategoriesEntityView, viewsPolicy.ParentCategories, "PaginateCatalogItemList", context);
            var allCategories = await this.Commander.Pipeline<IGetCategoriesPipeline>().Run(new GetCategoriesArgument(" "), context);
            if (allCategories != null && !string.IsNullOrEmpty(entity.ParentCategoryList))
            {
                var parentCategories = allCategories.Where(category =>
                    entity.ParentCategoryList.Split('|').Any(id =>
                        id.Equals(category.SitecoreId, StringComparison.OrdinalIgnoreCase)));
                foreach (var category in parentCategories)
                {
                    var categoryView = new EntityView
                    {
                        EntityId = entity.Id,
                        ItemId = category.Id,
                        EntityVersion = category.EntityVersion,
                        Name = viewsPolicy.Summary
                    };
                    var viewProperty = new ViewProperty
                    {
                        Name = "Id",
                        RawValue = category.Id,
                        IsReadOnly = true,
                        UiType = "EntityLink"
                    };
                    categoryView.Properties.Add(viewProperty);
                    var viewProperty1 = new ViewProperty
                    {
                        Name = "Name",
                        RawValue = category.Name,
                        IsReadOnly = true,
                    };
                    categoryView.Properties.Add(viewProperty1);
                    var viewProperty2 = new ViewProperty
                    {
                        Name = "DisplayName",
                        RawValue = category.DisplayName,
                        IsReadOnly = true
                    };
                    categoryView.Properties.Add(viewProperty2);
                    var viewProperty3 = new ViewProperty
                    {
                        Name = "Description",
                        RawValue = category.Description ?? string.Empty,
                        IsReadOnly = true
                    };
                    categoryView.Properties.Add(viewProperty3);
                    parentCategoriesEntityView.ChildViews.Add(categoryView);
                }
            }
            entityView.ChildViews.Add(parentCategoriesEntityView);
        }

        public GetSellableItemDetailsViewBlock(CommerceCommander commerceCommander) : base(commerceCommander)
        {
        }
    }
}
