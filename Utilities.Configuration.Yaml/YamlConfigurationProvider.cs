// <copyright file="YamlConfigurationProvider.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;

namespace SidekickNet.Utilities.Configuration.Yaml;

/// <summary>
/// Provides configuration via YAML files.
/// </summary>
public class YamlConfigurationProvider : FileConfigurationProvider
{
    private readonly Stack<string> scopes = new Stack<string>();

    private string path = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlConfigurationProvider"/> class.
    /// </summary>
    /// <param name="source">The source of the YAML file.</param>
    public YamlConfigurationProvider(FileConfigurationSource source)
        : base(source)
    {
    }

    /// <inheritdoc/>
    public override void Load(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);
        if (yamlStream.Documents[0].RootNode is not YamlMappingNode rootMapping)
        {
            throw new FormatException($"YAML configuration file must contain a root mapping.");
        }

        this.Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        this.LoadYamlMapping(rootMapping);
    }

    private void SetPath()
    {
        this.path = ConfigurationPath.Combine(this.scopes.Reverse());
    }

    private void EnterScope(string scope)
    {
        this.scopes.Push(scope);
        this.SetPath();
    }

    private void EnterScope(YamlNode node)
    {
        this.EnterScope(node.ToString());
    }

    private void ExitScope()
    {
        this.scopes.Pop();
        this.SetPath();
    }

    private void LoadYamlNode(YamlNode node)
    {
        switch (node.NodeType)
        {
            case YamlNodeType.Alias:
                // YamlDotNet replaces aliases with the anchor value, so no need to handle aliases here
                break;
            case YamlNodeType.Mapping:
                this.LoadYamlMapping((YamlMappingNode)node);
                break;
            case YamlNodeType.Scalar:
                if (this.Data.ContainsKey(this.path))
                {
                    throw new FormatException($"Duplicate configuration key '{this.path}'.");
                }

                this.Data[this.path] = node.ToString();
                break;
            case YamlNodeType.Sequence:
                this.LoadYamlSequence((YamlSequenceNode)node);
                break;
            default:
                throw new NotSupportedException($"YAML node type '{node.NodeType}' not supported.");
        }
    }

    private void LoadYamlMapping(YamlMappingNode node)
    {
        foreach (var property in node)
        {
            this.EnterScope(property.Key);
            this.LoadYamlNode(property.Value);
            this.ExitScope();
        }
    }

    private void LoadYamlSequence(YamlSequenceNode sequence)
    {
        var index = 0;
        foreach (var element in sequence)
        {
            this.EnterScope(index++.ToString());
            this.LoadYamlNode(element);
            this.ExitScope();
        }
    }
}