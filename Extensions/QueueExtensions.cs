using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer.Extensions
{
    static class QueueExtensions
    {
        public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
                queue.Enqueue(item);
        }

        public static IEnumerable<T> DequeueAll<T>(this Queue<T> queue)
        {
            List<T> allItems = new();
            while (queue.Count != 0)
            {
                var _ = queue.TryDequeue(out var singleItem);
                allItems.Add(singleItem);
            }
            return allItems;
        }
    }
}
