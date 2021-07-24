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
        public bool UnlockTimeEntry { get; set; } = false;

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
            if (json.UnlockTime >= EntryThreshold.UnlockTime)
            {
                vd.UnlockTimeEntry = true;
            }

            return vd;
        }

        private static uint GetViolationScore(ViolationData vd)
        {
            uint score = 0;
            if (vd.UnlockTimeEntry)
                score += SuspicionScore.UnlockTime;
            return score;
        }

        public static bool IsSuspicious(SuspicionDegree suspicionDegree)
        {
            return suspicionDegree > SuspicionDegree.Indeterminate;
        }

        public static string GenerateMessage(SuspicionData susInfo)
        {
            StringBuilder sb = new StringBuilder();
            if (susInfo.ViolationData.UnlockTimeEntry)
            {
                sb.AppendLine(susInfo.TxHash);
            }
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

    class EntryThreshold
    {
        public static readonly uint UnlockTime = 3_200_250;

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
