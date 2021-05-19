using Monero.Client.Daemon.POD;
using MoneroTransactionSniffer.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MoneroTransactionSniffer
{
    enum SuspicionDegree
    {
        Indeterminate = 0,
        Low,
        Medium,
        High,
        VeryHigh,
    }

    class SuspicionData
    {
        public SuspicionData(string txHash, SuspicionDegree degree)
        {
            TxHash = txHash;
            SuspicionDegree = degree;
        }

        public string TxHash { get; private set; }
        public SuspicionDegree SuspicionDegree { get; private set; }

        public ViolationData ViolationData { get; set; }
        public ulong UnlockTime { get; set; }
        public ulong Fee { get; set; }
    }

    class ViolationData
    {
        public bool UnlockTimeViolation { get; set; } = false;
        public bool FeeViolation { get; set; } = false;
        public bool IsDoubleSpend { get; set; } = false;
        public bool NonDefaultExtraField { get; set; } = false;
    }

    class MoneroTransactionIdentifier
    {
        public static (bool IsSuspicious, SuspicionData Information) GetSuspicionData(MemoryPoolTransaction tx)
        {
            var susInfo = GetSuspicionDegree(tx);
            return (susInfo.SuspicionData != default, susInfo.SuspicionData);
        }

        private static (SuspicionDegree Degree, SuspicionData SuspicionData) GetSuspicionDegree(MemoryPoolTransaction tx)
        {
            // Lets drill down into the information...
            var json = JsonSerializer.Deserialize<MemoryPoolTransactionJsonDecoding>(tx.AsJson, null);
            var vd = GetViolationData(tx, json);
            uint score = GetViolationScore(vd);
            var degree = SuspicionDegreeCalculator.GetSuspicionDegree(score);

            SuspicionData suspicionData = null;
            bool isSuspicious = IsSuspicious(degree);
            if (isSuspicious)
                suspicionData = new SuspicionData(tx.TxHash, degree)
                {
                    ViolationData = vd,
                    UnlockTime = json.UnlockTime,
                    Fee = json.RingSignature.Fee,
                };

            return (degree, suspicionData);
        }

        private static ViolationData GetViolationData(MemoryPoolTransaction tx, MemoryPoolTransactionJsonDecoding json)
        {
            ViolationData vd = new();
            if (tx.DoubleSpendSeen)
            {
                vd.IsDoubleSpend = true;
            }
            if (json.UnlockTime >= SuspicionThresholds.UnlockTime)
            {
                vd.UnlockTimeViolation = true;
            }
            if (json.RingSignature.Fee >= SuspicionThresholds.Fee)
            {
                vd.FeeViolation = true;
            }
            int bitCpunt = json.Extra.Count * 8;
            if (bitCpunt >= 500)
            {
                vd.NonDefaultExtraField = true;
            }
            return vd;
        }

        private static uint GetViolationScore(ViolationData vd)
        {
            uint score = 0;
            if (vd.FeeViolation)
                score += SuspicionScore.Fee;
            if (vd.IsDoubleSpend)
                score += SuspicionScore.DoubleSpend;
            if (vd.UnlockTimeViolation)
                score += SuspicionScore.UnlockTime;
            if (vd.NonDefaultExtraField)
                score += SuspicionScore.PaymentID;
            return score;
        }

        public static bool IsSuspicious(SuspicionDegree suspicionDegree)
        {
            return suspicionDegree > SuspicionDegree.Indeterminate;
        }

        public static string GenerateMessage(SuspicionData susInfo)
        {
            StringBuilder sb = new StringBuilder($"Suspicion Level: {susInfo.SuspicionDegree}!");
            sb.AppendLine();
            sb.AppendLine($"https://xmrchain.net/tx/{susInfo.TxHash}");
            sb.AppendLine();
            if (susInfo.ViolationData.IsDoubleSpend)
                sb.AppendLine($"Double Spend Found!");
            if (susInfo.ViolationData.UnlockTimeViolation)
            {
                sb.AppendLine($"Unlock Time is greater than 0! (unlock_time = {susInfo.UnlockTime})");
                if (susInfo.UnlockTime < 1_000_000)
                    sb.AppendLine($"You also misused this field! unlock_time is absolute block height, not relative block height!");
            }
            if (susInfo.ViolationData.FeeViolation)
                sb.AppendLine($"Very large fee! ({MoneroDenominationConverter.PiconeroToMonero(susInfo.Fee):N12} XMR)");
            if (susInfo.ViolationData.NonDefaultExtraField)
                sb.AppendLine($"This transactions 'extra' field is packed. Either you're using a PaymentID, or including other information in this field.");
            sb.AppendLine();
            sb.AppendLine($"#monero #privacy");
            return sb.ToString();
        }
    }

    class SuspicionScore
    {
        public static readonly uint DoubleSpend = 10;
        public static readonly uint UnlockTime = 8;
        public static readonly uint PaymentID = 5;
        public static readonly uint Fee = 3;
    }

    class SuspicionThresholds
    {
        public static readonly uint UnlockTime = 1;
        public static readonly ulong Fee = 10_000_000_000;
        public static readonly string DefaultPaymentID = "";
    }

    class SuspicionDegreeCalculator
    {
        public static SuspicionDegree GetSuspicionDegree(uint suspicionScore)
        {
            if (suspicionScore >= VERY_HIGH_SCORE)
                return SuspicionDegree.VeryHigh;
            else if (suspicionScore >= HIGH_SCORE)
                return SuspicionDegree.High;
            else if (suspicionScore >= MEDIUM_SCORE)
                return SuspicionDegree.Medium;
            else if (suspicionScore >= LOW_SCORE)
                return SuspicionDegree.Low;
            else
                return SuspicionDegree.Indeterminate;
        }

        private const uint VERY_HIGH_SCORE = 10;
        private const uint HIGH_SCORE = 8;
        private const uint MEDIUM_SCORE = 5;
        private const uint LOW_SCORE = 3;
    }
}
