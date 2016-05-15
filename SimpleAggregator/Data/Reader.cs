﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    abstract class ReaderBase {
        CalculatorBase calculator;
        FileStream fileStream;
        StreamReader streamReader;
        CalculatorOptions options;

        protected abstract string FileName { get; }

        public ReaderBase(CalculatorOptions options, CalculatorBase calculator) {
            this.options = options;
            this.calculator = calculator;
            fileStream = new FileStream(Path.Combine(options.SourcePath, FileName), FileMode.Open);
            streamReader = new StreamReader(fileStream);

        }

        protected virtual int GetTimeStamp(string[] lineParts) {
            return Convert.ToInt32(lineParts[0]);
        }
        protected abstract string GetRowValue(string[] lineParts);
        protected abstract string[] GetColumnValues(string[] lineParts);

        public void ReadNextTimeStamp(int timeIndex) {
            while(!streamReader.EndOfStream) {
                string line = streamReader.ReadLine();
                string[] lineParts = line.Split(',');
                var timeStamp = GetTimeStamp(lineParts);
                int currentTimeIndex = (int)(timeStamp / this.options.TimeFrame);
                if(timeIndex == currentTimeIndex) {
                    calculator.AddValue(GetRowValue(lineParts), GetColumnValues(lineParts));
                } else {
                    break;
                }
            }
        }
    }
    class ProcReader : ReaderBase {
        protected override string FileName { get { return "proc.txt"; } }

        public ProcReader(CalculatorOptions options, CalculatorBase calculator) : base(options, calculator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { lineParts[3] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[2];
        }
    }
    class DnsReader : ReaderBase {
        protected override string FileName { get { return "dns.txt"; } }

        public DnsReader(CalculatorOptions options, CalculatorBase calculator) : base(options, calculator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { "dns" + lineParts[2] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[1];
        }
    }
    class FlowsReader : ReaderBase {
        protected override string FileName { get { return "flows.txt"; } }

        public FlowsReader(CalculatorOptions options, CalculatorBase calculator) : base(options, calculator) {

        }

        protected override string[] GetColumnValues(string[] lineParts) {
            return new string[1] { "flows" + lineParts[4] + lineParts[5] };
        }

        protected override string GetRowValue(string[] lineParts) {
            return lineParts[2];
        }
    }
}
