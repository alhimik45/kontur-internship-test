namespace Kontur.GameStats.Server.Util
{
    public static class DbEntry
    {
        public static DbEntry<T1, T2> Of<T1, T2>(T1 key, T2 value)
        {
            return new DbEntry<T1, T2>(key, value);
        }
    }

    public class DbEntry<TKey, TValue>
    {
        public int Id { get; set; }
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public DbEntry()
        {
        }

        public DbEntry(TKey key, TValue value)
        {
            Id = key.GetHashCode();
            Key = key;
            Value = value;
        }
    }
}