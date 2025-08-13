// <copyright file="ConfigurationProcessingExtensions.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace SidekickNet.Utilities.Configuration.Processing;

/// <summary>
/// Extensions for <see cref="IConfigurationBuilder"/>
/// to support configuration mixins and string interpolation.
/// </summary>
public static class ConfigurationProcessingExtensions
{
    private static readonly string[] KeyDelimiter = [ConfigurationPath.KeyDelimiter];

    private static readonly Regex ConfigExpressionRegex = new("{(?<expression>[^}]+)}", RegexOptions.Compiled);

    /// <summary>
    /// For example:
    /// {
    ///   "Family": {
    ///     "LastName": "Smith"
    ///   },
    ///   "Member": {
    ///     "{Family}": true,
    ///     "FirstName": "John"
    ///   }
    /// }
    /// "Member" will have both "LastName" and "FirstName" because properties of "Family" is mixed in "Member".
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    public static void AddConfigurationMixins(this IConfigurationBuilder builder)
    {
        var config = builder.Build();
        var keys = config.AsEnumerable().Select(pair => pair.Key).ToArray();
        var mixins = new Dictionary<string, string?>();

        foreach (var key in keys)
        {
            var segments = key.Split(KeyDelimiter, StringSplitOptions.None);
            if (segments.Length == 1)
            {
                continue;
            }

            var lastSegment = segments[^1];
            var match = ConfigExpressionRegex.Match(lastSegment);
            if (!match.Success)
            {
                continue;
            }

            bool.TryParse(config[key], out var containsMixin);
            if (!containsMixin)
            {
                continue;
            }

            // Prefix includes the separator, e.g. "Foo:"
            var sourcePrefix = match.Groups["expression"].Value + ConfigurationPath.KeyDelimiter;
            var sourceKeys = keys.Where(k => k.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase));
            var targetPrefix = key[..^lastSegment.Length];
            foreach (var sourceKey in sourceKeys)
            {
                var targetKey = targetPrefix + sourceKey[sourcePrefix.Length..];
                if (config[targetKey] == null)
                {
                    // Only add mixin if there is no existing value
                    mixins[targetKey] = config[sourceKey];
                }
            }
        }

        if (mixins.Count > 0)
        {
            builder.AddInMemoryCollection(mixins);
        }
    }

    /// <summary>
    /// Uses string interpolation in app configuration.
    /// For example:
    /// {
    ///   "Manager": {
    ///     "Name": "Jack"
    ///   },
    ///   "ApproverName": "{Manager:Name}"
    /// }
    /// The property "ApproverName" will have value "Jack" once interpolation is completed.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    public static void InterpolateConfiguration(this IConfigurationBuilder builder)
    {
        var config = builder.Build();
        var keys = config.AsEnumerable().Select(pair => pair.Key).ToArray();
        var values = keys
            .Select(key => (Key: key, Value: config[key]))
            .Where(t => t.Value != null)
            .ToDictionary(t => t.Key, object (t) => t.Value!);
        var interpolatedConfig = new Dictionary<string, string?>();
        foreach (var key in keys.Where(k => config[k] != null))
        {
            var seen = new HashSet<string>();
            var value = config[key];
            if (value == null)
            {
                continue;
            }

            while (true)
            {
                var interpolatedValue = value.Interpolate(values, out var expressions, ConfigExpressionRegex);
                if (expressions == null)
                {
                    // No substitution happened
                    break;
                }

                // Repeat interpolation until there is no substitution
                var cyclicReference = expressions.FirstOrDefault(seen.Contains);
                if (cyclicReference != null)
                {
                    throw new FormatException($"Cyclic reference '{cyclicReference}' in configuration '{key}'.");
                }

                value = interpolatedConfig[key] = interpolatedValue;
            }
        }

        if (interpolatedConfig.Count > 0)
        {
            builder.AddInMemoryCollection(interpolatedConfig);
        }
    }
}