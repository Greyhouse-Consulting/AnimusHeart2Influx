namespace AnimusHeart2Influx.Animus
{
    public class AnimusConfiguration
    {
        public AnimusConfiguration(string animusKey, string animusUrl)
        {
            Key = animusKey;
            Url = animusUrl;
        }

        public string Key { get; }
        public string Url { get; }
    }
}