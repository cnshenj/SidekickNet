// <copyright file="JsonFormatSerializer.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SidekickNet.Utilities.Json
{
    /// <summary>
    /// Provides functionality to serialize objects or value types to JSON strings
    /// and to deserialize JSON strings into objects or value types.
    /// </summary>
    public class JsonFormatSerializer : IFormatSerializer
    {
        private static readonly JsonSerializerSettings DefaultSerializerOptions;

        private readonly JsonSerializer serializer;

        /// <summary>
        /// Initializes static members of the <see cref="JsonFormatSerializer"/> class.
        /// </summary>
        static JsonFormatSerializer()
        {
            DefaultSerializerOptions = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            DefaultSerializerOptions.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFormatSerializer"/> class.
        /// </summary>
        /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
        public JsonFormatSerializer(JsonSerializerSettings? serializerOptions = default)
        {
            this.serializer = JsonSerializer.Create(serializerOptions ?? DefaultSerializerOptions);
        }

        /// <inheritdoc/>
        public string MediaTypeName => "application/json";

        /// <inheritdoc/>
        public string Serialize(object? value)
        {
            using var writer = new StringWriter();
            this.serializer.Serialize(writer, value);
            return writer.ToString();
        }

        /// <inheritdoc />
        public byte[] SerializeToBytes(object? value)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            this.serializer.Serialize(writer, value);
            return stream.ToArray();
        }

        /// <inheritdoc/>
        public Task<Stream> SerializeAsync(object? value)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            this.serializer.Serialize(writer, value);
            stream.Position = 0;
            return Task.FromResult((Stream)stream);
        }

        /// <inheritdoc/>
        public object? Deserialize(string text, Type? type = default)
        {
            using var reader = new StringReader(text);
            return this.serializer.Deserialize(reader, type ?? typeof(JToken));
        }

        /// <inheritdoc />
        public object? Deserialize(byte[] bytes, Type? type = default)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new StreamReader(stream);
            return this.serializer.Deserialize(reader, type ?? typeof(JToken));
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string text) => (T?)this.Deserialize(text, typeof(T));

        /// <inheritdoc />
        public T? Deserialize<T>(byte[] bytes) => (T?)this.Deserialize(bytes, typeof(T));

        /// <inheritdoc/>
        public ValueTask<object?> DeserializeAsync(Stream stream, Type? type = default)
        {
            using var reader = new StreamReader(stream);
            var result = this.serializer.Deserialize(reader, type ?? typeof(JToken));
            return new ValueTask<object?>(result);
        }

        /// <inheritdoc/>
        public async ValueTask<T?> DeserializeAsync<T>(Stream stream)
        {
            var result = await this.DeserializeAsync(stream, typeof(T));
            return (T?)result;
        }
    }
}
