// <copyright file="ConfigurationExtensions.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.Hosting;
using SidekickNet.Utilities.Configuration.Processing;
using SidekickNet.Utilities.Configuration.Yaml;

namespace SidekickNet.Utilities.AspNetCore
{
    /// <summary>
    /// Extensions for web hosting.
    /// </summary>
    public static class ConfigurationExtensions
    {
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
            return hostBuilder.ConfigureAppConfiguration((_, builder) => builder.InterpolateConfiguration());
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
            return hostBuilder.ConfigureAppConfiguration((_, builder) => builder.AddConfigurationMixins());
        }

        /// <summary>
        /// Uses YAML files in app configuration.
        /// </summary>
        /// <param name="hostBuilder">The builder to build web host.</param>
        /// <param name="beforeSourceType">
        /// The configuration source type before which to insert YAML file configuration.
        /// If <c>null</c>, YAML file configuration will be added as last source.
        /// </param>
        /// <param name="baseFileName">The YAML configuration file name without extension and environment name.</param>
        /// <returns>The <paramref name="hostBuilder"/>.</returns>
        public static IHostBuilder UseYamlConfiguration(
            this IHostBuilder hostBuilder,
            Type? beforeSourceType = null,
            string baseFileName = "appsettings")
        {
            return hostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var environment = context.HostingEnvironment;
                configBuilder
                    .AddYamlFile($"{baseFileName}.yml", beforeSourceType: beforeSourceType)
                    .AddYamlFile(
                        $"{baseFileName}.{environment.EnvironmentName}.yml",
                        beforeSourceType: beforeSourceType);
            });
        }
    }
}