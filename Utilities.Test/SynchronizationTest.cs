using System;
using SidekickNet.Utilities.Synchronization;
using Xunit;

using Convert = SidekickNet.Utilities.BasicConvert;

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
                result = await @lock.AcquireLockAsync(TimeSpan.Zero);
                Assert.True(result);

                // Can't be acquired again without release
                result = await semaphore.WaitAsync(TimeSpan.Zero);
                Assert.False(result);
            }

            // @lock out of scope, semaphore was released and can be acquired again
            result = await semaphore.WaitAsync(TimeSpan.Zero);
            Assert.True(result);
        }
    }
}
