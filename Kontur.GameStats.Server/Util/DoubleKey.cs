using System;
using System.IO;

namespace Kontur.GameStats.Server.Util
{
    public class DoubleKey
    {
        public string Key1 { get; set; }
        public string Key2 { get; set; }

        public static DoubleKey Of(string key1, string key2)
        {
            return new DoubleKey
            {
                Key1 = key1,
                Key2 = key2
            };
        }

        public override string ToString()
        {
            return Key1 + Path.DirectorySeparatorChar + Key2;
        }

        public string ToEscapedString()
        {
            return Uri.EscapeDataString(Key1) + Path.DirectorySeparatorChar + Uri.EscapeDataString(Key2);
        }
    }
}
