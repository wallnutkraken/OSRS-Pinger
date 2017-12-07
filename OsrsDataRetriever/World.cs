namespace OsrsDataRetriever
{
    public class World
    {
        public string Url { get; internal set; }
        public int Number { get; internal set; }

        internal World(string url, int number)
        {
            Number = number;
            Url = url;
        }
    }
}
