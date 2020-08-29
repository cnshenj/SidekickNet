// <copyright file="AccessLockFactory.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Provides a mechanism that synchronizes access to keys.
    /// </summary>
    /// <typeparam name="TKey">The type of keys.</typeparam>
    public class AccessLockFactory<TKey>
    {
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> semaphores = new ConcurrentDictionary<TKey, SemaphoreSlim>();

        private readonly TimeSpan timeout;

        private readonly int maxCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessLockFactory{TKey}"/> class.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the default time period to wait to acquire access locks.</param>
        /// <param name="maxCount">The maximum number of requests that can be granted concurrently for each key.</param>
        public AccessLockFactory(TimeSpan? timeout = default, int maxCount = 1)
        {
            this.timeout = timeout ?? AccessLock.Indefinite;
            this.maxCount = maxCount;
        }

        /// <summary>
        /// Gets a new access lock for the specified key.
        /// The lock may or may not be acquired.
        /// </summary>
        /// <param name="key">The key to get access lock for.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> that represents the time period to wait to acquire access locks.</param>
        /// <returns>A new access lock for the specified key.</returns>
        public AccessLock TryGetLock(TKey key, TimeSpan? timeout = default)
        {
            var semaphore = this.semaphores.GetOrAdd(key, _ => new SemaphoreSlim(this.maxCount, this.maxCount));
            return AccessLock.TryCreate(semaphore, timeout ?? this.timeout);
        }

        /// <summary>
        /// Gets a new access lock for the specified key.
        /// The lock is guaranteed to be acquired. Or an exception is thrown.
        /// </summary>
        /// <param name="key">The key to get access lock for.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> that represents the time period to wait to acquire access locks.</param>
        /// <returns>A new access lock for the specified key.</returns>
        public AccessLock GetLock(TKey key, TimeSpan? timeout = default)
        {
            var semaphore = this.semaphores.GetOrAdd(key, _ => new SemaphoreSlim(this.maxCount, this.maxCount));
            return AccessLock.Create(semaphore, timeout ?? this.timeout);
        }
    }
}
