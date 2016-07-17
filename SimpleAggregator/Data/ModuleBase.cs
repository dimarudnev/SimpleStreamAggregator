using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    interface IRedTeam {
        int AnomalyIndex(int timeStamp, string comp);
        bool IsUnderAttack(string comp);
    }
    interface IModuleBase {
        string SourcePath { get; }
        string DestinationPath { get; }

        IRedTeam CreateRedTeam(CalculatorOptions options);
        List<ReaderBase> CreateReaders(CalculatorOptions options, Aggregator aggregator);
        
    }
}
