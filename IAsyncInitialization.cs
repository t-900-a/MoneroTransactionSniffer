using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MoneroTransactionSniffer
{
    interface IAsyncInitialization
    {
        Task Initialization { get; }
    }
}
