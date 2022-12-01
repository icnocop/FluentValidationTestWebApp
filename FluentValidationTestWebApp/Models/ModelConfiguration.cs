using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;

namespace FluentValidationTestWebApp.Models
{
    public class ModelConfiguration : IModelConfiguration
    {
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            builder.EntitySet<Item>("Items");
            builder.EntitySet<ItemA>("ItemAs");

            // Item
            EntityTypeConfiguration<Item> item = builder.EntityType<Item>();
            item.Namespace = "Item";
            item.Abstract();

            // Item A
            EntityTypeConfiguration<ItemA> a = builder.EntityType<ItemA>();
            a.Namespace = "Item";
            a.DerivesFrom<Item>();

            // Item B
            EntityTypeConfiguration<ItemB> b = builder.EntityType<ItemB>();
            b.Namespace = "Item";
            b.DerivesFrom<Item>();
        }
    }
}
