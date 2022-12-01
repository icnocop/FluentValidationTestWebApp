namespace FluentValidationTestWebApp.Serialization
{
    using System;

    /// <summary>
    /// Defines a child class in the inheritance chain.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    internal class JsonInheritanceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInheritanceAttribute"/> class.
        /// </summary>
        /// <param name="key">The discriminator key.</param>
        /// <param name="type">The child class type.</param>
        public JsonInheritanceAttribute(string key, Type type)
        {
            this.Key = key;
            this.Type = type;
        }

        /// <summary>
        /// Gets the discriminator key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the child class type.
        /// </summary>
        public Type Type { get; }
    }
}
