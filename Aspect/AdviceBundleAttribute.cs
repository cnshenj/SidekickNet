// <copyright file="AdviceBundleAttribute.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// Bundles multiple advices together.
    /// </summary>
    public abstract class AdviceBundleAttribute : AdviceAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceBundleAttribute"/> class.
        /// </summary>
        /// <param name="advices">The advices to be bundled.</param>
        public AdviceBundleAttribute(params AdviceAttribute[] advices)
        {
            if (advices.Length == 0)
            {
                throw new ArgumentException("At least one advice must be provided.", nameof(advices));
            }

            this.Advices = advices;
        }

        /// <summary>Gets the advices in this bundle.</summary>
        public AdviceAttribute[] Advices { get; }

        /// <inheritdoc/>
        public override void Apply(IInvocationInfo invocation) =>
            throw new NotSupportedException("Advice bundle cannot be directly applied.");
    }
}
