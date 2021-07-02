// <copyright file="IFormatSerializer.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Serializes objects to string representations and deserializes string representations to objects.
    /// </summary>
    public interface IFormatSerializer
    {
        /// <summary>Gets the media type format of serialized values.</summary>
        string MediaTypeName { get; }

        /// <summary>
        /// Converts a value to its string representation.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The string representation of the given object.</returns>
        string Serialize(object? value);

        /// <summary>
        /// Converts a value to a serialized stream.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The stream containing the serialized value.</returns>
        Task<Stream> SerializeAsync(object? value);

        /// <summary>
        /// Parses the text representing a single value into an instance of a specified type.
        /// </summary>
        /// <param name="text">The string representation of a value.</param>
        /// <param name="type">The type of the object to convert to and return.</param>
        /// <returns>The object extracted from the given string representation.</returns>
        object? Deserialize(string text, Type? type = default);

        /// <summary>
        /// Reads a serialized value from a stream into an instance of a specified type.
        /// </summary>
        /// <param name="stream">The stream containing the serialized value.</param>
        /// <param name="type">The type of the object to convert to and return.</param>
        /// <returns>The object extracted from the stream.</returns>
        ValueTask<object?> DeserializeAsync(Stream stream, Type? type = default);

        /// <summary>
        /// Parses the text representing a single value into an instance of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to convert to and return.</typeparam>
        /// <param name="text">The string representation of a value.</param>
        /// <returns>The object extracted from the given string representation.</returns>
#if !NETSTANDARD2_0
        [return: MaybeNull]
#endif
        T Deserialize<T>(string text);

        /// <summary>
        /// Reads a serialized value from a stream into an instance of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to convert to and return.</typeparam>
        /// <param name="stream">The stream containing the serialized value.</param>
        /// <returns>The object extracted from the stream.</returns>
        ValueTask<T> DeserializeAsync<T>(Stream stream);
    }
}
