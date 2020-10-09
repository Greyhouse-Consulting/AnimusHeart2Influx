using Shouldly;
using Xunit;

namespace AnimusHeart2Influx.tests
{
    public class MessageSamplerService_Tests
    {
        [Fact]
        public void Should_calculate_max_messages_per_minute_correct()
        {
            // Arrange
            var ss = new MessageCounter(7000);
            
            // Act
            var m = ss.MaxMessagesPerSampleSlot;

            // Assert
            m.ShouldBeInRange(4.86, 4.87);
        }
    }
}