using Monero.Client.Daemon;
using Monero.Client.Network;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoneroTransactionSniffer
{

    class MoneroTransactionSnifferConfiguration
    {
        public MoneroTransactionSnifferConfiguration()
        {
            // TODO: Use Microsoft's dependency injection framework
            var nvc = ConfigurationManager.AppSettings;
            Uri = nvc["Hostname"];
            Port = Convert.ToUInt32(nvc["Port"]);
            TwitterPosterConfiguration = new TwitterPosterConfiguration()
            {
                ApiSecret = nvc["ApiSecret"],
                ApiKey = nvc["ApiKey"],
                BearerToken = nvc["BearerToken"],
                ConsumerToken = nvc["ConsumerToken"],
                ConsumerTokenSecret = nvc["ConsumerTokenSecret"],
            };
        }

        public string Uri { get; set; }
        public uint Port { get; set; }
        public TwitterPosterConfiguration TwitterPosterConfiguration { get; set; }
    }

    interface IMoneroTransactionSniffer
    {
        Task RunAsync(CancellationToken token);
    }

    class MoneroTransactionSnifferTool : IMoneroTransactionSniffer
    {
        private MoneroTransactionSnifferTool(MoneroTransactionSnifferConfiguration moneroSnifferConfiguration)
        {
            _twitterPoster = new TwitterPoster(moneroSnifferConfiguration.TwitterPosterConfiguration);
        }

        public static Task<MoneroTransactionSnifferTool> CreateAsync(MoneroTransactionSnifferConfiguration moneroTransactionSnifferConfiguration)
        {
            var mts = new MoneroTransactionSnifferTool(moneroTransactionSnifferConfiguration);
            return mts.InitializeAsync(moneroTransactionSnifferConfiguration);
        }

        private async Task<MoneroTransactionSnifferTool> InitializeAsync(MoneroTransactionSnifferConfiguration moneroSnifferConfiguration)
        {
            _memoryPoolPoller = await MemoryPoolPoller.CreateAsync(new MemoryPoolPollerConfiguration()
            {
                MoneroDaemonClient = await MoneroDaemonClient.CreateAsync(moneroSnifferConfiguration.Uri, moneroSnifferConfiguration.Port).ConfigureAwait(false),
            });
            return this;
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), token).ConfigureAwait(false);
                var newTransactions = _memoryPoolPoller.GetNewTransactions();
                newTransactions.ForEach(async t =>
                {
                    try
                    {
                        var (isSuspicious, susInfo) = MoneroTransactionIdentifier.GetSuspicionData(t);
                        if (isSuspicious)
                        {
                            var message = MoneroTransactionIdentifier.GenerateMessage(susInfo);
                            await _twitterPoster.PostAsync(message).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }

        private IMemoryPoolPoller _memoryPoolPoller;
        private readonly ITwitterPoster _twitterPoster;
    }
}
