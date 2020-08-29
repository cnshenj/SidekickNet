// <copyright file="AdviceTypeBundle.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// Bundles multiple advice types.
    /// </summary>
    public abstract class AdviceTypeBundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceTypeBundle"/> class.
        /// </summary>
        /// <param name="adviceTypes">Types of advices.</param>
        public AdviceTypeBundle(params Type[] adviceTypes)
        {
            if (adviceTypes.Length == 0)
            {
                throw new ArgumentException("At least one advice type must be provided.", nameof(adviceTypes));
            }

            if (adviceTypes.Any(t => !typeof(IAdvice).IsAssignableFrom(t) && !typeof(AdviceTypeBundle).IsAssignableFrom(t)))
            {
                throw new ArgumentException(
                    $"Advice type must implement interface '{nameof(IAdvice)}' or inherit from '{nameof(AdviceTypeBundle)}'.",
                    nameof(adviceTypes));
            }

            this.AdviceTypes = adviceTypes;
        }

        /// <summary>Gets the advice types in this bundle.</summary>
        public ICollection<Type> AdviceTypes { get; }
    }
}
