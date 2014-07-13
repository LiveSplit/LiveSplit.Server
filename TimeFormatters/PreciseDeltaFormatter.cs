using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.TimeFormatters
{
    class PreciseDeltaFormatter : ITimeFormatter
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
                return "-";
            else
                return formattedTime;
        }
    }
}
