using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator.ISCX {
    class IscxModule : IModuleBase {
        public string DestinationPath {
            get { return @"C:\Data"; }
        }

        public string SourcePath {
            get { return @"C:\Data\Source"; }
        }

        public List<ReaderBase> CreateReaders(CalculatorOptions options, Aggregator aggregator) {
            return new List<ReaderBase> { new IscxReader(SourcePath, options, aggregator) };
        }

        public IRedTeam CreateRedTeam(CalculatorOptions options) {
            return new IscxRedTeam();
        }
    }
}
