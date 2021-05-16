using System;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer.Extensions
{
    static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> hashset, IEnumerable<T> items)
        {
            foreach (var item in items)
                hashset.Add(item);
        }
    }
}
