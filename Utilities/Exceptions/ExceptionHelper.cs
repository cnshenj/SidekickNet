// <copyright file="ExceptionHelper.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Globalization;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Helper methods for exceptions.
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>The error message to indicate that a string cannot be null, empty or contain only whitespace.</summary>
        public const string NullOrWhiteSpaceError = "cannot be null, empty or contain only whitespace";

        private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;

        /// <summary>
        /// Creates an exception that is thrown when an entity is not found.
        /// </summary>
        /// <param name="entityKindName">The name of the entity's kind.</param>
        /// <param name="entityId">The string that can uniquely identify the entity that is not found.</param>
        /// <returns>The created exception.</returns>
        public static NotFoundException CreateEntityNotFoundException(string entityKindName, object entityId) =>
            new NotFoundException($"{TextInfo.ToTitleCase(entityKindName)} '{entityId}' not found.");

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> that is thrown when the argument with the specified name
        /// is <c>null</c>, empty or contains only whitespace.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <returns>The <see cref="ArgumentException"/>.</returns>
        public static ArgumentException CreateNullOrWhiteSpaceException(string argumentName) =>
            new ArgumentException($"The argument '{argumentName}' {NullOrWhiteSpaceError}.", argumentName);
    }
}
