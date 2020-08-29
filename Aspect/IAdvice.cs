// <copyright file="IAdvice.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

namespace SidekickNet.Aspect
{
    /// <summary>
    /// Represents an advice in Aspect Oriented Programming (AOP).
    /// </summary>
    public interface IAdvice
    {
        /// <summary>
        /// Gets or sets the precedence order of this advice when being applied.
        /// An advice with smaller order number is applied before an advice with larger order number.
        /// </summary>
        int Order { get; set; }

        /// <summary>Gets or sets the next advice to apply.</summary>
        IAdvice? Next { get; set; }

        /// <summary>Gets or sets a value indicating whether exceptions during invocation should be swallowed.</summary>
        bool SwallowExceptions { get; set; }

        /// <summary>
        /// Applies the advice the a target.
        /// </summary>
        /// <param name="invocation">The invocation of the target method.</param>
        void Apply(IInvocationInfo invocation);
    }
}
