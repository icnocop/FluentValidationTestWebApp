using FluentValidationTestWebApp.Serialization;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FluentValidationTestWebApp.Models
{
    /// <summary>
    /// Item
    /// </summary>
    [SwaggerDiscriminator("@odata.type")]
    [SwaggerSubType(typeof(Item))]
    [SwaggerSubType(typeof(ItemA))]
    [SwaggerSubType(typeof(ItemB))]
    [JsonConverter(typeof(JsonInheritanceConverter), "@odata.type")]
    [JsonInheritance(ItemA.OdataType, typeof(ItemA))]
    [JsonInheritance(ItemB.OdataType, typeof(ItemB))]
    public abstract class Item
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
