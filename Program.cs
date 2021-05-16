using MoneroTransactionSniffer;
using System.Threading;

var cts = new CancellationTokenSource();
MoneroTransactionSnifferTool mtst = await MoneroTransactionSnifferTool.CreateAsync(new()).ConfigureAwait(false);
await mtst.RunAsync(cts.Token).ConfigureAwait(false);