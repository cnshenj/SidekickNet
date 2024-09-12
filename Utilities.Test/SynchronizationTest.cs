using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SidekickNet.Utilities.Synchronization;
using Xunit;

namespace SidekickNet.Utilities.Test
{
    public class SynchronizationTest
    {
        [Fact]
        public async void Wait_And_Release_Local_Semaphore()
        {
            var semaphore = new LocalSemaphore(1, 1);
            var result = await semaphore.WaitAsync(TimeSpan.Zero);
            Assert.True(result);

            await semaphore.ReleaseAsync();

            // After release it can be acquired again
            result = await semaphore.WaitAsync(TimeSpan.Zero);
            Assert.True(result);
        }

        [Fact]
        public async void Await_Using_AccessLock()
        {
            bool result;
            var semaphore = new LocalSemaphore(1, 1);
            {
                await using var @lock = new AccessLock(semaphore);
                result = await @lock.TryAcquireLockAsync(TimeSpan.Zero);
                Assert.True(result);

                // Can't be acquired again without release
                result = await semaphore.WaitAsync(TimeSpan.Zero);
                Assert.False(result);
            }

            // @lock out of scope, semaphore was released and can be acquired again
            result = await semaphore.WaitAsync(TimeSpan.Zero);
            Assert.True(result);
        }

        [Fact]
        public void Get_Lock_From_Factory()
        {
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            using var lock1 = factory.GetLock("foo");
            using var lock2 = factory.GetLock("bar");
            Assert.True(lock1.Acquired);
            Assert.True(lock2.Acquired);
        }

        [Fact]
        public void Get_Lock_Again_Throw_Exception()
        {
            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            using var @lock = factory.GetLock(key);
            Assert.Throws<TimeoutException>(() => factory.GetLock(key, TimeSpan.Zero));
        }

        [Fact]
        public async Task Try_Get_Lock_Again_Not_Acquired()
        {
            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            using var @lock = factory.GetLock(key);
            await Task.Run(() =>
            {
                using var lockAgain = factory.TryGetLock(key, TimeSpan.Zero);
                Assert.False(lockAgain.Acquired);
            });
        }

        [Fact]
        public async Task Get_Lock_Again_After_Release()
        {
            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            Task<DateTime> task;
            using (var @lock = factory.GetLock(key))
            {
                Assert.True(@lock.Acquired);
                task = Task.Run(() =>
                {
                    using var lockAgain = factory.TryGetLock(key);
                    Assert.True(lockAgain.Acquired);
                    return DateTime.Now;
                });
                Thread.Sleep(100);
            }

            var releaseTime = DateTime.Now;
            var lockAgainTime = await task;
            // Since the release time is not obtained at the exact time when the lock is released,
            // allow 1 millisecond tolerance when comparing release time and lock time
            Assert.True(
                lockAgainTime > releaseTime - TimeSpan.FromMilliseconds(1),
                $"Access lock must be acquired again after being released. Release time: {releaseTime.Ticks}, Lock again time: {lockAgainTime.Ticks}.");
        }

        [Fact]
        public async Task Get_Lock_Async_From_Factory()
        {
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            await using var lock1 = await factory.GetLockAsync("foo");
            await using var lock2 = await factory.GetLockAsync("bar");
            Assert.True(lock1.Acquired);
            Assert.True(lock2.Acquired);
        }

        [Fact]
        public async Task Get_Lock_Async_Again_Throw_Exception()
        {
            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            await using var @lock = await factory.GetLockAsync(key);
            await Assert.ThrowsAsync<TimeoutException>(async () => await factory.GetLockAsync(key, TimeSpan.Zero));
        }

        [Fact]
        public async Task Try_Get_Lock_Async_Again_Not_Acquired()
        {
            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            await using var @lock = await factory.GetLockAsync(key);
            await Task.Run(async () =>
            {
                await using var lockAgain = await factory.TryGetLockAsync(key, TimeSpan.Zero);
                Assert.False(lockAgain.Acquired);
            });
        }

        [Fact]
        public async Task Try_Get_Lock_Concurrently_Only_One_Acquired()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            var task1 = factory.TryGetLockAsync(key, TimeSpan.FromMilliseconds(100));
            var task2 = factory.TryGetLockAsync(key, TimeSpan.FromMilliseconds(100));
            var locks = await Task.WhenAll(task1, task2);
            var locked = locks.Where(l => l.Acquired);

            // Only 1 lock should be acquired concurrently
            Assert.Single(locked);
        }

        [Fact]
        public async Task Get_Lock_Async_Again_After_Release()
        {
            const string key = "foobar";
            var factory = new AccessLockFactory<string>(_ => new LocalSemaphore(1, 1));
            Task<DateTime> task;
            await using (var @lock = await factory.GetLockAsync(key))
            {
                Assert.True(@lock.Acquired);
                task = Task.Run(async () =>
                {
                    await using var lockAgain = await factory.TryGetLockAsync(key);
                    Assert.True(lockAgain.Acquired);
                    return DateTime.Now;
                });
                Thread.Sleep(100);
            }

            var releaseTime = DateTime.Now;
            var lockAgainTime = await task;
            // Since the release time is not obtained at the exact time when the lock is released,
            // allow 1 millisecond tolerance when comparing release time and lock time
            Assert.True(
                lockAgainTime > releaseTime - TimeSpan.FromMilliseconds(1),
                "Access lock must be acquired again after being released.");
        }
    }
}
