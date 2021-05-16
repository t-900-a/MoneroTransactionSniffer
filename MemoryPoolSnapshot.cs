using MoneroTransactionSniffer.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer
{
    public class MemoryPoolSnapshot
    {
        public List<MemoryPoolTransaction> Transactions { get; set; } = new List<MemoryPoolTransaction>();
        public int TransactionCount { get => Transactions.Count; }
    }
}