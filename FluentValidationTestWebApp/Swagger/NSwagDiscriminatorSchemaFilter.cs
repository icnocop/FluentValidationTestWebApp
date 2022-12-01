namespace FluentValidationTestWebApp.Swagger
{
    using System.Reflection;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// NSwag Discriminator Schema Filter.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class NSwagDiscriminatorSchemaFilter : ISchemaFilter
    {
        /// <inheritdoc/>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var swaggerDiscriminatorAttribute = context.Type.GetCustomAttribute<SwaggerDiscriminatorAttribute>(false);

            if (swaggerDiscriminatorAttribute == null)
            {
                return;
            }

            var discriminatorName = swaggerDiscriminatorAttribute.PropertyName;

            if (!schema.Properties.ContainsKey(discriminatorName))
            {
                // Add the discriminator property
                schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string", MinLength = 1 });
            }

            // Flag it as required
            schema.Required.Add(discriminatorName);

            // Add "discriminator" metadata
            schema.Discriminator = new OpenApiDiscriminator
            {
                PropertyName = swaggerDiscriminatorAttribute.PropertyName,
            };

            var swaggerSubTypeAttributes = context.Type.GetCustomAttributes<SwaggerSubTypeAttribute>(false);
            if (swaggerSubTypeAttributes == null)
            {
                return;
            }

            foreach (var swaggerSubTypeAttribute in swaggerSubTypeAttributes)
            {
                var discriminatorValue = swaggerSubTypeAttribute.DiscriminatorValue;
                if (discriminatorValue == null)
                {
                    continue;
                }

                var discriminatorType = swaggerSubTypeAttribute.SubType;

                schema.Discriminator.Mapping.Add(discriminatorValue, $"#/components/schemas/{discriminatorType.Name}");
            }
        }
    }
}
