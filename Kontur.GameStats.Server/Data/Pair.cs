using System.Collections.Generic;
using LiteDB;

namespace Kontur.GameStats.Server.Data
{
    public static class Pair
    {
        public static Pair<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Pair<T1, T2>
            {
                Item1 = item1,
                Item2 = item2
            };
        }
    }

    public class Pair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Pair()
        {
        }

        protected bool Equals(Pair<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Pair<T1, T2>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T1>.Default.GetHashCode(Item1)*397) ^ EqualityComparer<T2>.Default.GetHashCode(Item2);
            }
        }
    }
}