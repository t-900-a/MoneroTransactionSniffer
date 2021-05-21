using Microsoft.Extensions.DependencyInjection;
using MoneroTransactionSniffer;
using System.Threading;

var cts = new CancellationTokenSource();
using var host = MoneroTransactionSnifferBuilder.BuildContext();
using var serviceScope = host.Services.CreateScope();
MoneroTransactionSnifferServiceProvider.ServiceProvider = host.Services;
var mts = MoneroTransactionSnifferServiceProvider.ServiceProvider.GetRequiredService<IMoneroTransactionSniffer>();
await mts.Initialization.ConfigureAwait(false);
await mts.RunAsync(cts.Token).ConfigureAwait(false);
