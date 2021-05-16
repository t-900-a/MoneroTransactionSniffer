using Monero.Client.Daemon;
using MoneroTransactionSniffer.Extensions;
using MoneroTransactionSniffer.Records;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MoneroTransactionSniffer
{
    public class MemoryPoolPollerConfiguration
    {
        public IMoneroDaemonClient MoneroDaemonClient { get; set; }
    }

    public interface IMemoryPoolPoller
    {
        List<MemoryPoolTransaction> GetNewTransactions();
    }

    public class MemoryPoolPoller : IMemoryPoolPoller
    {
        private MemoryPoolPoller(IMoneroDaemonClient moneroDaemonClient)
        {
            _moneroDaemonClient = moneroDaemonClient;
        }

        public static Task<MemoryPoolPoller> CreateAsync(MemoryPoolPollerConfiguration memoryPoolPollerConfiguration)
        {
            var mpp = new MemoryPoolPoller(memoryPoolPollerConfiguration.MoneroDaemonClient);
            return mpp.InitializeAsync();
        }

        private Task<MemoryPoolPoller> InitializeAsync()
        {
            _pollTask = PollMemoryPool();
            return Task.FromResult(this);
        }

        private async Task PollMemoryPool(CancellationToken token = default)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), token).ConfigureAwait(false);
                var memoryPool = await _moneroDaemonClient.GetTransactionPoolAsync(token).ConfigureAwait(false);
                var newTransactionsAdded = _memoryPoolStash.AcceptSnapshot(new MemoryPoolSnapshot()
                {
                    Transactions = memoryPool.Transactions.Select(x => new MemoryPoolTransaction(x.TxHash, x.IsDoubleSpendSeen, x.TxJson)).ToList(),
                });
                lock (_lock)
                    _newTransactions.Enqueue(newTransactionsAdded);
            }
        }

        public List<MemoryPoolTransaction> GetNewTransactions()
        {
            lock (_lock)
                return new List<MemoryPoolTransaction>(_newTransactions.DequeueAll());
        }

        private Task _pollTask = null;
        private readonly IMoneroDaemonClient _moneroDaemonClient = null;
        private readonly object _lock = new object();
        private readonly IMemoryPoolStash _memoryPoolStash = new MemoryPoolStash();
        private readonly Queue<MemoryPoolTransaction> _newTransactions = new Queue<MemoryPoolTransaction>();
    }
}