using FluentValidation;

namespace FluentValidationTestWebApp.Models
{
    /// <summary>
    /// Item A
    /// </summary>
    /// <seealso cref="FluentValidationTestWebApp.Models.Item" />
    public class ItemA : Item
    {
        public const string OdataType = "#Item.ItemA";

        public class ItemAValidator : AbstractValidator<ItemA>
        {
            public ItemAValidator()
            {
                this.RuleFor(x => x.Name)
                    .NotEmpty()
                    .Length(1, 50);
            }
        }
    }
}
