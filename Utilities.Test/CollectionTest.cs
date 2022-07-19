using System.Linq;

using Xunit;

namespace SidekickNet.Utilities.Test
{
    public class CollectionTest
    {
        [Fact]
        public void Create_Partitions_From_Enumerable()
        {
            var source = Enumerable.Range(1, 10).ToArray();
            const int size = 3;
            var partitions = source.ToPartitions(size).ToArray();

            // All partitions, except the last one should have exactly 'size' number of elements
            Assert.True(partitions.SkipLast(1).All(p => p.Count() == size));

            // The last partition should have no more than 'size' elements
            Assert.True(partitions.TakeLast(1).All(p => p.Count() <= size));

            // All partitions combined contain the same elements as the source, in the same order
            Assert.Equal(source, partitions.SelectMany(p => p));
        }
    }
}
