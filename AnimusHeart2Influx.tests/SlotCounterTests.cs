using System;
using Microsoft.Win32.SafeHandles;
using Moq;
using Shouldly;
using Xunit;

namespace AnimusHeart2Influx.tests
{
    public class SlotCounterTests
    {
        [Fact]
        public void Should_calculate_current_slot_correct()
        {   
            // Arrange
            var right = new Mock<IRight>();
            right.Setup(r => r.Now).Returns(new DateTime(2020, 1, 1));
            var sc = new SlotCounter(right.Object);
            
            // Act, Assert
            sc.CurrentSlot.ShouldBe(0);
        }

        [Fact]
        public void Should_set_next_slot_time_correct()
        {
            // Arrange
            var right = new Mock<IRight>();
            right.Setup(r => r.Now).Returns(new DateTime(2020, 1, 1));
            var sc = new SlotCounter(right.Object);

            // Act
            sc.CalculateNextSlotStart();

            // Assert
            sc.NextSlotTimeStart.ShouldBe(new DateTime(2020, 1, 1).AddMinutes(6));
        }
    }
}