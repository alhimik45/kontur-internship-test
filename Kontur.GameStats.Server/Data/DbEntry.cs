using LiteDB;

namespace Kontur.GameStats.Server.Data
{
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