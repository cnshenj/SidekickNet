// <copyright file="DelegateComparer.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Compares the return values of a delegate.
    /// </summary>
    /// <typeparam name="T">The type of the parameter of the delegate.</typeparam>
    /// <typeparam name="TResult">The type of the return value of the delegate.</typeparam>
    public class DelegateComparer<T, TResult>
        : IEqualityComparer<T>
    {
        private readonly Func<T?, TResult> @delegate;

        private readonly IEqualityComparer<TResult> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateComparer{T, TResult}"/> class.
        /// </summary>
        /// <param name="delegate">The delegate that return values.</param>
        /// <param name="comparer">Compares objects for equality.</param>
        public DelegateComparer(Func<T?, TResult> @delegate, IEqualityComparer<TResult>? comparer = default)
        {
            this.@delegate = @delegate;
            this.comparer = comparer ?? EqualityComparer<TResult>.Default;
        }

        /// <inheritdoc/>
        public bool Equals(T? x, T? y)
        {
            return this.comparer.Equals(this.@delegate(x), this.@delegate(y));
        }

        /// <inheritdoc/>
        public int GetHashCode(T obj)
        {
            var value = this.@delegate(obj);
            return value is null ? 0 : this.comparer.GetHashCode(value);
        }
    }
}
