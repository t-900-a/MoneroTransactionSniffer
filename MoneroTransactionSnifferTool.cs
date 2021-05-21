using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monero.Client.Daemon;
using Monero.Client.Network;
using MoneroTransactionSniffer.Records;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoneroTransactionSniffer
{

    interface IMoneroTransactionSniffer : IAsyncInitialization
    {
        Task RunAsync(CancellationToken token);
    }

    class MoneroTransactionSnifferTool : BackgroundService, IMoneroTransactionSniffer
    {
        public Task Initialization { get; private set; }

        public MoneroTransactionSnifferTool(IOptions<TwitterApiCredentials> twitterCredentials, IOptions<MoneroNodeSettings> moneroNodeSettings, ILogger<MoneroTransactionSnifferTool> logger)
        {
            _logger = logger;
            _twitterPoster = new TwitterPoster(twitterCredentials.Value);
            Initialization = InitializeAsync(moneroNodeSettings.Value);
        }

        private async Task InitializeAsync(MoneroNodeSettings moneroNodeSettings)
        {
            _memoryPoolPoller = await MemoryPoolPoller.CreateAsync(new MemoryPoolPollerConfiguration()
            {
                MoneroDaemonClient = await MoneroDaemonClient.CreateAsync(moneroNodeSettings.Host, moneroNodeSettings.Port).ConfigureAwait(false),
            });
            return;
        }

        public Task RunAsync(CancellationToken token) => ExecuteAsync(token);

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
                var newTransactions = _memoryPoolPoller.GetNewTransactions();
                newTransactions.ForEach(async t =>
                {
                    await AnalyzeTransaction(t).ConfigureAwait(false);
                });
            }
        }

        private async Task AnalyzeTransaction(MemoryPoolTransaction t)
        {
            try
            {
                var (isSuspicious, susInfo) = MoneroTransactionIdentifier.GetSuspicionData(t);
                if (isSuspicious)
                {
                    var message = MoneroTransactionIdentifier.GenerateMessage(susInfo);
                    _logger.LogInformation(message);
                    await _twitterPoster.PostAsync(message).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private IMemoryPoolPoller _memoryPoolPoller;
        private readonly ITwitterPoster _twitterPoster;
        private readonly ILogger<MoneroTransactionSnifferTool> _logger;
    }
}
