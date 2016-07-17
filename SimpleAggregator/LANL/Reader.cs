using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    abstract class LanlReaderBase: ReaderBase {
        Aggregator aggregator;
        FileStream fileStream;
        StreamReader streamReader;
        CalculatorOptions options;

        protected abstract string FileName { get; }

        public LanlReaderBase(string path, CalculatorOptions options, Aggregator aggregator) {
            this.options = options;
            this.aggregator = aggregator;
            fileStream = new FileStream(Path.Combine(path, FileName), FileMode.Open, FileAccess.Read, FileShare.Read);
            streamReader = new StreamReader(fileStream);

        }

        protected virtual int GetTimeStamp(string[] lineParts) {
            return Convert.ToInt32(lineParts[0]);
        }
        protected abstract string GetRowValue(string[] lineParts);
        protected abstract string[] GetColumnValues(string[] lineParts);

        public override void ReadNextTimeStamp(int timeIndex) {
            while(!streamReader.EndOfStream) {
                string line = streamReader.ReadLine();
                string[] lineParts = line.Split(',');
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

        public override void Dispose() {
            streamReader.Close();
            streamReader.Dispose();
            fileStream.Close();
            fileStream.Dispose();
        }
    }
    class ProcReader : LanlReaderBase {
        //1,C553$@DOM1,C553,P16,Start
        protected override string FileName { get { return "proc.txt"; } }

        public ProcReader(string path, CalculatorOptions options, Aggregator aggregator) : base(path, options, aggregator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { "Proc=" + lineParts[3] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[2];
        }
    }
    class DnsReader : LanlReaderBase {
        //31,C161,C2109
        protected override string FileName { get { return "dns.txt"; } }

        public DnsReader(string path, CalculatorOptions options, Aggregator aggregator) : base(path, options, aggregator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { "dns" + lineParts[2] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[1];
        }
    }
    class FlowsReader : LanlReaderBase {
        //1,9,C3090,N10471,C3420,N46,6,3,144
        protected override string FileName { get { return "flows.txt"; } }

        public FlowsReader(string path, CalculatorOptions options, Aggregator aggregator) : base(path, options, aggregator) { }
        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[] {
                "Flows=" + lineParts[4],
                "Port=" + lineParts[5]
            };
        }
        protected override string GetRowValue(string[] lineParts) {
            return lineParts[2];
        }
    }
    class AuthReader : LanlReaderBase {
        //1,C625$@DOM1,U147@DOM1,C625,C625,Negotiate,Batch,LogOn,Success
        protected override string FileName { get { return "auth.txt"; } }

        public AuthReader(string path, CalculatorOptions options, Aggregator aggregator) : base(path, options, aggregator) { }
        protected override string[] GetColumnValues(string[] lineParts) {
            //if(lineParts[3] != lineParts[4]) {
            //    return new string[] {
            //        lineParts[4]
            //    };
            //} else {
            //    return new string[0];
            //}
            //return new [] { lineParts[4], lineParts[5], lineParts[7], lineParts[8] };

            return new[] {
                "Dest=" + (lineParts[3] == lineParts[4] ? "Self" : lineParts[4]),
                "Type=" + lineParts[5],
                "Result=" + lineParts[8]
            };
        }
        protected override string GetRowValue(string[] lineParts) {
            return lineParts[3];
        }
    }
}
