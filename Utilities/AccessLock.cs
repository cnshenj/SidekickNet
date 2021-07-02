// <copyright file="AccessLock.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Threading;

namespace SidekickNet.Utilities
{
    /// <summary>
    /// Provides a mechanism that synchronizes access to objects.
    /// </summary>
    public class AccessLock : IDisposable
    {
        /// <summary>A <see cref="TimeSpan"/> to test lock state and return immediately.</summary>
        public static readonly TimeSpan Immediate = TimeSpan.Zero;

        /// <summary>A <see cref="TimeSpan"/> to wait indefinitely for lock acquisition.</summary>
        public static readonly TimeSpan Indefinite = TimeSpan.FromMilliseconds(-1);

        private readonly SemaphoreSlim semaphore;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessLock"/> class.
        /// </summary>
        /// <param name="semaphore">The underlying semaphore that synchronizes access to objects.</param>
        private AccessLock(SemaphoreSlim semaphore)
        {
            this.semaphore = semaphore;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AccessLock"/> class.
        /// </summary>
        ~AccessLock() => this.Dispose(false);

        /// <summary>Gets a value indicating whether the lock is successfully acquired.</summary>
        public bool Acquired { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AccessLock"/>. The lock may or may not be acquired.
        /// </summary>
        /// <param name="semaphore">The underlying semaphore that synchronizes access to objects.</param>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait to acquire an access lock,
        /// a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely,
        /// or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the semaphore and return immediately.
        /// </param>
        /// <returns>A new <see cref="AccessLock"/>.</returns>
        public static AccessLock TryCreate(SemaphoreSlim semaphore, TimeSpan timeout)
        {
            var accessLock = new AccessLock(semaphore);
            accessLock.AcquireLock(timeout);
            return accessLock;
        }

        /// <summary>
        /// Creates a new <see cref="AccessLock"/>.
        /// The lock is acquired. Or a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The underlying semaphore that synchronizes access to objects.</param>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait to acquire an access lock,
        /// a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely,
        /// or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the semaphore and return immediately.
        /// </param>
        /// <returns>A new <see cref="AccessLock"/>.</returns>
        public static AccessLock Create(SemaphoreSlim semaphore, TimeSpan timeout)
        {
            var accessLock = TryCreate(semaphore, timeout);
            if (!accessLock.Acquired)
            {
                throw new TimeoutException("Unable to acquire the access lock within the specified time span.");
            }

            return accessLock;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether the method is directly or indirectly called by user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // No managed resource to dispose
                }

                if (this.Acquired)
                {
                    this.semaphore.Release();
                }

                this.disposed = true;
            }
        }

        private bool AcquireLock(TimeSpan timeout)
        {
            return this.Acquired = this.semaphore.Wait(timeout);
        }
    }
}
