using MoneroTransactionSniffer.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MoneroTransactionSniffer.Extensions;

namespace MoneroTransactionSniffer
{
    interface IMemoryPoolStash
    {
        List<MemoryPoolTransaction> AcceptSnapshot(MemoryPoolSnapshot memoryPoolSnapshot);
    }

    class MemoryPoolStash : IMemoryPoolStash
    {
        public List<MemoryPoolTransaction> AcceptSnapshot(MemoryPoolSnapshot memoryPoolSnapshot)
        {
            return _differ.Difference(memoryPoolSnapshot.Transactions);
        }

        private readonly IMemoryPoolDiffer _differ = new MemoryPoolDiffer();
    }
}
