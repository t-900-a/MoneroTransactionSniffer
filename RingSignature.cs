using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MoneroTransactionSniffer
{
    class RingSignature
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
        [JsonPropertyName("txnFee")]
        public ulong Fee { get; set; }
    }
}
