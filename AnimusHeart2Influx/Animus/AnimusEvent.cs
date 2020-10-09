namespace AnimusHeart2Influx.Animus
{

    public class Metadata
    {
    }

    public class Data
    {
        public double level { get; set; }
        public Metadata metadata { get; set; }
        public long timestamp { get; set; }
        public string unit { get; set; }
    }

    public class Metadata2
    {
    }

    public class Value
    {
        public double level { get; set; }
        public Metadata2 metadata { get; set; }
        public long timestamp { get; set; }
        public string unit { get; set; }
    }

    public class AnimusEvent
    {
        public Data data { get; set; }
        public string functionUID { get; set; }
        public string property { get; set; }
        public string topic { get; set; }
        public Value value { get; set; }
    }
}