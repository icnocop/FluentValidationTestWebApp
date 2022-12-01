namespace FluentValidationTestWebApp.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Defines the class as inheritance base class and adds a discriminator property to the serialized object.
    /// </summary>
    internal class JsonInheritanceConverter : JsonConverter
    {
        [ThreadStatic]
        private static bool isReading;

        [ThreadStatic]
        private static bool isWriting;

        private readonly Type baseType;
        private readonly string discriminator;
        private readonly bool readTypeProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class.
        /// </summary>
        public JsonInheritanceConverter()
            : this(DefaultDiscriminatorName, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class.
        /// </summary>
        /// <param name="discriminator">The discriminator.</param>
        public JsonInheritanceConverter(string discriminator)
            : this(discriminator, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class.
        /// </summary>
        /// <param name="discriminator">The discriminator.</param>
        /// <param name="readTypeProperty">Read the $type property to determine the type
        /// (fallback, should not be used as it might lead to security problems).</param>
        public JsonInheritanceConverter(string discriminator, bool readTypeProperty)
        {
            this.discriminator = discriminator;
            this.readTypeProperty = readTypeProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class which only applies for the given base type.
        /// </summary>
        /// <remarks>
        /// Use this constructor for global registered converters (not defined on class).
        /// </remarks>
        /// <param name="baseType">The base type.</param>
        public JsonInheritanceConverter(Type baseType)
            : this(baseType, DefaultDiscriminatorName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class which only applies for the given base type.
        /// </summary>
        /// <remarks>
        /// Use this constructor for global registered converters (not defined on class).
        /// </remarks>
        /// <param name="baseType">The base type.</param>
        /// <param name="discriminator">The discriminator.</param>
        public JsonInheritanceConverter(Type baseType, string discriminator)
            : this(discriminator, false)
        {
            this.baseType = baseType;
        }

        /// <summary>
        /// Gets the default discriminator name.
        /// </summary>
        public static string DefaultDiscriminatorName { get; } = "discriminator";

        /// <summary>
        /// Gets the discriminator property name.
        /// </summary>
        public virtual string DiscriminatorName => this.discriminator;

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                if (isWriting)
                {
                    isWriting = false;
                    return false;
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get
            {
                if (isReading)
                {
                    isReading = false;
                    return false;
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                isWriting = true;

                var jObject = JObject.FromObject(value, serializer);
                jObject.AddFirst(new JProperty(this.discriminator, JToken.FromObject(this.GetDiscriminatorValue(value.GetType()))));

                using (JsonReader jsonReader = jObject.CreateReader())
                {
                    writer.WriteToken(jsonReader);
                }
            }
            finally
            {
                isWriting = false;
            }
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            if (this.baseType != null)
            {
                var type = objectType;
                while (type != null)
                {
                    if (type == this.baseType)
                    {
                        return true;
                    }

                    type = type.GetTypeInfo().BaseType;
                }

                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = serializer.Deserialize<JObject>(reader);
            if (jObject == null)
            {
                return null;
            }

            var discriminator = jObject.GetValue(this.discriminator, StringComparison.OrdinalIgnoreCase)?.Value<string>();
            Type subtype;
            if (discriminator == null)
            {
                subtype = objectType;
            }
            else
            {
                subtype = this.GetDiscriminatorType(jObject, objectType, discriminator);
            }

            var objectContract = serializer.ContractResolver.ResolveContract(subtype) as JsonObjectContract;
            if (objectContract == null || objectContract.Properties.All(p => p.PropertyName != this.discriminator))
            {
                jObject.Remove(this.discriminator);
            }

            try
            {
                isReading = true;
                using (JsonReader jsonReader = jObject.CreateReader())
                {
                    return serializer.Deserialize(jsonReader, subtype);
                }
            }
            finally
            {
                isReading = false;
            }
        }

        /// <summary>Gets the discriminator value for the given type.</summary>
        /// <param name="type">The object type.</param>
        /// <returns>The discriminator value.</returns>
        public virtual string GetDiscriminatorValue(Type type)
        {
            var jsonInheritanceAttributeDiscriminator = GetSubtypeDiscriminator(type);
            if (jsonInheritanceAttributeDiscriminator != null)
            {
                return jsonInheritanceAttributeDiscriminator;
            }

            return type.Name;
        }

        /// <summary>Gets the type for the given discriminator value.</summary>
        /// <param name="jObject">The JSON object.</param>
        /// <param name="objectType">The object (base) type.</param>
        /// <param name="discriminatorValue">The discriminator value.</param>
        /// <returns>The discriminator type.</returns>
        protected virtual Type GetDiscriminatorType(JObject jObject, Type objectType, string discriminatorValue)
        {
            var jsonInheritanceAttributeSubtype = GetObjectSubtype(objectType, discriminatorValue);
            if (jsonInheritanceAttributeSubtype != null)
            {
                return jsonInheritanceAttributeSubtype;
            }

            if (objectType.Name == discriminatorValue)
            {
                return objectType;
            }

            var knownTypeAttributesSubtype = this.GetSubtypeFromKnownTypeAttributes(objectType, discriminatorValue);
            if (knownTypeAttributesSubtype != null)
            {
                return knownTypeAttributesSubtype;
            }

            var typeName = objectType.Namespace + "." + discriminatorValue;
            var subtype = objectType.GetTypeInfo().Assembly.GetType(typeName);
            if (subtype != null)
            {
                return subtype;
            }

            if (this.readTypeProperty)
            {
                var typeInfo = jObject.GetValue("$type");
                if (typeInfo != null)
                {
                    return Type.GetType(typeInfo.Value<string>());
                }
            }

            throw new InvalidOperationException("Could not find subtype of '" + objectType.Name + "' with discriminator '" + discriminatorValue + "'.");
        }

        private static Type GetObjectSubtype(Type baseType, string discriminatorName)
        {
            var jsonInheritanceAttributes = baseType
                .GetTypeInfo()
                .GetCustomAttributes(true)
                .OfType<JsonInheritanceAttribute>();

            return jsonInheritanceAttributes.SingleOrDefault(a => a.Key == discriminatorName)?.Type;
        }

        private static string GetSubtypeDiscriminator(Type objectType)
        {
            var jsonInheritanceAttributes = objectType
                .GetTypeInfo()
                .GetCustomAttributes(true)
                .OfType<JsonInheritanceAttribute>();

            Type baseType = objectType;
            while (baseType != null)
            {
                string discriminatorKey = jsonInheritanceAttributes.SingleOrDefault(a => a.Type == baseType)?.Key;
                if (discriminatorKey != null)
                {
                    return discriminatorKey;
                }

                baseType = baseType.BaseType;
            }

            return null;
        }

        private Type GetSubtypeFromKnownTypeAttributes(Type objectType, string discriminator)
        {
            var type = objectType;
            do
            {
                var knownTypeAttributes = type.GetTypeInfo().GetCustomAttributes(false)
                    .Where(a => a.GetType().Name == "KnownTypeAttribute");
                foreach (dynamic attribute in knownTypeAttributes)
                {
                    if (attribute.Type != null && attribute.Type.Name == discriminator)
                    {
                        return attribute.Type;
                    }
                    else if (attribute.MethodName != null)
                    {
                        var method = type.GetRuntimeMethod((string)attribute.MethodName, new Type[0]);
                        if (method != null)
                        {
                            var types = (System.Collections.Generic.IEnumerable<Type>)method.Invoke(null, new object[0]);
                            foreach (var knownType in types)
                            {
                                if (knownType.Name == discriminator)
                                {
                                    return knownType;
                                }
                            }

                            return null;
                        }
                    }
                }

                type = type.GetTypeInfo().BaseType;
            }
            while (type != null);

            return null;
        }
    }
}
