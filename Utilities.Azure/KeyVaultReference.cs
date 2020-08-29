// <copyright file="KeyVaultReference.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace SidekickNet.Utilities.Azure
{
    /// <summary>
    /// Represents a Key Vault reference.
    /// </summary>
    public class KeyVaultReference
    {
        /// <summary>A regular expression that matches Key Vault references.</summary>
        private static readonly Regex ReferenceRegex = new Regex(
            @"^@Microsoft\.KeyVault\((?:SecretUri=(?<vaultName>https://[^.]+\.vault\.azure\.net)/secrets/(?<secretName>[^/]+)(?:/(?<secretVersion>[^/]+))?|VaultName=(?<vaultName>[^;]+);SecretName=(?<secretName>[^;]+)(?:;SecretVersion=(?<secretVersion>[^;]+))?)\)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultReference"/> class.
        /// </summary>
        /// <param name="vaultName">The Key Vault DNS name.</param>
        /// <param name="secretName">The name of the secret that is identified this reference.</param>
        /// <param name="secretVersion">The version of the secret that is identified this reference.</param>
        protected KeyVaultReference(string vaultName, string secretName, string? secretVersion = default)
        {
            this.VaultName = vaultName;
            this.SecretName = secretName;
            this.SecretVersion = secretVersion;
        }

        /// <summary>Gets the Key Vault DNS name, such as https://name.vault.azure.net/.</summary>
        public string VaultName { get; }

        /// <summary>Gets the name of the secret that is identified this reference.</summary>
        public string SecretName { get; }

        /// <summary>Gets the version of the secret that is identified this reference.</summary>
        public string? SecretVersion { get; }

        /// <summary>
        /// Tries to parse a Key Vault reference string.
        /// </summary>
        /// <param name="referenceString">The reference string to parse.</param>
        /// <returns>
        /// The <see cref="KeyVaultReference"/> object parsed from <paramref name="referenceString"/>.
        /// Or <c>null</c> if <paramref name="referenceString"/> is not a valid Key Vault reference string.
        /// </returns>
        public static KeyVaultReference? TryParse(string referenceString)
        {
            var match = ReferenceRegex.Match(referenceString ?? string.Empty);
            if (!match.Success)
            {
                return null;
            }

            var secretVersionGroup = match.Groups["secretVersion"];
            var reference = new KeyVaultReference(
                match.Groups["vaultName"].Value,
                match.Groups["secretName"].Value,
                secretVersionGroup.Success ? secretVersionGroup.Value : null);

            return reference;
        }
    }
}
