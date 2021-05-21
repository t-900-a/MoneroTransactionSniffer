using System;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer
{
    public class TwitterApiCredentials
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string BearerToken { get; set; }
    }
}