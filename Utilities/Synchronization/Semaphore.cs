// <copyright file="Semaphore.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System.Threading;

namespace SidekickNet.Utilities.Synchronization;

using System;
using System.Threading.Tasks;

/// <summary>
/// Synchronization primitive using <see cref="System.Threading.SemaphoreSlim"/>.
/// </summary>
public class Semaphore : ISynchronizationPrimitive
{
    private readonly SemaphoreSlim semaphoreSlim;

    /// <summary>
    /// Initializes a new instance of the <see cref="Semaphore"/> class, specifying
    /// the initial and maximum number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
    public Semaphore(int initialCount)
    {
        this.semaphoreSlim = new SemaphoreSlim(initialCount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Semaphore"/> class, specifying
    /// the initial and maximum number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
    /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
    public Semaphore(int initialCount, int maxCount)
    {
        this.semaphoreSlim = new SemaphoreSlim(initialCount, maxCount);
    }

    /// <inheritdoc />
    public bool Wait(TimeSpan timeout)
    {
        return this.semaphoreSlim.Wait(timeout);
    }

    /// <inheritdoc />
    public void Release()
    {
        this.semaphoreSlim.Release();
    }

    /// <inheritdoc />
    public async ValueTask<bool> WaitAsync(TimeSpan timeout)
    {
        return await this.semaphoreSlim.WaitAsync(timeout).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask ReleaseAsync()
    {
        this.semaphoreSlim.Release();
        return default;
    }
}