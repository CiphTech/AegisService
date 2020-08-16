using System;

namespace Aegis.Model
{
    /// <summary>
    /// Represents sent message in conversation (MIC)
    /// </summary>
    public class MicInfo
    {
        public AegisMessageInfo Message { get; }

        public DateTime UtcTime { get; }

        public long Counter { get; }

        public MicInfo(AegisMessageInfo message, DateTime utcTime, long counter)
        {
            Message = message;
            UtcTime = utcTime;
            Counter = counter;
        }
    }
}