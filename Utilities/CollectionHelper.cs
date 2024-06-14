// <copyright file="CollectionHelper.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Helper methods for collections.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Performs the specified action on each element of <paramref name="sequence"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="sequence">A sequence of elements.</param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element.</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var element in sequence)
            {
                action(element);
            }
        }

        /// <summary>
        /// Adds the elements of the specified sequence to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The <see cref="ICollection{T}"/> that elements will be added to.</param>
        /// <param name="sequence">The sequence whose elements should be added to the <see cref="ICollection{T}"/>.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> sequence)
        {
            foreach (var element in sequence)
            {
                collection.Add(element);
            }
        }

#if !NET6_0_OR_GREATER // DistinctBy already provided by .NET 6 and above
        /// <summary>
        /// Returns distinct elements from a sequence, using a calculated key.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the calculated key.</typeparam>
        /// <param name="source">The source to get distinct elements from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="keyComparer">Determines whether the specified keys are equal.</param>
        /// <returns>The distinct elements from the specified sequence.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource?, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = default)
        {
            return source.Distinct(new DelegateComparer<TSource, TKey>(keySelector, keyComparer));
        }
#endif

        /// <summary>
        /// Gets the element with the specified key, or default value if the key doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of elements' values.</typeparam>
        /// <param name="dictionary">The dictionary that contains elements.</param>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The value of the element with the specified key, or default value if the key doesn't exist.</returns>
        public static TValue? AtOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }

        /// <summary>
        /// Gets the element with the specified key, or default value if the key doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of the value of the element to get.</typeparam>
        /// <param name="dictionary">The dictionary that contains elements.</param>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The value of the element with the specified key, or default value if the key doesn't exist.</returns>
        public static TValue? AtOrDefault<TKey, TValue>(this IDictionary<TKey, object> dictionary, TKey key)
            where TValue : struct
        {
            return dictionary.TryGetValue(key, out var value) ? BasicConvert.ToType<TValue>(value) : default;
        }

        /// <summary>
        /// Gets the element with the specified key, or <c>null</c> if the key doesn't exist.
        /// </summary>
        /// <typeparam name="TValue">The type of the value of the element to get.</typeparam>
        /// <param name="dictionary">The dictionary that contains elements.</param>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The value of the element with the specified key, or default value if the key doesn't exist.</returns>
        public static TValue? AtOrDefault<TValue>(this IDictionary<string, object> dictionary, string key)
            where TValue : struct
        {
            return dictionary.TryGetValue(key, out var value) ? BasicConvert.ToType<TValue>(value) : default;
        }

        /// <summary>
        /// Gets the item with the specified key, or <c>null</c> if the key doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the collection.</typeparam>
        /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
        /// <param name="collection">The dictionary that contains elements.</param>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The value of the element with the specified key, or default value if the key doesn't exist.</returns>
        public static TItem? AtOrDefault<TKey, TItem>(this KeyedCollection<TKey, TItem> collection, TKey key)
            where TKey : notnull
        {
            return collection.Contains(key) ? collection[key] : default;
        }

        /// <summary>
        /// Creates a <see cref="ISet{T}"/> from an <see cref="IEnumerable{T}"/>
        /// according to a specified comparer or the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The source of elements to create <see cref="ISet{T}"/> from.</param>
        /// <param name="comparer">An optional comparer to compare elements.</param>
        /// <returns>A <see cref="ISet{T}"/> that contains unique elements.</returns>
        public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = default)
        {
            return comparer == null ? [..source] : new HashSet<T>(source, comparer);
        }

        /// <summary>
        /// Creates a <typeparamref name="TCollection"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TItem">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TCollection">The type of the collection to create.</typeparam>
        /// <param name="source">The source of elements to create <typeparamref name="TCollection"/> from.</param>
        /// <returns>A <typeparamref name="TCollection"/> that contains elements from <paramref name="source"/>.</returns>
        public static TCollection ToCollection<TItem, TCollection>(this IEnumerable<TItem> source)
            where TCollection : ICollection<TItem>, new()
        {
            var collection = new TCollection();
            collection.AddRange(source);
            return collection;
        }

        /// <summary>
        /// Creates a series of <see cref="IEnumerable{T}"/> from an <see cref="IEnumerable{T}"/>.
        /// Each <see cref="IEnumerable{T}"/> contains <paramref name="size"/> elements,
        /// except the last one which contains the rest of elements.
        /// </summary>
        /// <typeparam name="TItem">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/> to create partitions from.</param>
        /// <param name="size">The size of partitions.</param>
        /// <returns>Partitions created from <paramref name="source"/>.</returns>
        public static IEnumerable<IEnumerable<TItem>> ToPartitions<TItem>(this IEnumerable<TItem> source, int size)
        {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return GetPartition(enumerator, size);
            }
        }

        private static ICollection<TItem> GetPartition<TItem>(IEnumerator<TItem> enumerator, int size)
        {
            var count = size;
            var list = new List<TItem>();
            do
            {
                list.Add(enumerator.Current);
            }
            while (--count > 0 && enumerator.MoveNext());

            return list;
        }
    }
}
