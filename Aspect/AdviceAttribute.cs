// <copyright file="AdviceAttribute.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// The attribute to add advices to program elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class AdviceAttribute : Attribute, IAdvice
    {
        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc/>
        public IAdvice? Next { get; set; }

        /// <inheritdoc/>
        public bool SwallowExceptions { get; set; }

        /// <inheritdoc/>
        public abstract void Apply(IInvocationInfo invocation);

        /// <summary>
        /// Proceeds with an invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        protected virtual void Proceed(IInvocationInfo invocation)
        {
            try
            {
                if (this.Next == null)
                {
                    var result = invocation.Proceed();
                    invocation.ReturnValue = result;
                }
                else
                {
                    this.Next.Apply(invocation);
                }
            }
            catch (Exception ex)
            {
                invocation.Exception = ex;
                if (!this.SwallowExceptions)
                {
                    throw;
                }
            }
        }
    }
}
