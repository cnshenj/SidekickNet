// <copyright file="AccessLock.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace SidekickNet.Utilities.Synchronization
{
    /// <summary>
    /// Provides a mechanism that synchronizes access to resources.
    /// The lock will be automatically released when disposed synchronously or asynchronously.
    /// </summary>
    public class AccessLock : IDisposable, IAsyncDisposable
    {
        /// <summary>A <see cref="TimeSpan"/> to test lock state and return immediately.</summary>
        public static readonly TimeSpan Immediate = TimeSpan.Zero;

        /// <summary>
        /// A <see cref="TimeSpan"/> to wait indefinitely for lock acquisition.
        /// Technically it should -1 ms.
        /// In reality, 10 days can be considered indefinite for locking purpose, and is friendly to calculation.
        /// </summary>
        public static readonly TimeSpan Indefinite = TimeSpan.FromDays(10);

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessLock"/> class.
        /// </summary>
        /// <param name="synchronizationPrimitive">The underlying synchronization primitive that synchronizes access to resources.</param>
        public AccessLock(ISynchronizationPrimitive synchronizationPrimitive)
        {
            this.SynchronizationPrimitive = synchronizationPrimitive;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AccessLock"/> class.
        /// </summary>
        ~AccessLock() => this.Dispose(false);

        /// <summary>Gets a value indicating whether the lock is successfully acquired.</summary>
        public bool Acquired { get; private set; }

        /// <summary>Gets the underlying synchronization primitive.</summary>
        protected ISynchronizationPrimitive SynchronizationPrimitive { get; }

        /// <summary>
        /// Acquires the lock to access the protected resource.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait,
        /// a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely,
        /// or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the primitive and return immediately.
        /// </param>
        /// <returns><c>true</c> if the current thread successfully entered the SemaphoreSlim; otherwise, <c>false</c>.</returns>
        public bool AcquireLock(TimeSpan timeout)
        {
            return this.Acquired = this.SynchronizationPrimitive.Wait(timeout);
        }

        /// <summary>
        /// Asynchronously acquires the lock to access the protected resource.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait,
        /// a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely,
        /// or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the primitive and return immediately.
        /// </param>
        /// <returns>
        /// A task that will complete with a result of <c>true</c> if the current thread successfully got the access,
        /// otherwise with a result of <c>false</c>.
        /// </returns>
        public ValueTask<bool> AcquireLockAsync(TimeSpan timeout)
        {
            return this.SynchronizationPrimitive.WaitAsync(timeout);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);

            this.Dispose(false);

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
                    this.SynchronizationPrimitive.Release();
                    this.Acquired = false;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Frees managed resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this.Acquired)
            {
                await this.SynchronizationPrimitive.ReleaseAsync().ConfigureAwait(false);
                this.Acquired = false;
            }
        }
    }
}
