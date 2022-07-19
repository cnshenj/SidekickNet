// <copyright file="ISynchronizationPrimitive.cs" company="Zhang Shen">
// Copyright (c) Zhang Shen. All rights reserved.
// </copyright>

namespace SidekickNet.Utilities.Synchronization;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// .NET has different interfaces for synchronization primitives.
/// For example, <see cref="Semaphore"/> and <see cref="SemaphoreSlim"/> have different interfaces.
/// Define a common interface for synchronization primitives.
/// </summary>
public interface ISynchronizationPrimitive
{
    /// <summary>
    /// Blocks the current thread until it can access the protected resource.
    /// </summary>
    /// <param name="timeout">
    /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait,
    /// a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely,
    /// or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the primitive and return immediately.
    /// </param>
    /// <returns><c>true</c> if the current thread successfully entered the SemaphoreSlim; otherwise, <c>false</c>.</returns>
    bool Wait(TimeSpan timeout);

    /// <summary>
    /// Releases the access to the protected resource.
    /// </summary>
    void Release();

    /// <summary>
    /// Asynchronously waits to get the access to the protected resource.
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
    ValueTask<bool> WaitAsync(TimeSpan timeout);

    /// <summary>
    /// Asynchronously releases the access to the protected resource.
    /// </summary>
    /// <returns>A task represents the asynchronous operation.</returns>
    ValueTask ReleaseAsync();
}