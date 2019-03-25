namespace XCentium.Commerce.Plugin.AddParentCategoriesToSellableItem
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines.Blocks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using GetSellableItemDetailsViewBlock = Sitecore.Commerce.Plugin.Catalog.GetSellableItemDetailsViewBlock;

    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IGetEntityViewPipeline>(configure =>
                {
                    configure.Replace<GetSellableItemDetailsViewBlock, Pipelines.Blocks.GetSellableItemDetailsViewBlock>();
                })
                .ConfigurePipeline<IPopulateEntityViewActionsPipeline>(configure =>
                {
                        configure.Add<PopulateParentCategoriesViewActionsBlock>()
                            .After<PopulateCategoriesViewActionsBlock>();
                })
                .ConfigurePipeline<IDoActionPipeline>(configure =>
                {
                    configure.Add<DoActionDisassociateFromCategoryBlock>().After<DoActionDisassociateBlock>();
                })
               );



            services.RegisterAllCommands(assembly);
        }
    }
}