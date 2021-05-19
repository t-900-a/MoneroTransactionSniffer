using System;
using System.Collections.Generic;
using System.Text;

namespace MoneroTransactionSniffer
{
    sealed class MoneroDenominationConverter
    {
        private const double PiconerosPerMonero = 1_000_000_000_000;

        public static double PiconeroToMonero(ulong piconero)
        {
            return piconero / PiconerosPerMonero;
        }
    }
}
