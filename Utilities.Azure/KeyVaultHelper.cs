// <copyright file="KeyVaultHelper.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace SidekickNet.Utilities.Azure
{
    /// <summary>
    /// Helper methods for Key Vault.
    /// </summary>
    public class KeyVaultHelper
    {
        private readonly TokenCredential tokenCredential;

        private readonly SecretClientOptions? options;

        private readonly ConcurrentDictionary<string, SecretClient> secretClients =
            new ConcurrentDictionary<string, SecretClient>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultHelper"/> class.
        /// </summary>
        /// <param name="tokenCredential">A <see cref="TokenCredential"/> used to authenticate requests to the vault.</param>
        /// <param name="options">Options to configure the management of requests sent to Key Vault.</param>
        public KeyVaultHelper(TokenCredential tokenCredential, SecretClientOptions? options = default)
        {
            this.tokenCredential = tokenCredential;
            this.options = options;
        }

        /// <summary>
        /// Gets a secret identified by a Key Vault reference.
        /// </summary>
        /// <param name="referenceString">The Key Vault reference string that identifies the secret to get.</param>
        /// <returns>The secret identified by a Key Vault reference.</returns>
        public Task<string> GetSecretAsync(string referenceString)
        {
            var reference = KeyVaultReference.TryParse(referenceString);
            if (reference == null)
            {
                throw new FormatException("Invalid key vault reference.");
            }

            return this.GetSecretAsync(reference);
        }

        /// <summary>
        /// Gets a secret identified by a Key Vault reference.
        /// </summary>
        /// <param name="reference">The Key Vault reference that identifies the secret to get.</param>
        /// <returns>The secret identified by <paramref name="reference"/>.</returns>
        public Task<string> GetSecretAsync(KeyVaultReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            return this.GetSecretAsync(
                reference.VaultName,
                reference.SecretName,
                reference.SecretVersion);
        }

        /// <summary>
        /// Gets a secret from Key Vault. A certain number of retries will be attempted if there is an error.
        /// </summary>
        /// <param name="vaultName">The DNS name of the Key Vault that contains the secret.</param>
        /// <param name="secretName">The name of the secret to get.</param>
        /// <param name="secretVersion">The version of the secret to get.</param>
        /// <returns>The secret identified by the parameters.</returns>
        public async Task<string> GetSecretAsync(
            string vaultName,
            string secretName,
            string? secretVersion = default)
        {
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                throw ExceptionHelper.CreateNullOrWhiteSpaceException(nameof(vaultName));
            }

            if (string.IsNullOrWhiteSpace(secretName))
            {
                throw ExceptionHelper.CreateNullOrWhiteSpaceException(nameof(secretName));
            }

            var secretClient = this.secretClients.GetOrAdd(
                vaultName,
                this.CreateSecretClient);
            var response = await secretClient.GetSecretAsync(secretName, secretVersion).ConfigureAwait(false);
            if (response == null)
            {
                throw new NotFoundException("Secret not found.");
            }

            return response.Value.Value;
        }

        private SecretClient CreateSecretClient(string vaultName)
        {
            return this.options == default
                ? new SecretClient(new Uri(vaultName), this.tokenCredential)
                : new SecretClient(new Uri(vaultName), this.tokenCredential, this.options);
        }
    }
}
