using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    abstract class ReaderBase {
        private CalculatorOptions options;
        private Aggregator aggregator;

        public ReaderBase(CalculatorOptions options, Aggregator aggregator) {
            this.options = options;
            this.aggregator = aggregator;
        }


        protected abstract int GetTimeStamp(string[] lineParts);
        protected abstract string GetRowValue(string[] lineParts);
        protected abstract string[] GetColumnValues(string[] lineParts);

        protected abstract bool ReadNextRecord(out string[] output);

        public void ReadNextTimeStamp(int timeIndex) {
            string[] lineParts;
            while(ReadNextRecord(out lineParts)) {
                var timeStamp = GetTimeStamp(lineParts);
                if(timeStamp >= options.StartTime) {
                    timeStamp -= options.StartTime;
                    int currentTimeIndex = (int)(timeStamp / this.options.TimeFrame);
                    if(timeIndex == currentTimeIndex) {
                        aggregator.AddValue(GetRowValue(lineParts), GetColumnValues(lineParts));
                    } else {
                        break;
                    }
                }
            }
        }
        public abstract void Dispose();
    }
}
