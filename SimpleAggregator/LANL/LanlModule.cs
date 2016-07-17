using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    class LanlModule : IModuleBase {
        public string DestinationPath {
            get { return @"D:\Data\LANL"; }
        }

        public string SourcePath {
            get { return @"D:\Data\LANL\Source"; }
        }

        public List<ReaderBase> CreateReaders(CalculatorOptions options, Aggregator aggregator) {
            return new List<ReaderBase> {
                new ProcReader(SourcePath, options, aggregator)
                //new DnsReader(options, aggregator),
                //new FlowsReader(options, aggregator),
                //new AuthReader(options, aggregator)
            };
        }

        public IRedTeam CreateRedTeam(CalculatorOptions options) {
            return new RedTeam(SourcePath, options);
        }
    }
}
