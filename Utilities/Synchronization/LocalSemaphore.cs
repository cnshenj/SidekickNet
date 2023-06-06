// <copyright file="LocalSemaphore.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SidekickNet.Utilities.Synchronization;

/// <summary>
/// Synchronization primitive using <see cref="SemaphoreSlim"/>.
/// It can only be used for synchronization on a local machine.
/// </summary>
public class LocalSemaphore : ISynchronizationPrimitive
{
    private readonly SemaphoreSlim semaphoreSlim;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSemaphore"/> class, specifying
    /// the initial and maximum number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
    public LocalSemaphore(int initialCount)
    {
        this.semaphoreSlim = new SemaphoreSlim(initialCount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSemaphore"/> class, specifying
    /// the initial and maximum number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
    /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
    public LocalSemaphore(int initialCount, int maxCount)
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