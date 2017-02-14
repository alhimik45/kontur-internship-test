using System.Collections.Generic;

namespace Kontur.GameStats.Server.Util
{
    public static class Pair
    {
        public static Pair<T1, T2> Of<T1, T2>(T1 item1, T2 item2)
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

        private bool Equals(Pair<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Pair<T1, T2>) obj);
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