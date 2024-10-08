﻿// <copyright file="ConfigurationExtensions.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SidekickNet.Utilities.AspNetCore
{
    /// <summary>
    /// Extensions for web hosting.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private static readonly string[] KeyDelimiter = new[] { ConfigurationPath.KeyDelimiter };

        private static readonly Regex ConfigExpressionRegex =
            new Regex("{(?<expression>[^}]+)}", RegexOptions.Compiled);

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
        /// <param name="hostBuilder">The builder to build web host.</param>
        /// <returns>The <paramref name="hostBuilder"/>.</returns>
        public static IHostBuilder UseConfigurationInterpolation(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration(InterpolateAppConfiguration);
        }

        /// <summary>
        /// Uses mixins in app configuration.
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
        /// <param name="hostBuilder">The builder to build web host.</param>
        /// <returns>The <paramref name="hostBuilder"/>.</returns>
        public static IHostBuilder UseConfigurationMixin(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration(AddAppConfigurationMixins);
        }

        private static void InterpolateAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            var config = builder.Build();
            var keys = config.AsEnumerable().Select(pair => pair.Key).ToArray();
            var values = keys
                .Select(key => (Key: key, Value: config[key]))
                .Where(t => t.Value != default)
                .ToDictionary(t => t.Key, t => (object)t.Value!);
            var interpolatedConfig = new Dictionary<string, string?>();
            foreach (var key in keys.Where(k => config[k] != null))
            {
                var seen = new HashSet<string>();
                var value = config[key];
                if (value == default)
                {
                    continue;
                }

                while (true)
                {
                    var interpolatedValue = value.Interpolate(values, out var expressions, ConfigExpressionRegex);
                    if (expressions == default)
                    {
                        // No substitution happened
                        break;
                    }

                    // Repeat interpolation until there is no substitution
                    var cyclicReference = expressions.FirstOrDefault(seen.Contains);
                    if (cyclicReference != default)
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

        private static void AddAppConfigurationMixins(HostBuilderContext context, IConfigurationBuilder builder)
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
    }
}
