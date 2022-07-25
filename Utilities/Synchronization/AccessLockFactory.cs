// <copyright file="AccessLockFactory.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SidekickNet.Utilities.Synchronization
{
    /// <summary>
    /// Provides a mechanism that synchronizes access to keys.
    /// </summary>
    /// <typeparam name="TKey">The type of keys.</typeparam>
    public class AccessLockFactory<TKey>
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, ISynchronizationPrimitive> primitives = new();

        private readonly Func<ISynchronizationPrimitive> primitiveFactory;

        private readonly TimeSpan timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessLockFactory{TKey}"/> class.
        /// </summary>
        /// <param name="primitiveFactory">The function that generate synchronization primitives.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the default time period to wait to acquire access locks.</param>
        public AccessLockFactory(Func<ISynchronizationPrimitive> primitiveFactory, TimeSpan? timeout = default)
        {
            this.primitiveFactory = primitiveFactory ?? throw new ArgumentNullException(nameof(primitiveFactory));
            this.timeout = timeout ?? AccessLock.Indefinite;
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
            var @lock = this.CreateLock(key);
            @lock.AcquireLock(timeout ?? this.timeout);
            return @lock;
        }

        /// <summary>
        /// Gets a new access lock for the specified key.
        /// The lock is guaranteed to be acquired. Or an exception is thrown.
        /// </summary>
        /// <param name="key">The key to get access lock for.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> that represents the time period to wait to acquire access locks.</param>
        /// <returns>A new access lock for the specified key.</returns>
        public async Task<AccessLock> GetLockAsync(TKey key, TimeSpan? timeout = default)
        {
            var @lock = this.CreateLock(key);
            await @lock.AcquireLockAsync(timeout ?? this.timeout);
            return @lock;
        }

        private AccessLock CreateLock(TKey key)
        {
            var primitive = this.primitives.GetOrAdd(key, _ => this.primitiveFactory());
            return new AccessLock(primitive);
        }
    }
}
