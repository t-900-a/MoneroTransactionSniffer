using MoneroTransactionSniffer.Extensions;
using MoneroTransactionSniffer.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer
{
    interface IMemoryPoolDiffer
    {
        List<MemoryPoolTransaction> Difference(IEnumerable<MemoryPoolTransaction> transactions);
    }

    class MemoryPoolDiffer : IMemoryPoolDiffer
    {

        public List<MemoryPoolTransaction> Difference(IEnumerable<MemoryPoolTransaction> transactions)
        {
            var newTransactions = new HashSet<MemoryPoolTransaction>(transactions);
            newTransactions.ExceptWith(_currentTransactions);
            _currentTransactions.Clear();
            _currentTransactions.AddRange(transactions);
            return new List<MemoryPoolTransaction>(newTransactions);
        }

        private readonly HashSet<MemoryPoolTransaction> _currentTransactions = new HashSet<MemoryPoolTransaction>();
    }
}
