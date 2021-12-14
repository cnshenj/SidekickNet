// <copyright file="BasicConvert.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Converts values between basic types: value types, <see cref="string"/>, <see cref="Nullable{T}"/>, <see cref="DBNull"/>.
    /// </summary>
    public static class BasicConvert
    {
        private static readonly BasicTypeInfoCollection TypeInfoMap = new BasicTypeInfoCollection();

        private static readonly ConverterCollection Converters = new ConverterCollection();

        /// <summary>
        /// Returns an object of a specified basic type whose value is equivalent to a specified value.
        /// A basic type is either a value type, or <see cref="string"/>, or <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of object to return.</param>
        /// <returns>
        /// An object whose type is <paramref name="targetType"/> and whose value is equivalent to <paramref name="value"/>.
        /// </returns>
        public static object? ToType(object? value, Type targetType)
        {
            var targetTypeInfo = TypeInfoMap[targetType];
            if (!targetTypeInfo.IsBasicType)
            {
                throw new ArgumentException(
                    $"Type '{targetType.FullName}' is not a value type, or string, or Nullable<T>.");
            }

            var valueType = value?.GetType();
            if (valueType == targetType || valueType == targetTypeInfo.DataType)
            {
                // No conversion needed
                return value;
            }

            // Last condition will never be reached, needed to make nullable check happy
            if (value is null or DBNull
                || (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                || valueType == null)
            {
                return targetTypeInfo.DefaultValue;
            }

            // If the value is a nullable, extract the underlying value
            var valueTypeInfo = TypeInfoMap[valueType];
            if (valueTypeInfo.IsNullable)
            {
                if (valueTypeInfo.HasValue(value))
                {
                    var underlyingValue = valueTypeInfo.GetUnderlyingValue(value);
                    return ToType(underlyingValue, targetTypeInfo.DataType);
                }
                else
                {
                    return targetTypeInfo.DefaultValue;
                }
            }

            var converter = Converters.Get(targetTypeInfo.DataType, value);
            return converter.Convert(value);
        }

        /// <summary>
        /// Returns an object of a specified basic type whose value is equivalent to a specified object.
        /// A basic type is either a value type, or <see cref="string"/>, or <see cref="Nullable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="value">The object to convert.</param>
        /// <returns>
        /// An object whose type is <typeparamref name="T"/> and whose value is equivalent to <paramref name="value"/>.
        /// </returns>
        public static T? ToType<T>(object? value) => (T?)ToType(value, typeof(T));

        private class BasicTypeInfo
        {
            private readonly PropertyInfo? hasValueProperty;

            private readonly PropertyInfo? valueProperty;

            public BasicTypeInfo(Type type)
            {
                this.Type = type;
                var isString = type == typeof(string);
                this.IsNullable = type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                this.IsBasicType = type.IsValueType || isString || this.IsNullable;
                if (this.IsNullable)
                {
                    this.hasValueProperty = type.GetProperty(
                        nameof(Nullable<bool>.HasValue),
                        BindingFlags.Instance | BindingFlags.Public);
                    this.valueProperty = type.GetProperty(
                        nameof(Nullable<bool>.Value),
                        BindingFlags.Instance | BindingFlags.Public);
                    this.UnderlyingType = type.GetGenericArguments()[0];
                }

                this.DataType = this.UnderlyingType ?? this.Type;
                this.DefaultValue = isString || this.IsNullable ? null : Activator.CreateInstance(type);
            }

            public Type Type { get; }

            public bool IsNullable { get; }

            public bool IsBasicType { get; }

            public Type? UnderlyingType { get; }

            public Type DataType { get; }

            public object? DefaultValue { get; }

            public bool HasValue(object obj)
            {
                if (this.hasValueProperty == null)
                {
                    throw new NotSupportedException($"Type '{this.Type.FullName}' is not Nullable<T>.");
                }

                return (bool)this.hasValueProperty.GetValue(obj)!;
            }

            public object GetUnderlyingValue(object obj)
            {
                if (this.valueProperty == null)
                {
                    throw new NotSupportedException($"Type '{this.Type.FullName}' is not Nullable<T>.");
                }

                return this.valueProperty.GetValue(obj)!;
            }
        }

        private class BasicTypeInfoCollection
        {
            private readonly ConcurrentDictionary<Type, BasicTypeInfo> dictionary =
                new ConcurrentDictionary<Type, BasicTypeInfo>();

            public BasicTypeInfo this[Type type] => this.dictionary.GetOrAdd(type, t => new BasicTypeInfo(t));
        }

        private class Converter
        {
            private readonly Type targetType;

            private readonly Type sourceType;

            private readonly ConstructorInfo? constructor;

            private readonly MethodInfo? parser;

            private readonly MethodInfo? toString;

            private readonly bool canChangeType;

            public Converter(Type targetType, object sourceValue)
            {
                this.targetType = targetType;
                this.sourceType = sourceValue.GetType();

                try
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    System.Convert.ChangeType(sourceValue, targetType);
                    this.canChangeType = true;
                }
                catch (InvalidCastException)
                {
                    // Keep canChangeType as false
                }

                if (!this.canChangeType)
                {
                    var methods = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == "Parse");
                    this.parser = FindConverter(this.sourceType, methods);

                    if (this.parser == null)
                    {
                        var constructors = targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                        this.constructor = FindConverter(this.sourceType, constructors);
                    }
                }

                if (targetType == typeof(string))
                {
                    var methods = this.sourceType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.Name == "ToString");
                    this.toString = methods.FirstOrDefault(m => m.GetParameters().Length == 0);
                }
            }

            public object Convert(object obj)
            {
                if (this.canChangeType)
                {
                    return System.Convert.ChangeType(obj, this.targetType);
                }
                else if (this.parser != null)
                {
                    return this.parser.Invoke(null, new[] { obj })!;
                }
                else if (this.constructor != null)
                {
                    return this.constructor.Invoke(new[] { obj });
                }
                else if (this.toString != null)
                {
                    return this.toString.Invoke(obj, Array.Empty<object>())!;
                }
                else
                {
                    throw new InvalidCastException($"Invalid cast from '{this.sourceType.FullName}' to '{this.targetType.FullName}'.");
                }
            }

            private static T? FindConverter<T>(Type sourceType, IEnumerable<T> members)
                where T : MethodBase
            {
                T? matchMember = default;
                Type? matchFirstParameterType = default;

                foreach (var member in members)
                {
                    var parameters = member.GetParameters();

                    // First parameter must be assignable from source type
                    var firstParameterType = parameters.FirstOrDefault()?.ParameterType;
                    if (firstParameterType == null || !firstParameterType.IsAssignableFrom(sourceType))
                    {
                        continue;
                    }

                    // All other parameters must be optional
                    if (parameters.Skip(1).Any(p => !p.IsOptional))
                    {
                        continue;
                    }

                    // If a member is already found,
                    // only replace it if this constructor's first parameter is "closer" to source type
                    if (matchFirstParameterType == null
                        || matchFirstParameterType.IsAssignableFrom(firstParameterType))
                    {
                        if (firstParameterType == sourceType)
                        {
                            // Exact match found
                            return member;
                        }

                        matchMember = member;
                        matchFirstParameterType = firstParameterType;
                    }
                }

                return matchMember;
            }
        }

        private class ConverterCollection
        {
            private readonly ConcurrentDictionary<(Type TargetType, Type SourceType), Converter> dictionary =
                new ConcurrentDictionary<(Type TargetType, Type SourceType), Converter>();

            public Converter Get(Type targetType, object sourceValue)
            {
                return this.dictionary.GetOrAdd(
                    (targetType, sourceValue.GetType()),
                    key => new Converter(key.TargetType, sourceValue));
            }
        }
    }
}
