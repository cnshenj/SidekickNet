// <copyright file="ConfigurationExtensions.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using Azure.Core;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using SidekickNet.Utilities.Azure;

namespace SidekickNet.Utilities.AspNetCore.Azure
{
    /// <summary>
    /// Extensions for web hosting.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Uses Key Vault reference in app configuration.
        /// </summary>
        /// <param name="hostBuilder">The builder to build web host.</param>
        /// <param name="tokenCredential">A <see cref="TokenCredential"/> used to authenticate requests to the vault.</param>
        /// <param name="options">Options to configure the management of requests sent to Key Vault.</param>
        /// <returns>The <paramref name="hostBuilder"/>.</returns>
        public static IHostBuilder UseKeyVaultReference(
            this IHostBuilder hostBuilder,
            TokenCredential tokenCredential,
            SecretClientOptions? options = default)
        {
            var keyVaultHelper = new KeyVaultHelper(tokenCredential, options);
            return hostBuilder.ConfigureAppConfiguration(
                (_, configurationBuilder) => ResolveKeyVaultReferences(configurationBuilder, keyVaultHelper));
        }

        /// <summary>
        /// Uses Key Vault reference in the configuration.
        /// </summary>
        /// <param name="configurationBuilder">The builder to build the configuration.</param>
        /// <param name="tokenCredential">A <see cref="TokenCredential"/> used to authenticate requests to the vault.</param>
        /// <param name="options">Options to configure the management of requests sent to Key Vault.</param>
        /// <returns>The <paramref name="configurationBuilder"/>.</returns>
        public static IConfigurationBuilder UseKeyVaultReference(
            this IConfigurationBuilder configurationBuilder,
            TokenCredential tokenCredential,
            SecretClientOptions? options = default)
        {
            var keyVaultHelper = new KeyVaultHelper(tokenCredential, options);
            ResolveKeyVaultReferences(configurationBuilder, keyVaultHelper);
            return configurationBuilder;
        }

        private static void ResolveKeyVaultReferences(IConfigurationBuilder builder, KeyVaultHelper keyVaultHelper)
        {
            var config = builder.Build();
            var resolved = new Dictionary<string, string>();
            foreach (var pair in config.AsEnumerable())
            {
                var keyVaultReference = KeyVaultReference.TryParse(pair.Value ?? string.Empty);
                if (keyVaultReference != null)
                {
                    resolved[pair.Key] = keyVaultHelper!.GetSecretAsync(keyVaultReference).Result;
                }
            }

            if (resolved.Any())
            {
                builder.AddInMemoryCollection(resolved);
            }
        }
    }
}
