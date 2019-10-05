using System;

namespace LiveSplit.TimeFormatters
{
    public class PreciseDeltaFormatter : ITimeFormatter
    {
        public TimeAccuracy Accuracy { get; set; }

        public PreciseDeltaFormatter(TimeAccuracy accuracy)
        {
            Accuracy = accuracy;
        }

        public string Format(TimeSpan? time)
        {
            var deltaTime = new DeltaTimeFormatter();
            deltaTime.Accuracy = Accuracy;
            deltaTime.DropDecimals = false;
            var formattedTime = deltaTime.Format(time);
            if (time == null)
                return TimeFormatConstants.DASH;

            return formattedTime;
        }
    }
}
