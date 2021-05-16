using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MoneroTransactionSniffer
{
    class MemoryPoolTransactionJsonDecoding
    {
        [JsonPropertyName("version")]
        public uint Version { get; set; }
        [JsonPropertyName("unlock_time")]
        public ulong UnlockTime { get; set; }
        [JsonPropertyName("rct_signatures")]
        public RingSignature RingSignature { get; set; }
    }
}
