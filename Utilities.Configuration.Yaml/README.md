# Utilities.Configuration.Yaml

A C# library for YAML-based configuration management, providing easy integration with .NET configuration systems.

## Features

- Parse and load YAML configuration files

## Usage
### Basic Usage

```csharp
using Microsoft.Extensions.Configuration;
using Utilities.Configuration.Yaml;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddYamlFile("appsettings.yml", optional: false, reloadOnChange: true)
    .Build();
```