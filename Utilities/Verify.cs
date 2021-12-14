// <copyright file="Verify.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SidekickNet.Utilities;

/// <summary>
/// Helper methods to verify conditions.
/// </summary>
public static class Verify
{
#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// Verify that an argument is not <c>null</c>.
    /// </summary>
    /// <param name="argument">The argument to verify.</param>
    /// <param name="argumentExpression">The expression that represents the <paramref name="argument"/>.</param>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <c>null</c>.</exception>
    public static void NotNull<T>(
        T argument,
        [CallerArgumentExpression("argument")] string? argumentExpression = default)
        where T : class
    {
        if (argument == default)
        {
            throw new ArgumentNullException(paramName: argumentExpression);
        }
    }

    /// <summary>
    /// Verify that a string argument is not <c>null</c> or empty.
    /// </summary>
    /// <param name="argument">The string to verify.</param>
    /// <param name="argumentExpression">The expression that represents the <paramref name="argument"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is <c>null</c> or empty.</exception>
    public static void NotEmpty(
        string? argument,
        [CallerArgumentExpression("argument")] string? argumentExpression = default)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException($"Value cannot be null or empty.", argumentExpression);
        }
    }

    /// <summary>
    /// Verify that a collection argument is not <c>null</c> or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="argument">The collection to verify.</param>
    /// <param name="argumentExpression">The expression that represents the <paramref name="argument"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is <c>null</c> or empty.</exception>
    public static void NotEmpty<T>(
        ICollection<T>? argument,
        [CallerArgumentExpression("argument")] string? argumentExpression = default)
    {
        if (argument == default || argument.Count == 0)
        {
            throw new ArgumentException($"Value cannot be null or empty.", argumentExpression);
        }
    }
#endif
}