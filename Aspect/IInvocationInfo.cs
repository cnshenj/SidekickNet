// <copyright file="IInvocationInfo.cs" company="Zhang Shen">
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
    public interface IInvocationInfo
    {
        /// <summary>
        /// Occurs before awaiting an asynchronous task.
        /// Some interception implementations require special handling in asynchronous context.
        /// </summary>
        event EventHandler? BeforeAwait;

        /// <summary>Gets the target object of invocation.</summary>
        object Target { get; }

        /// <summary>
        /// Gets the method being executed.
        /// Do not invoke it directly. Instead, invoke the executor.
        /// </summary>
        MethodInfo Method { get; }

        /// <summary>Gets or sets a method that executes <see cref="Method"/>.</summary>
        MethodInfo Executor { get; set; }

        /// <summary>Gets the arguments the caller passed to the method.</summary>
        object[]? Arguments { get; }

        /// <summary>Gets or sets a <see cref="Func{TResult}"/> to proceed with the invocation.</summary>
        Func<object> Proceed { get; set; }

        /// <summary>Gets or sets the return value of the invocation.</summary>
        object? ReturnValue { get; set; }

        /// <summary>Gets or sets the exception encountered during invocation.</summary>
        Exception? Exception { get; set; }

        /// <summary>Gets additional user-defined information about the invocaiton.</summary>
        IDictionary<string, object> Data { get; }

        /// <summary>Initializes the context before awaiting an asynchronous task.</summary>
        void InitializeAwait();
    }
}
