using LiteDB;

namespace Kontur.GameStats.Server.Data
{
    public class DbEntry<TId,TValue>
    {
        [BsonId]
        public TId Id { get; set; }
        public TValue Value { get; set; }

        public DbEntry()
        {
        }

        public DbEntry(TId id, TValue value)
        {
            Id = id;
            Value = value;
        }
    }
}