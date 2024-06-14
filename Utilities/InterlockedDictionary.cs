// <copyright file="InterlockedDictionary.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using SidekickNet.Utilities.Synchronization;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Represents a thread-safe collection of key/value pairs that can be accessed by multiple threads concurrently.
    /// It supports async delegate for value generation when adding/updating.
    /// Write operations are atomic, which means a values is only generated once for each key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <remarks>
    /// <see cref="ConcurrentDictionary{TKey, TValue}"/> allows value factory delegates to run in parallel for the sam key.
    /// In some cases, the delegate may be expensive, or not thread-safe for the same key.
    /// <see cref="InterlockedDictionary{TKey, TValue}"/> makes sure that: at any given time,
    /// Only one value factory delegate for the same key can be executed.
    /// </remarks>
    public class InterlockedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        where TKey : notnull
    {
        private static readonly IEqualityComparer<TValue> ValueComparer = EqualityComparer<TValue>.Default;

        private readonly AccessLockFactory<TKey> lockFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterlockedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lockTimeout">A <see cref="TimeSpan"/> that represents the time period to wait to acquire access locks.</param>
        public InterlockedDictionary(TimeSpan? lockTimeout = default)
        {
            this.lockFactory = new AccessLockFactory<TKey>(_ => new LocalSemaphore(1, 1), lockTimeout);
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary by using the specified function if the key does not already exist.
        /// Returns the new value, or the existing value if the key exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">
        /// The function used to generate a value for the key.
        /// It is guaranteed to be only invoked once for each key.
        /// </param>
        /// <returns>
        /// The value for the key.
        /// This will be either the existing value for the key if the key is already in the dictionary,
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Designed for more frequent read operations than write operations,
            // so check for key without lock first to improve read performance
            if (!this.ContainsKey(key))
            {
                using var @lock = this.lockFactory.GetLock(key);

                // Check one more time in case another thread has already obtained lock and added the key
                if (!this.ContainsKey(key))
                {
                    this.Add(key, valueFactory(key));
                }
            }

            return this[key];
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary by using the specified function if the key does not already exist.
        /// Returns the new value, or the existing value if the key exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">
        /// The function used to generate a value for the key.
        /// It is guaranteed to be only invoked once for each key.
        /// </param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// The result of the task is the value for the key.
        /// This will be either the existing value for the key if the key is already in the dictionary,
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Designed for more frequent read operations than write operations,
            // so check for key without lock first to improve read performance
            if (!this.ContainsKey(key))
            {
                await using var @lock = await this.lockFactory.GetLockAsync(key);

                // Check one more time in case another thread has already obtained lock and added the key
                if (!this.ContainsKey(key))
                {
                    var value = await valueFactory(key).ConfigureAwait(false);
                    this.Add(key, value);
                }
            }

            return this[key];
        }

        /// <summary>
        /// Uses the specified functions and argument
        /// to add a key/value pair to the <see cref="InterlockedDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or to update a key/value pair in the <see cref="InterlockedDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
        /// <param name="updateValueFactory">
        /// The function used to generate a new value for an existing key based on the key's existing value.
        /// </param>
        /// <returns>
        /// The new value for the key.
        /// This will be either be the result of <paramref name="addValueFactory"/> (if the key was absent)
        /// or the result of <paramref name="updateValueFactory"/> (if the key was present).
        /// </returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            using var @lock = this.lockFactory.GetLock(key);
            var value = this.ContainsKey(key) ? updateValueFactory(key, this[key]) : addValueFactory(key);
            this[key] = value;
            return value;
        }

        /// <summary>
        /// Uses the specified functions and argument
        /// to add a key/value pair to the <see cref="InterlockedDictionary{TKey, TValue}"/> if the key does not already exist,
        /// or to update a key/value pair in the <see cref="InterlockedDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
        /// <param name="updateValueFactory">
        /// The function used to generate a new value for an existing key based on the key's existing value.
        /// </param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// The result of the task is the new value for the key.
        /// This will be either be the result of <paramref name="addValueFactory"/> (if the key was absent)
        /// or the result of <paramref name="updateValueFactory"/> (if the key was present).
        /// </returns>
        public async Task<TValue> AddOrUpdateAsync(
            TKey key,
            Func<TKey, Task<TValue>> addValueFactory,
            Func<TKey, TValue, Task<TValue>> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await using var @lock = await this.lockFactory.GetLockAsync(key);
            var task = this.ContainsKey(key) ? updateValueFactory(key, this[key]) : addValueFactory(key);
            var value = await task.ConfigureAwait(false);
            this[key] = value;
            return value;
        }

        /// <summary>
        /// Updates the value associated with key to <paramref name="newValue"/>
        /// if the existing value with key is equal to <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">The key of the value that is compared with <paramref name="comparisonValue"/> and possibly replaced.</param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/>
        /// if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared with the value of the element that has the specified <paramref name="key"/>.
        /// </param>
        /// <returns><c>true</c> if the value with key was equal to comparisonValue and was replaced with newValue; otherwise, <c>false</c>.</returns>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            using var @lock = this.lockFactory.GetLock(key);

            var found = this.TryGetValue(key, out var value);
            if (found && ValueComparer.Equals(value, comparisonValue))
            {
                this[key] = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="InterlockedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        /// When this method returns, contains the object removed from the <see cref="InterlockedDictionary{TKey, TValue}"/>,
        /// or the default value of the TValue type if key does not exist.
        /// </param>
        /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
#if NETSTANDARD2_0
        public bool TryRemove(TKey key, out TValue value)
#else
        public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
#endif
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            using var @lock = this.lockFactory.GetLock(key);
            var found = this.TryGetValue(key, out value);
            return found && this.Remove(key);
        }
    }
}
