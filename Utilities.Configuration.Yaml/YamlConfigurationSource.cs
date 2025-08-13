// <copyright file="YamlConfigurationSource.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;

namespace SidekickNet.Utilities.Configuration.Yaml;

/// <summary>
/// Represents a YAML file as an <see cref="IConfigurationSource"/>.
/// </summary>
public class YamlConfigurationSource : FileConfigurationSource
{
    /// <inheritdoc/>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        this.EnsureDefaults(builder);
        return new YamlConfigurationProvider(this);
    }
}