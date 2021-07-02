// <copyright file="YamlConfigurationExtensions.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SidekickNet.Utilities.AspNetCore
{
    /// <summary>
    /// Extensions for web hosting.
    /// </summary>
    public static class YamlConfigurationExtensions
    {
        /// <summary>
        /// Adds a YAML configuration file to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="yamlFilePath">The path of the YAML file relative to base path in <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <param name="beforeSourceType">
        /// The configuration source type before which to insert YAML file configuration.
        /// If <c>null</c>, YAML file configuration will be added as last source.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder builder,
            string yamlFilePath,
            bool optional = true,
            bool reloadOnChange = true,
            Type? beforeSourceType = default)
        {
            void ConfigureSource(YamlConfigurationSource source)
            {
                source.Path = yamlFilePath;
                source.Optional = optional;
                source.ReloadOnChange = reloadOnChange;
                source.ResolveFileProvider();
            }

            return builder.AddYamlFile(ConfigureSource, beforeSourceType);
        }

        /// <summary>
        /// Adds a YAML configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <param name="beforeSourceType">
        /// The configuration source type before which to insert YAML file configuration.
        /// If <c>null</c>, YAML file configuration will be added as last source.
        /// </param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder builder,
            Action<YamlConfigurationSource> configureSource,
            Type? beforeSourceType = default)
        {
            if (beforeSourceType != null)
            {
                for (var i = 0; i < builder.Sources.Count; ++i)
                {
                    var source = builder.Sources[i];
                    if (beforeSourceType.IsInstanceOfType(source))
                    {
                        var yamlSource = new YamlConfigurationSource();
                        configureSource(yamlSource);
                        builder.Sources.Insert(i, yamlSource);
                        return builder;
                    }
                }
            }

            return builder.Add(configureSource);
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
            Type? beforeSourceType = default,
            string baseFileName = "appsettings")
        {
            return hostBuilder.ConfigureAppConfiguration(
                (context, configBuilder) =>
                {
                    var environment = context.HostingEnvironment;
                    configBuilder
                        .AddYamlFile($"{baseFileName}.yml", beforeSourceType: beforeSourceType)
                        .AddYamlFile($"{baseFileName}.{environment.EnvironmentName}.yml", beforeSourceType: beforeSourceType);
                });
        }
    }
}
