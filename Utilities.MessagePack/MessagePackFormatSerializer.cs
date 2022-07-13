// <copyright file="MessagePackFormatSerializer.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using MessagePack;

namespace SidekickNet.Utilities.MessagePack;

/// <summary>
/// Provides functionality to serialize objects or value types to MessagePack.
/// and to deserialize MessagePack into objects or value types.
/// </summary>
public class MessagePackFormatSerializer : IFormatSerializer
{
    private const string NotSupportedMessage = "MessagePack data are not strings.";

    private readonly MessagePackSerializerOptions? options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackFormatSerializer"/> class.
    /// </summary>
    /// <param name="options">The options of the serializer.</param>
    public MessagePackFormatSerializer(MessagePackSerializerOptions? options = default)
    {
        this.options = options;
    }

    /// <inheritdoc />
    public string MediaTypeName => "application/msgpack";

    /// <inheritdoc />
    public string Serialize(object? value)
    {
        throw new NotSupportedException(NotSupportedMessage);
    }

    /// <inheritdoc/>
    public byte[] SerializeToBytes(object? value)
    {
        return MessagePackSerializer.Typeless.Serialize(value, this.options);
    }

    /// <inheritdoc />
    public async Task<Stream> SerializeAsync(object? value)
    {
        var stream = new MemoryStream();
        await MessagePackSerializer.Typeless.SerializeAsync(stream, value, this.options);
        return stream;
    }

    /// <inheritdoc/>
    public object? Deserialize(string text, Type? type = default)
    {
        throw new NotSupportedException(NotSupportedMessage);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] bytes, Type? type = default)
    {
        return MessagePackSerializer.Typeless.Deserialize(bytes, this.options);
    }

    /// <inheritdoc/>
    public ValueTask<object?> DeserializeAsync(Stream stream, Type? type = default)
    {
        return MessagePackSerializer.Typeless.DeserializeAsync(stream, this.options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        throw new NotSupportedException(NotSupportedMessage);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] bytes) => (T?)this.Deserialize(bytes, typeof(T));

    /// <inheritdoc/>
    public async ValueTask<T?> DeserializeAsync<T>(Stream stream)
    {
        var obj = await this.DeserializeAsync(stream, typeof(T));
        return (T?)obj;
    }
}