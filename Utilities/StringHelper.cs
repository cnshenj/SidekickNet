// <copyright file="StringHelper.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Helper methods for strings.
    /// </summary>
    public static class StringHelper
    {
        private static readonly Regex DefaultInterpolationRegex =
            new Regex(@"{(?<expression>[^}]+)(?<alignment>,(?:\+|-)?\d+)?(?<format>:[^}]+)?}", RegexOptions.Compiled);

        /// <summary>
        /// Determines whether two specified String objects have the same value.
        /// </summary>
        /// <param name="a">The first string to compare, or <c>null</c>.</param>
        /// <param name="b">The second string to compare, or <c>null</c>.</param>
        /// <param name="comparisonType">
        /// One of the enumeration values that specifies how the strings will be compared.
        /// The default value is <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value of the a parameter is equal to the value of the b parameter; otherwise, <c>false</c>.
        /// </returns>
        public static bool EqualsIgnoreCase(
            this string a,
            string b,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return string.Equals(a, b, comparisonType);
        }

        /// <summary>
        /// Converts a string to a stream.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <returns>The converted stream.</returns>
        public static Stream ToStream(this string source) => new MemoryStream(Encoding.UTF8.GetBytes(source));

        /// <summary>
        /// Interpolates an string using expressions.
        /// </summary>
        /// <param name="source">The string to interpolate.</param>
        /// <param name="values">The values of expressions.</param>
        /// <param name="expressions">
        /// A sequence of expressions that have been substituted by values;
        /// or <c>null</c> if no expression is found in <paramref name="source"/>.
        /// </param>
        /// <param name="expressionRegex">
        /// The optional regular expression to match expressions.
        /// If omitted, the default C# string interpolation expression is used.
        /// </param>
        /// <returns>The interpolated string.</returns>
        public static string Interpolate(
            this string source,
            IDictionary<string, object> values,
            out IEnumerable<string>? expressions,
            Regex? expressionRegex = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            expressionRegex ??= DefaultInterpolationRegex;

            var builder = new StringBuilder();
            var substituted = new List<string>();
            var index = 0;
            Match match;
            while ((match = expressionRegex.Match(source, index)).Success)
            {
                if (match.Index > index)
                {
                    builder.Append(source, index, match.Index - index);
                }

                var expression = match.Groups["expression"].Value;
                if (values.ContainsKey(expression))
                {
                    var format = $"{{0{match.Groups["alignment"].Value}{match.Groups["format"].Value}}}";
                    builder.AppendFormat(format, values[expression]);
                    substituted.Add(expression);
                }
                else
                {
                    builder.Append(match.Groups[0].Value);
                }

                index = match.Index + match.Length;
            }

            if (index < source.Length)
            {
                builder.Append(source, index, source.Length - index);
            }

            expressions = substituted.Count > 0 ? substituted : null;
            return builder.ToString();
        }

        /// <summary>
        /// Interpolates an string using expressions.
        /// </summary>
        /// <param name="source">The string to interpolate.</param>
        /// <param name="values">The values of expressions.</param>
        /// <param name="expressionRegex">
        /// The optional regular expression to match expressions.
        /// If omitted, the default C# string interpolation expression is used.
        /// </param>
        /// <returns>The interpolated string.</returns>
        public static string Interpolate(
            this string source,
            IDictionary<string, object> values,
            Regex? expressionRegex = default)
        {
            return Interpolate(source, values, out _, expressionRegex);
        }
    }
}
