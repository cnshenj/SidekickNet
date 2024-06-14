// <copyright file="ConstructorResolver.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// Finds the most appropriate constructor of a type.
    /// </summary>
    /// <param name="type">The type to get constructor.</param>
    /// <returns>the most appropriate constructor of <paramref name="type"/>.</returns>
    public delegate ConstructorInfo ResolveConstructor(Type type);

    /// <summary>
    /// Provides delegates to resolve constructors for types.
    /// </summary>
    public static class ConstructorResolver
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> SoleConstructors = new();

        /// <summary>
        /// Gets the only constructor of a type.
        /// </summary>
        /// <param name="type">The type to get constructor of.</param>
        /// <returns>The only constructor of <paramref name="type"/>.</returns>
        public static ConstructorInfo GetSoleConstructor(Type type)
        {
            return SoleConstructors.GetOrAdd(type, GetConstructor);

            // Local function
            static ConstructorInfo GetConstructor(Type type)
            {
                var constructors = type.GetConstructors();
                if (constructors.Length > 1)
                {
                    throw new NotSupportedException($"Type '{type.FullName}' has more than one public constructors.");
                }

                return constructors[0];
            }
        }
    }
}
