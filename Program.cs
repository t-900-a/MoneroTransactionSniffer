using MoneroTransactionSniffer;
using System.Threading;

var cts = new CancellationTokenSource();
MoneroTransactionSnifferTool mtst = await MoneroTransactionSnifferTool.CreateAsync(new MoneroTransactionSnifferConfiguration()
{
    Uri = "localhost",
    Port = 18081,
}).ConfigureAwait(false);
await mtst.RunAsync(cts.Token).ConfigureAwait(false);