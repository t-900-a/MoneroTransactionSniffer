using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit { }
}

namespace MoneroTransactionSniffer.Records
{
    public record MemoryPoolTransaction(string TxHash, bool DoubleSpendSeen, string AsJson);
}