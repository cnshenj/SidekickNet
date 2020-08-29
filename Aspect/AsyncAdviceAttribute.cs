// <copyright file="AsyncAdviceAttribute.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SidekickNet.Aspect
{
    /// <summary>
    /// An advice that can be applied asynchrounously.
    /// </summary>
    public abstract class AsyncAdviceAttribute : AdviceAttribute
    {
        private readonly MethodInfo applyMethodDefinition;

        private readonly ConcurrentDictionary<Type, MethodInfo> applyMethods =
            new ConcurrentDictionary<Type, MethodInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncAdviceAttribute"/> class.
        /// </summary>
        public AsyncAdviceAttribute()
        {
            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            this.applyMethodDefinition = methods.First(m => m.Name == nameof(this.ApplyAsync) && m.IsGenericMethodDefinition);
        }

        /// <inheritdoc/>
        public override void Apply(IInvocationInfo invocation)
        {
            var returnType = invocation.Method.ReturnType;
            if (returnType == typeof(Task))
            {
                invocation.ReturnValue = this.ApplyAsync(invocation);
            }
            else if (returnType.IsConstructedGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];
                var applyMethod = this.applyMethods.GetOrAdd(
                    resultType,
                    type => this.applyMethodDefinition.MakeGenericMethod(type));
                invocation.ReturnValue = applyMethod.Invoke(this, new object[] { invocation });
            }
            else
            {
                var message = $"{nameof(AsyncAdviceAttribute)} can only be applied to methods returning Task or Task<T>."
                    + $" Method '{invocation.Method.Name}' returns '{returnType.Name}'.";
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Let the invocation to proceed and return the return value.
        /// </summary>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <param name="invocation">The invocation that will proceed.</param>
        /// <returns>The return value of the invocation.</returns>
        protected T ProceedAndReturn<T>(IInvocationInfo invocation)
        {
            this.Proceed(invocation);
            return (T)invocation.ReturnValue!;
        }

        /// <summary>
        /// Asynchronously apply the advice.
        /// </summary>
        /// <param name="invocation">The invocation of the target method.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task ApplyAsync(IInvocationInfo invocation);

        /// <summary>
        /// Asynchronously apply the advice.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the target method.</typeparam>
        /// <param name="invocation">The invocation of the target method.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task<T> ApplyAsync<T>(IInvocationInfo invocation);
    }
}
