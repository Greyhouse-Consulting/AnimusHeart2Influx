using System;
using System.Resources;

namespace AnimusHeart2Influx
{
    public class MessageCounter
    {
        private int _ticks;
        private DateTime _sampleBegin;
        public double MaxMessagesPerSampleSlot { get; }

        public MessageCounter(int maxWebSocketMessagesPerDay)
        {
            MaxMessagesPerSampleSlot = ((float)maxWebSocketMessagesPerDay / 288);

            Reset();
        }

        public void Reset()
        {
            _ticks = 0;
            _sampleBegin = DateTime.Now;
        }

        public bool HasReachSlotLimit() => _ticks > MaxMessagesPerSampleSlot;

        public void Tick()
        {
            _ticks++;
        }

        public float MessagesPerMinute()
        {
            return (float)_ticks / SampleTime.Minutes;
        }

        public TimeSpan SampleTime => DateTime.Now - _sampleBegin;

    }

    public class SlotCounter
    {
        private readonly IRight _right;

        public SlotCounter(IRight right)
        {
            _right = right;
        }

        public int CurrentSlot => (_right.Now.Minute + _right.Now.Hour * 60) / 5;

        public DateTime NextSlotTimeStart { get; private set; }

        public void CalculateNextSlotStart()
        {
            NextSlotTimeStart = _right.Now.Date.AddMinutes(5 * (CurrentSlot + 1));
        }
    }

    public interface IRight
    {
        public DateTime Now { get; }
    }

    public class Right : IRight
    {
        public DateTime Now => DateTime.Now;
    }
}