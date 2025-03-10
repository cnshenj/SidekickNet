// <copyright file="InvocationInfo.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// The invocation info of the target method.
    /// </summary>
    public class InvocationInfo : IInvocationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationInfo"/> class.
        /// </summary>
        /// <param name="target">The target object of invocation.</param>
        /// <param name="method">The method being executed.</param>
        /// <param name="arguments">The arguments the caller passed to the method.</param>
        /// <param name="executor">The method that executes the target method.</param>
        public InvocationInfo(object target, MethodInfo method, object[]? arguments, MethodInfo? executor = default)
        {
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
            this.Method = method ?? throw new ArgumentNullException(nameof(method));
            this.Executor = executor ?? method;
            this.Arguments = arguments;
            this.Proceed = this.DefaultProceed;
        }

        /// <inheritdoc/>
        public event EventHandler? BeforeAwait;

        /// <inheritdoc/>
        public object Target { get; }

        /// <inheritdoc/>
        public MethodInfo Method { get; }

        /// <inheritdoc/>
        public MethodInfo Executor { get; set; }

        /// <inheritdoc/>
        public object[]? Arguments { get; }

        /// <inheritdoc/>
        public Func<object> Proceed { get; set; }

        /// <inheritdoc/>
        public object? ReturnValue { get; set; }

        /// <inheritdoc/>
        public Exception? Exception { get; set; }

        /// <inheritdoc/>
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

        /// <inheritdoc/>
        public void InitializeAwait() => this.BeforeAwait?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// The default way to proceed with method execution.
        /// </summary>
        /// <returns>The return value of the method execution.</returns>
        private object DefaultProceed()
        {
            this.ReturnValue = this.Executor.Invoke(this.Target, this.Arguments);
            return this.ReturnValue!;
        }
    }
}
